using System.Threading.Channels;
using CodeLab.ServerSentEvents.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

//here we create and register a channel for order streaming
var channel = Channel.CreateUnbounded<OrderPlacement>();
builder.Services.AddSingleton(channel); //register the channel itself
builder.Services.AddSingleton(channel.Reader); //register the reader
builder.Services.AddSingleton(channel.Writer); //register the writer

//Here we create a buffer to process our orders
builder.Services.AddSingleton<OrderEventBuffer>();
//here we create a hosted service to process our orders
builder.Services.AddHostedService<OrderProducerService>();

builder.Services.AddCors();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.MapOrdersEndpoints();

app.UseHttpsRedirection();

app.Run();