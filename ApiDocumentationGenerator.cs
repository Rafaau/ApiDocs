using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using ApiDocs.Models;

namespace ApiDocs;

/// <summary>
/// Class for generating API documentation.
/// </summary>
public class ApiDocumentationGenerator
{
    /// <summary>
    /// Generates documentation for all endpoints in the application.
    /// </summary>
    /// <returns>Returns a list of <see cref="EndpointInfo"/> objects representing the endpoints.</returns>
    public List<EndpointInfo> GenerateDocumentation()
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly == null)
        {
            throw new InvalidOperationException("Unable to determine the entry assembly. Ensure the method is called from within the application context.");
        }

        var controllerTypes = assembly.GetTypes()
            .Where(type => type.IsSubclassOf(typeof(ControllerBase)) || type.GetCustomAttribute<ApiControllerAttribute>() != null);

        var endpoints = new List<EndpointInfo>();

        foreach (var controller in controllerTypes)
        {
            var methods = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttributes<HttpMethodAttribute>(true).Any());

            foreach (var method in methods)
            {
                var httpAttributes = method.GetCustomAttributes<HttpMethodAttribute>(true);
                foreach (var attr in httpAttributes)
                {
                    var endpoint = new EndpointInfo
                    {
                        ControllerName = controller.Name,
                        ActionName = method.Name,
                        HttpMethod = attr.GetType().Name.Replace("Http", "").Replace("Attribute", ""),
                        Route = GetRoute(controller, method),
                        ReturnType = new
                        {
                            Name = GetFriendlyTypeName(method.ReturnType),
                            Structure = GetTypeStructure(method.ReturnType)
                        },
                        Parameters = method.GetParameters()
                            .Select(p => new Models.ParameterInfo
                            {
                                Name = p.Name,
                                Type = new
                                {
                                    p.ParameterType.Name,
                                    Structure = GetTypeStructure(p.ParameterType)
                                },
                                Source = GetParameterSource(p, method)
                            }).ToList(),
                        Summary = method.GetCustomAttribute<DisplayAttribute>()?.Description ?? string.Empty
                    };

                    endpoints.Add(endpoint);
                }
            }
        }

        return endpoints;
    }

    /// <summary>
    /// Gets the route for a given controller and method.
    /// </summary>
    /// <param name="controller">Controller type.</param>
    /// <param name="method">Method info.</param>
    /// <returns>Returns the route for the endpoint.</returns>
    private string GetRoute(Type controller, MethodInfo method)
    {
        var routeAttribute = controller.GetCustomAttribute<RouteAttribute>();
        var controllerRoute = routeAttribute?.Template ?? "[controller]";

        var actionRoute =
            method.GetCustomAttribute<HttpPostAttribute>()?.Template ??
            method.GetCustomAttribute<HttpGetAttribute>()?.Template ??
            method.GetCustomAttribute<HttpPutAttribute>()?.Template ??
            method.GetCustomAttribute<HttpDeleteAttribute>()?.Template ??
            method.GetCustomAttribute<HttpPatchAttribute>()?.Template ??
            string.Empty;

        string finalRoute = (controllerRoute + "/" + actionRoute).Replace("[controller]", controller.Name.Replace("Controller", ""));

        if (finalRoute.EndsWith("/"))
            finalRoute = finalRoute.Substring(0, finalRoute.Length - 1);

        var routeParams = method.GetParameters()
            .Where(p => GetParameterSource(p, method) == "Route")
            .Select(p => p.Name);

        foreach (var routeParam in routeParams)
        {
            finalRoute = finalRoute.Replace($"{{{routeParam}}}", $"{{{routeParam}}}");
        }

        return finalRoute;
    }

    /// <summary>
    /// Gets the source of a parameter in a method.
    /// </summary>
    /// <param name="parameter">Parameter info.</param>
    /// <param name="method">Method info.</param>
    /// <returns>Returns the source of the parameter.</returns>
    private string GetParameterSource(System.Reflection.ParameterInfo parameter, MethodInfo method)
    {
        var attributes = parameter.GetCustomAttributes(false);

        if (attributes.Any(attr => attr.GetType() == typeof(FromQueryAttribute)))
            return "Query";
        if (attributes.Any(attr => attr.GetType() == typeof(FromBodyAttribute)))
            return "Body";
        if (attributes.Any(attr => attr.GetType() == typeof(FromRouteAttribute)))
            return "Route";
        if (attributes.Any(attr => attr.GetType() == typeof(FromHeaderAttribute)))
            return "Header";
        if (attributes.Any(attr => attr.GetType() == typeof(FromFormAttribute)))
            return "Form";

        var routeAttribute = method.GetCustomAttribute<RouteAttribute>()?.Template;
        var actionRoute = method.GetCustomAttributes<HttpMethodAttribute>(true)
            .Select(attr => attr.Template)
            .FirstOrDefault(template => !string.IsNullOrEmpty(template));

        var combinedRoute = (routeAttribute + "/" + actionRoute)?.ToLowerInvariant() ?? string.Empty;
        if (combinedRoute.Contains($"{{{parameter.Name.ToLowerInvariant()}:") ||
            combinedRoute.Contains($"{{{parameter.Name.ToLowerInvariant()}}}"))
        {
            return "Route";
        }

        return "Query";
    }

    /// <summary>
    /// Gets a friendly name for a type.
    /// </summary>
    /// <param name="type">Type to get the friendly name for.</param>
    /// <returns>Returns the friendly name of the type.</returns>
    private string GetFriendlyTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            var genericTypeName = type.Name.Substring(0, type.Name.IndexOf('`'));
            var genericArgs = type.GetGenericArguments();
            var genericArgsNames = string.Join(", ", genericArgs.Select(GetFriendlyTypeName));
            return $"{genericTypeName}<{genericArgsNames}>";
        }

        return type.Name;
    }

    /// <summary>
    /// Gets the structure of a type.
    /// </summary>
    /// <param name="type">Type to get the structure for.</param>
    /// <returns>Returns the structure of the type.</returns>
    private object GetTypeStructure(Type type)
    {
        if (type.IsGenericType)
        {
            var genericTypeName = type.Name.Substring(0, type.Name.IndexOf('`'));
            var genericArgs = type.GetGenericArguments();
            return new
            {
                Type = $"{genericTypeName}<{string.Join(", ", genericArgs.Select(arg => arg.Name))}>",
                GenericArguments = genericArgs.Select(GetTypeStructure).ToList()
            };
        }

        if (type.IsClass && type != typeof(string))
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            return new
            {
                Type = type.Name,
                Properties = properties.Select(prop => new
                {
                    Name = prop.Name,
                    PropertyType = GetFriendlyTypeName(prop.PropertyType),
                    Structure = GetTypeStructure(prop.PropertyType)
                }).ToList()
            };
        }

        return new { Type = type.Name };
    }
}


