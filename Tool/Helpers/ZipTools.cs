using System;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using TABFusionRMS.Web.Tool.Helpers;

namespace TabFusionRMS.Web.Tool.Helpers
{

    internal sealed class ZipTools
    {
        private ZipTools()
        {
        }
        public static void ZipFiles(string baseDirectory, string[] files, string zipFileName)
        {
            if (baseDirectory is null)
            {
                throw new ArgumentNullException("baseDirectory");
            }
            if (zipFileName is null)
            {
                throw new ArgumentNullException("zipFileName");
            }

            if (files is null || files.Length == 0)
            {
                return;
            }

            baseDirectory = baseDirectory.ToLower(CultureInfo.CurrentCulture);
            int baseDirectoryLength = baseDirectory.Length;

            using (var package = Package.Open(zipFileName, FileMode.Create))
            {
                foreach (string file in files)
                {
                    if (string.IsNullOrEmpty(file) || !File.Exists(file) || !file.ToLower(CultureInfo.CurrentCulture).StartsWith(baseDirectory, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Add the part, relative to the base directory in the /path/file.ext format
                    string uri = file.Substring(baseDirectoryLength);
                    if (string.IsNullOrEmpty(uri))
                    {
                        continue;
                    }

                    // Repalce \ with /
                    uri = uri.Replace('\\', '/');
                    // Spaces is not supported
                    uri = uri.Replace(" ", "_");
                    if (!uri.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        uri = "/" + uri;
                    }

                    var partUri = new Uri(uri, UriKind.Relative);
                    var part = package.CreatePart(partUri, System.Net.Mime.MediaTypeNames.Application.Zip, System.IO.Packaging.CompressionOption.Normal);

                    using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        ServiceHelper.CopyStream(fileStream, part.GetStream());
                    }
                }
            }
        }
    }
}