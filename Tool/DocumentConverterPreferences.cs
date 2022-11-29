using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Leadtools;
using Leadtools.Annotations.Core;
using Leadtools.Caching;
using Leadtools.Codecs;
using Leadtools.Documents;
using Leadtools.Documents.Converters;
using Leadtools.Forms.DocumentWriters;
using Leadtools.Forms.Ocr;

namespace TabFusionRMS.WebCS.Tools.Demos
{

    // Options for the DocumentConverterDialog
    [Serializable()]
    public class DocumentConverterPreferences
    {
        public DocumentConverterPreferences()
        {
            InputFirstPage = 1;
            InputLastPage = -1;
            DocumentFormat = DocumentFormat.User;
            RasterImageFormat = RasterImageFormat.Unknown;
            RasterImageBitsPerPixel = 24;
            OutputAnnotationsMode = DocumentConverterAnnotationsMode.None;
            EnableSvgConversion = true;
            SvgImagesRecognitionMode = DocumentConverterSvgImagesRecognitionMode.Auto;
            EmptyPageMode = DocumentConverterEmptyPageMode.None;
            UseThreads = true;
            ErrorMode = DocumentConverterJobErrorMode.Continue;
            EnableTrace = true;
            JobName = "My Job";
            OpenOutputDocument = true;
            OpenOutputDocumentAllowed = true;
            OCREngineType = OcrEngineType.Advantage;
        }

        public DocumentConverterPreferences Clone()
        {
            var result = new DocumentConverterPreferences();
            result.InputDocumentFileName = InputDocumentFileName;
            result.InputDocumentPageCount = InputDocumentPageCount;
            result.InputFirstPage = InputFirstPage;
            result.InputLastPage = InputLastPage;
            result.InputMaximumPages = InputMaximumPages;
            result.InputAnnotationsFileName = InputAnnotationsFileName;
            result.LoadEmbeddedAnnotation = LoadEmbeddedAnnotation;
            result.DocumentFormat = DocumentFormat;
            result.RasterImageFormat = RasterImageFormat;
            result.RasterImageBitsPerPixel = RasterImageBitsPerPixel;
            result.OutputDocumentFileName = OutputDocumentFileName;
            result.OutputAnnotationsMode = OutputAnnotationsMode;
            result.OutputAnnotationsFileName = OutputAnnotationsFileName;
            result.PageNumberingTemplate = PageNumberingTemplate;
            result.EnableSvgConversion = EnableSvgConversion;
            result.SvgImagesRecognitionMode = SvgImagesRecognitionMode;
            result.EmptyPageMode = EmptyPageMode;
            result.UseThreads = UseThreads;
            result.PreprocessingDeskew = PreprocessingDeskew;
            result.PreprocessingInvert = PreprocessingInvert;
            result.PreprocessingOrient = PreprocessingOrient;
            result.ErrorMode = ErrorMode;
            result.EnableTrace = EnableTrace;
            result.JobName = JobName;
            result.OpenOutputDocument = OpenOutputDocument;
            result.OpenOutputDocumentAllowed = OpenOutputDocumentAllowed;
            result.OCREngineType = OCREngineType;
            result.OCREngineRuntimePath = OCREngineRuntimePath;
            result.RasterCodecsOptionsPath = RasterCodecsOptionsPath;
            result.DocumentWriterOptionsPath = DocumentWriterOptionsPath;
            result.CacheDirectory = CacheDirectory;
            result.CacheDataSerializationMode = CacheDataSerializationMode;
            result.CachePolicySerializationMode = CachePolicySerializationMode;

            result.DocumentId = DocumentId;
            result.OutputFiles = OutputFiles;
            result.PurgeOutputFilesOnError = PurgeOutputFilesOnError;

            // Be careful, we are using the same objects here
            result.RasterCodecsInstance = RasterCodecsInstance;
            result.OcrEngineInstance = OcrEngineInstance;
            result.DocumentWriterInstance = DocumentWriterInstance;
            result.IsSilentMode = IsSilentMode;
            result.ErrorMessage = ErrorMessage;
            result.AnnRenderingEngine = AnnRenderingEngine;

            return result;
        }

        // The input document file name
        public string InputDocumentFileName
        {
            get
            {
                return m_InputDocumentFileName;
            }
            set
            {
                m_InputDocumentFileName = value;
            }
        }
        private string m_InputDocumentFileName;

        // Used by the UI. If it has 0 means we dont know
        [XmlIgnore()]
        public int InputDocumentPageCount
        {
            get
            {
                return m_InputDocumentPageCount;
            }
            set
            {
                m_InputDocumentPageCount = value;
            }
        }
        private int m_InputDocumentPageCount;

        // Used by the UI. If it has 1 means first page
        public int InputFirstPage
        {
            get
            {
                return m_InputFirstPage;
            }
            set
            {
                m_InputFirstPage = value;
            }
        }
        private int m_InputFirstPage;

        // Used by the UI. If it has -1 means last page
        public int InputLastPage
        {
            get
            {
                return m_InputLastPage;
            }
            set
            {
                m_InputLastPage = value;
            }
        }
        private int m_InputLastPage;

        // Maximum number of pages to convert, less than 1 means no limit. Works with a Document input only
        public int InputMaximumPages
        {
            get
            {
                return m_InputMaximumPages;
            }
            set
            {
                m_InputMaximumPages = value;
            }
        }
        private int m_InputMaximumPages;

        // The input annotation file
        public string InputAnnotationsFileName
        {
            get
            {
                return m_InputAnnotationsFileName;
            }
            set
            {
                m_InputAnnotationsFileName = value;
            }
        }
        private string m_InputAnnotationsFileName;

        // Or if the annotations will be loaded from the input document file directly
        public bool LoadEmbeddedAnnotation
        {
            get
            {
                return m_LoadEmbeddedAnnotation;
            }
            set
            {
                m_LoadEmbeddedAnnotation = value;
            }
        }
        private bool m_LoadEmbeddedAnnotation;

        // Document format to use
        public DocumentFormat DocumentFormat
        {
            get
            {
                return m_DocumentFormat;
            }
            set
            {
                m_DocumentFormat = value;
            }
        }
        private DocumentFormat m_DocumentFormat;
        // Or RasterImage format
        public RasterImageFormat RasterImageFormat
        {
            get
            {
                return m_RasterImageFormat;
            }
            set
            {
                m_RasterImageFormat = value;
            }
        }
        private RasterImageFormat m_RasterImageFormat;
        // Options to use when saving as Raster
        public int RasterImageBitsPerPixel
        {
            get
            {
                return m_RasterImageBitsPerPixel;
            }
            set
            {
                m_RasterImageBitsPerPixel = value;
            }
        }
        private int m_RasterImageBitsPerPixel;

        // The output document file name
        public string OutputDocumentFileName
        {
            get
            {
                return m_OutputDocumentFileName;
            }
            set
            {
                m_OutputDocumentFileName = value;
            }
        }
        private string m_OutputDocumentFileName;

        // Output annotation mode and file name
        public DocumentConverterAnnotationsMode OutputAnnotationsMode
        {
            get
            {
                return m_OutputAnnotationsMode;
            }
            set
            {
                m_OutputAnnotationsMode = value;
            }
        }
        private DocumentConverterAnnotationsMode m_OutputAnnotationsMode;
        public string OutputAnnotationsFileName
        {
            get
            {
                return m_OutputAnnotationsFileName;
            }
            set
            {
                m_OutputAnnotationsFileName = value;
            }
        }
        private string m_OutputAnnotationsFileName;

        // Page Numbering Template
        public string PageNumberingTemplate
        {
            get
            {
                return m_PageNumberingTemplate;
            }
            set
            {
                m_PageNumberingTemplate = value;
            }
        }
        private string m_PageNumberingTemplate;

        // SVG conversion
        public bool EnableSvgConversion
        {
            get
            {
                return m_EnableSvgConversion;
            }
            set
            {
                m_EnableSvgConversion = value;
            }
        }
        private bool m_EnableSvgConversion;

        // If to recognize embedded images in SVG images
        public DocumentConverterSvgImagesRecognitionMode SvgImagesRecognitionMode
        {
            get
            {
                return m_SvgImagesRecognitionMode;
            }
            set
            {
                m_SvgImagesRecognitionMode = value;
            }
        }
        private DocumentConverterSvgImagesRecognitionMode m_SvgImagesRecognitionMode;

        // Empty page mode
        public DocumentConverterEmptyPageMode EmptyPageMode
        {
            get
            {
                return m_EmptyPageMode;
            }
            set
            {
                m_EmptyPageMode = value;
            }
        }
        private DocumentConverterEmptyPageMode m_EmptyPageMode;

        // Use threads
        public bool UseThreads
        {
            get
            {
                return m_UseThreads;
            }
            set
            {
                m_UseThreads = value;
            }
        }
        private bool m_UseThreads;

        // Preprocessing
        public bool PreprocessingDeskew
        {
            get
            {
                return m_PreprocessingDeskew;
            }
            set
            {
                m_PreprocessingDeskew = value;
            }
        }
        private bool m_PreprocessingDeskew;
        public bool PreprocessingInvert
        {
            get
            {
                return m_PreprocessingInvert;
            }
            set
            {
                m_PreprocessingInvert = value;
            }
        }
        private bool m_PreprocessingInvert;
        public bool PreprocessingOrient
        {
            get
            {
                return m_PreprocessingOrient;
            }
            set
            {
                m_PreprocessingOrient = value;
            }
        }
        private bool m_PreprocessingOrient;

        // Misc.
        public DocumentConverterJobErrorMode ErrorMode
        {
            get
            {
                return m_ErrorMode;
            }
            set
            {
                m_ErrorMode = value;
            }
        }
        private DocumentConverterJobErrorMode m_ErrorMode;
        public bool EnableTrace
        {
            get
            {
                return m_EnableTrace;
            }
            set
            {
                m_EnableTrace = value;
            }
        }
        private bool m_EnableTrace;
        public string JobName
        {
            get
            {
                return m_JobName;
            }
            set
            {
                m_JobName = value;
            }
        }
        private string m_JobName;
        public bool OpenOutputDocument
        {
            get
            {
                return m_OpenOutputDocument;
            }
            set
            {
                m_OpenOutputDocument = value;
            }
        }
        private bool m_OpenOutputDocument;
        [XmlIgnore()]
        public bool OpenOutputDocumentAllowed
        {
            get
            {
                return m_OpenOutputDocumentAllowed;
            }
            set
            {
                m_OpenOutputDocumentAllowed = value;
            }
        }
        private bool m_OpenOutputDocumentAllowed;

        // OCR
        public OcrEngineType OCREngineType
        {
            get
            {
                return m_OCREngineType;
            }
            set
            {
                m_OCREngineType = value;
            }
        }
        private OcrEngineType m_OCREngineType;
        public string OCREngineRuntimePath
        {
            get
            {
                return m_OCREngineRuntimePath;
            }
            set
            {
                m_OCREngineRuntimePath = value;
            }
        }
        private string m_OCREngineRuntimePath;

        // RasterCodecs
        public string RasterCodecsOptionsPath
        {
            get
            {
                return m_RasterCodecsOptionsPath;
            }
            set
            {
                m_RasterCodecsOptionsPath = value;
            }
        }
        private string m_RasterCodecsOptionsPath;

        // DocumentWriter options
        public string DocumentWriterOptionsPath
        {
            get
            {
                return m_DocumentWriterOptionsPath;
            }
            set
            {
                m_DocumentWriterOptionsPath = value;
            }
        }
        private string m_DocumentWriterOptionsPath;

        // Cache directory
        public string CacheDirectory
        {
            get
            {
                return m_CacheDirectory;
            }
            set
            {
                m_CacheDirectory = value;
            }
        }
        private string m_CacheDirectory;

        // Cache Data Serialization Mode
        public CacheSerializationMode CacheDataSerializationMode
        {
            get
            {
                return m_CacheDataSerializationMode;
            }
            set
            {
                m_CacheDataSerializationMode = value;
            }
        }
        private CacheSerializationMode m_CacheDataSerializationMode;

        // Cache Policy Serialization Mode
        public CacheSerializationMode CachePolicySerializationMode
        {
            get
            {
                return m_CachePolicySerializationMode;
            }
            set
            {
                m_CachePolicySerializationMode = value;
            }
        }
        private CacheSerializationMode m_CachePolicySerializationMode;

        // Input Document ID
        public string DocumentId
        {
            get
            {
                return m_DocumentId;
            }
            set
            {
                m_DocumentId = value;
            }
        }
        private string m_DocumentId;

        // Output files
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] OutputFiles
        {
            get
            {
                return m_OutputFiles;
            }
            set
            {
                m_OutputFiles = value;
            }
        }
        private string[] m_OutputFiles;

        // Delete the output files if an error occurs
        public bool PurgeOutputFilesOnError
        {
            get
            {
                return m_PurgeOutputFilesOnError;
            }
            set
            {
                m_PurgeOutputFilesOnError = value;
            }
        }
        private bool m_PurgeOutputFilesOnError;

        [XmlIgnore()]
        public RasterCodecs RasterCodecsInstance
        {
            get
            {
                return m_RasterCodecsInstance;
            }
            set
            {
                m_RasterCodecsInstance = value;
            }
        }
        private RasterCodecs m_RasterCodecsInstance;

        [XmlIgnore()]
        public IOcrEngine OcrEngineInstance
        {
            get
            {
                return m_OcrEngineInstance;
            }
            set
            {
                m_OcrEngineInstance = value;
            }
        }
        private IOcrEngine m_OcrEngineInstance;

        [XmlIgnore()]
        public DocumentWriter DocumentWriterInstance
        {
            get
            {
                return m_DocumentWriterInstance;
            }
            set
            {
                m_DocumentWriterInstance = value;
            }
        }
        private DocumentWriter m_DocumentWriterInstance;

        [XmlIgnore()]
        public AnnRenderingEngine AnnRenderingEngine
        {
            get
            {
                return m_AnnRenderingEngine;
            }
            set
            {
                m_AnnRenderingEngine = value;
            }
        }
        private AnnRenderingEngine m_AnnRenderingEngine;

        [XmlIgnore()]
        public bool IsSilentMode
        {
            get
            {
                return m_IsSilentMode;
            }
            set
            {
                m_IsSilentMode = value;
            }
        }
        private bool m_IsSilentMode;

        public string ErrorMessage
        {
            get
            {
                return m_ErrorMessage;
            }
            set
            {
                m_ErrorMessage = value;
            }
        }
        private string m_ErrorMessage;

        // The demo name
        public static string DemoName
        {
            get
            {
                return Demos.DocumentConverterPreferences.m_DemoName;
            }
            set
            {
                DocumentConverterPreferences.m_DemoName = value;
            }
        }
        private static string m_DemoName;

        public static string XmlFileName
        {
            get
            {
                return DocumentConverterPreferences.m_XmlFileName;
            }
            set
            {
                DocumentConverterPreferences.m_XmlFileName = value;
            }
        }
        private static string m_XmlFileName;

        private static string GetOutputFileName()
        {
            if (string.IsNullOrEmpty(DocumentConverterPreferences.XmlFileName))
            {
                throw new InvalidOperationException("Set XmlFileName before calling this method");
            }

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DocumentConverterPreferences.XmlFileName + Convert.ToString(".xml"));
        }

        private static XmlSerializer _serializer = new XmlSerializer(typeof(DocumentConverterPreferences));

        // Load the preferences from local application data, if not found or error, returns default preferences
        public static DocumentConverterPreferences Load()
        {
            try
            {
                string fileName = DocumentConverterPreferences.GetOutputFileName();
                if (File.Exists(fileName))
                {
                    return DocumentConverterPreferences.Load(fileName);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return new DocumentConverterPreferences();
        }

        public static DocumentConverterPreferences Load(string fileName)
        {
            using (var reader = new XmlTextReader(fileName))
            {
                return DocumentConverterPreferences._serializer.Deserialize(reader) as DocumentConverterPreferences;
            }
        }

        // Save the preferences to local application data
        public void Save()
        {
            try
            {
                string fileName = DocumentConverterPreferences.GetOutputFileName();
                Save(fileName);
            }
            catch
            {
            }
        }

        public void Save(string fileName)
        {
            using (var writer = new XmlTextWriter(fileName, Encoding.Unicode))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 2;
                DocumentConverterPreferences._serializer.Serialize(writer, this);
            }
        }

        // Run the conversion
        public bool Run(ObjectCache cache, Document document, DocumentConverter converter)
        {
            ErrorMessage = null;
            OutputFiles = null;

            // Show its info
            ShowInfo(document);

            bool disposeConverter = false;

            try
            {
                // If the user did not specify a document converter, create one
                if (converter is null)
                {
                    converter = new DocumentConverter();
                    disposeConverter = true;
                }

                // Set the options in the document converter from our data
                SetOptions(converter, cache, document);

                // Create the document converter job job
                var job = CreateConverterJob(converter, document);

                // Run it
                Trace.WriteLine("Running job...");
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                converter.Jobs.RunJob(job);
                stopwatch.Stop();

                // Show the results
                DocumentConverterPreferences.ShowResults(job);

                // Handle the output files (if any)
                HandleOutputFiles(job);

                Trace.WriteLine("----------------------------");
                Trace.WriteLine("Total conversion time: " + stopwatch.Elapsed.ToString());

                // See if we need to open the final document. LTD format has no viewer ...
                ViewOutputFile(job);

                // Check the errors
                if (job.Status == DocumentConverterJobStatus.Aborted)
                {
                    // Get the first error
                    if (job.Errors.Count > 0)
                    {
                        ErrorMessage = job.Errors[0].Error.Message;
                    }
                }

                return job.Status != DocumentConverterJobStatus.Aborted;
            }
            catch (OcrException ex)
            {
                ErrorMessage = ex.Message;
                Trace.WriteLine(string.Format("OCR error code: {0}" + Microsoft.VisualBasic.Constants.vbLf + "{1}", ex.Code, ex.Message));
            }
            catch (RasterException ex)
            {
                ErrorMessage = ex.Message;
                Trace.WriteLine(string.Format("LEADTOOLS error code: {0}" + Microsoft.VisualBasic.Constants.vbLf + "{1}", ex.Code, ex.Message));
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                Trace.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                // If we created the converter, dispose it
                if (converter is not null && disposeConverter)
                {
                    converter.Dispose();
                }
            }

            return false;
        }

        private void HandleOutputFiles(DocumentConverterJob job)
        {
            if (job.Status != DocumentConverterJobStatus.Aborted)
            {
                // Copy the output files to job.OutputFiles so the user can get them
                OutputFiles = new string[job.OutputFiles.Count];
                job.OutputFiles.CopyTo(OutputFiles, 0);

                // Show output files
                Trace.WriteLine("----------------------------");
                Trace.WriteLine("Output files:");
                foreach (string outputFile in job.OutputFiles)
                    Trace.WriteLine(outputFile);
            }
            // An error occured, see if we need to delete the output files
            else if (PurgeOutputFilesOnError)
            {
                DocumentConverterPreferences.DeleteAllFiles(job.OutputFiles);
            }
        }

        private static void ShowResults(DocumentConverterJob job)
        {
            // If we have errors, show them
            Trace.WriteLine("----------------------------------");
            Trace.WriteLine("Status: " + Convert.ToString((int)job.Status));

            if (job.Errors.Count > 0)
            {
                // We have errors, show them
                Trace.WriteLine("----------------------------------");
                Trace.WriteLine("Errors found:");
                foreach (DocumentConverterJobError error in job.Errors)
                {
                    if (error.InputDocumentPageNumber != 0)
                    {
                        Trace.WriteLine(string.Format("Operation: {0} - Page: {1} - Error: {2}", error.Operation, error.InputDocumentPageNumber, error.Error));
                    }
                    else
                    {
                        Trace.WriteLine(string.Format("Operation: {0} - Error: {1}", error.Operation, error.Error));
                    }
                }
            }
        }

        private DocumentConverterJob CreateConverterJob(DocumentConverter converter, Document document)
        {
            // Set the maximum page
            int firstPage = InputFirstPage;
            if (firstPage == 0)
            {
                firstPage = 1;
            }

            int lastPage = InputLastPage;
            if (lastPage == 0)
            {
                lastPage = -1;
            }

            if (document is not null && InputMaximumPages > 0)
            {
                if (lastPage == -1)
                {
                    lastPage = document.Pages.Count;
                }
                lastPage = Math.Min(lastPage, firstPage + InputMaximumPages - 1);
            }

            // Create a job
            var jobData = new DocumentConverterJobData()
            {
                InputDocumentFileName = document is null ? InputDocumentFileName : null,
                Document = document,
                InputDocumentFirstPageNumber = firstPage,
                InputDocumentLastPageNumber = lastPage,
                DocumentFormat = DocumentFormat,
                RasterImageFormat = RasterImageFormat,
                RasterImageBitsPerPixel = RasterImageBitsPerPixel,
                OutputDocumentFileName = OutputDocumentFileName,
                AnnotationsMode = OutputAnnotationsMode,
                OutputAnnotationsFileName = OutputAnnotationsFileName,
                JobName = JobName,
                UserData = null
            };

            if (document is not null)
            {
                jobData.InputAnnotationsFileName = InputAnnotationsFileName;
            }

            var job = converter.Jobs.CreateJob(jobData);
            return job;
        }

        private void SetOptions(DocumentConverter converter, ObjectCache cache, Document document)
        {
            converter.SetAnnRenderingEngineInstance(AnnRenderingEngine);

            // Set the RasterCodecs instance, should go into the DocumentFactory class which will be used to load the document
            if (RasterCodecsInstance is not null)
            {
                DocumentFactory.RasterCodecsTemplate = RasterCodecsInstance;
            }

            // Set the OCR engine
            if (OcrEngineInstance is not null && OcrEngineInstance.IsStarted)
            {
                converter.SetOcrEngineInstance(OcrEngineInstance, false);
            }

            if (DocumentWriterInstance is not null)
            {
                converter.SetDocumentWriterInstance(DocumentWriterInstance);
            }

            // Set pre-processing options
            converter.Preprocessor.Deskew = PreprocessingDeskew;
            converter.Preprocessor.Invert = PreprocessingInvert;
            converter.Preprocessor.Orient = PreprocessingOrient;

            // Enable trace
            converter.Diagnostics.EnableTrace = EnableTrace;

            // Setup the load document options
            var loadDocumentOptions = new LoadDocumentOptions();
            // Setup cachce
            loadDocumentOptions.Cache = cache;
            loadDocumentOptions.UseCache = cache is not null;

            if (document is null)
            {
                // Set the input annotation mode or file name
                loadDocumentOptions.LoadEmbeddedAnnotations = LoadEmbeddedAnnotation;
                if (!LoadEmbeddedAnnotation && !string.IsNullOrEmpty(InputAnnotationsFileName) && File.Exists(InputAnnotationsFileName))
                {
                    // We will use this instead of DocumentConverterJobData.InputAnnotationsFileName (this will override it anyway if we give the
                    // document converter a loadDocumentOptions)
                    loadDocumentOptions.AnnotationsUri = new Uri(InputAnnotationsFileName);
                }
            }

            converter.LoadDocumentOptions = loadDocumentOptions;

            // Set options
            converter.Options.JobErrorMode = ErrorMode;
            if (!string.IsNullOrEmpty(PageNumberingTemplate))
            {
                converter.Options.PageNumberingTemplate = PageNumberingTemplate;
            }
            converter.Options.EnableSvgConversion = EnableSvgConversion;
            converter.Options.SvgImagesRecognitionMode = SvgImagesRecognitionMode;
            converter.Options.EmptyPageMode = EmptyPageMode;
            converter.Options.UseThreads = UseThreads;
            converter.Diagnostics.EnableTrace = EnableTrace;
        }

        private void ShowInfo(Document document)
        {
            Console.WriteLine("-----------------------");

            if (document is null)
            {
                Console.WriteLine(Convert.ToString("  InputDocumentFileName: ") + InputDocumentFileName);
            }
            else
            {
                Console.WriteLine("  InputDocument: " + document.DocumentId);
                string documentPath;
                var documentUri = document.Uri;
                if (documentUri.IsFile)
                {
                    documentPath = documentUri.LocalPath;
                }
                else
                {
                    documentPath = documentUri.ToString();
                }
                Console.WriteLine(Convert.ToString("  InputDocumentPath: ") + documentPath);
            }

            Console.WriteLine("  InputDocumentPageCount: " + Convert.ToString(InputDocumentPageCount));
            Console.WriteLine("  InputFirstPage: " + Convert.ToString(InputFirstPage));
            Console.WriteLine("  InputLastPage: " + Convert.ToString(InputLastPage));

            if (document is null)
            {
                Console.WriteLine(Convert.ToString("  InputAnnotationsFileName: ") + InputAnnotationsFileName);
                Console.WriteLine("  LoadEmbeddedAnnotation: " + Convert.ToString(LoadEmbeddedAnnotation));
            }

            Console.WriteLine("  DocumentFormat: " + Convert.ToString((int)DocumentFormat));
            Console.WriteLine("  RasterImageFormat: " + Convert.ToString((int)RasterImageFormat));
            Console.WriteLine("  RasterImageBitsPerPixel: " + Convert.ToString(RasterImageBitsPerPixel));
            Console.WriteLine(Convert.ToString("  OutputDocumentFileName: ") + Convert.ToString(OutputDocumentFileName));
            Console.WriteLine(Convert.ToString("  OutputAnnotationsFileName: ") + Convert.ToString(OutputAnnotationsFileName));
            Console.WriteLine("  OutputAnnotationsMode: " + Convert.ToString((int)OutputAnnotationsMode));
            Console.WriteLine("  EnableSvgConversion: " + Convert.ToString(EnableSvgConversion));
            Console.WriteLine("  SvgImagesRecognitionMode: " + Convert.ToString((int)SvgImagesRecognitionMode));
            Console.WriteLine("  EmptyPageMode: " + Convert.ToString((int)EmptyPageMode));
            Console.WriteLine("  UseThreads: " + Convert.ToString(UseThreads));
            Console.WriteLine("  PreprocessingDeskew: " + Convert.ToString(PreprocessingDeskew));
            Console.WriteLine("  PreprocessingInvert: " + Convert.ToString(PreprocessingInvert));
            Console.WriteLine("  PreprocessingOrient: " + Convert.ToString(PreprocessingOrient));
            Console.WriteLine("  ErrorMode: " + Convert.ToString((int)ErrorMode));
            Console.WriteLine("  EnableTrace: " + Convert.ToString(EnableTrace));
            Console.WriteLine(Convert.ToString("  JobName: ") + JobName);
            Console.WriteLine("  OpenOutputDocument: " + Convert.ToString(OpenOutputDocument));
        }

        private void ViewOutputFile(DocumentConverterJob job)
        {
            if (job.Status == DocumentConverterJobStatus.Aborted || !OpenOutputDocument || job.JobData.DocumentFormat == DocumentFormat.Ltd)
            {
                return;
            }

            try
            {
                if (File.Exists(job.JobData.OutputDocumentFileName))
                {
                    Process.Start(job.JobData.OutputDocumentFileName);
                }
                else
                {
                    // Might be multiple files, try the directory
                    string outputDocumentDir = Path.GetDirectoryName(job.JobData.OutputDocumentFileName);
                    if (Directory.Exists(outputDocumentDir))
                    {
                        Process.Start(outputDocumentDir);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                throw;
            }
        }

        private static void DeleteAllFiles(IList<string> files)
        {
            if (files is null)
            {
                return;
            }

            foreach (string file__1 in files)
            {
                if (File.Exists(file__1))
                {
                    try
                    {
                        File.Delete(file__1);
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}