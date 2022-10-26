# NaiveHttpServer [![version](https://img.shields.io/nuget/v/NaiveHttpServer.svg)](https://www.nuget.org/packages/NaiveHttpServer)

A simple C# http server based on the HttpListener.

## How to Use

```csharp
// Build Routers
var router = new RouterBuilder()
    .Get("/user/:id", async ctx =>
    {
        // Gets parameters from URL or QUERY string by name.
        if (ctx.TryGetParameter("id", out string id))
        {
            // TODO: using the id parameter.
            await ctx.Response.Json(new
            {
                id,
                // TODO: other properties.
            });
        }
    })
    .Post("/user", async ctx =>
    {
        dynamic body = ctx.Request.JsonFromBody<User>();
        // TODO: using the body object.
    })
    .Put("/user/:id", async ctx => { /* TODO: Do something. */ })
    .Delete("/user/:id", async ctx => { /* TODO: Do something. */ })
    .Build();

// Create server instance
var server = new Server("localhost", 2333);

// Configure server
server
    .Use(Middlewares.Log)
    .Use(Middlewares.Execute)
    .Use(router)
    .Use(Middlewares.StaticFile("/files", Environment.CurrentDirectory))
    .Use(Middlewares.NotFound(documentUrl: "http://api.project.com/v1"));

// Launch server
server.Start();
```
