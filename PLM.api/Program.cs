using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NsfwSpyNS;
using PLM.api.Config;
using PLM.api.Data;
using PLM.api.Repositories;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(Options =>

{

    Options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "PLM api", Version = "v1" });

    Options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new Microsoft.OpenApi.Models.OpenApiSecurityScheme

    {

        Name = "Authorization",

        In = ParameterLocation.Header,

        Type = SecuritySchemeType.ApiKey,

        Scheme = JwtBearerDefaults.AuthenticationScheme

    });

    Options.AddSecurityRequirement(new OpenApiSecurityRequirement{

    {

        new OpenApiSecurityScheme

        {

            Reference=new OpenApiReference

            {

                Type= ReferenceType.SecurityScheme,

                Id= JwtBearerDefaults.AuthenticationScheme

            },

            Scheme="Oauth2",

            Name=JwtBearerDefaults.AuthenticationScheme,

            In=ParameterLocation.Header

        },

        new List<string>()

    }

}); ;

});
builder.Services.AddDbContext<PLMDbContext>(options =>

    options.UseSqlServer(builder.Configuration.GetConnectionString("PLMConnectionString")));

builder.Services.AddDbContext<PLMAuthDbContext>(options =>

    options.UseSqlServer(builder.Configuration.GetConnectionString("PLMAuthConnectionString")));

builder.Services.AddIdentityCore<IdentityUser>()

    .AddRoles<IdentityRole>()

    .AddTokenProvider<DataProtectorTokenProvider<IdentityUser>>("PLM")

    .AddEntityFrameworkStores<PLMAuthDbContext>()

    .AddDefaultTokenProviders();

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
builder.Services.Configure<IdentityOptions>(Options =>

{

    Options.Password.RequireDigit = false;

    Options.Password.RequireLowercase = false;

    Options.Password.RequireUppercase = false;

    Options.Password.RequireNonAlphanumeric = false;

    Options.Password.RequiredLength = 3;

    Options.Password.RequiredUniqueChars = 1;



});
builder.Services.AddAuthentication(options =>

{

    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

})

.AddJwtBearer(options =>

{

    options.TokenValidationParameters = new TokenValidationParameters

    {

        ValidateIssuer = true,

        ValidateAudience = true,

        ValidateLifetime = true,

        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings.Issuer,

        ValidAudience = jwtSettings.Audience,

        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))

    };

});
builder.Services.AddSingleton<INsfwSpy, NsfwSpy>();
builder.Services.AddTransient<ICustomEmailSender, CustomEmailSender>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();


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
