using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebApi.Template.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

using ActionName = WebApi.Template.Extensions.ActionNameAttribute;

namespace WebApi.Template.Extensions
{
    public class ReplaceVersionWithExactValueInPath : IDocumentFilter
    {
        private readonly ReflectionCache _reflectionCache;

        public ReplaceVersionWithExactValueInPath(ReflectionCache reflectionCache)
        {
            _reflectionCache = reflectionCache;
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var newPaths = new OpenApiPaths();
            foreach (var item in swaggerDoc.Paths)
            {
                var arr = item.Key.Split('/');
                // route as /api/[controller]/[action] mode
                if (_reflectionCache.AllControllers.Any(x => x.Name == $"{arr[arr.Length - 2]}Controller"))
                {
                    var methods = _reflectionCache.AllControllers.FirstOrDefault(x => x.Name == $"{arr[arr.Length - 2]}Controller")
                        .GetMethods();
                    var action = arr[arr.Length - 1];

                    var version = "v" + methods
                        .FirstOrDefault(x => x.Name == action &&
                            x.IsPublic &&
                            x.GetCustomAttribute<ApiVersionAttribute>() != null)
                        .GetCustomAttribute<ApiVersionAttribute>()?.Versions
                        .FirstOrDefault()
                        .ToString();
                    var settedAction = methods
                        .FirstOrDefault(x => x.Name == action &&
                            x.IsPublic &&
                            x.GetCustomAttribute<ApiVersionAttribute>() != null)
                        .GetCustomAttribute<ActionNameAttribute>()?.Name;
                    action = settedAction ?? action;

                    if (swaggerDoc.Info.Version == version)
                    {
                        newPaths.Add($"/api/{version}/{arr[arr.Length - 2]}/{action}", item.Value);
                    }
                }
                // // route as /api/[controller] mode
                // else if (controllers.Any(x => x.Name == $"{arr[arr.Length - 1]}Controller"))
                // {
                //     var newPathItemOperations = new Dictionary<OperationType, OpenApiOperation>();

                //     var methods = controllers.First(x => x.Name == $"{arr[arr.Length - 1]}Controller")
                //         .GetMethods()
                //         .Where(x => item.Value.Operations.Keys.Select(y => y.ToString()).Contains(x.Name) &&
                //             (x.GetCustomAttribute<HttpGetAttribute>() != null ||
                //             x.GetCustomAttribute<HttpPostAttribute>() != null ||
                //             x.GetCustomAttribute<HttpPutAttribute>() != null ||
                //             x.GetCustomAttribute<HttpPatchAttribute>() != null ||
                //             x.GetCustomAttribute<HttpDeleteAttribute>() != null));
                //     foreach(var method in methods)
                //     {
                //         var version = "v" + method.GetCustomAttribute<ApiVersionAttribute>()?.Versions
                //             .First()
                //             .ToString();
                        
                //         if (swaggerDoc.Info.Version == version)
                //         {
                //             if(Enum.TryParse(method.Name, out OperationType opType))
                //             {
                //                 newPathItemOperations.Add(opType, item.Value.Operations[opType]);    
                //             }
                //         }
                //     }

                //     item.Value.Operations = newPathItemOperations;
                //     newPaths.Add(item.Key.Replace("v{version}", swaggerDoc.Info.Version), item.Value);
                // }
            }

            swaggerDoc.Paths = newPaths;
        }
    }
}