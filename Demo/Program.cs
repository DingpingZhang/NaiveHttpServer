using NaiveHttpServer;

var server = new Server("localhost", 2333);

server
    .Use(Middlewares.Log)
    .Use(Middlewares.Execute)
    .Use(Middlewares.StaticFile("/files", Environment.CurrentDirectory))
    .Use(Middlewares.NotFound(documentUrl: "http://api.project.com/v1"));

server.Start();

Console.ReadKey();

server.Stop();
