using Microsoft.AspNetCore.Builder;

namespace ApiDocs;

/// <summary>
/// Extension methods for the <see cref="WebApplication"/> class.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Maps a route of the API documentation to the application.
    /// </summary>
    /// <param name="app">An instance of the <see cref="WebApplication"/> class.</param>
    /// <param name="route">Route to map the API documentation to. Default is "/docs".</param>
    public static void MapApiDocs(this WebApplication app, string route = "/docs")
    {
        app.MapGet(route, () =>
        {
            var generator = new ApiDocumentationGenerator();
            var docs = generator.GenerateDocumentation();
            return docs;
        });
    }
}
