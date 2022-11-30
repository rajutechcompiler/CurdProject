
using System.Data;
using System.Data.SqlClient;
using Leadtools;
using Leadtools.Codecs;
using Microsoft.VisualBasic; // Install-Package Microsoft.VisualBasic
using Microsoft.VisualBasic.CompilerServices; // Install-Package Microsoft.VisualBasic
using Smead.Security;
//using TabFusionRMS.WebVB.Common;

public partial class DocumentViewerModel
{
    public DocumentViewerModel()
    {
        slimShared.SlimShared.AppName = "TAB FusionRMS Web Access";
        AttachmentList = new List<UIparams>();
        VersionList = new List<UIparams>();
        MsgFileCheckout = new List<string>();
        AttachmentCartList = new List<AttachmentCart>();
        IsVault = false;
        isPermission = true;
    }
    public string DocumentService { get; set; }
    public string RecordId { get; set; }
    public string TableName { get; set; }
    public bool IsVault { get; set; }
    public string Tableid { get; set; }
    public string crumbName { get; set; }
    public List<UIparams> VersionList { get; set; }
    public List<UIparams> AttachmentList { get; set; }
    public List<AttachmentCart> AttachmentCartList { get; set; }
    // Property FilesPathList As List(Of String)
    public bool renameOnScan { get; set; }
    public string documentKey { get; set; }
    public string itemName { get; set; }
    public string FilePath { get; set; }
    public string ErrorMsg { get; set; }
    public string paths { get; set; }
    public string Name { get; set; }
    public string HasLink { get; set; }
    public string AttachmentNumberClick { get; set; }
    public bool isError { get; set; }
    public string NewProperty
    {
        get
        {
            return AttachmentNumberClick;
        }
        set
        {
            AttachmentNumberClick = value;
        }
    }
    public UIparams Params { get; set; }
    public List<string> MsgFileCheckout { get; set; }
    public int isCheckoutTome { get; set; }
    public int isCheckout { get; set; }
    public int isCheckoutDesktop { get; set; }
    public bool isPermission { get; set; }


}

public partial class DocumentViewerOrphanModel : DocumentViewerModel
{
    public DocumentViewerOrphanModel()
    {

    }
    public string? DocumentService { get; set; }
    public int PointerId = 0;
    public int trackbalesId;
    public int RecordType;
    public string FileName;
    public DocumentViewerModel ViewModel;
    public bool ShouldLast = false;
    public int AttachmentNumber = -1;
}


public partial class UIparams
{
    public UIparams()
    {
        // empty constructor
    }

    public UIparams(string filename, string path, string attachmentNumber, int versionNumber, int pageNumber, int RecordType, int NoteCountPc, int PointerId, int trackablsid, Passport passport, DocumentViewerModel ViewModel)
    {
        var encrypt = Common.EncryptURLParameters(path);
        this.path = encrypt;
        FileName = filename;
        VersionNumber = versionNumber;
        attchNumber = attachmentNumber;
        PageNumber = pageNumber;
        this.RecordType = RecordType;
        if (!File.Exists(path))
        {
            this.path = "NF";
        }
        // Me.Note = NoteCount
        this.PointerId = PointerId.ToString().PadLeft(30, '0');
        TrackbleId = trackablsid;
        // run this method to check note and annotation
        if (!ViewModel.TableName.Equals("Orphan"))
        {
            InitCheckAnnotation(passport, ViewModel, Convert.ToInt32(attachmentNumber), versionNumber);
        }
        HasAnnotation = CountAnnotation;
        if (RecordType == 1)
        {
            Note = CountNoteImage;
        }
        else
        {
            Note = NoteCountPc;
        }

    }
    public string path { get; set; }
    public string FileName { get; set; }
    public int VersionNumber { get; set; }
    public string attchNumber { get; set; }
    public int PageNumber { get; set; }
    public int RecordType { get; set; }
    public int Note { get; set; }
    public string PointerId { get; set; }
    public int TrackbleId { get; set; }
    public int HasAnnotation { get; set; }
    public int CountAnnotation { get; set; }
    public int CountNoteImage { get; set; }
    private bool InitCheckAnnotation(Passport passport, DocumentViewerModel viewModel, int AttachmentNumber, int VarsionNumber)
    {
        using (var cmd = new SqlCommand("SP_RMS_GetFilesPaths", passport.Connection()))
        {
            cmd.CommandType = (System.Data.CommandType)Conversions.ToInteger(CommandType.StoredProcedure);
            cmd.Parameters.AddWithValue("@tableId", viewModel.RecordId.PadLeft(30, '0'));
            cmd.Parameters.AddWithValue("@tableName", viewModel.TableName);
            cmd.Parameters.AddWithValue("@AttachmentNumber", AttachmentNumber);
            cmd.Parameters.AddWithValue("@RecordVersion", VarsionNumber);

            var adp = new SqlDataAdapter(cmd);
            var dTable = new DataTable();
            int datat = adp.Fill(dTable);
            foreach (DataRow row in dTable.Rows)
            {
                var getpath = row["FullPath"];
                int pointerid = Convert.ToInt32(row["pointerId"]);
                bool cAnnotation = CheckAnnotation(pointerid, passport);
                bool cNote = CheckNote(pointerid, passport);

                // check for annotation;
                if (cAnnotation)
                {
                    CountAnnotation = CountAnnotation + 1;
                }
                if (cNote)
                {
                    CountNoteImage = CountNoteImage + 1;
                }
            }
            return false;
        }
    }
    private bool CheckAnnotation(int pointerid, Passport passport)
    {
        var lstImages = new List<int>();
        string sql = "select * from Annotations a where a.[Table] = 'REDLINE' and a.TableId = '010001' + RIGHT('000000000000000000000000' + CAST(@pointerid AS VARCHAR), 24)";
        using (SqlConnection conn = passport.Connection())
        {
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@pointerid", pointerid);
                return Conversions.ToBoolean(cmd.ExecuteScalar());
            }
        }
    }
    private bool CheckNote(int pointerid, Passport passport)
    {
        var lstImages = new List<int>();

        string sql = "select * from Annotations a where a.[Table] = 'ImagePointers' and a.TableId = RIGHT('000000000000000000000000000000' + CAST(@pointerid AS VARCHAR), 30)";
        using (SqlConnection conn = passport.Connection())
        {
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@pointerid", pointerid);
                return Conversions.ToBoolean(cmd.ExecuteScalar());
            }
        }
    }
}

// Public Class FileDownloads
// Public Function GetFile(attachmentsList As String) As List(Of AttachmentsFileInfo)
// Dim attchName = attachmentsList.Split(",")
// Dim listfiles As List(Of AttachmentsFileInfo) = New List(Of AttachmentsFileInfo)()
// For Each filedecrypt In attchName
// Dim fc = Common.DecryptURLParameters(filedecrypt)
// Dim attachName = fc.Substring(fc.LastIndexOf("\") + 1)
// listfiles.Add(New AttachmentsFileInfo() With {
// .FileName = attachName,
// .FilePath = HttpUtility.UrlDecode(fc.ToString())
// })
// Next
// Return listfiles
// End Function
// End Class
public partial class FileDownloads
{
    internal Passport _passport { get; set; }
    internal List<string> pathString { get; set; }
    internal string _serverPath { get; set; }
    public List<string> deleteTempFile { get; set; }
    public string Path { get; set; }
    public string TableName { get; set; }
    public string TableId { get; set; }
    public int AttachNum { get; set; }
    public int AttachVer { get; set; }

    public List<AttachmentsFileInfo> GetFiles(List<FileDownloads> attachmentsList, Passport passport, string serverPath)
    {
        _passport = passport;
        _serverPath = serverPath;
        deleteTempFile = new List<string>();
        var listfiles = new List<AttachmentsFileInfo>();
        foreach (var filedecrypt in attachmentsList)
        {
            var fc = Common.DecryptURLParameters(filedecrypt.Path);
            var attachName = fc.Substring(fc.LastIndexOf(@"\") + 1);

            if (CheckIsDesktopFileBeforeDownload(filedecrypt))
            {
                string fileNewPath = this.SaveTempPDFFileToDisk(attachName);
                listfiles.Add(new AttachmentsFileInfo()
                {
                    FileName = fileNewPath.Substring(fileNewPath.LastIndexOf(@"\") + 1),
                    FilePath = System.Net.WebUtility.UrlDecode(fileNewPath.ToString())
                });
            }
            else
            {
                listfiles.Add(new AttachmentsFileInfo()
                {
                    FileName = attachName,
                    FilePath = System.Net.WebUtility.UrlDecode(fc.ToString())
                });
            }
        }
        return listfiles;
    }
    internal bool CheckIsDesktopFileBeforeDownload(FileDownloads @params)
    {
        pathString = new List<string>();

        using (var cmd = new SqlCommand("SP_RMS_GetFilesPaths", _passport.Connection()))
        {
            cmd.CommandType = (System.Data.CommandType)Conversions.ToInteger(CommandType.StoredProcedure);
            cmd.Parameters.AddWithValue("@tableId", @params.TableId);
            cmd.Parameters.AddWithValue("@tableName", @params.TableName);
            cmd.Parameters.AddWithValue("@AttachmentNumber", @params.AttachNum);
            cmd.Parameters.AddWithValue("@RecordVersion", @params.AttachVer);

            var adp = new SqlDataAdapter(cmd);
            var dTable = new DataTable();
            int datat = adp.Fill(dTable);
            foreach (DataRow row in dTable.Rows)
            {
                var getpath = row["FullPath"];
                // Dim pointerid = Convert.ToInt32(row("pointerId"))
                pathString.Add((string)getpath);
            }
        }

        if (pathString.Count > 1)
            return true;
        return false;
    }

    internal string SaveTempPDFFileToDisk(string filename)
    {
        int bitsPerPixel = 1;
        var codec = new RasterCodecs();
        codec.ThrowExceptionsOnInvalidImages = false;
        RasterImage srcImage = default;
        bool firstLoop = true;

        foreach (var img in pathString)
        {
            if (firstLoop == true)
            {
                srcImage = codec.Load(img, 0, CodecsLoadByteOrder.RgbOrGray, 1, -1);
                if (srcImage.BitsPerPixel >= 2)
                    bitsPerPixel = 24;
                if (srcImage.BitsPerPixel > 24)
                    bitsPerPixel = 24;
                firstLoop = false;
            }
            else
            {
                using (RasterImage addpage = codec.Load(img, 0, CodecsLoadByteOrder.RgbOrGray, 1, -1))
                {
                    if (addpage.BitsPerPixel >= 2)
                        bitsPerPixel = 24;
                    if (addpage.BitsPerPixel > 24)
                        bitsPerPixel = 24;
                    srcImage.AddPages(addpage, 1, addpage.PageCount);
                }
            }
        }

        string file = System.IO.Path.GetFileNameWithoutExtension(filename);
        string filePath = _serverPath + file + ".pdf";
        deleteTempFile.Add(filePath);
        // codec.Save(srcImage, "d:\test\" + Guid.NewGuid.ToString + "_temptif.tif", RasterImageFormat.Tif, 8)
        codec.Save(srcImage, filePath, RasterImageFormat.RasPdf, bitsPerPixel);
        return filePath;
    }
}

public partial class AttachmentsFileInfo
{
    public int FileId { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
}

public partial class ZipStreamWrapper : Stream
{

    public ZipStreamWrapper(Stream stream)
    {
        baseStream = stream;
        lengthf = 0;
    }

    private int lengthf;
    private Stream baseStream;

    public override bool CanRead
    {
        get
        {
            return true;
        }
    }

    public override bool CanSeek
    {
        get
        {
            return false;
        }
    }

    public override bool CanWrite
    {
        get
        {
            return true;
        }
    }

    public override long Length
    {
        get
        {
            return lengthf;
        }
    }

    public override long Position
    {
        get
        {
            return lengthf;
        }
        set
        {
            throw new NotImplementedException();
        }
    }

    public override void Flush()
    {
        baseStream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return baseStream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        baseStream.Write(buffer, offset, count);
        lengthf += count;
    }
}

public partial class AttachmentCart
{
    public AttachmentCart(int Id, int userid, string record, string filepath, string filename, int attacnum, int attachver)
    {
        userId = userid;
        Record = record;
        var encrypt = Common.EncryptURLParameters(filepath);
        filePath = Conversions.ToString(Strings.Chr(225)) + encrypt;
        fileName = filename;
        this.Id = Id;
        attachNum = attacnum;
        attachVer = attachver;

    }
    public int Id { get; set; }
    public int userId { get; set; }
    public string Record { get; set; }
    public string filePath { get; set; }
    public string fileName { get; set; }
    public int note { get; set; }
    public int attachNum { get; set; }
    public int attachVer { get; set; }

}