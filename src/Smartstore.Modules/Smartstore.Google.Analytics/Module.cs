﻿global using System;
global using System.Collections.Generic;
global using System.ComponentModel.DataAnnotations;
global using System.IO;
global using System.Linq;
global using System.Threading.Tasks;
global using Smartstore.Core.Localization;
global using Smartstore.Google.Analytics.Settings;
global using Smartstore.Web.Modelling;
global using Smartstore.Web.Models.Cart;
global using Smartstore.Web.Models.Catalog;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Smartstore.Core.Identity;
using Smartstore.Core.Widgets;
using Smartstore.Engine.Modularity;
using Smartstore.Google.Analytics.Components;
using Smartstore.Http;

namespace Smartstore.Google.Analytics
{
    internal class Module : ModuleBase, IConfigurable, IActivatableWidget, ICookiePublisher
    {
        private readonly IProviderManager _providerManager;
        private readonly WidgetSettings _widgetSettings;

        public Module(IProviderManager providerManager,  WidgetSettings widgetSettings)
        {
            _providerManager = providerManager;
            _widgetSettings = widgetSettings;
        }

        public ILogger Logger { get; set; } = NullLogger.Instance;
        public Localizer T { get; set; } = NullLocalizer.Instance;

        public RouteInfo GetConfigurationRoute()
            => new("Configure", "GoogleAnalytics", new { area = "Admin" });

        public Task<IEnumerable<CookieInfo>> GetCookieInfosAsync()
        {
            var widget = _providerManager.GetProvider<IActivatableWidget>("Smartstore.Google.Analytics");
            if (!widget.IsWidgetActive(_widgetSettings))
            {
                return Task.FromResult(Enumerable.Empty<CookieInfo>());
            }

            var cookieInfo = new CookieInfo
            {
                Name = T("Plugins.FriendlyName.SmartStore.Google.Analytics"),
                Description = T("Plugins.Widgets.GoogleAnalytics.CookieInfo"),
                CookieType = CookieType.Analytics
            };

            return Task.FromResult(new CookieInfo[] { cookieInfo }.AsEnumerable());
        }

        public Widget GetDisplayWidget(string widgetZone, object model, int storeId)
            => new ComponentWidget(typeof(GoogleAnalyticsViewComponent), model);

        public string[] GetWidgetZones()
            => new[] { "head" };

        public override async Task InstallAsync(ModuleInstallationContext context)
        {
            await ImportLanguageResourcesAsync();
            await TrySaveSettingsAsync(new GoogleAnalyticsSettings
            {
                TrackingScript = AnalyticsScriptUtility.GetTrackingScript(),
                EcommerceScript = AnalyticsScriptUtility.GetEcommerceScript(),
                EcommerceDetailScript = AnalyticsScriptUtility.GetEcommerceDetailScript()
            });
            await base.InstallAsync(context);
        }

        public override async Task UninstallAsync()
        {
            await DeleteLanguageResourcesAsync();
            await DeleteSettingsAsync<GoogleAnalyticsSettings>();
            await base.UninstallAsync();
        }
    }
}