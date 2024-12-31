using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using PortalDoFranqueadoAPI.Middleware;
using System;
using Microsoft.Data.SqlClient;
using System.Text;
using PortalDoFranqueadoAPI.Repositories;
using PortalDoFranqueadoAPI.Repositories.Interfaces;
using PortalDoFranqueadoAPI.Models.Validations.Interfaces;
using PortalDoFranqueadoAPI.Models.Validations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors();
builder.Services.AddControllers();
//builder.Services.AddTransient<MySqlConnection>(_ => new MySqlConnection(builder.Configuration["ConnectionStrings:Default"]));

#if DEBUG
var sqlConnection = builder.Configuration.GetConnectionString("Default");
var secretToken = builder.Configuration["AppSettings:SecretToken"];
#else
var sqlConnection = Environment.GetEnvironmentVariable("SQLAZURECONNSTR_Default");
var secretToken = Environment.GetEnvironmentVariable("APPSETTING_SecretToken");
#endif

if (string.IsNullOrEmpty(sqlConnection))
    throw new InvalidOperationException("Não foi possível obter a string de conexão do banco de dados.");

if (string.IsNullOrEmpty(secretToken))
    throw new InvalidOperationException("Não foi possível obter o token de segurança.");

//Repositories
builder.Services.AddTransient(_ => new SqlConnection(sqlConnection));
builder.Services.AddScoped<IAuxiliaryRepository, AuxiliaryRepository>();
builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
builder.Services.AddScoped<ICollectionRepository, CollectionRepository>();
builder.Services.AddScoped<IFamilyRepository, FamilyRepository>();
builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<IInformativeRepository, InformativeRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
builder.Services.AddScoped<IPurchaseSuggestionRepository, PurchaseSuggestionRepository>();
builder.Services.AddScoped<IStoreRepository, StoreRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

//Validations
builder.Services.AddScoped<IPurchaseSuggestionValidation, PurchaseSuggestionValidation>();
builder.Services.AddScoped<IPurchaseValidation, PurchaseValidation>();

var key = Encoding.ASCII.GetBytes(secretToken);
builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.Run();