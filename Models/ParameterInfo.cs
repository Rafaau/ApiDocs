namespace ApiDocs.Models;

/// <summary>
/// Represents information about a parameter in the API documentation.
/// </summary>
public class ParameterInfo
{
    /// <summary>
    /// The name of the parameter.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The type of the parameter.
    /// </summary>
    public object Type { get; set; }

    /// <summary>
    /// The source of the parameter.
    /// </summary>
    public string Source { get; set; }
}

