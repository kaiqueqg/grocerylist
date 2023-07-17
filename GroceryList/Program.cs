using GroceryList.Data;
using GroceryList.Data.Caching;
using GroceryList.Data.Services;
using GroceryList.Data.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//LOGGER
builder.Host.UseSerilog((context, configuration) =>
{
	configuration.Enrich.FromLogContext()
	.Enrich.WithMachineName()
	.WriteTo.Console()
	.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(context.Configuration["ElasticConfiguration:Uri"]))
	{
		IndexFormat = $"{context.Configuration["ApplicationName"]}-logs-{context.HostingEnvironment.EnvironmentName?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}",
		AutoRegisterTemplate=true,
		NumberOfShards = 2,
		NumberOfReplicas = 1
	})
	.Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
	.ReadFrom.Configuration(context.Configuration);
});

//CACHING
string? redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
Console.WriteLine("Redis connection string: " + redisConnectionString);
string? redisInstanceName = Environment.GetEnvironmentVariable("REDIS_INSTANCE_NAME");
Console.WriteLine("Redis instance name: " + redisInstanceName);
builder.Services.AddScoped<ICachingService, CachingService>();
builder.Services.AddStackExchangeRedisCache(o =>
{
	o.InstanceName = redisInstanceName != null && redisInstanceName != "" ? redisInstanceName : builder.Configuration["RedisCache:InstanceName"];
  o.Configuration = redisConnectionString != null && redisConnectionString != "" ? redisConnectionString : builder.Configuration["RedisCache:Configuration"];
});

//DATABASE
builder.Services.AddDbContext<SqlServerContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSqlServerConnection"));
});

builder.Services.AddSingleton<MongoDbService>();

//UNIT OF WORK
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

//CORS
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAny", builder =>
	{
		builder
		.AllowAnyOrigin()
		.AllowAnyMethod()
		.AllowAnyHeader();
	});
});

//AUTHENTICATION
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = false,
			ValidateAudience = false,
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Authentication:Jwt:Key"]))
		};
	});

//BUILD
var app = builder.Build();

if(app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors("AllowAny");

app.Run();
