using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using Models;
using Repositories;
using NLog;
using NLog.Web;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings()
.GetCurrentClassLogger();
logger.Debug("init main");

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Host.UseNLog();

// BsonSeralizer... fortæller at hver gang den ser en Guid i alle entiteter skal den serializeres til en string. 
BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

// Fetch secrets from Vault. Jeg initierer vaultService og bruger metoden derinde GetSecretAsync
var vaultService = new VaultRepository(logger, builder.Configuration);
var mySecret = await vaultService.GetSecretAsync("Secret");
var myIssuer = await vaultService.GetSecretAsync("Issuer");
// logger.Info($"Secret: {mySecret} and Issuer: {myIssuer}");
if (mySecret == null || myIssuer == null)
{
    Console.WriteLine("Failed to retrieve secrets from Vault");
    throw new ApplicationException("Failed to retrieve secrets from Vault");
}
builder.Services
.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = myIssuer,
        ValidAudience = "http://localhost",
        IssuerSigningKey =
    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(mySecret))
    };
});
// Tilføjer authorization politikker som bliver brugt i controlleren, virker ik
builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
    });
// Add services to the container.

//tilføjer Repository til services.
builder.Services.AddSingleton<IVaultRepository>(vaultService);

var ConnectionAuctionDB = await vaultService.GetSecretAsync("ConnectionAuctionDB");
builder.Services.Configure<MongoDBSettings>(options =>
{
    options.ConnectionAuctionDB = ConnectionAuctionDB ?? throw new ArgumentNullException("ConnectionDB environment variable not set");
});

//tilføjer Repository til services
builder.Services.AddTransient<IMongoDBRepository, MongoDBUserRepository>();



// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
