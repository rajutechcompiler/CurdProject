using Leadtools.Codecs;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Drawing;
using System.Security.AccessControl;
using System.Text;
using TabFusionRMS.Models;

//this class will call all fusion micro services in the future Moti Mashiah. in development ...
namespace TabFusionRMS.WebCS.FusionElevenModels
{
    public class ApiCalls : BaseModel
    {

        public ApiCalls(IConfiguration config)
        {
            DocumentServices = new DocumentService(config);
        }
        public DocumentService DocumentServices { get; }

        public async Task<string> APIPOST(string url, object obj)
        {

            var content = JsonConvert.SerializeObject(obj);
            var reqContent = new StringContent(content, Encoding.UTF8, "application/json");
            var client = new HttpClient();
            var call = client.PostAsync($"{url}", reqContent);
            return await call.Result.Content.ReadAsStringAsync();
        }

    }
    public class DocumentService
    {
        private static string hostService { get; set; }
        public DocumentService(IConfiguration config)
        {
            hostService = config.GetSection("lt.leadtools.documentService").Value;
        }
        public string SaveTempFileToCacheURL = $"{hostService}/Fusion/SaveTempFileTocache"; //pass List<string>
        private string GetStreamFlyOutFirstPageURL = $"Fusion/GetStreamFlyOutFirstPage";
        private string GetCodecInfoFromFileURL = $"Fusion/GetCodecInfoFromFile";
        private string GetCodecInfoFromFileListURL = $"Fusion/GetCodecInfoFromFileList";
        private string GetCacheLocationURL = $"Fusion/GetCacheLocation";

        public async Task<FileStreamResult> APIGETStreamFlyOutFirstPage(string filePath, string fullPath, bool validAttachment)
        {
            var client = new HttpClient();
            var url = $"{hostService}/{this.GetStreamFlyOutFirstPageURL}?filePath={filePath}&fullPath={fullPath}&validAttachment={validAttachment}";
            var call = client.GetAsync(url);
            var str = await call.Result.Content.ReadAsStreamAsync();

            return new FileStreamResult(str, "image/jpg");
        }
        //not in use. 
        public async Task<CodecsImageInfo> GetCodecInfoFromFile(string fileName, string extension)
        {
            var client = new HttpClient();
            var url = $"{hostService}/{this.GetCodecInfoFromFileURL}?fileName={fileName}&extension={extension}";
            var call = client.GetAsync(url);
            string stringResult = await call.Result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CodecsImageInfo>(stringResult);
        }

        public async Task<List<ListOfCodecInfo>> GetCodecInfoFromFileList(List<string> Filepathlist)
        {
            var client = new HttpClient();
            var content = JsonConvert.SerializeObject(Filepathlist);
            var reqContent = new StringContent(content, Encoding.UTF8, "application/json");
            var url = $"{hostService}/{this.GetCodecInfoFromFileListURL}";
            var call = client.PostAsync(url, reqContent);
            string stringResult = await call.Result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ListOfCodecInfo>>(stringResult);
        }

        public async Task<string> GetCacheLocation()
        {
            var client = new HttpClient();
            var call = client.GetAsync($"{hostService}/{this.GetCacheLocationURL}");
            return await call.Result.Content.ReadAsStringAsync();
        }
    }

    public class ListOfCodecInfo
    {
        public string? filepath { get; set; }
        public CodecsImageInfo? Info { get; set; }
    }

}
