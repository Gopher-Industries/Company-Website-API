using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProjectX.WebAPI.Debug;
using ProjectX.WebAPI.Models;
using ProjectX.WebAPI.Services;
using Swashbuckle.AspNetCore.Filters;
using System.Collections;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Configuration.AddEnvironmentVariables();

if (builder.Environment.IsDevelopment() is true)
    builder.Configuration.AddUserSecrets<Program>(true, true);
else
    builder.Configuration.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("ProjectXAPIConfiguration"))));
builder.Configuration.AddJsonFile("appsettings.json");
builder.Services.Configure<ApplicationHostSettings>(builder.Configuration.GetSection("ApplicationHosting"));
builder.Services.Configure<ApplicationIdentitySettings>(builder.Configuration.GetSection("ApplicationIdentity"));
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<IEmailService, GmailService>();
builder.Services.AddSingleton<IAuthenticationService, BCryptAuthenticationService>();
builder.Services.AddSingleton<IDialogFlowService, DialogFlowService>();
builder.Services.AddSingleton<IDatabaseService, FirestoreDatabase>();
builder.Services.AddSingleton<ITimelineService, TimelineService>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddMemoryCache(builder =>
{
    builder.SizeLimit = 50000000;
});
builder.Services.AddSwaggerGen(options =>
{

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ProjectX API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Bearer",
        BearerFormat = "JWT",
        Scheme = "bearer",
        Description = "Add the access token to be identified as a user.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
    {
        new OpenApiSecurityScheme
        {
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
        },
        new string[] { }
    }
    });

    options.EnableAnnotations();
    options.ExampleFilters();

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    //options.DocumentFilter<>

});
//builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();
builder.Services.AddSwaggerExamples();
//
// Add JWT authentication bearer schema

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["ApplicationHosting:ExternalUrl"],
        ValidAudience = builder.Configuration["ApplicationHosting:ExternalUrl"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                builder.Configuration["ApplicationIdentity:AccessJWTSecret"]
        ))
    };
});

//var Cert = new X509Certificate2(builder.Configuration["SSL:CertificatePath"], builder.Configuration["SSL:CertificatePassword"], X509KeyStorageFlags.MachineKeySet);

//var pub = Cert.GetPublicKeyString();
//var priv = Cert.PrivateKey.ToXmlString(false);

// If we're not in development mode, startup the kesteral server and use our certificates!
if (builder.Environment.IsDevelopment() is false)
{

    // Remove default URL's
    builder.WebHost.UseUrls("http://0.0.0.0:80");

    //builder.WebHost.UseKestrel(serverOptions =>
    //{
    //    serverOptions.Listen(System.Net.IPAddress.Parse("0.0.0.0"), 443, listenOptions =>
    //    {

    //        listenOptions.UseHttps(
    //            builder.Configuration["SSL:CertificatePath"],
    //            builder.Configuration["SSL:CertificatePassword"]
    //        );
    //    });
    //});
}

var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.Services.GetRequiredService<IOptions<ApplicationHostSettings>>().Value.HostingUrl = Environment.GetEnvironmentVariable("ASPNETCORE_URLS")?.Split(';').FirstOrDefault();
//}

app.MapSwagger();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
