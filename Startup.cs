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
                    var version = arr[1];
                    var controller = arr[2];
                    var action = arr[3];

                    if(!reflectionCache.AllApiVersions.Contains(version.TrimStart('v')))
                    {
                        // redirect to error api while this version hasn't been supported
                        var errmsg = $"Version {version} not supported.";
                        context.Request.Method = "GET";
                        context.Request.Path = new Microsoft.AspNetCore.Http.PathString("/api/Error");
                        context.Request.QueryString = new QueryString($"?message={errmsg}");
                    }
                    else
                    {
                        // trying to get all actions with this name
                        var realAction = reflectionCache.AllControllers.FirstOrDefault(x => x.Name == $"{controller}Controller")
                            .GetMethods()
                            .Where(x => x.IsPublic &&
                                x.GetCustomAttribute<ApiVersionAttribute>() != null &&
                                Convert.ToDouble(x.GetCustomAttribute<ApiVersionAttribute>().Versions.FirstOrDefault()?.ToString()) <= Convert.ToDouble(version.TrimStart('v')) &&
                                (x.Name == action || x.GetCustomAttribute<ActionNameAttribute>()?.Name == action))
                            .OrderByDescending(x => x.GetCustomAttribute<ApiVersionAttribute>().Versions.FirstOrDefault()?.ToString())
                            .First();
                        var realVersion = $"{realAction.GetCustomAttribute<ApiVersionAttribute>().Versions.FirstOrDefault()?.ToString()}";

                        if(realAction != null)
                        {
                            context.Request.Path = new Microsoft.AspNetCore.Http.PathString($"/api/v{realVersion}/{controller}/{realAction.Name}");
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
