var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.GmailOrganizer_Web>("web");

builder.Build().Run();
