﻿using System;
using System.Collections.Generic;
using System.Web;
using Kunukn.GooglemapsClustering.Data;
using Kunukn.GooglemapsClustering.DataUtility;

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

            // GOOGLEMAPS CLUSTERING DATABASE LOAD SIMULATION
            var websitepath = HttpContext.Current.Server.MapPath("~") + @"AreaGMC\Files\Points.csv";
            var points = Dataset.LoadDatasetFromDatabase(websitepath, DataUtility.LoadType.Csv);
            foreach (var p in points)            
                p.Normalize();

            Application[Names.Dataset] = points;            
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
            Session[Names.Filter] = new HashSet<string>();
            Session[Names.SessionStart] = DateTime.UtcNow as DateTime?;
        }

        void Session_End(object sender, EventArgs e)
        {
            // Code that runs when a session ends. 
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer 
            // or SQLServer, the event is not raised.

        }

    }
}