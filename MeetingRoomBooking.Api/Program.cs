using MeetingRoomBooking.Application.Interfaces;
using MeetingRoomBooking.Application.Services;
using MeetingRoomBooking.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<IMeetingRoomRepository, InMemoryMeetingRoomRepository>();
builder.Services.AddScoped<IBookingRepository, InMemoryBookingRepository>();
builder.Services.AddScoped<IMeetingRoomService, MeetingRoomService>();
builder.Services.AddScoped<IBookingService, BookingService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
