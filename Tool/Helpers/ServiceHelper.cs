using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using Leadtools;
using Leadtools.Caching;
using Leadtools.Codecs;
using Leadtools.Documents;
using Leadtools.Forms.Ocr;
using Microsoft.Extensions.Hosting.Internal;

namespace TABFusionRMS.Web.Tool.Helpers
{
    public enum ServiceVendor
    {
        Dropbox = 0,
        GoogleDrive = 1,
        OneDrive = 2,
        SharePoint = 3
    }
    public enum OcrEngineStatus
    {
        /// <summary>
        ///   The OCR Engine was not set, and thus is not being used.
        /// </summary>
        Unset = 0,

        /// <summary>
        ///   An error occurred with the setup.
        /// </summary>
        Error = 1,

        /// <summary>
        ///   The OCR Engine should be working normally.
        /// </summary>
        Ready = 2
    }
    internal sealed partial class ServiceHelper
    {
        private ServiceHelper()
        {
        }
        public const string Key_CORS_Origins = "CORS.Origins";
        public const string Key_CORS_Headers = "CORS.Headers";
        public const string Key_CORS_Methods = "CORS.Methods";

        public const string Key_Access_Passcode = "Access.Passcode";

        public const string Key_License_FilePath = "lt.License.FilePath";
        public const string Key_License_DeveloperKey = "lt.License.DeveloperKey";

        public const string Key_Cache_Directory = "lt.Cache.Directory";
        public const string Key_Cache_ProxyHeader = "lt.Cache.ProxyHeader";
        public const string Key_Cache_AccessTimeout = "lt.Cache.AccessTimeout";
        public const string Key_Cache_SlidingExpiration = "lt.Cache.SlidingExpiration";

        public const string Key_PreCache_DictionaryXml = "lt.PreCache.DictionaryXml";
        public const string Default_PreCache_DictionaryXml = @".\App_Data\PreCacheDictionary.xml";

        public const string Key_PreCache_Directory = "lt.PreCache.Directory";
        public const string Default_PreCache_Directory = @".\precache";

        public const string Key_RasterCodecs_OptionsFilePath = "lt.RasterCodecs.OptionsFilePath";

        public const string Key_Barcodes_Reader_OptionsFilePath = "lt.Barcodes.Reader.OptionsFilePath";

        public const string Key_Ocr_EngineType = "lt.Ocr.EngineType";
        public const string Key_Ocr_RuntimeDirectory = "lt.Ocr.RuntimeDirectory";

        public const string Key_DocumentConverter_ExePath = "lt.DocumentConverter.ExePath";
        public const string Key_DocumentConverter_MaximumPages = "lt.DocumentConverter.MaximumPages";

        public const string Key_Svg_GZip = "lt.Svg.GZip";

        public static string CORSOrigins
        {
            get
            {
                return m_CORSOrigins;
            }
            private set
            {
                m_CORSOrigins = value;
            }
        }
        private static string m_CORSOrigins;
        public static string CORSHeaders
        {
            get
            {
                return m_CORSHeaders;
            }
            private set
            {
                m_CORSHeaders = value;
            }
        }
        private static string m_CORSHeaders;
        public static string CORSMethods
        {
            get
            {
                return m_CORSMethods;
            }
            private set
            {
                m_CORSMethods = value;
            }
        }
        private static string m_CORSMethods;
        static ServiceHelper()
        {
            // Try to set our CORS options from local.config.
            // If nothing's there, use "*" (all) for each of them
            CORSOrigins = GetSettingValue(Key_CORS_Origins);
            if (string.IsNullOrWhiteSpace(CORSOrigins))
            {
                CORSOrigins = "*";
            }
            CORSHeaders = GetSettingValue(Key_CORS_Headers);
            if (string.IsNullOrWhiteSpace(CORSHeaders))
            {
                CORSHeaders = "*";
            }
            CORSMethods = GetSettingValue(Key_CORS_Methods);
            if (string.IsNullOrWhiteSpace(CORSMethods))
            {
                CORSMethods = "*";
            }
        }

        public static string GetSettingValue(string key)
        {
            HttpContext currentContext = new HttpContextAccessor().HttpContext;
            string value = null;
            if (currentContext is not null)
            {
                IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

                if (configuration[key] is not null)
                {
                    value = configuration[key];
                }
            }
            if (value is null)
            {
                // Load it from the config file
               Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(typeof(ServiceHelper).Assembly.Location);
                if (config is not null)
                {
                    // Get the appSettings section
                    //AppSettingsSection appSettings = (AppSettingsSection)config.GetSection("appSettings");
                    IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                    if (configuration is not null)
                    {
                        if (configuration[key] is not null)
                        {
                            value = configuration[key].Text();
                        }
                        else
                        {
                            value = string.Empty;
                        }
                    }
                }
            }

            return value;
        }

        public static bool IsLicenseChecked
        {
            get
            {
                return m_IsLicenseChecked;
            }
            set
            {
                m_IsLicenseChecked = value;
            }
        }
        private static bool m_IsLicenseChecked;

        public static bool IsKernelExpired
        {
            get
            {
                return m_IsKernelExpired;
            }
            set
            {
                m_IsKernelExpired = value;
            }
        }
        private static bool m_IsKernelExpired;

        public static void InitializeService()
        {
            // This method is called by Application_Start of the web service
            // * We will initialize the global and static objects used through out the demos and
            // * Each controller will be able to use these same objects.
            // * Controller-specific initialization is performed in InitializeController
            // 


            // Set the license, initialize the cache and various objects

            // For the license, the TestController.Ping method is used to check teh status of this
            // So save the values here to get them later
            try
            {
                SetLicense();
                IsLicenseChecked = true;
                IsKernelExpired = RasterSupport.KernelExpired;
            }
            catch
            {
                IsLicenseChecked = false;
                IsKernelExpired = true;
            }

            if (!IsKernelExpired)
            {
                // The license is OK, continue
                try
                {
                    CreateCache();
                    PreCacheHelper.CreatePreCache();
                    ServiceHelper.SetRasterCodecsOptions(DocumentFactory.RasterCodecsTemplate, 0);
                }
                // Let this pass, it is checked again in TestController.Ping
                catch
                {
                }

                CreateOCREngine();
            }
        }

        public static void CleanupService()
        {
            // This method is called by Application_End of the web service
            // * We will clean up and destroy the global static objects created in
            // * InitializeService
            // 

            if (_ocrEngine is not null)
            {
                _ocrEngine.Dispose();
                _ocrEngine = null;
            }
        }

        public static void InitializeController()
        {
            // This method is called by the constructor of each controller
            // * It is assumed that InitializeService has been called before by
            // * Application_Start of the web service
            // * 
            // * Do any per-request setup here
            // 

        }

        public static void SetLicense()
        {
            // 
            // * Set the license and license key here.
            // * While this is called with each call to the service,
            // * the lines below with RasterSupport.KernelExpired
            // * will exit early to avoid checking repeatedly.
            // 


            if (!RasterSupport.KernelExpired)
            {
                return;
            }

            // file path may be relative or absolute
            // dev key may be relative, absolute, or the full text
            string licensePath = null;
            string devKey = null;

            // First check the config file (appSettings)
            string licSetting = GetSettingValue(Key_License_FilePath);
            if (!string.IsNullOrEmpty(licSetting))
            {
                // If settings are wrong from here on out, we need to throw an exception
                licSetting = GetAbsolutePath(licSetting);
                if (File.Exists(licSetting))
                {
                    licensePath = licSetting;

                    // devKey can be relative, absolute, or file contents
                    string devKeySetting = GetSettingValue(Key_License_DeveloperKey);
                    if (devKeySetting is not null)
                    {
                        devKeySetting = devKeySetting.Trim();
                    }

                    if (devKeySetting is not null && IsAbsolutePath(devKeySetting) && File.Exists(devKeySetting))
                    {
                        string devKeyFile = devKeySetting;
                        devKey = File.ReadAllText(devKeyFile);
                    }
                    else if (devKeySetting is not null)
                    {
                        // Coule be a relative path or a developer key, see if the file exist
                        string devKeyFile = GetAbsolutePath(devKeySetting);
                        if (File.Exists(devKeyFile))
                        {
                            devKey = File.ReadAllText(devKeyFile);
                        }
                        else
                        {
                            devKey = devKeySetting;
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Developer key in configuration was invalid.");
                    }
                }
                else
                {
                    throw new InvalidOperationException("License file path in configuration does not exist.");
                }
            }
            else
            {
                // Was not found there, check the bin folder
                HttpContext currentContext = new HttpContextAccessor().HttpContext;
                if (currentContext is not null)
                {
                    //string licBinPath = ContextService.httpContext.Server.MapPath("~/bin/LEADTOOLS.LIC");

                    //check raju leadtool path
                    string licBinPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath") + string.Format("bin/LEADTOOLS.LIC");

                    if (File.Exists(licBinPath))
                    {
                        licensePath = licBinPath;
                        // get value for devKey, process to get file contents
                        //string devKeyBinPath = currentContext.Server.MapPath("~/bin/LEADTOOLS.LIC.key");

                        //check raju leadtool path
                        string devKeyBinPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath") + string.Format("bin/LEADTOOLS.LIC.key"); 
                        if (File.Exists(devKeyBinPath))
                        {
                            devKey = File.ReadAllText(devKeyBinPath);
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(licensePath) && !string.IsNullOrEmpty(devKey))
            {
                RasterSupport.SetLicense(licensePath, devKey);
            }
            else
            {
                // This will work for development only
                RasterSupport.SetLicense("", "Nag");
            }
        }

        public static void CheckCacheAccess()
        {
            // Check if the cache directory setup by the user in the config file is valid and accessible

            // Do this by loading up the cache, adding a region, an item and deleting it
            // This mimics what the documents library will do
            if (_cache is null)
            {
                throw new InvalidOperationException("Cache has not been setup");
            }

            // If this is the default FileCache then try to add/remove an item
            // This check can be performed with all caches but is extra important with
            // FileCache since forgetting to setup the correct read/write access
            // on the cache directory is a common issue when setting up the service
            if (_cache is FileCache)
            {
                string regionName = Guid.NewGuid().ToString("N");
                var policy = CreatePolicy();
                string key = "key";
                _cache.Add(key, "data", policy, regionName);
                // Verify
                string data = _cache.Get(key, regionName) as string;
                if (data is null || string.CompareOrdinal(data, "data") != 0)
                {
                    throw new InvalidOperationException("Could not read cache item");
                }

                // Delete
                _cache.DeleteItem(key, regionName);
            }
        }

        public static string GetAbsolutePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath) || relativePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                // Not a legal path
                return relativePath;
            }

            relativePath = relativePath.Trim();
            if (!Path.IsPathRooted(relativePath))
            {
                relativePath = Path.Combine((string)AppDomain.CurrentDomain.GetData("ContentRootPath"), relativePath);
            }
            return relativePath;
        }

        public static bool IsAbsolutePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath) || relativePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                // Not a legal path
                return false;
            }

            return Path.IsPathRooted(relativePath.Trim());
        }

        public static void CreateCache()
        {
            // Called by InitializeService the first time the service is run
            // Initialize the global Cache object

            // Get the directory to use with FileCache
            string cacheDirectory = GetSettingValue(Key_Cache_Directory);
            cacheDirectory = GetAbsolutePath(cacheDirectory);
            if (string.IsNullOrEmpty(cacheDirectory))
            {
                throw new InvalidOperationException("Set the cache path in '" + Key_Cache_Directory + "' in the configuration file");
            }

            string value = GetSettingValue(Key_Cache_AccessTimeout);
            if (value is not null)
            {
                value = value.Trim();
            }
            var accessTimeout = TimeSpan.Zero;
            int ms = 0;
            if (!string.IsNullOrEmpty(value) && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ms))
            {
                accessTimeout = TimeSpan.FromMilliseconds(ms);
            }

            // Now create FileCache
            var fileCache = new FileCache();
            fileCache.CacheDirectory = cacheDirectory;

            // We cannot setup the cache virtual directory at this point
            // since we need to use the current HttpContext.Request which does not exist yet

            // Choose how we want to serialize the data. We choose JSON for human-readability.
            fileCache.DataSerializationMode = CacheSerializationMode.Json;
            fileCache.PolicySerializationMode = CacheSerializationMode.Json;

            if (accessTimeout.TotalSeconds > 0d)
            {
                fileCache.AccessTimeout = accessTimeout;
            }

            _cache = fileCache;
        }

        public static void UpdateCacheSettings(HttpResponseMessage response)
        {
            TimeSpan slidingExpiration;
            string value = GetSettingValue(Key_Cache_SlidingExpiration);
            if (value is not null)
            {
                value = value.Trim();
            }
            if (string.IsNullOrEmpty(value) || !TimeSpan.TryParse(value, out slidingExpiration))
            {
                slidingExpiration = TimeSpan.FromMinutes(60d);
            }
            response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                Public = true,
                MaxAge = slidingExpiration
            };
        }

        public static CacheItemPolicy CreatePolicy()
        {
            var policy = new CacheItemPolicy();

            TimeSpan slidingExpiration;
            string value = GetSettingValue(Key_Cache_SlidingExpiration);
            if (value is not null)
            {
                value = value.Trim();
            }
            if (!string.IsNullOrEmpty(value) && TimeSpan.TryParse(value, out slidingExpiration))
            {
                policy.SlidingExpiration = slidingExpiration;
            }

            return policy;
        }

        public static CacheItemPolicy CreateForeverPolicy()
        {
            // Creates a 3-year policy (for pre-cached items)
            var policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTime.MaxValue;
            return policy;
        }

        // LEADTOOLS Documents library uses 300 as the default DPI, so we use the same
        public const int DefaultResolution = 300;

        public static void SetRasterCodecsOptions(RasterCodecs rasterCodecs, int resolution)
        {
            // Set up any extra options to use here
            if (resolution == 0)
            {
                resolution = DefaultResolution;
            }

            // Set the load resolution
            rasterCodecs.Options.Wmf.Load.XResolution = resolution;
            rasterCodecs.Options.Wmf.Load.YResolution = resolution;
            rasterCodecs.Options.RasterizeDocument.Load.XResolution = resolution;
            rasterCodecs.Options.RasterizeDocument.Load.YResolution = resolution;

            // See if we have an options file in the config
            string value = GetSettingValue(Key_RasterCodecs_OptionsFilePath);
            value = GetAbsolutePath(value);
            if (!string.IsNullOrEmpty(value))
            {
                rasterCodecs.LoadOptions(value);
            }

            // In Web API, resources are pulled from a Temp folder.
            // * So rastercodecs needs to be given an initial path that corresponds
            // * to the /bin folder.
            // * There is an after-build target that copies these files from the proper /bin19 folder.
            // 

            string binPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath);
            if (Directory.Exists(binPath) && File.Exists(Path.Combine(binPath, "Leadtools.PdfEngine.dll")))
            {
                rasterCodecs.Options.Pdf.InitialPath = binPath;
            }
        }

        // Public Shared Function SetBarcodeReadOptions(reader As BarcodeReader) As Boolean
        // ' See if we have an options file in the config
        // Dim value As String = GetSettingValue(Key_Barcodes_Reader_OptionsFilePath)
        // value = GetAbsolutePath(value)
        // If String.IsNullOrEmpty(value) Then
        // ' Return false to indicate that the user did not set any barcode options.
        // ' We will try different options ourselves.
        // Return False
        // End If

        // reader.LoadOptions(value)
        // Return True
        // End Function

        // Public Shared Sub InitBarcodeReader(reader As BarcodeReader, doublePass As Boolean)
        // ' Default options to read most barcodes
        // reader.ImageType = BarcodeImageType.Unknown

        // ' Both directions for 1D
        // Dim oneDOptions As OneDBarcodeReadOptions = TryCast(reader.GetDefaultOptions(BarcodeSymbology.UPCA), OneDBarcodeReadOptions)
        // oneDOptions.SearchDirection = BarcodeSearchDirection.HorizontalAndVertical

        // Dim gs1Options As GS1DatabarStackedBarcodeReadOptions = TryCast(reader.GetDefaultOptions(BarcodeSymbology.GS1DatabarStacked), GS1DatabarStackedBarcodeReadOptions)
        // gs1Options.SearchDirection = BarcodeSearchDirection.HorizontalAndVertical

        // Dim fourStateOptions As FourStateBarcodeReadOptions = TryCast(reader.GetDefaultOptions(BarcodeSymbology.USPS4State), FourStateBarcodeReadOptions)
        // fourStateOptions.SearchDirection = BarcodeSearchDirection.HorizontalAndVertical

        // Dim patchCodeOptions As PatchCodeBarcodeReadOptions = TryCast(reader.GetDefaultOptions(BarcodeSymbology.PatchCode), PatchCodeBarcodeReadOptions)
        // patchCodeOptions.SearchDirection = BarcodeSearchDirection.HorizontalAndVertical

        // Dim postNetOptions As PostNetPlanetBarcodeReadOptions = TryCast(reader.GetDefaultOptions(BarcodeSymbology.PostNet), PostNetPlanetBarcodeReadOptions)
        // postNetOptions.SearchDirection = BarcodeSearchDirection.HorizontalAndVertical

        // Dim pharmaCodeOptions As PharmaCodeBarcodeReadOptions = TryCast(reader.GetDefaultOptions(BarcodeSymbology.PharmaCode), PharmaCodeBarcodeReadOptions)
        // pharmaCodeOptions.SearchDirection = BarcodeSearchDirection.HorizontalAndVertical

        // ' Double pass
        // oneDOptions.EnableDoublePass = doublePass

        // Dim dataMatrixOptions As DatamatrixBarcodeReadOptions = TryCast(reader.GetDefaultOptions(BarcodeSymbology.Datamatrix), DatamatrixBarcodeReadOptions)
        // dataMatrixOptions.EnableDoublePass = doublePass

        // Dim pdf417Options As PDF417BarcodeReadOptions = TryCast(reader.GetDefaultOptions(BarcodeSymbology.PDF417), PDF417BarcodeReadOptions)
        // pdf417Options.EnableDoublePass = doublePass

        // Dim microPdf4127Options As MicroPDF417BarcodeReadOptions = TryCast(reader.GetDefaultOptions(BarcodeSymbology.MicroPDF417), MicroPDF417BarcodeReadOptions)
        // microPdf4127Options.EnableDoublePass = doublePass

        // Dim qrOptions As QRBarcodeReadOptions = TryCast(reader.GetDefaultOptions(BarcodeSymbology.QR), QRBarcodeReadOptions)
        // qrOptions.EnableDoublePass = doublePass

        // reader.ImageType = BarcodeImageType.Unknown
        // End Sub

        public static void SafeDeleteFile(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                try
                {
                    File.Delete(fileName);
                }
                catch
                {
                }
            }
        }

        // Global Cache object used by all controllers.
        // This object is created during service initialization
        private static ObjectCache _cache = default;
        public static ObjectCache Cache
        {
            get
            {
                return _cache;
            }
        }

        private static OcrEngineStatus _OcrEngineStatus = OcrEngineStatus.Unset;
        public static OcrEngineStatus OcrEngineStatus
        {
            get
            {
                return _OcrEngineStatus;
            }
        }

        // Global IOcrEngine instance used by all the controllers
        // This object is created during service initialization
        private static IOcrEngine _ocrEngine;
        public static IOcrEngine GetOCREngine()
        {
            return _ocrEngine;
        }

        public static string CheckOCRRuntimeDirectory()
        {
            // Check if we are running on a machine that has LEADTOOLS installed, try to get the path automatically
            //string appPath = HostingEnvironment.ApplicationPhysicalPath;

            //chck raju
            string appPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath");
            var ocrRuntimeDirs = new[] { @"..\..\..\..\..\..\Bin\Common\OCRAdvantageRuntime" };

            // Leave this as a loop in the event that we decide to add more options later.
            foreach (string runtimeDir in ocrRuntimeDirs)
            {
                string dir = Path.GetFullPath(Path.Combine(appPath, runtimeDir));
                if (Directory.Exists(dir) && Directory.EnumerateFileSystemEntries(dir).Any())
                {
                    return dir;
                }
            }

            // Not found
            return null;
        }

        // changes for OCR Moti mashiah
        public static void InitCodecs(RasterCodecs codecs, int resolution)
        {
            if (resolution == 0)
                resolution = DefaultResolution;
            codecs.Options.Wmf.Load.XResolution = resolution;
            codecs.Options.Wmf.Load.YResolution = resolution;
            codecs.Options.RasterizeDocument.Load.XResolution = resolution;
            codecs.Options.RasterizeDocument.Load.YResolution = resolution;
            string rasterCodecsOptionsFilePath = GetSettingValue(Key_RasterCodecs_OptionsFilePath);
            if (!string.IsNullOrEmpty(rasterCodecsOptionsFilePath) && File.Exists(rasterCodecsOptionsFilePath))
                codecs.LoadOptions(rasterCodecsOptionsFilePath);
        }

        public static IOcrEngine CreateOCREngine(RasterCodecs codecs)
        {
            // Dim runtimeDirectory As String = CheckOCRRuntimeDirectory()
            string runtimeDirectory = GetSettingValue(Key_Ocr_RuntimeDirectory);
            if (string.IsNullOrEmpty(runtimeDirectory))
                throw new ArgumentException("OCR Engine directory not set");
            return CreateOCREngine(runtimeDirectory, codecs);
        }

        private static IOcrEngine CreateOCREngine(string runtimeDirectory, RasterCodecs codecs)
        {
            IOcrEngine ocrEngine = OcrEngineManager.CreateEngine(OcrEngineType.Advantage, false);

            try
            {
                // Dim pathDir = "C:\inetpub\wwwroot\TAB\Web Access\OcrAdvantageRuntime"
                ocrEngine.Startup(codecs, default, default, runtimeDirectory);
                return ocrEngine;
            }
            catch
            {
                if (ocrEngine is not null)
                    ocrEngine.Dispose();
                Trace.WriteLine("The OCR Engine could not be started. This application will continue to run, but without OCR functionality.");
                throw;
            }
        }
        // end method added for Ocr

        public static void CreateOCREngine()
        {
            if (_ocrEngine is not null)
            {
                _ocrEngine.Dispose();
            }

            // Reset the OCR Engine Status
            _OcrEngineStatus = OcrEngineStatus.Unset;

            string engineTypeString = GetSettingValue(Key_Ocr_EngineType);
            if (string.IsNullOrEmpty(engineTypeString))
            {
                return;
            }

            OcrEngineType engineType = OcrEngineType.Advantage;
            try
            {
                // not necessary since we set to ADVANTAGE above, but here as an example.
                if (engineTypeString.Equals("advantage", StringComparison.OrdinalIgnoreCase))
                {
                    engineType = OcrEngineType.Advantage;
                }
                else if (engineTypeString.Equals("professional", StringComparison.OrdinalIgnoreCase))
                {
                    engineType = OcrEngineType.Professional;
                }
            }
            catch
            {
                // Error with engine type
                _OcrEngineStatus = OcrEngineStatus.Error;
                return;
            }

            // Check for a location of the OCR Runtime
            string runtimeDirectory = GetSettingValue(Key_Ocr_RuntimeDirectory);
            runtimeDirectory = GetAbsolutePath(runtimeDirectory);
            if (string.IsNullOrEmpty(runtimeDirectory))
            {
                runtimeDirectory = CheckOCRRuntimeDirectory();
            }

            // If still unset, get out
            if (string.IsNullOrEmpty(runtimeDirectory))
            {
                return;
            }

            // Use Advantage OCR engine
            IOcrEngine ocrEngine = OcrEngineManager.CreateEngine(engineType, true);

            try
            {
                // Start it up
                ocrEngine.Startup(default, default, default, runtimeDirectory);
                _ocrEngine = ocrEngine;
                _OcrEngineStatus = OcrEngineStatus.Ready;
            }
            catch
            {
                ocrEngine.Dispose();
                _OcrEngineStatus = OcrEngineStatus.Error;
                Trace.WriteLine("The OCR Engine could not be started. This application will continue to run, but without OCR functionality.");
            }
        }

        public static string GetFileUri(Uri uri)
        {
            if (!uri.IsAbsoluteUri)
            {
                return uri.ToString();
            }
            if ((uri.Scheme ?? "") == (Uri.UriSchemeFile ?? ""))
            {
                return uri.LocalPath;
            }
            else
            {
                return null;
            }
        }

        public static void CopyStream(Stream source, Stream target)
        {
            const int bufferSize = 1024 * 64;
            var buffer = new byte[65536];
            int bytesRead = 0;
            do
            {
                bytesRead = source.Read(buffer, 0, bufferSize);
                if (bytesRead > 0)
                {
                    target.Write(buffer, 0, bytesRead);
                }
            }
            while (bytesRead > 0);
        }

        public static byte[] ReadStream(Stream source, long position, int dataSize)
        {
            var buffer = new byte[dataSize];
            if (source.CanSeek)
            {
                source.Seek(position, SeekOrigin.Begin);
            }
            else
            {
                while (position > 0L)
                {
                    source.ReadByte();
                    position -= 1L;
                }
            }

            source.Read(buffer, 0, dataSize);
            return buffer;
        }
    }

}