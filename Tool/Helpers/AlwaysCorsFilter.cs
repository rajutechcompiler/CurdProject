using Microsoft.AspNetCore.Mvc.Filters;
using TABFusionRMS.Web.Tool.Helpers;

namespace TabFusionRMS.WebCS.Tool.Helpers
{
    public class AlwaysCorsFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
        {
            if(actionExecutedContext !=null)
            {
                if(actionExecutedContext.HttpContext.Response !=null)
                {
                    actionExecutedContext.HttpContext.Response.Headers.Remove("Access-Control-Allow-Origin");
                    actionExecutedContext.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", ServiceHelper.CORSOrigins);
                }
            }
            this.OnActionExecuted(actionExecutedContext);
        }
    }
}
