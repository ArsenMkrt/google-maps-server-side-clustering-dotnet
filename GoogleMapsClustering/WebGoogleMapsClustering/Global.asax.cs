using System;
using System.Reflection;
using System.Web;
using System.Web.Routing;
using Kunukn.GooglemapsClustering.Clustering.Data;
using Kunukn.GooglemapsClustering.Clustering.Utility;
using Kunukn.GooglemapsClustering.Clustering.WebService;
using Kunukn.GooglemapsClustering.WebGoogleMapClustering.AreaGMC.Code.Logging;

namespace Kunukn.GooglemapsClustering.WebGoogleMapClustering
{
    /// <summary>
    /// Author: Kunuk Nykjaer
    /// </summary>
    public class Global : System.Web.HttpApplication
    {
        private static Log4Net _log;

        void Application_Start(object sender, EventArgs e)
        {            
            // Code that runs on application startup            
           _log = new Log4Net();

            // Database load simulation
            var websitepath = HttpContext.Current.Server.MapPath("~") + @"AreaGMC\Files\Points.csv";
            var points = Dataset.LoadDatasetFromDatabase(websitepath, LoadType.Csv);
            foreach (var p in points) p.Normalize();
                
            MemoryDatabase.SetPoints(points);                        
            RegisterRoutes();
        }

        private static void RegisterRoutes()
        {
            // Default            
            RouteTable.Routes.MapPageRoute("", "", "~/Default.html");
         
            // Ajax Service Endpoint
            RouteTable.Routes.Add(new System.ServiceModel.Activation.ServiceRoute("", 
                new System.ServiceModel.Activation.WebServiceHostFactory(), typeof(AjaxService)));
        }

        void Application_End(object sender, EventArgs e)
        {            
        }

        void Application_Error(object sender, EventArgs e)
        {
            var ex = this.Server.GetLastError();
            _log.Error(MethodBase.GetCurrentMethod(), ex);
        }

        void Session_Start(object sender, EventArgs e)
        {            
        }

        void Session_End(object sender, EventArgs e)
        {            
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {            
        }        
    }
}
