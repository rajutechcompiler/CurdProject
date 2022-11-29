using System;
using System.Diagnostics;
using System.IO;
using Leadtools;
using Leadtools.Annotations.Core;
using Leadtools.Caching;
using Leadtools.Codecs;
using Leadtools.Documents;
using Leadtools.Documents.Converters;
using Leadtools.Forms.DocumentWriters;
using Leadtools.Forms.Ocr;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TABFusionRMS.Web.Tool.Model.Document;
using TabFusionRMS.WebCS.Tools.Demos;
using TabFusionRMS.WebCS.Controllers;
using TabFusionRMS.Web.Tool.Helpers;

namespace TABFusionRMS.Web.Tool.Helpers
{
    internal sealed partial class ConverterHelper
    {
        private ConverterHelper()
        {
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static ConvertResponse Convert(string documentId, DocumentConverterJobData jobData)
        {
            // Verify input
            if (string.IsNullOrEmpty(documentId))
            {
                throw new ArgumentNullException("documentId");
            }
            if (jobData is null)
            {
                throw new ArgumentNullException("jobData");
            }

            if (jobData.DocumentFormat == DocumentFormat.User && jobData.RasterImageFormat == RasterImageFormat.Unknown)
            {
                throw new ArgumentException("Either DocumentFormat must not be User or RasterImageFormat must not be Unknown", "jobData");
            }
            if (jobData.DocumentFormat != DocumentFormat.User && jobData.RasterImageFormat != RasterImageFormat.Unknown)
            {
                throw new ArgumentException("Either DocumentFormat must be User or RasterImageFormat must be Unknown", "jobData");
            }

            // We must have the converter EXE
            string documentConverterExePath = ServiceHelper.GetSettingValue(ServiceHelper.Key_DocumentConverter_ExePath);
            documentConverterExePath = ServiceHelper.GetAbsolutePath(documentConverterExePath);
            if (string.IsNullOrEmpty(documentConverterExePath))
            {
                documentConverterExePath = CheckSetupDocumentConverterExePath();
            }

            if (!File.Exists(documentConverterExePath))
            {
                throw new InvalidOperationException("DocumentConverter executable not found. Set a valid path for the 'lt.DocumentConverter.ExePath' key in the .config file.");
            }

            // Temp files to save the options to sending to the converter
            string preferencesFileName = null;
            DocumentConverterPreferences preferences = default;

            // Cache region and key for the final document
            string cacheRegion = null;
            string documentCacheKey = null;
            bool isBeginAddDocumentCache = false;
            string annotationsCacheKey = null;
            bool isBeginAddAnnotationsCache = false;
            ObjectCache cache = ServiceHelper.Cache;
            DefaultCacheCapabilities cacheCaps = cache.DefaultCacheCapabilities;

            if ((cacheCaps & DefaultCacheCapabilities.ExternalResources) != DefaultCacheCapabilities.ExternalResources || (cacheCaps & DefaultCacheCapabilities.VirtualDirectory) != DefaultCacheCapabilities.VirtualDirectory)
            {
                throw new InvalidOperationException("This operation is only supported with a cache that supports external resources and virtual directory.");
            }

            bool success = false;

            try
            {
                // Create the preferences file
                preferences = new DocumentConverterPreferences();
                SetupPreferences(cache, preferences, jobData);

                // Fill in the rest

                // Document ID
                preferences.DocumentId = documentId;

                // Get the file extension to use
                string extension;
                if (jobData.DocumentFormat != DocumentFormat.User)
                {
                    extension = DocumentWriter.GetFormatFileExtension(jobData.DocumentFormat);
                }
                else
                {
                    extension = RasterCodecs.GetExtension(jobData.RasterImageFormat);
                }

                // Output file name
                cacheRegion = Guid.NewGuid().ToString("N");

                // Document Cache Key is used for file name, use documentName if provided
                documentCacheKey = jobData.DocumentName is not null ? jobData.DocumentName : "file";
                if (!string.IsNullOrEmpty(extension))
                {
                    documentCacheKey += "." + extension;
                }

                // Begin adding the document file to the cache
                // This will let us get a file name from the cache but not commit it yet
                preferences.OutputDocumentFileName = ServiceHelper.GetFileUri(cache.BeginAddExternalResource(documentCacheKey, cacheRegion, true));
                isBeginAddDocumentCache = true;

                if (preferences.OutputAnnotationsMode == DocumentConverterAnnotationsMode.External)
                {
                    // User asked to save the annotation to a separate file
                    // Get a new item file the cache
                    annotationsCacheKey = jobData.DocumentName is not null ? jobData.DocumentName : "annotations";
                    if (!string.IsNullOrEmpty(extension))
                    {
                        annotationsCacheKey += ".xml";
                    }
                    preferences.OutputAnnotationsFileName = ServiceHelper.GetFileUri(cache.BeginAddExternalResource(annotationsCacheKey, cacheRegion, true));
                    // Not in this demo we will never use this annotation file, instead will be zipped in the final result
                    // so we will never commit this entry
                    isBeginAddAnnotationsCache = true;
                }

                // Save the preferences
                preferencesFileName = RasterDefaults.GetTemporaryFileName();
                preferences.Save(preferencesFileName);

                // Run the converter
                int exitCode = 1;
                using (var process = new Process())
                {
                    process.StartInfo.FileName = documentConverterExePath;
                    process.StartInfo.Arguments = string.Format("\"{0}\"", preferencesFileName);
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.Start();
                    process.WaitForExit();
                    exitCode = process.ExitCode;
                }

                // Re-load the preferences so we get the results
                try
                {
                    DocumentConverterPreferences tempPreferences = DocumentConverterPreferences.Load(preferencesFileName);
                    preferences = tempPreferences.Clone();
                }
                catch
                {
                }

                if (exitCode == 0)
                {
                    // Converter was successful
                    var response = new ConvertResponse();

                    // Check if we have need to archive the results in a ZIP file before returning them to the user
                    bool archiveRequired = IsArchiveRequired(preferences.OutputFiles, extension);
                    if (archiveRequired)
                    {
                        string archiveName = (jobData.DocumentName is not null ? jobData.DocumentName : "document") + ".zip";
                        // Yes, create a new cache item and add the results
                        // and uncommit the old items (not needed anymore)
                        ArchiveResults(archiveName, cache, documentId, preferences.OutputFiles, cacheRegion, response);
                        success = true;
                    }
                    else
                    {
                        // No archiving, so save the original annotations and document
                        cache.EndAddExternalResource(true, documentCacheKey, true, ServiceHelper.CreatePolicy(), cacheRegion);
                        isBeginAddDocumentCache = false;

                        string documentFileName = ServiceHelper.GetFileUri(cache.GetItemExternalResource(documentCacheKey, cacheRegion, false));

                        // If we have *one* output document url (and then maybe the annotations) then set documentName.
                        response.Document = new ConvertItem()
                        {
                            Name = documentCacheKey,
                            Url = CacheController.CreateConversionResultUri(cacheRegion, documentCacheKey),
                            Length = GetFileLength(documentFileName),
                            MimeType = GetMimeType(documentFileName, extension)
                        };

                        // Check for annotations by the set cache key
                        if (annotationsCacheKey is not null)
                        {
                            cache.EndAddExternalResource(true, annotationsCacheKey, true, ServiceHelper.CreatePolicy(), cacheRegion);
                            isBeginAddAnnotationsCache = false;
                            string annotationsFileName = ServiceHelper.GetFileUri(cache.GetItemExternalResource(annotationsCacheKey, cacheRegion, false));

                            response.Annotations = new ConvertItem()
                            {
                                Name = annotationsCacheKey,
                                Url = CacheController.CreateConversionResultUri(cacheRegion, annotationsCacheKey),
                                Length = GetFileLength(annotationsFileName),
                                MimeType = "text/xml"
                            };
                        }
                        success = true;
                    }

                    return response;
                }
                // There was an error, get the error message
                else if (preferences.ErrorMessage is not null)
                {
                    throw new InvalidOperationException(preferences.ErrorMessage);
                }
                else
                {
                    throw new InvalidOperationException("General error in conversion");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Convert - Error:{1}{0}documentId:{2}", Environment.NewLine, ex.Message, documentId), "Error");
                throw;
            }
            finally
            {
                // Delete the temporary files we created
                ServiceHelper.SafeDeleteFile(preferencesFileName);
                if (preferences is not null)
                {
                    ServiceHelper.SafeDeleteFile(preferences.DocumentWriterOptionsPath);
                }

                if (preferences is not null)
                {
                    ServiceHelper.SafeDeleteFile(preferences.InputAnnotationsFileName);
                }

                try
                {
                    // Delete cache items if we havent commited yet
                    if (isBeginAddDocumentCache)
                    {
                        cache.EndAddExternalResource(false, documentCacheKey, false, default, cacheRegion);
                    }
                    if (isBeginAddAnnotationsCache)
                    {
                        cache.EndAddExternalResource(false, annotationsCacheKey, false, default, cacheRegion);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("EndAdd - Error:{1}{0}documentId:{2}", Environment.NewLine, ex.Message, documentId), "Warning");
                }

                if (!success)
                {
                    try
                    {
                        // Delete region if we did not convert the document successfully
                        if (cacheRegion is not null)
                        {
                            cache.DeleteRegion(cacheRegion);
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(string.Format("DeleteRegion - Error:{1}{0}documentId:{2}", Environment.NewLine, ex.Message, documentId), "Warning");
                    }
                }
            }
        }

        private static string CheckSetupDocumentConverterExePath()
        {
            // Check if we are running on a machine that has LEADTOOLS installed, try to get the path automatically
            //string appPath = HostingEnvironment.ApplicationPhysicalPath;
            string appPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath");

            var setupBinDirs = new[] { @"..\..\..\..\..\..\Bin\DotNet4\Win32", @"..\..\..\..\..\..\Bin\DotNet4\x64", @"..\..\..\..\..\..\Bin19\DotNet4\Win32", @"..\..\..\..\..\..\Bin19\DotNet4\x64" };

            const string setupExeName = "CSDocumentConverterDemo_original.exe";
            foreach (string setupBinDir in setupBinDirs)
            {
                string dir = Path.GetFullPath(Path.Combine(appPath, setupBinDir));
                if (Directory.Exists(dir))
                {
                    string exePath = Path.Combine(dir, setupExeName);
                    if (File.Exists(exePath))
                    {
                        return exePath;
                    }
                }
            }

            // Not found
            return null;
        }

        private static DocumentConverterPreferences SetupPreferences(ObjectCache cache, DocumentConverterPreferences preferences, DocumentConverterJobData jobData)
        {
            // Match up as many of DocumentConverterPreferences props with DocumentConverterJobData props as possible
            preferences.JobName = jobData.JobName;

            // Cache
            FileCache fileCache = cache as FileCache;
            if (cache is null)
            {
                throw new InvalidOperationException("Cache must be FileCache for this operation");
            }

            preferences.CacheDirectory = fileCache.CacheDirectory;
            preferences.CacheDataSerializationMode = fileCache.DataSerializationMode;
            preferences.CachePolicySerializationMode = fileCache.PolicySerializationMode;

            // Document
            preferences.InputDocumentFileName = null;
            preferences.InputFirstPage = jobData.InputDocumentFirstPageNumber;
            preferences.InputLastPage = jobData.InputDocumentLastPageNumber;

            // Set Read the maximum number of pages to convert
            preferences.InputMaximumPages = 0;
            string maximumPagesString = ServiceHelper.GetSettingValue(ServiceHelper.Key_DocumentConverter_MaximumPages);
            if (!string.IsNullOrEmpty(maximumPagesString))
            {
                int inputMaximumPages;
                if (int.TryParse(maximumPagesString, out inputMaximumPages))
                {
                    preferences.InputMaximumPages = inputMaximumPages;
                }
            }

            // Format
            preferences.DocumentFormat = jobData.DocumentFormat;
            preferences.RasterImageFormat = jobData.RasterImageFormat;
            preferences.RasterImageBitsPerPixel = jobData.RasterImageBitsPerPixel;

            // Delete all files generated if an error occur
            preferences.PurgeOutputFilesOnError = true;

            // Annotations
            preferences.InputAnnotationsFileName = null;
            if (!string.IsNullOrEmpty(jobData.Annotations))
            {
                // Load the modified annotations, and pass them to the converter
                var annCodecs = new AnnCodecs();
                AnnContainer[] annContainers = annCodecs.LoadAllFromString(jobData.Annotations);
                if (annContainers is not null && annContainers.Length > 0)
                {
                    preferences.InputAnnotationsFileName = RasterDefaults.GetTemporaryFileName();
                    annCodecs.SaveAll(preferences.InputAnnotationsFileName, annContainers, AnnFormat.Annotations);
                }
            }

            preferences.LoadEmbeddedAnnotation = jobData.AnnotationsMode != DocumentConverterAnnotationsMode.None;
            preferences.OutputAnnotationsMode = jobData.AnnotationsMode;

            // Set the options
            preferences.PageNumberingTemplate = jobData.PageNumberingTemplate;
            preferences.EnableSvgConversion = jobData.EnableSvgConversion;
            preferences.SvgImagesRecognitionMode = jobData.SvgImagesRecognitionMode;
            preferences.EmptyPageMode = jobData.EmptyPageMode;
            preferences.PreprocessingDeskew = jobData.PreprocessorDeskew;
            preferences.PreprocessingOrient = jobData.PreprocessorOrient;
            preferences.PreprocessingInvert = jobData.PreprocessorInvert;
            preferences.ErrorMode = jobData.JobErrorMode;
            preferences.EnableTrace = false;
            preferences.OpenOutputDocument = false;

            // OCR
            string engineTypeString = ServiceHelper.GetSettingValue(ServiceHelper.Key_Ocr_EngineType);
            if (!string.IsNullOrEmpty(engineTypeString))
            {
                try
                {
                    if (engineTypeString.Equals("advantage", StringComparison.OrdinalIgnoreCase))
                    {
                        preferences.OCREngineType = OcrEngineType.Advantage;
                    }
                    else if (engineTypeString.Equals("professional", StringComparison.OrdinalIgnoreCase))
                    {
                        preferences.OCREngineType = OcrEngineType.Advantage;
                    }
                    else if (engineTypeString.Equals("arabic", StringComparison.OrdinalIgnoreCase))
                    {
                        preferences.OCREngineType = OcrEngineType.Advantage;
                    }
                }
                catch
                {
                }
            }

            // Check for a location of the OCR Runtime
            string runtimeDirectory = ServiceHelper.GetSettingValue(ServiceHelper.Key_Ocr_RuntimeDirectory);
            runtimeDirectory = ServiceHelper.GetAbsolutePath(runtimeDirectory);
            if (string.IsNullOrEmpty(runtimeDirectory))
            {
                runtimeDirectory = ServiceHelper.CheckOCRRuntimeDirectory();
            }
            preferences.OCREngineRuntimePath = runtimeDirectory;

            // RasterCodecs options
            string rasterCodecsOptionsFilePath = ServiceHelper.GetSettingValue(ServiceHelper.Key_RasterCodecs_OptionsFilePath);
            rasterCodecsOptionsFilePath = ServiceHelper.GetAbsolutePath(rasterCodecsOptionsFilePath);
            preferences.RasterCodecsOptionsPath = rasterCodecsOptionsFilePath;

            if (jobData.DocumentFormat != DocumentFormat.User)
            {
                // Saving as document, get the options
                // Convert the Document Options
                DocumentOptions documentOptions = ConverterHelper.GetDocumentOptions(jobData.DocumentFormat, jobData.DocumentOptions);
                if (documentOptions is not null)
                {
                    // Serialize it to a temp file to pass it to the converter EXE
                    var documentWriter = new DocumentWriter();
                    documentWriter.SetOptions(jobData.DocumentFormat, documentOptions);
                    preferences.DocumentWriterOptionsPath = RasterDefaults.GetTemporaryFileName();
                    documentWriter.SaveOptions(preferences.DocumentWriterOptionsPath);
                }
            }

            return preferences;
        }

        private static bool IsArchiveRequired(string[] outputFiles, string extension)
        {
            // Check if we need to archive the result (put in a ZIP file)

            // If we have more than 1 file
            if (outputFiles.Length > 1)
            {
                return true;
            }

            // If its not one of the known extensions
            switch (extension ?? "")
            {
                // These raster formats are OK

                // These document formats are OK
                case "gif":
                case "jpg":
                case "tif":
                case "bmp":
                case "pdf":
                case "png":
                case "doc":
                case "docx":
                case "xls":
                case "xlsx":
                case "ppt":
                case "pptx":
                case "svg":
                case "txt":
                    {
                        return false;
                    }

                default:
                    {

                        return true;
                    }
            }
        }

        private static void ArchiveResults(string archiveName, ObjectCache cache, string documentId, string[] outputFiles, string cacheRegion, ConvertResponse response)
        {
            string cacheKey = null;
            bool beginAdd = false;

            try
            {
                cacheKey = archiveName;
                // Create a new cache item to archive the file(s)
                string archiveFileName = ServiceHelper.GetFileUri(cache.BeginAddExternalResource(cacheKey, cacheRegion, true));
                beginAdd = true;

                // Create an archive and add the files
                ZipTools.ZipFiles(Path.GetDirectoryName(archiveFileName), outputFiles, archiveFileName);

                cache.EndAddExternalResource(true, cacheKey, true, ServiceHelper.CreatePolicy(), cacheRegion);
                beginAdd = false;

                Uri url = CacheController.CreateConversionResultUri(cacheRegion, cacheKey);
                string fileName = ServiceHelper.GetFileUri(cache.GetItemExternalResource(cacheKey, cacheRegion, false));
                response.Archive = new ConvertItem()
                {
                    Name = cacheKey,
                    Url = url,
                    MimeType = "multipart/x-zip",
                    Length = GetFileLength(fileName)
                };
            }
            finally
            {
                try
                {
                    // Delete the old output files, not needed anymore
                    foreach (string file in outputFiles)
                    {
                        if (File.Exists(file))
                        {
                            try
                            {
                                File.Delete(file);
                            }
                            catch
                            {
                            }
                        }
                    }

                    // Delete cache item if we havent commited yet
                    if (beginAdd)
                    {
                        cache.EndAddExternalResource(false, cacheKey, false, ServiceHelper.CreatePolicy(), cacheRegion);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("EndAdd - Error:{1}{0}documentId:{2}", Environment.NewLine, ex.Message, documentId), "Warning");
                }
            }
        }

        private static long GetFileLength(string fileName)
        {
            return new FileInfo(fileName).Length;
        }

        private static string GetMimeType(string fileName, string extension)
        {
            // Check the extension if it is a TXT file - must faster than doing Info
            if (extension is not null && extension.ToLower() == "txt")
            {
                return "text/plain";
            }

            try
            {
                using (var rasterCodecs = new RasterCodecs())
                {
                    RasterImageFormat format = rasterCodecs.GetFormat(fileName);
                    return RasterCodecs.GetMimeType(format);
                }
            }
            catch
            {
                return "application/octet-stream";
            }
        }

        public static DocumentOptions GetDocumentOptions(DocumentFormat documentFormat, JObject options)
        {
            // Get the type
            if (options is null)
            {
                return default;
            }

            Type documentOptionsType = null;
            switch (documentFormat)
            {
                case DocumentFormat.Ltd:
                    {
                        documentOptionsType = typeof(LtdDocumentOptions);
                        break;
                    }

                case DocumentFormat.Pdf:
                    {
                        documentOptionsType = typeof(PdfDocumentOptions);
                        break;
                    }

                case DocumentFormat.Doc:
                    {
                        documentOptionsType = typeof(DocDocumentOptions);
                        break;
                    }

                case DocumentFormat.Rtf:
                    {
                        documentOptionsType = typeof(RtfDocumentOptions);
                        break;
                    }

                case DocumentFormat.Html:
                    {
                        documentOptionsType = typeof(HtmlDocumentOptions);
                        break;
                    }

                case DocumentFormat.Text:
                    {
                        documentOptionsType = typeof(TextDocumentOptions);
                        break;
                    }

                case DocumentFormat.Emf:
                    {
                        documentOptionsType = typeof(EmfDocumentOptions);
                        break;
                    }

                case DocumentFormat.Xps:
                    {
                        documentOptionsType = typeof(XpsDocumentOptions);
                        break;
                    }

                case DocumentFormat.Docx:
                    {
                        documentOptionsType = typeof(DocxDocumentOptions);
                        break;
                    }

                case DocumentFormat.Xls:
                    {
                        documentOptionsType = typeof(XlsDocumentOptions);
                        break;
                    }

                case DocumentFormat.Pub:
                    {
                        documentOptionsType = typeof(PubDocumentOptions);
                        break;
                    }

                case DocumentFormat.Mob:
                    {
                        documentOptionsType = typeof(MobDocumentOptions);
                        break;
                    }

                case DocumentFormat.Svg:
                    {
                        documentOptionsType = typeof(SvgDocumentOptions);
                        break;
                    }

                case DocumentFormat.AltoXml:
                    {
                        documentOptionsType = typeof(AltoXmlDocumentOptions);
                        break;
                    }

                case DocumentFormat.User:
                    {
                        break;
                    }

                default:
                    {
                        break;
                    }
            }

            if (documentOptionsType is not null)
            {
                return options.ToObject(documentOptionsType) as DocumentOptions;
            }
            else
            {
                return default;
            }
        }
    }

}