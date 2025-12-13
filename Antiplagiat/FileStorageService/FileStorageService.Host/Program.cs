using FileStorageService.Infrastructure;
using FileStorageService.Infrastructure.Data;
using FileStorageService.Presentation.Endpoints;
using FileStorageService.UseCases;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Swagger 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure();
builder.Services.AddUseCases();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FileStorageDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FileStorageService.Host v1");
});

// Эндпоинты для работ
app.MapWorksEndpoints();

app.Run();