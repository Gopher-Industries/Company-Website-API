using Authentication.Models;
using Authentication.Models.Settings;
using Authentication.Services;
using Database.Models;
using Database.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using Users.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IUsersAPI, MockUsersAPI>();
builder.Services.AddSingleton<Authentication.Services.IAuthenticationService, BCryptAuthenticationService>();
builder.Services.AddSingleton<IDatabaseService, FirestoreDatabase>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddMemoryCache(builder =>
{
    builder.SizeLimit = 50000000;
});

builder.Services.Configure<ApplicationHostSettings>(builder.Configuration.GetRequiredSection("Hosting:Dev"));
builder.Services.Configure<ApplicationSecurityKeys>(builder.Configuration.GetRequiredSection("Keys:Dev"));
builder.Services.Configure<FirestoreConfiguration>(builder.Configuration.GetRequiredSection("Keys:Dev"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{

    var RSAInstance = RSA.Create();
    RSAInstance.ImportFromPem(builder.Configuration["Keys:Dev:RefreshTokenPrivateKey"]);
    var rsaSecurityKey = new RsaSecurityKey(RSAInstance);

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Hosting:Dev:ExternalUrl"],
        ValidAudience = builder.Configuration["Hosting:Dev:ExternalUrl"],
        IssuerSigningKey = rsaSecurityKey
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Services.GetService<IDatabaseService>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
