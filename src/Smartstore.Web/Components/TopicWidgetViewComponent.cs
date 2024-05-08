﻿using Smartstore.Core.Content.Topics;
using Smartstore.Core.Identity;

namespace Smartstore.Web.Components
{
    public class TopicWidgetViewComponent : SmartViewComponent
    {
        private readonly ICookieConsentManager _cookieManager;

        public TopicWidgetViewComponent(ICookieConsentManager cookieManager)
        {
            _cookieManager = cookieManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(TopicWidget model)
        {
            // Check for Cookie Consent
            if (model.CookieType != null && !await _cookieManager.IsCookieAllowedAsync(model.CookieType.Value))
            {
                return Empty();
            }

            Services.DisplayControl.Announce(new Topic { Id = model.Id });

            return View(model);
        }
    }
}
