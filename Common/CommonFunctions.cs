using System;
using System.Drawing;
//using System.Web.Mvc;
using Leadtools;
//using Leadtools.WinForms;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Smead.RecordsManagement.Imaging;
using Exceptions = Smead.RecordsManagement.Imaging.Permissions.ExceptionString;
using static Smead.RecordsManagement.Navigation;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Text;
public class CommonFunctions
{

    private static int SafeInt(string value)
    {
        try
        {
            return Conversions.ToInteger(value);
        }
        catch (Exception)
        {
            return 0;
        }
    }
    //public static FileStreamResult GetImageFlyOut(string docdata)
    //{
    //    bool stampWithMessage = false;
    //    string message = string.Empty;
    //    bool validAttachment = false;
    //    string fullPath = string.Empty;
    //    var bmp = Smead.RecordsManagement.Imaging.Export.Output.NotAvailableImage();

    //    var @params = new string[] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty };
    //    var data = Strings.Split(DecryptString(docdata), DelimiterText);
    //    int count = data.GetUpperBound(0);

    //    for (int i = 0; i <= 4; i++)
    //    {
    //        if (i < count)
    //            @params[i] = data[i];
    //    }
    //    try
    //    {
    //        string filePath = Attachments.GetImageFlyout(@params[0].Replace("'", string.Empty).Replace("\"", string.Empty), SafeInt(@params[1]), @params[2], @params[3], @params[4], ref validAttachment, ref fullPath);

    //        if (!validAttachment)
    //        {
    //            if (!string.IsNullOrEmpty(fullPath))
    //            {
    //                // 2. realize redactions (cannot be done)

    //                var format = Smead.RecordsManagement.Imaging.Export.Output.Format.Jpg;

    //                using (var codec = new RasterCodecs())
    //                {
    //                    using (var img = codec.Load(fullPath, 1))
    //                    {

    //                        var rc = new Rectangle(0, 0, Attachment.FlyoutSize.Width, Attachment.FlyoutSize.Height);

    //                        if (img.BitsPerPixel <= 2)
    //                            format = Smead.RecordsManagement.Imaging.Export.Output.Format.Tif;
    //                        if (img.Width < Attachment.FlyoutSize.Width || img.Height < Attachment.FlyoutSize.Height)
    //                        {
    //                            rc.Width = img.Width;
    //                            rc.Height = img.Height;
    //                        }

    //                        rc = RasterImageList.GetFixedAspectRatioImageRectangle(img.Width, img.Height, rc);

    //                        var command = new Leadtools.ImageProcessing.ResizeCommand();
    //                        command.Flags = RasterSizeFlags.None;
    //                        command.DestinationImage = new RasterImage(RasterMemoryFlags.Conventional, rc.Width, rc.Height, img.BitsPerPixel, img.Order, img.ViewPerspective, img.GetPalette(), IntPtr.Zero, 0L);
    //                        command.Run(img);

    //                        codec.Save(command.DestinationImage, filePath, TranslateToLeadToolsFormat(format, img.BitsPerPixel), Attachment.ConvertBitsPerPixel(format, img.BitsPerPixel));

    //                        var reason = RasterImageConverter.TestCompatible(command.DestinationImage, true);
    //                        var pf = RasterImageConverter.GetNearestPixelFormat(command.DestinationImage);
    //                        if (reason != ImageIncompatibleReason.Compatible)
    //                            RasterImageConverter.MakeCompatible(command.DestinationImage, pf, true);

    //                        using (Bitmap bmp1 = (Bitmap)RasterImageConverter.ConvertToImage(command.DestinationImage, ConvertToImageOptions.None))
    //                        {
    //                            using (var stream = new System.IO.MemoryStream())
    //                            {
    //                                bmp1.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
    //                                return new FileStreamResult(new System.IO.MemoryStream(stream.ToArray()), "image/jpg");
    //                            }
    //                        }
    //                    }
    //                }
    //            }

    //            else
    //            {
    //                if (!string.IsNullOrEmpty(filePath))
    //                {
    //                    if (filePath.ToLower().StartsWith(Exceptions.FileNotFound.ToLower()))
    //                    {
    //                        bmp = Smead.RecordsManagement.Imaging.Export.Output.Invalid();
    //                        if (stampWithMessage)
    //                            Attachments.DrawTextOnErrorImage(bmp, Exceptions.FileNotFound);
    //                    }
    //                    else
    //                    {
    //                        if (bmp is null)
    //                            bmp = Smead.RecordsManagement.Imaging.Export.Output.NotAvailableImage();
    //                        if (stampWithMessage)
    //                            Attachments.DrawTextOnErrorImage(bmp, filePath);
    //                    }
    //                }
    //                else
    //                {
    //                    if (bmp is null)
    //                        bmp = Smead.RecordsManagement.Imaging.Export.Output.NotAvailableImage();
    //                    if (stampWithMessage)
    //                        Attachments.DrawTextOnErrorImage(bmp, "File Not Found");
    //                }

    //                using (var stream = new System.IO.MemoryStream())
    //                {
    //                    bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
    //                    return new FileStreamResult(new System.IO.MemoryStream(stream.ToArray()), "image/jpg");
    //                }
    //            }
    //        }
    //        else
    //        {
    //            using (var codec = new RasterCodecs())
    //            {
    //                using (var img = codec.Load(filePath, 1))
    //                {
    //                    var reason = RasterImageConverter.TestCompatible(img, true);
    //                    var pf = RasterImageConverter.GetNearestPixelFormat(img);
    //                    if (reason != ImageIncompatibleReason.Compatible)
    //                        RasterImageConverter.MakeCompatible(img, pf, true);

    //                    using (Bitmap bmp1 = (Bitmap)RasterImageConverter.ConvertToImage(img, ConvertToImageOptions.None))
    //                    {
    //                        using (var stream = new System.IO.MemoryStream())
    //                        {
    //                            bmp1.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
    //                            return new FileStreamResult(new System.IO.MemoryStream(stream.ToArray()), "image/jpg");
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    catch
    //    {
    //        return null;
    //    }
    //}
    //public static FileStreamResult GetSubImageFlyOut(string filePath, string fullPath, bool validAttachment)
    //{
    //    try
    //    {
           
    //        bool stampWithMessage = false;
    //        var bmp = Smead.RecordsManagement.Imaging.Export.Output.NotAvailableImage();

    //        if (!validAttachment)
    //        {
    //            if (!string.IsNullOrEmpty(fullPath) & System.IO.File.Exists(fullPath))
    //            {
    //                // 2. realize redactions (cannot be done)

    //                var format = Smead.RecordsManagement.Imaging.Export.Output.Format.Jpg;

    //                using (var codec = new RasterCodecs())
    //                {
    //                    using (var img = codec.Load(fullPath, 1))
    //                    {

    //                        var rc = new Rectangle(0, 0, Attachment.FlyoutSize.Width, Attachment.FlyoutSize.Height);

    //                        if (img.BitsPerPixel <= 2)
    //                            format = Smead.RecordsManagement.Imaging.Export.Output.Format.Tif;
    //                        if (img.Width < Attachment.FlyoutSize.Width || img.Height < Attachment.FlyoutSize.Height)
    //                        {
    //                            rc.Width = img.Width;
    //                            rc.Height = img.Height;
    //                        }

    //                        rc = RasterImageList.GetFixedAspectRatioImageRectangle(img.Width, img.Height, rc);

    //                        var command = new Leadtools.ImageProcessing.ResizeCommand();
    //                        command.Flags = RasterSizeFlags.None;
    //                        command.DestinationImage = new RasterImage(RasterMemoryFlags.Conventional, rc.Width, rc.Height, img.BitsPerPixel, img.Order, img.ViewPerspective, img.GetPalette(), IntPtr.Zero, 0L);
    //                        command.Run(img);

    //                        codec.Save(command.DestinationImage, filePath, TranslateToLeadToolsFormat(format, img.BitsPerPixel), Attachment.ConvertBitsPerPixel(format, img.BitsPerPixel));

    //                        var reason = RasterImageConverter.TestCompatible(command.DestinationImage, true);
    //                        var pf = RasterImageConverter.GetNearestPixelFormat(command.DestinationImage);
    //                        if (reason != ImageIncompatibleReason.Compatible)
    //                            RasterImageConverter.MakeCompatible(command.DestinationImage, pf, true);

    //                        using (Bitmap bmp1 = (Bitmap)RasterImageConverter.ConvertToImage(command.DestinationImage, ConvertToImageOptions.None))
    //                        {
    //                            using (var stream = new System.IO.MemoryStream())
    //                            {
    //                                bmp1.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
    //                                return new FileStreamResult(new System.IO.MemoryStream(stream.ToArray()), "image/jpg");
    //                            }
    //                        }
    //                    }
    //                }
    //            }

    //            else
    //            {
    //                if (!string.IsNullOrEmpty(filePath))
    //                {
    //                    if (filePath.ToLower().StartsWith(Exceptions.FileNotFound.ToLower()))
    //                    {
    //                        bmp = Smead.RecordsManagement.Imaging.Export.Output.Invalid();
    //                        if (stampWithMessage)
    //                            Attachments.DrawTextOnErrorImage(bmp, Exceptions.FileNotFound);
    //                    }
    //                    else
    //                    {
    //                        if (bmp is null)
    //                            bmp = Smead.RecordsManagement.Imaging.Export.Output.NotAvailableImage();
    //                        if (stampWithMessage)
    //                            Attachments.DrawTextOnErrorImage(bmp, filePath);
    //                    }
    //                }
    //                else
    //                {
    //                    if (bmp is null)
    //                        bmp = Smead.RecordsManagement.Imaging.Export.Output.NotAvailableImage();
    //                    if (stampWithMessage)
    //                        Attachments.DrawTextOnErrorImage(bmp, "File Not Found");
    //                }

    //                using (var stream = new System.IO.MemoryStream())
    //                {
    //                    bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
    //                    return new FileStreamResult(new System.IO.MemoryStream(stream.ToArray()), "image/jpg");
    //                }
    //            }
    //        }
    //        else
    //        {
    //            using (var codec = new RasterCodecs())
    //            {
    //                using (var img = codec.Load(filePath, 1))
    //                {
    //                    var reason = RasterImageConverter.TestCompatible(img, true);
    //                    var pf = RasterImageConverter.GetNearestPixelFormat(img);
    //                    if (reason != ImageIncompatibleReason.Compatible)
    //                        RasterImageConverter.MakeCompatible(img, pf, true);

    //                    using (Bitmap bmp1 = (Bitmap)RasterImageConverter.ConvertToImage(img, ConvertToImageOptions.None))
    //                    {
    //                        using (var stream = new System.IO.MemoryStream())
    //                        {
    //                            bmp1.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
    //                            return new FileStreamResult(new System.IO.MemoryStream(stream.ToArray()), "image/jpg");
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        throw ex;
    //    }

    //}

    public static RasterImageFormat TranslateToLeadToolsFormat(Smead.RecordsManagement.Imaging.Export.Output.Format format, int bitsPerPixel)
    {
        switch (format)
        {
            case Smead.RecordsManagement.Imaging.Export.Output.Format.Bmp:
                {
                    return RasterImageFormat.Bmp;
                }
            case Smead.RecordsManagement.Imaging.Export.Output.Format.Gif:
                {
                    return RasterImageFormat.Gif;
                }
            case Smead.RecordsManagement.Imaging.Export.Output.Format.Png:
                {
                    return RasterImageFormat.Png;
                }
            case Smead.RecordsManagement.Imaging.Export.Output.Format.Tif:
                {
                    if (bitsPerPixel <= 2)
                        return RasterImageFormat.TifxFaxG4;
                    return RasterImageFormat.Tif;
                }

            default:
                {
                    return RasterImageFormat.Jpeg;
                }
        }
    }
}