using Microsoft.EntityFrameworkCore;
using PaymentsService.Persistence;
using PaymentsService.Messaging;
using PaymentsService.Outbox;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PaymentsDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("PaymentsDb");
    opt.UseNpgsql(cs);
});
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddHostedService<PaymentRequestedConsumer>();
builder.Services.AddHostedService<OutboxPublisher>();
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
    db.Database.Migrate();
}
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();