using System.Reflection;
using Corretora.Api.Common.Messaging;
using Corretora.Api.Database;

[assembly: Module("Corretora")]

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IBanco, BancoEmMemoria>();
builder.Services.AddMessaging(Assembly.GetExecutingAssembly());

builder.Services
    .AddGraphQLServer()
    .AddCorretora();

var app = builder.Build();

app.MapGraphQL();

app.Run();

public partial class Program;
