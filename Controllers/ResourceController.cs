using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json.Linq;
using TabFusionRMS.Resource;

namespace TabFusionRMS.WebCS.Controllers
{
    public class ResourceController : BaseController
    {
        //public ResourceController(IHttpContextAccessor httpContext) : base(httpContext)
        //{ }
        [HttpGet]
        public object GetMobileResources(Languages.ResModule moduleName)
        {
            try
            {
                Dictionary<string, string> resouceObject;
                resouceObject = Languages.get_GetValuesByModule(moduleName);

                // #Region "Add keys in Dictionary"
                var resourceSetCommon = new Dictionary<string, string>();
                resourceSetCommon = Languages.get_GetValuesByModule(Languages.ResModule.common);

                foreach (var resource in resourceSetCommon)
                {
                    if (!resouceObject.ContainsKey(resource.Key))
                    {
                        resouceObject.Add(resource.Key, resource.Value);
                    }
                }
                // #End Region
                // Return resouceObject

                return Json(resouceObject);

            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        public IActionResult GetResources(Languages.ResModule moduleName)
        {
            try
            {
                var resouceObject = new Dictionary<string, string>();

                if (moduleName == Languages.ResModule.all)
                {
                    var resObjApplication = new Dictionary<string, string>();
                    var resObjClients = new Dictionary<string, string>();
                    var resObjData = new Dictionary<string, string>();
                    var resObjDatabase = new Dictionary<string, string>();
                    var resObjDirectories = new Dictionary<string, string>();
                    var resObjImport = new Dictionary<string, string>();
                    var resObjLabelmanager = new Dictionary<string, string>();
                    var resObjReports = new Dictionary<string, string>();
                    var resObjRetention = new Dictionary<string, string>();
                    var resObjScanner = new Dictionary<string, string>();
                    var resObjSecurity = new Dictionary<string, string>();
                    var resObjTables = new Dictionary<string, string>();
                    var resObjViews = new Dictionary<string, string>();
                    var resObjCommon = new Dictionary<string, string>();
                    var resObjHtmlViewer = new Dictionary<string, string>();

                    resObjApplication = Languages.get_GetValuesByModule(Languages.ResModule.application);
                    resObjClients = Languages.get_GetValuesByModule(Languages.ResModule.clients);
                    resObjData = Languages.get_GetValuesByModule(Languages.ResModule.data);
                    resObjDatabase = Languages.get_GetValuesByModule(Languages.ResModule.database);
                    resObjDirectories = Languages.get_GetValuesByModule(Languages.ResModule.directories);
                    resObjImport = Languages.get_GetValuesByModule(Languages.ResModule.import);
                    resObjLabelmanager = Languages.get_GetValuesByModule(Languages.ResModule.labelmanager);
                    resObjReports = Languages.get_GetValuesByModule(Languages.ResModule.reports);
                    resObjRetention = Languages.get_GetValuesByModule(Languages.ResModule.retention);
                    resObjScanner = Languages.get_GetValuesByModule(Languages.ResModule.scanner);
                    resObjSecurity = Languages.get_GetValuesByModule(Languages.ResModule.security);
                    resObjTables = Languages.get_GetValuesByModule(Languages.ResModule.tables);
                    resObjViews = Languages.get_GetValuesByModule(Languages.ResModule.views);
                    resObjCommon = Languages.get_GetValuesByModule(Languages.ResModule.common);
                    resObjHtmlViewer = Languages.get_GetValuesByModule(Languages.ResModule.htmlviewer);
                    return Json(new
                    {
                        resApp = resObjApplication,
                        resClient = resObjClients,
                        resData = resObjData,
                        resDatabase = resObjDatabase,
                        resDirectories = resObjDirectories,
                        resImport = resObjImport,
                        resLabelmanager = resObjLabelmanager,
                        resReports = resObjReports,
                        resRetention = resObjRetention,
                        resScanner = resObjScanner,
                        resSecurity = resObjSecurity,
                        resTables = resObjTables,
                        resViews = resObjViews,
                        resCommon = resObjCommon,
                        resHTMLViewer = resObjHtmlViewer
                    });
                }
                else
                {
                    resouceObject = Languages.get_GetValuesByModule(moduleName);
                    return Json(resouceObject);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
