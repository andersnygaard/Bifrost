﻿using System.Web;
using Bifrost.Configuration;

namespace Bifrost.QuickStart
{
    public class BifrostConfigurator : ICanConfigure
    {
        public void Configure(IConfigure configure)
        {
            var dataPath = HttpContext.Current.Server.MapPath("~/App_Data");
            configure
                .Serialization
                    .UsingJson()
                .Events
                    .UsingRavenDB(e=>e.WithUrl("http://localhost:8080").WithDefaultDatabase("BifrostQuickStart"))
                    //.UsingRavenDBEmbedded(e=>e.LocatedAt(dataPath).WithManagementStudio())
                .Events
                    .Asynchronous(e=>e.UsingSignalR())
                .DefaultStorage
                    .UsingRavenDB(e => e.WithUrl("http://localhost:8080").WithDefaultDatabase("BifrostQuickStart"))
                    //.UsingRavenDBEmbedded(e=>e.LocatedAt(dataPath))
                .Frontend
                    .Web(w=>w.AsSinglePageApplication())
                .WithMimir();
        }
    }
}