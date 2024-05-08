﻿global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Net.Mime;
global using System.Threading.Tasks;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.OData.Deltas;
global using Microsoft.AspNetCore.OData.Results;
global using Microsoft.EntityFrameworkCore;
global using Smartstore.Core.Common;
global using Smartstore.Core.Data;
global using Smartstore.Core.Security;
global using static System.Net.Mime.MediaTypeNames.Application;
global using static Microsoft.AspNetCore.Http.StatusCodes;
using Smartstore.Engine.Modularity;
using Smartstore.Http;

namespace Smartstore.Web.Api
{
    internal class Module : ModuleBase, IConfigurable
    {
        public static string SystemName => "Smartstore.WebApi";

        public RouteInfo GetConfigurationRoute()
            => new("Configure", "WebApiAdmin", new { area = "Admin" });

        public override async Task InstallAsync(ModuleInstallationContext context)
        {
            await TrySaveSettingsAsync<WebApiSettings>();
            await ImportLanguageResourcesAsync();
            await base.InstallAsync(context);
        }

        public override async Task UninstallAsync()
        {
            await DeleteSettingsAsync<WebApiSettings>();
            await DeleteLanguageResourcesAsync();
            await base.UninstallAsync();
        }
    }
}
