namespace ApiDocs.Models;

/// <summary>
/// Represents information about an endpoint in the API documentation.
/// </summary>
public class EndpointInfo
{
    /// <summary>
    /// The name of the controller that the endpoint is in.
    /// </summary>
    public string ControllerName { get; set; }

    /// <summary>
    /// The name of the action method that the endpoint is for.
    /// </summary>
    public string ActionName { get; set; }

    /// <summary>
    /// The HTTP method that the endpoint responds to.
    /// </summary>
    public string HttpMethod { get; set; }

    /// <summary>
    /// The route that the endpoint is accessible at.
    /// </summary>
    public string Route { get; set; }

    /// <summary>
    /// The return type of the endpoint.
    /// </summary>
    public object ReturnType { get; set; }

    /// <summary>
    /// The parameters that the endpoint accepts.
    /// </summary>
    public List<ParameterInfo> Parameters { get; set; }

    /// <summary>
    /// A summary of what the endpoint does.
    /// </summary>
    public string Summary { get; set; }
}