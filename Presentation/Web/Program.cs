using Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Web.Controllers;
using WebSocketManager = Web.Controllers.WebSocketManager;

using Microsoft.Extensions.Configuration;
using Serilog.Events;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpLogging;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ��������� ����������� Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information) // �����������: ��������� verbosity Microsoft
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog(); // ���������� Serilog � ASP.NET Core

// Add services to the container.
builder.Services.AddControllersWithViews(); //  ���������� AddControllersWithViews, � �� AddControllers, �.�. � ��� ���� Views

// Dependency Injection
builder.Services.AddScoped<IMessageRepository>(provider =>
    new MessageRepository(connectionString, provider.GetRequiredService<ILogger<MessageRepository>>()));// Scoped, �.�. ����������� ��� ������� ������� ����
builder.Services.AddSingleton<IWebSocketManager, WebSocketManager>(); // Singleton, �.�. WebSocketManager ������ ���������

// Swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MessageService API", Version = "v1" });
});

// Enable WebSockets
builder.Services.AddWebSockets(options => {
    options.KeepAliveInterval = TimeSpan.FromSeconds(30); // ����������� KeepAlive
});

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Show detailed error pages in development
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MessageService API v1"));
}
else
{
    app.UseExceptionHandler("/Home/Error"); //  � production �������������� �� �������� ������
    app.UseHsts(); //  � production �������� HSTS
}

app.UseHttpsRedirection(); //  ����������� ��� ��������� �� https
app.UseStaticFiles(); //  ����� ���� View ����� ������������ css and js
app.UseRouting();

app.UseHttpLogging();

app.UseWebSockets(); //  �������� middleware ��� WebSockets

app.UseAuthorization();

//  ����������� ��������
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Client1}/{id?}");

//  ����� ����������
app.Run();