using System;
using System.Collections.Generic;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;
using Kunukn.GooglemapsClustering.Data;
using Kunukn.GooglemapsClustering.DataUtility;
using Kunukn.GooglemapsClustering.WebGoogleMapClustering.AreaGMC.WebService;

namespace Kunukn.GooglemapsClustering.WebGoogleMapClustering
{
    /// <summary>
    /// Author: Kunuk Nykjaer
    /// </summary>
    public class Global : System.Web.HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup            

            // Database load simulation
            var websitepath = HttpContext.Current.Server.MapPath("~") + @"AreaGMC\Files\Points.csv";
            var points = Dataset.LoadDatasetFromDatabase(websitepath, DataUtility.LoadType.Csv);
            foreach (var p in points)
            {
                p.Normalize();
            }
                
            Application[SessionKeys.GMC_Dataset] = points;
            
            RegisterRoutes();
        }

        private static void RegisterRoutes()
        {
            // Default            
            RouteTable.Routes.MapPageRoute("","", "~/Default.aspx");

            // Ajax Service Endpoint
            RouteTable.Routes.Add(new System.ServiceModel.Activation.ServiceRoute("", new System.ServiceModel.Activation.WebServiceHostFactory(), typeof(AjaxService)));
        }

        void Application_End(object sender, EventArgs e)
        {
            //  Code that runs on application shutdown
        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs
        }

        void Session_Start(object sender, EventArgs e)
        {
            // Code that runs when a new session is started
            Session[SessionKeys.GMC_Filter] = new HashSet<string>();
            Session[SessionKeys.GMC_SessionStart] = DateTime.UtcNow as DateTime?;
            Session[SessionKeys.GMC_ClusteringEnabled] = "1";
        }

        void Session_End(object sender, EventArgs e)
        {            
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            //HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            //HttpContext.Current.Response.Cache.SetNoStore();
            //EnableCrossDomainAjaxCall();
        }

        static void EnableCrossDomainAjaxCall()
        {
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            {
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST");
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept");
                HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "1728000");
                HttpContext.Current.Response.End();
            }
        }
    }
}
