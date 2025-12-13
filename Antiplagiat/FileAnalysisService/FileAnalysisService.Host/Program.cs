using FileAnalysisService.Infrastructure;
using FileAnalysisService.Infrastructure.Data;
using FileAnalysisService.Presentation.Endpoints;
using FileAnalysisService.UseCases;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// TimeProvider
builder.Services.AddSingleton(TimeProvider.System);

// Swagger 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure();
builder.Services.AddUseCases();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FileAnalysisDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FileAnalysisService.Host v1");
});

app.MapAnalysisEndpoints();

app.Run();