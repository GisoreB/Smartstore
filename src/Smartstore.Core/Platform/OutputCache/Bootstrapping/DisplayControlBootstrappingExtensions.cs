﻿using Microsoft.Extensions.DependencyInjection.Extensions;
using Smartstore.Core.OutputCache;

namespace Smartstore.Core.Bootstrapping
{
    public static class DisplayControlBootstrappingExtensions
    {
        public static IServiceCollection AddDisplayControl(this IServiceCollection services)
        {
            Guard.NotNull(services);

            services.TryAddScoped<IDisplayControl, DisplayControl>();
            services.TryAddSingleton<IOutputCacheInvalidationObserver>(NullOutputCacheInvalidationObserver.Instance);

            return services;
        }
    }
}
