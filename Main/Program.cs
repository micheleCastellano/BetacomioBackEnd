using Main.Authentication;
using Main.Data;
using Main.EmailSender;
using Main.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using NLog;
using NLog.Web;
using NuGet.Protocol.Core.Types;
using System.Text.Json.Serialization;
using Main.Controllers;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();


try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Services.AddControllers().AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    });
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Host.UseNLog();

    builder.Services.AddDbContext<AdventureWorksLt2019Context>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("AdventureWorksLt2019")));
    builder.Services.AddDbContext<BetacomioContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("Betacomio")));


    builder.Services.AddTransient<ICredentialRepository, CredentialRepository>();
    builder.Services.AddTransient<IEmailSender, EmailSender>();


    builder.Services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", opt => { });
    // NEW AUTHENTICATION SCHEME HERE TO CHECK IF USER HAS ADMIN ACCESS
    builder.Services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, AdminAuthenticationHandler>("AdminAuthentication", opt => { });

    //builder.Services.AddAuthorization(opt =>
    //               opt.AddPolicy("BasicAuthentication", new AuthorizationPolicyBuilder("BasicAuthentication").RequireAuthenticatedUser().Build()));
    // CAN REPLACE CODE ABOVE WITH CODE BELOW
    // TO AUTHORIZE CONTROLLERS WE ADD THIS ATTRIBUTE: es. [Authorize(Policy = "Customer")]
    builder.Services.AddAuthorization(opt =>
    {
        opt.AddPolicy("Customer", policy =>
        {
            policy.AuthenticationSchemes.Add("BasicAuthentication");
            policy.RequireAuthenticatedUser();
        });
    });
    builder.Services.AddAuthorization(opt =>
    {
        opt.AddPolicy("Admin", policy =>
        {
            policy.AuthenticationSchemes.Add("AdminAuthentication");
            policy.RequireAuthenticatedUser();
        });
    });
    // END NEW

    builder.Services.AddCors(opt =>
    {
        opt.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
        });
    });
    builder.Services.AddControllers().AddJsonOptions(opt => opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseCors();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}catch (Exception ex)
{
    logger.Error(ex);
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}