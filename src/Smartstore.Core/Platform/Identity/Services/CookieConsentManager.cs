﻿using System.Net;
using Autofac;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Smartstore.Core.Common.Services;
using Smartstore.Core.Data;
using Smartstore.Core.Localization;
using Smartstore.Core.Web;
using Smartstore.Net;

namespace Smartstore.Core.Identity
{
    public partial class CookieConsentManager : ICookieConsentManager
    {
        private readonly static object _lock = new();
        private static IList<Type> _cookiePublisherTypes = null;

        private readonly SmartDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHelper _webHelper;
        private readonly ITypeScanner _typeScanner;
        private readonly PrivacySettings _privacySettings;
        private readonly IComponentContext _componentContext;
        private readonly IGeoCountryLookup _countryLookup;

        private bool? _isCookieConsentRequired;

        public CookieConsentManager(
            SmartDbContext db,
            IHttpContextAccessor httpContextAccessor,
            IWebHelper webHelper,
            ITypeScanner typeScanner,
            PrivacySettings privacySettings,
            IComponentContext componentContext,
            IGeoCountryLookup countryLookup)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _webHelper = webHelper;
            _typeScanner = typeScanner;
            _privacySettings = privacySettings;
            _componentContext = componentContext;
            _countryLookup = countryLookup;
        }

        public async Task<bool> IsCookieConsentRequiredAsync()
        {
            return _isCookieConsentRequired ??= await IsCookieConsentRequiredCoreAsync(_webHelper.GetClientIpAddress());
        }

        protected virtual async Task<bool> IsCookieConsentRequiredCoreAsync(IPAddress ipAddress)
        {
            if (_privacySettings.CookieConsentRequirement == CookieConsentRequirement.NeverRequired)
            {
                return false;
            }
            else 
            {
                var geoCountry = _countryLookup.LookupCountry(ipAddress);
                if (geoCountry != null)
                {
                    if (_privacySettings.CookieConsentRequirement == CookieConsentRequirement.DependsOnCountry)
                    {           
                        var country = await _db.Countries
                            .AsNoTracking()
                            .ApplyIsoCodeFilter(geoCountry.IsoCode)
                            .FirstOrDefaultAsync();

                        if (country != null && !country.DisplayCookieManager)
                        {
                            return true;
                        }
                    }
                    else if (_privacySettings.CookieConsentRequirement == CookieConsentRequirement.RequiredInEUCountriesOnly)
                    {
                        return geoCountry.IsInEu; 
                    }
                }
            }
            
            return true;
        }

        public virtual async Task<IList<CookieInfo>> GetCookieInfosAsync(bool withUserCookies = false)
        {
            var result = new List<CookieInfo>();
            var publishers = GetAllCookiePublishers();

            foreach (var publisher in publishers)
            {
                var cookieInfo = await publisher.GetCookieInfosAsync();
                if (cookieInfo != null)
                {
                    result.AddRange(cookieInfo);
                }
            }

            // Add user defined cookies from privacy settings.
            if (withUserCookies)
            {
                result.AddRange(GetUserCookieInfos(true));
            }

            return result;
        }

        public virtual IReadOnlyList<CookieInfo> GetUserCookieInfos(bool translated = true)
        {
            if (_privacySettings.CookieInfos.HasValue())
            {
                var cookieInfos = JsonConvert.DeserializeObject<List<CookieInfo>>(_privacySettings.CookieInfos);

                if (cookieInfos?.Any() ?? false)
                {
                    if (translated)
                    {
                        foreach (var info in cookieInfos)
                        {
                            info.Name = info.GetLocalized(x => x.Name);
                            info.Description = info.GetLocalized(x => x.Description);
                        }
                    }

                    return cookieInfos;
                }
            }

            return new List<CookieInfo>();
        }

        public virtual async Task<bool> IsCookieAllowedAsync(CookieType cookieType)
        {
            Guard.NotNull(cookieType);

            if (!await IsCookieConsentRequiredAsync())
            {
                return true;
            }

            var request = _httpContextAccessor?.HttpContext?.Request;
            if (request != null && request.Cookies.TryGetValue(CookieNames.CookieConsent, out var value) && value.HasValue())
            {
                try
                {
                    var cookieData = JsonConvert.DeserializeObject<ConsentCookie>(value);

                    if ((cookieData.AllowAnalytics && cookieType == CookieType.Analytics) ||
                        (cookieData.AllowThirdParty && cookieType == CookieType.ThirdParty) ||
                        cookieType == CookieType.Required)
                    {
                        return true;
                    }
                }
                catch
                {
                    // Let's be tolerant in case of error.
                    return true;
                }
            }

            // If no cookie was set return false.
            return false;
        }

        public virtual ConsentCookie GetCookieData()
        {
            var context = _httpContextAccessor?.HttpContext;
            if (context != null)
            {
                var cookieName = CookieNames.CookieConsent;

                if (context.Items.TryGetValue(cookieName, out var obj))
                {
                    return obj as ConsentCookie;
                }

                if (context.Request?.Cookies?.TryGetValue(cookieName, out string value) ?? false)
                {
                    try
                    {
                        var cookieData = JsonConvert.DeserializeObject<ConsentCookie>(value);
                        context.Items[cookieName] = cookieData;

                        return cookieData;
                    }
                    catch
                    {
                    }
                }
            }

            return null;
        }

        public virtual void SetConsentCookie(bool allowAnalytics = false, bool allowThirdParty = false)
        {
            var context = _httpContextAccessor?.HttpContext;
            if (context != null)
            {
                var cookieData = new ConsentCookie
                {
                    AllowAnalytics = allowAnalytics,
                    AllowThirdParty = allowThirdParty
                };

                var cookies = context.Response.Cookies;
                var cookieName = CookieNames.CookieConsent;

                var options = new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddDays(365),
                    HttpOnly = true,
                    IsEssential = true,
                    Secure = _webHelper.IsCurrentConnectionSecured()
                };

                cookies.Delete(cookieName, options);
                cookies.Append(cookieName, JsonConvert.SerializeObject(cookieData), options);
            }
        }

        protected virtual IEnumerable<ICookiePublisher> GetAllCookiePublishers()
        {
            if (_cookiePublisherTypes == null)
            {
                lock (_lock)
                {
                    if (_cookiePublisherTypes == null)
                    {
                        _cookiePublisherTypes = _typeScanner.FindTypes<ICookiePublisher>().ToList();
                    }
                }
            }

            var cookiePublishers = _cookiePublisherTypes
                .Select(type =>
                {
                    if (_componentContext.IsRegistered(type) && _componentContext.TryResolve(type, out var publisher))
                    {
                        return (ICookiePublisher)publisher;
                    }

                    return _componentContext.ResolveUnregistered(type) as ICookiePublisher;
                })
                .ToArray();

            return cookiePublishers;
        }
    }

    /// <summary>
    /// Infos that will be serialized and stored as string in a cookie.
    /// </summary>
    public class ConsentCookie
    {
        public bool AllowAnalytics { get; set; }
        public bool AllowThirdParty { get; set; }
    }
}
