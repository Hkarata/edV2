using System.Text;
using System.Text.Json;
using eDereva.Api.Extensions;
using eDereva.Api.Registrars;
using eDereva.Application.Context;
using eDereva.Domain.DataProtection;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!)),
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = configuration["Jwt:Issuer"]!,
            ValidAudience = configuration["Jwt:Audience"]!,
            ValidateLifetime = true
        };
    });

builder.Services.AddScoped<IDatabaseContext, DatabaseContext>();

builder.Services.AddFastEndpoints();

builder.Logging.ClearProviders();

builder.Logging.AddJsonConsole(options =>
    options.JsonWriterOptions = new JsonWriterOptions
    {
        Indented = true
    });

builder.Logging.EnableRedaction();

builder.Services.AddRedaction(options =>
{
    options.SetRedactor<StarRedactor>(new DataClassificationSet(DataTaxonomy.SensitiveData));

    options.SetHmacRedactor(x =>
    {
        x.Key = Convert.ToBase64String(Encoding.UTF8.GetBytes("Fortheloveofgodloadandstorethissecurely"));
        x.KeyId = 69;
    }, new DataClassificationSet(DataTaxonomy.PiiData));
});


builder.Services.AddServices();

builder.Services.AddRepositories();

builder.Services.AddCors(options =>
    options.AddPolicy("CorsPolicy", corsPolicyBuilder =>
        corsPolicyBuilder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("X-New-Token")
    ));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddDocumentTransformer<ApiKeySecuritySchemeTransformer>();
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5282); // Listen on all interfaces
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "eDereva API";
        options.Theme = ScalarTheme.Mars;
        options.WithPreferredScheme("Bearer");
        options.WithApiKeyAuthentication(keyOptions => { keyOptions.Token = "Token"; });
        options.AddServer(new ScalarServer
        (
            "http://13.246.238.118:5282/",
            "Dev server"
        ));
        options.AddServer(new ScalarServer
        (
            "http://localhost:5282/",
            "Dev server"
        ));
    });
}

app.UseCors(corsPolicyBuilder =>
{
    corsPolicyBuilder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
});

app.UseHttpsRedirection();

app.UseFastEndpoints(options =>
{
    options.Endpoints.RoutePrefix = "api";
    options.Versioning.Prefix = "v";
    options.Versioning.DefaultVersion = 1;
    options.Versioning.PrependToRoute = true;
});

app.Run();