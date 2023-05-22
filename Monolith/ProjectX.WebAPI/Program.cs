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

//
// Setting up configuration
//

builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("appsettings.json", false);

// builder.Configuration.AddUserSecrets<Program>(true, true);

// if (builder.Configuration["ApplicationIdentity:AccessJWTSecret"] is null)
// {
//    if (Environment.GetEnvironmentVariable("ProjectXAPIConfiguration") is not null)
//        builder.Configuration.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("ProjectXAPIConfiguration"))));
//    else
//        throw new FileNotFoundException("No secrets configuration file found.");
// }

builder.Services.Configure<ApplicationHostSettings>(builder.Configuration.GetSection("ApplicationHosting"));
builder.Services.Configure<ApplicationIdentitySettings>(builder.Configuration.GetSection("ApplicationIdentity"));
builder.Services.Configure<ApplicationHashSettings>(builder.Configuration.GetSection("ApplicationHash"));
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<IEmailService, SendGridService>();
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

// If we're not in development mode, startup the kesteral server and use our certificates!
if (builder.Environment.IsDevelopment() is false)
{

    // Check if the port is injected as an environment varable $PORT

    if (builder.Configuration["PORT"] is not null)
        // Use the port given from the environment variable
        builder.WebHost.UseUrls($"http://0.0.0.0:{builder.Configuration["PORT"]}");
    else
        // Default port is 8000
        builder.WebHost.UseUrls("http://0.0.0.0:8080");

}


// CoreyBock: Adding CORS arguments
builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    // Allowed domain set as exactly the appengine access link
    builder.WithOrigins("https://sit-22t3-gopher-websit-a242043.ts.r.appspot.com");
}));

var app = builder.Build();

//
// Hotload api connections'
var DatabaseInitialize = Task.Run(() => app.Services.GetRequiredService<IDatabaseService>().Initialize(app.Configuration));
//app.Services.GetRequiredService<IDialogFlowService>();
app.Services.GetRequiredService<IDatabaseService>().Initialize(app.Configuration);

app.MapSwagger();
app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();
app.UseCors("corsapp");

app.MapControllers();

await DatabaseInitialize;

app.Run();
