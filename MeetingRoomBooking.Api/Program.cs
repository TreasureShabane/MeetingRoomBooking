using MeetingRoomBooking.Api.Middleware;
using MeetingRoomBooking.Application.Interfaces;
using MeetingRoomBooking.Application.Services;
using MeetingRoomBooking.Infrastructure.Data;
using MeetingRoomBooking.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var configuredDatabasePath = builder.Configuration["Database:FileName"]
    ?? Path.Combine("App_Data", "meeting-room-booking.db");
var databasePath = Path.IsPathRooted(configuredDatabasePath)
    ? configuredDatabasePath
    : Path.Combine(builder.Environment.ContentRootPath, configuredDatabasePath);
Directory.CreateDirectory(Path.GetDirectoryName(databasePath) ?? builder.Environment.ContentRootPath);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? [];

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddAuthorization();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddDbContext<BookingDbContext>(options =>
    options.UseSqlite($"Data Source={databasePath}"));

builder.Services.AddScoped<IMeetingRoomRepository, SqliteMeetingRoomRepository>();
builder.Services.AddScoped<IBookingRepository, SqliteBookingRepository>();
builder.Services.AddScoped<IMeetingRoomService, MeetingRoomService>();
builder.Services.AddScoped<IBookingService, BookingService>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.Run();

public partial class Program;
