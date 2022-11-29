using System;
using Leadtools.Annotations.Core;
using Leadtools.Documents;
using TABFusionRMS.Web.Tool.Helpers;
using TABFusionRMS.Web.Tool.Model;
using TabFusionRMS.Web.Tool.Model.Page;

namespace TabFusionRMS.Web.Tool.Helpers
{
    internal sealed class AnnotationMethods
    {
        private AnnotationMethods()
        {
        }
        public static GetAnnotationsResponse GetAnnotations(GetAnnotationsRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException("request");
            }

            if (string.IsNullOrEmpty(request.DocumentId))
            {
                throw new ArgumentException("documentId must not be null");
            }

            if (request.PageNumber < 0)
            {
                throw new ArgumentException("'pageNumber' must be a value greater than or equal to 0");
            }

            // If page number is 0, get all annotations

            // Now load the document
            var cache = ServiceHelper.Cache;
            using (var document = DocumentFactory.LoadFromCache(cache, request.DocumentId))
            {
                DocumentHelper.CheckLoadFromCache(document);

                var annCodec = new AnnCodecs();
                string annotations = null;
                if (request.PageNumber == 0)
                {
                    var containers = document.Annotations.GetAnnotations(request.CreateEmpty);
                    annotations = annCodec.SaveAllToString(containers, AnnFormat.Annotations);
                }
                else
                {
                    DocumentHelper.CheckPageNumber(document, request.PageNumber);

                    var documentPage = document.Pages[request.PageNumber - 1];

                    var container = documentPage.GetAnnotations(request.CreateEmpty);
                    if (container is not null)
                    {
                        annotations = annCodec.SaveToString(container, AnnFormat.Annotations, request.PageNumber);
                    }
                }
                return new GetAnnotationsResponse() { Annotations = annotations };
            }
        }

        public static Response SetAnnotations(SetAnnotationsRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException("request");
            }

            if (string.IsNullOrEmpty(request.DocumentId))
            {
                throw new ArgumentException("documentId must not be null");
            }

            if (request.PageNumber < 0)
            {
                throw new ArgumentException("'pageNumber' must be a value greater than or equal to 0");
            }

            // If pageNumber 0, set for all pages

            var annCodec = new AnnCodecs();
            AnnContainer[] containers = null;

            if (!string.IsNullOrEmpty(request.Annotations))
            {
                containers = annCodec.LoadAllFromString(request.Annotations);
            }

            // Now load the document
            var cache = ServiceHelper.Cache;
            using (var document = DocumentFactory.LoadFromCache(cache, request.DocumentId))
            {
                DocumentHelper.CheckLoadFromCache(document);

                // Check if it's a pre-cached Document. If so, in this case don't set the annotations
                // Because we don't want multiple users changing the properties of the document.
                if (PreCacheHelper.PreCacheExists && PreCacheHelper.CheckDocument(document.Uri, document.Images.MaximumImagePixelSize) is not null)
                {
                    return new Response();
                }

                // If the document is read-only then below will fail. So, temporarily reset the value
                bool wasReadOnly = document.IsReadOnly;
                document.IsReadOnly = false;

                if (request.PageNumber == 0)
                {
                    // Set all
                    document.Annotations.SetAnnotations(containers);
                }
                else
                {
                    DocumentHelper.CheckPageNumber(document, request.PageNumber);

                    var documentPage = document.Pages[request.PageNumber - 1];
                    AnnContainer container = null;
                    if (containers is not null)
                    {
                        if (containers.Length == 1)
                        {
                            container = containers[0];
                        }
                        else
                        {
                            int i = 0;
                            while (i < containers.Length && container is null)
                            {
                                if (containers[i].PageNumber == request.PageNumber)
                                {
                                    container = containers[i];
                                }
                                i += 1;
                            }
                        }
                    }

                    documentPage.SetAnnotations(container);
                }

                // reset the read-only value before saving into the cache
                document.IsReadOnly = wasReadOnly;
                document.SaveToCache();
            }
            return new Response();
        }
    }

}