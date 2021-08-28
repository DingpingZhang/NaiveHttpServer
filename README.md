# SimpleHttpServer

A simple C# http server based on the HttpListener.

## How to Use

```csharp
Middleware<Context> router = new RouterBuilder()
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

Server server = new("localhost", 2333);

server
    .Use(Middlewares.Log)
    .Use(Middlewares.Execute)
    .Use(router)
    .Use(Middlewares.StaticFiles("/files", Environment.CurrentDirectory))
    .Use(Middlewares.NotFound(documentUrl: "http://api.project.com/v1"));

server.Start();
```
