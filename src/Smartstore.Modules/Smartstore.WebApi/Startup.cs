﻿using System.Diagnostics;
using System.IO;
using System.Xml.XPath;
using Autofac;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.OData;
using Microsoft.OpenApi.Models;
using Smartstore.Engine;
using Smartstore.Engine.Builders;
using Smartstore.Web.Api.Bootstrapping;
using Smartstore.Web.Api.Security;
using Smartstore.Web.Api.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Smartstore.Web.Api
{
    // TODO: (mg) (core) at /$odata all endpoints are listed twice (named "N/A").
    // TODO: (mg) (core) IEEE754Compatible=true is not supported\working: https://github.com/OData/WebApi/issues/1460
    // TODO: (mg) (core) implement Rate Limiting when switching to .NET 7: https://devblogs.microsoft.com/dotnet/announcing-rate-limiting-for-dotnet/

    internal class Startup : StarterBase
    {
        public override void ConfigureServices(IServiceCollection services, IApplicationContext appContext)
        {
            services.AddSingleton<IApiUserStore, ApiUserStore>();
            services.AddScoped<IWebApiService, WebApiService>();

            services.Configure<AuthenticationOptions>((options) =>
            {
                if (!options.Schemes.Any(x => x.Name == "Api"))
                {
                    options.AddScheme<BasicAuthenticationHandler>("Api", null);
                }
            });

            services.Configure<MvcOptions>(o =>
            {
                o.RespectBrowserAcceptHeader = true;
                o.Conventions.Add(new ApiControllerModelConvention());
            });

            services.AddSwaggerGen(o =>
            {
                // INFO: "name" equals ApiExplorer.GroupName. Must be globally unique, URI-friendly and should be in lower case.
                o.SwaggerDoc("webapi1", new OpenApiInfo
                {
                    Version = "1",
                    Title = "Smartstore Web API"
                });

                // INFO: required workaround to avoid conflict errors for identical action names such as "Get" when opening swagger UI.
                // Alternative: set-up a path template for each (!) OData action method.
                o.ResolveConflictingActions(descriptions => descriptions.First());

                //o.IgnoreObsoleteActions();
                //o.IgnoreObsoleteProperties();

                // Avoids "Conflicting schemaIds" (multiple types with the same name but different namespaces).
                o.CustomSchemaIds(type => type.GetFriendlyId(true));

                o.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
                {
                    Name = HeaderNames.Authorization,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Basic",
                    In = ParameterLocation.Header,
                    Description = "Please enter your public and private API key."
                });

                o.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Basic",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                // INFO: provide unique OperationId. By default ApiDescription.RelativePath is used but that is not unique.
                // Prevents multiple descriptions from opening at the same time when clicking a method.
                o.CustomOperationIds(x => x.HttpMethod.ToLower().Grow(x.RelativePath, "/"));

                // Ordering within a group does not work. See https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/401
                //o.OrderActionsBy(x => ...);

                // Filters.
                o.DocumentFilter<SwaggerDocumentFilter>();
                o.OperationFilter<SwaggerOperationFilter>();
                //o.SchemaFilter<SwaggerSchemaFilter>();

                //o.MapType<decimal>(() => new OpenApiSchema
                //{
                //    Type = "number($double)",
                //    Example = new OpenApiDouble(16.5)
                //});

                IncludeXmlComments(o, appContext);
            });

            // INFO: needs to be placed after AddSwaggerGen(). Without this statement, the examples in the documentation
            // will contain everything, every tiny bit of any related object will be serialized.
            services.AddSwaggerGenNewtonsoftSupport();
        }

        public override void ConfigureContainer(ContainerBuilder builder, IApplicationContext appContext)
        {
            builder.RegisterType<ODataOptionsConfigurer>().As<IConfigureOptions<ODataOptions>>().SingleInstance();
        }

        public override void ConfigureMvc(IMvcBuilder mvcBuilder, IServiceCollection services, IApplicationContext appContext)
        {
            //services.TryAddEnumerable(ServiceDescriptor.Transient<IODataControllerActionConvention, CustomRoutingConvention>());

            mvcBuilder.AddOData();

            // INFO: no effect using OData 8.0.11 and OData.NewtonsoftJson 8.0.4. JSON is never written with Newtonsoft.Json.
            //.AddODataNewtonsoftJson();
        }

        public override void BuildPipeline(RequestPipelineBuilder builder)
        {
            if (builder.ApplicationContext.IsInstalled)
            {
                var routePrefix = WebApiSettings.SwaggerRoutePrefix;
                var isDev = builder.ApplicationContext.HostEnvironment.IsDevelopment();

                builder.Configure(StarterOrdering.BeforeRoutingMiddleware, app =>
                {
                    app.UseSwagger(o =>
                    {
                        o.RouteTemplate = routePrefix + "/{documentName}/swagger.{json|yaml}";
                        //o.SerializeAsV2 = true;
                    });

                    app.UseSwaggerUI(o =>
                    {
                        o.SwaggerEndpoint($"webapi1/swagger.json", "Web API");
                        o.RoutePrefix = routePrefix;

                        // Only show schemas dropdown for developers.
                        o.DefaultModelsExpandDepth(isDev ? 0 : -1);
                        //o.DefaultModelRendering(ModelRendering.Model);

                        o.EnablePersistAuthorization();
                        //o.EnableTryItOutByDefault();
                        //o.DisplayOperationId();
                        o.DisplayRequestDuration();
                        o.DocExpansion(DocExpansion.List);
                        o.EnableFilter();
                        //o.ShowCommonExtensions();
                        //o.InjectStylesheet("/swagger-ui/custom.css");

                        // Perf.
                        o.DefaultModelExpandDepth(2);
                        //o.DocExpansion(DocExpansion.None);
                        // Highlighting kills JavaScript rendering on large JSON results like product lists.
                        o.ConfigObject.AdditionalItems.Add("syntaxHighlight", false);
                    });
                });

                builder.Configure(StarterOrdering.BeforeRoutingMiddleware, app => 
                {
                    if (isDev)
                    {
                        // Navigate to ~/$odata to determine whether any endpoints did not match an odata route template.
                        app.UseODataRouteDebug();
                    }

                    // Add OData /$query middleware.
                    app.UseODataQueryRequest();

                    // Add the OData Batch middleware to support OData $batch.
                    app.UseODataBatching();

                    app.Use((context, next) =>
                    {
                        // Fixes null for IHttpContextAccessor.HttpContext when executing odata batch items.
                        // Needs to be placed after UseODataBatching. See
                        // https://github.com/dotnet/aspnet-api-versioning/issues/633
                        // https://github.com/OData/WebApi/issues/2294
                        var contextAccessor = builder.ApplicationContext.Services.Resolve<IHttpContextAccessor>();
                        contextAccessor.HttpContext ??= context;

                        return next(context);
                    });

                    // If you want to use /$openapi, enable the middleware.
                    //app.UseODataOpenApi();
                });

                builder.Configure(StarterOrdering.AfterWorkContextMiddleware, app =>
                {
                    app.Use(async (context, next) =>
                    {
                        try
                        {
                            await next(context);
                        }
                        catch (Exception ex)
                        {
                            ProcessException(context, ex);
                        }
                    });
                });
            }
        }

        private static void ProcessException(HttpContext context, Exception ex)
        {
            if (context?.Request?.Path.StartsWithSegments("/odata", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                // INFO: could be ODataException, InvalidCastException and many more.
                // Let the ErrorController handle our below ODataErrorException and (if accepted)
                // return the standard OData error JSON instead of something the client do not expect.
                var odataError = new ODataError
                {
                    ErrorCode = Status500InternalServerError.ToString(),
                    Message = ex.Message,
                    InnerError = new ODataInnerError(ex)
                };

                var odataEx = new ODataErrorException(ex.Message, ex, odataError);
                odataEx.Data["JsonContent"] = odataError.ToString();
                odataEx.ReThrow();
            }
            else
            {
                ex.ReThrow();
            }
        }

        private static void IncludeXmlComments(SwaggerGenOptions options, IApplicationContext appContext)
        {
            try
            {
                var modelProviders = appContext.TypeScanner
                    .FindTypes<IODataModelProvider>()
                    .Select(x => (IODataModelProvider)Activator.CreateInstance(x))
                    .ToList();

                // INFO: XPathDocument closes the input stream.
                modelProviders
                    .Select(x => x.GetXmlCommentsStream(appContext))
                    .Concat(new[] { GetXmlCommentsStream(appContext, "Smartstore.Core.xml") })
                    .Where(x => x != null)
                    .Each(x => options.IncludeXmlComments(() => new XPathDocument(x), true));
            }
            catch
            {
            }
        }

        private static Stream GetXmlCommentsStream(IApplicationContext appContext, string fileName)
        {
            var path = Path.Combine(appContext.RuntimeInfo.BaseDirectory, fileName);
            if (File.Exists(path))
            {
                return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            else
            {
                Debug.WriteLine($"Cannot find {fileName}. Expected location: {path}.");
            }

            return null;
        }
    }
}
