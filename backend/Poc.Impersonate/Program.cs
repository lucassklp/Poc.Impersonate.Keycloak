using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Poc.Impersonate.Consumers;
using Rx.Http;
using Rx.Http.Extensions;
using Rx.Http.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<RxHttpClient>()
.ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler()
    {
        AllowAutoRedirect = false
    };
});

builder.Services.AddRxHttpLogger<RxHttpDefaultLogger>();
builder.Services.AddScoped<KeycloakConsumer>();


var authenticationOptions = builder.Configuration
    .GetSection(KeycloakAuthenticationOptions.Section)
    .Get<KeycloakAuthenticationOptions>()!;

var authorizationOptions = builder.Configuration
    .GetSection(KeycloakProtectionClientOptions.Section)
    .Get<KeycloakProtectionClientOptions>()!;

builder.Services.AddKeycloakAuthentication(authenticationOptions, options => options.RequireHttpsMetadata = false);
builder.Services.AddKeycloakAuthorization(authorizationOptions);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
