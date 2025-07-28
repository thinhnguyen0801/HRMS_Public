using Serilog.Sinks.MSSqlServer;
using Serilog;
using Quartz;
using HNOne.DailyJob.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// AddCors
builder.Services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
// Add services
builder.Services.InstallerExtensionsInAssembly(builder.Configuration);
var logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer(
                connectionString: builder.Configuration.GetConnectionString("DbConnection"),
                sinkOptions: new MSSqlServerSinkOptions()
                {
                    AutoCreateSqlDatabase = false,
                    AutoCreateSqlTable = false,
                    TableName = "IssueLogs",
                },
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error
                ).CreateLogger();
builder.Logging.AddSerilog(logger);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();

app.Run();
