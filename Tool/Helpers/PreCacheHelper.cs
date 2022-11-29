using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using Leadtools;
using Leadtools.Caching;
using Leadtools.Codecs;
using Leadtools.Documents;
using Leadtools.Forms.Ocr;
using TABFusionRMS.Web.Tool.Model.PreCache;

namespace TABFusionRMS.Web.Tool.Helpers
{
    public sealed partial class PreCacheHelper
    {
        private PreCacheHelper()
        {
        }
        // 
        // * The PreCacheHelper is a static utility class that holds a secondary LEADTOOLS Cache.
        // * This Cache does not hold Document objects like the Documents Library Cache does; instead, it
        // * holds a URI => Document ID mapping for documents that have been loaded into the Documents cache.
        // * The pre-cache can thus be used to return the same cached document ID for a URL, which then is used
        // * to load the same Document from the Documents Cache. All requests for a given URI will respond with the same
        // * object instead of creating new cache entries.
        // * 
        // * Note that the PreCacheHelper uses lock() for basic thread safety. The internal Cache code is always thread and application safe,
        // * whereas this basic utility class is not.
        // 


        private const string PreCacheLegendName = "Legend";
        private static ObjectCache _preCache = default;
        public static bool PreCacheExists
        {
            get
            {
                return _preCache is not null;
            }
        }

        // Lock for when we need to synchronously update the legend cache item
        private static readonly object LegendLock = new object();
        private static readonly object CreationLock = new object();
        private static readonly object ReadingLock = new object();

        public static int[] DefaultSizes = new int[] { 4096, 2048 };

        private static Dictionary<string, string> GetLegend(bool createIfNotExist)
        {
            Dictionary<string, string> legend = _preCache.Get<Dictionary<string, string>>(PreCacheLegendName, default);
            if (legend is null && createIfNotExist)
            {
                legend = new Dictionary<string, string>();
            }
            return legend;
        }

        private static string GetRegionName(Uri requestUri)
        {
            // our uri will be the region name, but we obviously can't have certain characters.
            // so create a hash (32 characters).

            string uri = requestUri.ToString().ToLower();
            var bytes = Encoding.ASCII.GetBytes(uri);
            byte[] hash = null;
            using (var md5 = MD5.Create())
            {
                hash = md5.ComputeHash(bytes);
            }

            // change it back to a string
            var result = new StringBuilder(hash.Length * 2);
            for (int i = 0, loopTo = hash.Length - 1; i <= loopTo; i++)
                result.Append(hash[i].ToString("X2"));

            return result.ToString();
        }
        private static string GetKeyName(int size)
        {
            return "size_" + size;
        }

        public static void CreatePreCache()
        {
            // check first, so not everyone here will get caught by the lock
            if (_preCache is not null)
            {
                return;
            }

            // Use the lock. Obviously this won't work across multiple processes
            lock (CreationLock)
            {
                // check again, for those who got caught by the lock
                if (_preCache is not null)
                {
                    return;
                }

                string preCacheDirectory = ServiceHelper.GetSettingValue(ServiceHelper.Key_PreCache_Directory);
                if (string.IsNullOrEmpty(preCacheDirectory))
                {
                    preCacheDirectory = ServiceHelper.Default_PreCache_Directory;
                }

                preCacheDirectory = ServiceHelper.GetAbsolutePath(preCacheDirectory);
                if (string.IsNullOrEmpty(preCacheDirectory))
                {
                    // No setting, pre-caching is disabled
                    return;
                }

                try
                {
                    var cache = new FileCache();
                    cache.CacheDirectory = preCacheDirectory;
                    if (!Directory.Exists(preCacheDirectory))
                    {
                        Directory.CreateDirectory(preCacheDirectory);
                    }

                    // Choose how we want to serialize the data. We choose JSON for human-readability.
                    cache.DataSerializationMode = CacheSerializationMode.Json;
                    cache.PolicySerializationMode = CacheSerializationMode.Json;

                    _preCache = cache;
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Could not create pre-cache", e);
                }
            }
        }

        public static PreCacheDocumentResponse AddExistingDocument(Uri uri, Leadtools.Documents.Document document)
        {
            // Add an existing document to the pre-cache.
            // This is used in LoadFromUri for those who wish to get the same cached document
            // from the same URL all the time.
            // Note we do no special loading of text or images here.

            // Get the safe hash name from the uri
            string regionName = GetRegionName(uri);

            // The PreCacheEntries are what will be cached, based on this map of sizes to documentId values.
            var sizeIdDictionary = new Dictionary<int, string>();

            var maximumImagePixelSizes = DefaultSizes;
            // The PreCacheResponseItems are what we will return as a confirmation.
            var responseItems = new PreCacheResponseSizeItem[maximumImagePixelSizes.Length];

            for (int index = 0, loopTo = maximumImagePixelSizes.Length - 1; index <= loopTo; index++)
            {
                int size = maximumImagePixelSizes[index];
                // If it's in the cache, delete it (deletes from PreCache also)
                string documentId = InternalCheckDocument(regionName, GetKeyName(size));
                if (documentId is not null)
                {
                    DocumentHelper.DeleteDocument(documentId, false, false);
                }
                else
                {
                    documentId = document.DocumentId;
                }

                responseItems[index] = new PreCacheResponseSizeItem()
                {
                    Seconds = 0,
                    DocumentId = documentId,
                    MaximumImagePixelSize = size
                };

                // add to our dictionary for updating the pre-cache all at once
                sizeIdDictionary.Add(size, documentId);
            }

            // Add all the info to the PreCache
            AddDocumentToPreCache(regionName, uri, sizeIdDictionary);

            return new PreCacheDocumentResponse()
            {
                Item = new PreCacheResponseItem()
                {
                    Uri = uri.ToString(),
                    RegionHash = regionName,
                    Items = responseItems
                }
            };
        }

        public static PreCacheDocumentResponse AddDocument(ObjectCache cache, PreCacheDocumentRequest request)
        {
            var loadOptions = new LoadDocumentOptions();
            loadOptions.Cache = cache;
            loadOptions.UseCache = true;

            // Get the expiry policy
            CacheItemPolicy cachePolicy;

            if (request.ExpiryDate == new DateTime())
            {
                cachePolicy = ServiceHelper.CreateForeverPolicy();
            }
            else
            {
                cachePolicy = new CacheItemPolicy() { AbsoluteExpiration = request.ExpiryDate };
            }

            loadOptions.CachePolicy = cachePolicy;
            // Get the maximum pixel size, if the user did not pass one, use the default values of 4096 and 2048 (used by the DocumentViewerDemo)
            int[] maximumImagePixelSizes = request.MaximumImagePixelSizes;
            if (maximumImagePixelSizes is null || maximumImagePixelSizes.Length == 0)
            {
                maximumImagePixelSizes = DefaultSizes;
            }

            // Sort the maximum image pixel size from largest to smallest
            // We will re-use the values from largest to set the smaller images text and SVG since they do
            // not change
            Array.Sort(maximumImagePixelSizes, new Comparison<int>((x, y) => y.CompareTo(x)));

            // Get the safe hash name from the uri
            string regionName = GetRegionName(request.Uri);

            // The PreCacheEntries are what will be cached, based on this map of sizes to documentId values.
            var sizeIdDictionary = new Dictionary<int, string>();

            // The PreCacheResponseItems are what we will return as a confirmation.
            var responseItems = new PreCacheResponseSizeItem[maximumImagePixelSizes.Length];

            IOcrEngine ocrEngine = ServiceHelper.GetOCREngine();

            // Largest document (to re-use)
            Leadtools.Documents.Document largestDocument = default;

            try
            {
                // Now load the document and cache it
                for (int index = 0, loopTo = maximumImagePixelSizes.Length - 1; index <= loopTo; index++)
                {
                    // No duplicates
                    if (index > 0 && maximumImagePixelSizes[index] == maximumImagePixelSizes[index - 1])
                    {
                        continue;
                    }

                    int size = maximumImagePixelSizes[index];

                    // If it's in the cache, delete it (deletes from PreCache also)
                    string documentId = InternalCheckDocument(regionName, GetKeyName(size));
                    if (documentId is not null)
                    {
                        DocumentHelper.DeleteDocument(documentId, false, false);
                    }

                    // keep track for logging purposes
                    var start = DateTime.Now;

                    // re-use the load options, just change the size
                    loadOptions.MaximumImagePixelSize = size;

                    // Cache the Document
                    var document = AddDocumentToCache(largestDocument, ocrEngine, request.Uri, loadOptions, request.CacheOptions);
                    try
                    {
                        var stop = DateTime.Now;
                        documentId = document.DocumentId;

                        responseItems[index] = new PreCacheResponseSizeItem()
                        {
                            Seconds = Math.Round((stop - start).TotalSeconds, 4),
                            DocumentId = documentId,
                            MaximumImagePixelSize = size
                        };

                        // add to our dictionary for updating the pre-cache all at once
                        sizeIdDictionary.Add(size, documentId);
                    }
                    finally
                    {
                        if (largestDocument is null)
                        {
                            largestDocument = document;
                        }
                        else
                        {
                            document.Dispose();
                        }
                    }
                }
            }
            finally
            {
                if (largestDocument is not null)
                {
                    largestDocument.Dispose();
                }
            }

            // Add all the info to the PreCache
            AddDocumentToPreCache(regionName, request.Uri, sizeIdDictionary);

            return new PreCacheDocumentResponse()
            {
                Item = new PreCacheResponseItem()
                {
                    Uri = request.Uri.ToString(),
                    RegionHash = regionName,
                    Items = responseItems
                }
            };
        }

        private static Leadtools.Documents.Document AddDocumentToCache(Leadtools.Documents.Document largestDocument, IOcrEngine ocrEngine, Uri documentUri, LoadDocumentOptions loadOptions, DocumentCacheOptions cacheOptions)
        {
            // Adds the document to the cache. Note that a new cache entry is created for each different maximumImagePixelSize.

            Leadtools.Documents.Document document = DocumentFactory.LoadFromUri(documentUri, loadOptions);
            try
            {
                if (document is null)
                {
                    throw new InvalidOperationException("Failed to load URI: " + Convert.ToString(documentUri));
                }

                // We will modify this document...
                bool wasReadOnly = document.IsReadOnly;
                document.IsReadOnly = false;

                if (document.Text.TextExtractionMode != DocumentTextExtractionMode.OcrOnly && !document.Images.IsSvgSupported && ocrEngine is not null)
                {
                    document.Text.OcrEngine = ocrEngine;
                }

                // Set in the cache options that we want
                document.CacheOptions = cacheOptions;

                // prepare the document, caching as much as possible.
                if (document.IsStructureSupported && !document.Structure.IsParsed)
                {
                    document.Structure.Parse();
                }

                // Need to cache the SVG with and without getting the back image
                var loadSvgOptions = new CodecsLoadSvgOptions();

                foreach (DocumentPage page in document.Pages)
                {
                    // If we have a previous largest document, use the same
                    // SVG and text instead of recreating them (they do not change based on image size)
                    DocumentPage largestDocumentPage = default;

                    if (largestDocument is not null)
                    {
                        largestDocumentPage = largestDocument.Pages[page.PageNumber - 1];
                    }

                    if (cacheOptions == DocumentCacheOptions.None)
                    {
                        // We are done, do not cache the images, svg or text
                        continue;
                    }

                    // SVG, this does not depend on the image size
                    using (Leadtools.Svg.SvgDocument svg = page.GetSvg(default))
                    {
                    }

                    using (Leadtools.Svg.SvgDocument svg = page.GetSvg(loadSvgOptions))
                    {
                    }

                    // SVG back image, this is different based on the image size
                    using (RasterImage svgBack = page.GetSvgBackImage(RasterColor.FromKnownColor(RasterKnownColor.White)))
                    {
                    }

                    // Image, this is different based on the image size
                    using (RasterImage image = page.GetImage())
                    {
                    }

                    // Thumbnail, this does not depend on the image size but there is no set thumbnail method
                    using (RasterImage thumbnailImage = page.GetThumbnailImage())
                    {
                    }

                    // Text, this does not depend on the image size
                    if (largestDocumentPage is null)
                    {
                        page.GetText();
                    }
                    else
                    {
                        DocumentPageText pageText = largestDocumentPage.GetText();
                        page.SetText(pageText);
                    }
                }

                document.AutoDeleteFromCache = false;
                document.AutoDisposeDocuments = true;
                document.AutoSaveToCache = false;
                // Stop caching
                document.CacheOptions = DocumentCacheOptions.None;
                document.IsReadOnly = wasReadOnly;

                // save it to the regular cache
                document.SaveToCache();

                return document;
            }
            catch (Exception generatedExceptionName)
            {
                if (document is not null)
                {
                    document.Dispose();
                }
                throw;
            }
        }

        private static void AddDocumentToPreCache(string hashRegion, Uri uri, Dictionary<int, string> sizeItems)
        {
            // keep it in the precache until deleted.
            CacheItemPolicy cachePolicy = ServiceHelper.CreateForeverPolicy();

            foreach (KeyValuePair<int, string> dictionaryEntry in sizeItems)
            {
                // create our object to place in the cache
                var cacheEntry = new PreCacheEntry()
                {
                    maximumImagePixelSize = dictionaryEntry.Key,
                    documentId = dictionaryEntry.Value,
                    reads = 0
                };

                // use the regionHash as our region name, the size as our key, and the entry as our value.
                var cacheItem = new CacheItem<PreCacheEntry>(GetKeyName(cacheEntry.maximumImagePixelSize), cacheEntry, hashRegion);
                _preCache.Add(cacheItem, cachePolicy);
            }

            lock (LegendLock)
            {
                // Also add this to our "Legend" file that will just hold all the regionHash <==> uri mappings in one place.
                var legendMappings = GetLegend(true);
                if (legendMappings.ContainsKey(hashRegion))
                {
                    legendMappings.Remove(hashRegion);
                }
                legendMappings.Add(hashRegion, uri.ToString());

                // Add it back to the cache
                _preCache.Add(new CacheItem<Dictionary<string, string>>(PreCacheLegendName, legendMappings), ServiceHelper.CreateForeverPolicy());
            }
        }

        public static string CheckDocument(Uri documentUri, int maximumImagePixelSize)
        {
            return InternalCheckDocument(GetRegionName(documentUri), GetKeyName(maximumImagePixelSize));
        }

        private static string InternalCheckDocument(string regionName, string keyName)
        {
            // Add a lock to the read, since we're changing the cache item and re-saving it.
            lock (ReadingLock)
            {
                PreCacheEntry entry = _preCache.Get<PreCacheEntry>(keyName, regionName);
                if (entry is not null)
                {
                    // Update the "reads" property
                    entry.reads += 1;
                    _preCache.Add(new CacheItem<PreCacheEntry>(keyName, entry, regionName), ServiceHelper.CreateForeverPolicy());
                    return entry.documentId;
                }
                else
                {
                    return null;
                }
            }
        }

        public static ReportPreCacheResponse ReportDocuments(ObjectCache cache, bool syncWithCache)
        {
            var responseEntries = new List<PreCacheResponseItem>();
            var responseRemoved = new List<PreCacheResponseItem>();

            // we'll need the legend file to get the URI back from the regionName.
            var legendMappings = GetLegend(false);

            // Note: since we have a set structure, we won't need to check case regionName == null.
            _preCache.EnumerateRegions((string regionName) =>
            {
                var Uri = "";
                if (legendMappings.ContainsKey(regionName))
                {
                    Uri = legendMappings[regionName];
                };

                List<PreCacheResponseSizeItem> entriesSizeItems = new List<PreCacheResponseSizeItem>();

                List<PreCacheResponseSizeItem> removedSizeItems = new List<PreCacheResponseSizeItem>();

                _preCache.EnumerateKeys(regionName, (string keyName) =>
                {

                    PreCacheEntry entry = _preCache.Get<PreCacheEntry>(keyName, regionName);

                    PreCacheResponseSizeItem sizeItem = new PreCacheResponseSizeItem()
                    {
                        DocumentId = entry.documentId,
                        MaximumImagePixelSize = entry.maximumImagePixelSize,
                        Reads = entry.reads
                    };


                    if(syncWithCache)
                    {
                        // if the cache entry did not exist, add it to the removed items.
                        using(Leadtools.Documents.Document document = DocumentFactory.LoadFromCache(cache, entry.documentId))
                        {
                           if(document == null)
                            {
                                removedSizeItems.Add(sizeItem);
                                return;                                                                  
                            }
                        }
                                                                                      
                    }

                    // if !syncWithCache or it was not null
                    entriesSizeItems.Add(sizeItem);

                });

                if(entriesSizeItems.Count > 0)
                {
                    PreCacheResponseItem entriesItem = new PreCacheResponseItem() {
                        RegionHash = regionName,
                        Uri = Uri,
                        Items = entriesSizeItems.ToArray()
                    };
                    responseEntries.Add(entriesItem);
                }

                if(removedSizeItems.Count>0)
                {
                    PreCacheResponseItem removedItem = new PreCacheResponseItem()
                    {
                        RegionHash = regionName,
                        Uri = Uri,
                        Items = removedSizeItems.ToArray()
                    };
                    responseRemoved.Add(removedItem);
                }

            });

            // do the deletion
            if (responseRemoved.Count > 0)
            {
                foreach (PreCacheResponseItem responseItem in responseRemoved)
                {
                    foreach (PreCacheResponseSizeItem sizeItem in responseItem.Items)
                        _preCache.DeleteItem(GetKeyName(sizeItem.MaximumImagePixelSize), responseItem.RegionHash);
                }
                _preCache.EnumerateRegions((string regionName) => CheckDeleteRegion(regionName));
            }

            return new ReportPreCacheResponse()
            {
                Entries = responseEntries.ToArray(),
                Removed = responseRemoved.ToArray()
            };
        }

        public static void RemoveDocument(Uri uri, int[] maximumImagePixelSizes)
        {
            string regionName = GetRegionName(uri);
            // If maximumImagePixelSizes is null, delete all of the sizes for the document.
            if (maximumImagePixelSizes is null)
            {
                _preCache.DeleteRegion(regionName);
            }
            else
            {
                foreach (int size in maximumImagePixelSizes)
                    _preCache.DeleteItem(GetKeyName(size), regionName);
            }
            CheckDeleteRegion(regionName);
        }

        private static void CheckDeleteRegion(string regionName)
        {
            // check if the region is now empty. If so,
            // 1) delete it
            // 2) remove it from the "Table of Contents"

            bool isEmpty = true;
            _preCache.EnumerateKeys(regionName, (string keyName) => isEmpty = false);
            if (isEmpty)
            {
                _preCache.DeleteRegion(regionName);

                // lock here since we're updating the legend item
                lock (LegendLock)
                {
                    // Also delete this from our "Legend" file that holds all the uri <==> regionHash mappings in one place.
                    var legendMappings = GetLegend(false);
                    if (legendMappings is null)
                    {
                        return;
                    }
                    if (legendMappings.ContainsKey(regionName.ToString()))
                    {
                        legendMappings.Remove(regionName.ToString());
                    }

                    // Add it back to the cache
                    _preCache.Add(new CacheItem<Dictionary<string, string>>(PreCacheLegendName, legendMappings), ServiceHelper.CreateForeverPolicy());
                }
            }
        }
    }


}