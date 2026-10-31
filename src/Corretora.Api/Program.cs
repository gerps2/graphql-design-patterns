using System.Reflection;
using System.Text.Json.Serialization;
using Corretora.Api.Common;
using Corretora.Api.Database;

[assembly: Module("Corretora")]

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddSingleton<IBanco, BancoEmMemoria>();
builder.Services.AddQueries(Assembly.GetExecutingAssembly());

builder.Services
    .AddGraphQLServer()
    .AddApplicationService<IBanco>()
    .AddCorretora();

var app = builder.Build();

app.MapEndpoints(Assembly.GetExecutingAssembly());
app.MapGraphQL();

app.Run();

public partial class Program;
