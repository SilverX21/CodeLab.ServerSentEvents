var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.CodeLab_ServerSentEvents_Api>("codelab-serversentevents-api");

builder.Build().Run();
