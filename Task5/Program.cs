using Task5.Audio;
using Task5.Audio.Interfaces;
using Task5.Covers;
using Task5.Covers.Interfaces;
using Task5.Generators;
using Task5.Generators.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy
            .WithOrigins("http://localhost:5173", "http://localhost:5174")
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});

builder.Services.AddScoped<ISongGenerator, SongGenerator>();
builder.Services.AddScoped<IAudioGenerator, WavAudioGenerator>();
builder.Services.AddScoped<ICoverGenerator, PngCoverGenerator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
