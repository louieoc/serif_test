using Scalar.AspNetCore;
using SearchCore.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register CmsRatesService as a *typed client*. This is the recommended way to
// use IHttpClientFactory: the factory manages the HttpClient lifecycle
// (connection pooling, DNS refresh) and injects a configured HttpClient into
// the service's constructor. Calling AddHttpClient also registers
// IHttpClientFactory itself, so it can be injected directly into controllers.
builder.Services.AddHttpClient<ICmsRatesService, CmsRatesService>(client =>
{
    // Centene's CDN rejects requests without a User-Agent (403).
    client.DefaultRequestHeaders.UserAgent.ParseAdd("SearchApi/1.0");
    client.Timeout = TimeSpan.FromMinutes(5); // CMS files can be very large
});

builder.Services.AddScoped<ICmsTocService, LocalCmsTocFileService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // interactive API UI at /scalar/v1
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
