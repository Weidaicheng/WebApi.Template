using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebApi.Template.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

using ActionNameAttribute = WebApi.Template.Extensions.ActionNameAttribute;
using StatusCodeEnum = WebApi.Template.Models.Enums.StatusCode;
using WebApi.Template.Models;

namespace WebApi.Template
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            ReflectionCache = new ReflectionCache();
            ReflectionCache.AllControllers = typeof(Program).Assembly
                .GetTypes()
                .Where(x => typeof(ControllerBase).IsAssignableFrom(x));
            ReflectionCache.AllApiVersions = ReflectionCache.AllControllers.SelectMany(x => x.GetMethods()
                .Where(x => x.IsPublic && x.GetCustomAttribute<ApiVersionAttribute>() != null)
                .SelectMany(x => x.GetCustomAttribute<ApiVersionAttribute>().Versions))
                .GroupBy(x => x.ToString())
                .Select(x => x.Key);
        }

        public IConfiguration Configuration { get; }

        public ReflectionCache ReflectionCache { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options =>
                options.Filters.Add(new ExceptionFilter(services.BuildServiceProvider().GetService<ILogger<ExceptionFilter>>())))
                    .ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);

            // add cached model
            services.AddSingleton<ReflectionCache>(provider => ReflectionCache);

            // add api versioning
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = false;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

            // add swagger
            services.AddSwaggerGen(options =>
            {
                options.DocInclusionPredicate((docName, apiDesc) =>
                {
                    if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo))
                        return false;

                    var versions = methodInfo.DeclaringType
                        .GetMethods()
                        .Where(x => x.IsPublic && x.GetCustomAttribute<ApiVersionAttribute>() != null)
                        .SelectMany(x => x.GetCustomAttribute<ApiVersionAttribute>().Versions);

                    return versions.Any(v => $"v{v.ToString()}" == docName);
                });

                options.OperationFilter<RemoveVersionFromParameter>();
                options.DocumentFilter<ReplaceVersionWithExactValueInPath>();

                foreach (var version in ReflectionCache.AllApiVersions)
                {
                    options.SwaggerDoc($"v{version}", new OpenApiInfo() { Title = "WebApi.Template API", Version = $"v{version}" });
                }
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ReflectionCache reflectionCache)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.Use(async (context, next) =>
            {
                // Do work that doesn't write to the Response.
                if(context.Request.Path.HasValue &&
                    context.Request.Path.Value.StartsWith("/api/"))
                {
                    // arr as this:
                    // api, version, controller, action
                    var arr = context.Request.Path.Value.Split("/")
                        .Where(x => !string.IsNullOrEmpty(x))
                        .ToArray();
                    var versionStr = arr[1];
                    var controllerStr = arr[2];
                    var actionStr = arr[3];

                    if(!reflectionCache.AllApiVersions.Contains(versionStr.TrimStart('v')))
                    {
                        // redirect to error api while this version hasn't been supported
                        var errmsg = $"Version {versionStr} not found.";
                        context.Request.Method = "GET";
                        context.Request.Path = new Microsoft.AspNetCore.Http.PathString("/api/Error");
                        context.Request.QueryString = new QueryString($"?message={errmsg}&status={StatusCodeEnum.NotFound}");
                    }
                    else
                    {
                        // trying to get all actions with this name
                        var controller = reflectionCache.AllControllers.FirstOrDefault(x => x.Name == $"{controllerStr}Controller");
                        if(controller != null)
                        {
                            var realAction = controller
                            .GetMethods()
                            .FirstOrDefault(x => x.IsPublic &&
                                x.GetCustomAttribute<ApiVersionAttribute>() != null &&
                                Convert.ToDouble(x.GetCustomAttribute<ApiVersionAttribute>().Versions.FirstOrDefault()?.ToString()) == Convert.ToDouble(versionStr.TrimStart('v')) &&
                                (x.Name == actionStr || x.GetCustomAttribute<ActionNameAttribute>()?.Name == actionStr));
                            if (realAction != null)
                            {
                                context.Request.Path = new Microsoft.AspNetCore.Http.PathString($"/api/{versionStr}/{controllerStr}/{realAction.Name}");
                            }
                            else
                            {
                                // redirect to error api while this action hasn't been supported
                                var errmsg = $"Api {context.Request.Path.Value} not found.";
                                context.Request.Method = "GET";
                                context.Request.Path = new Microsoft.AspNetCore.Http.PathString("/api/Error");
                                context.Request.QueryString = new QueryString($"?message={errmsg}&status={StatusCodeEnum.NotFound}");
                            }
                        }
                        else
                        {
                            // redirect to error api while this controller hasn't been supported
                            var errmsg = $"Api {context.Request.Path.Value} not found.";
                            context.Request.Method = "GET";
                            context.Request.Path = new Microsoft.AspNetCore.Http.PathString("/api/Error");
                            context.Request.QueryString = new QueryString($"?message={errmsg}&status={StatusCodeEnum.NotFound}");
                        }
                    }
                }
                
                // Do logging or other work that doesn't write to the Response.
                await next.Invoke();
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // add swagger UI
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                foreach (var version in ReflectionCache.AllApiVersions)
                {
                    c.SwaggerEndpoint($"/swagger/v{version}/swagger.json", $"WebApi.Template API V{version}");
                }
            });
        }
    }
}
