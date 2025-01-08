<div align="center">
  <a href="https://www.nuget.org/packages/ApiDocs/">
     <img src="https://img.shields.io/badge/ApiDocs-1.0.0-blue?color=%2300A76E">
  </a>
</div>

<br><br>

**ApiDocs** provides a quick and easy way to generate API documentation for your **ASP.NET Core Web API** just by adding a single endpoint to your application. It uses reflection to scan the controllers and actions in your application and generates a JSON document that describes the API endpoints, their parameters, and their responses.

The only thing you need to do is call the **MapApiDocs** extension method on your **WebApplication** instance (Program.cs). 
By default, the documentation will be available at the **/docs** route.

>app.MapApiDocs();
