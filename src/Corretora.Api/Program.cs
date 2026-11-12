using System.Reflection;
using Corretora.Api.Common.Data;
using Corretora.Api.Common.Decorators;
using Corretora.Api.Common.Messaging;
using Corretora.Api.Database;
using Corretora.Api.Database.Interceptors;
using Corretora.Api.Database.Repositories;
using Corretora.Api.Features.Contas;
using Corretora.Api.Features.Ordens;
using Corretora.Api.Features.Ordens.EnviarOrdem;
using Corretora.Api.Features.Usuarios;
using Corretora.Api.Features.Usuarios.CriarUsuario;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

[assembly: Module("Corretora")]

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CorretoraDbContext>(options =>
{
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("Corretora"))
        .UseSnakeCaseNamingConvention();

    if (builder.Configuration.GetValue<bool>("Aula:FalharAoGravarOrdem"))
        options.AddInterceptors(new FalhaEntreGravacoesInterceptor());
});
builder.Services.AddScoped<IUnitOfWork>(services => services.GetRequiredService<CorretoraDbContext>());
builder.Services.AddScoped<IContaRepository, ContaRepository>();
builder.Services.AddScoped<IOrdemRepository, OrdemRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IBanco, BancoPostgres>();
builder.Services.AddMessaging(Assembly.GetExecutingAssembly());
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// O embrulho escrito à mão: quem pede o handler do command recebe o decorator com o handler dentro.
// Vem depois do AddMessaging porque, para a mesma interface, o último registro vence.
builder.Services.AddScoped<ICommandHandler<EnviarOrdemCommand, EnviarOrdemOutput>>(services =>
    new ValidationDecorator.CommandHandler<EnviarOrdemCommand, EnviarOrdemOutput>(
        services.GetRequiredService<EnviarOrdemHandler>(),
        services.GetServices<IValidator<EnviarOrdemCommand>>()));
builder.Services.AddScoped<ICommandHandler<CriarUsuarioCommand, CriarUsuarioOutput>>(services =>
    new ValidationDecorator.CommandHandler<CriarUsuarioCommand, CriarUsuarioOutput>(
        services.GetRequiredService<CriarUsuarioHandler>(),
        services.GetServices<IValidator<CriarUsuarioCommand>>()));

builder.Services
    .AddGraphQLServer()
    .AddCorretora()
    .AddMutationConventions(applyToAllMutations: true)
    // Cada campo da mutation ganha o seu escopo e o seu DbContext.
    .ModifyOptions(options => options.DefaultMutationDependencyInjectionScope = DependencyInjectionScope.Resolver);

var app = builder.Build();

app.MapGraphQL();

app.Run();

public partial class Program;
