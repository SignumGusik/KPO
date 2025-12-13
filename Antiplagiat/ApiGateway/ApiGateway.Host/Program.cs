using ApiGateway.Infrastructure;
using ApiGateway.Presentation.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Refit-клиенты
builder.Services.AddInfrastructure();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapWorksEndpoints();
app.Run();