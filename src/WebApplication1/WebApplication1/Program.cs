using Azure.Core;
using Azure.Messaging.EventGrid.SystemEvents;
using Azure.Messaging.EventGrid;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Azure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<StorageService>();
builder.Services.AddScoped<EventGridService>();
builder.Services.AddScoped<EventHubService>();
builder.Services.AddHostedService<DevService>();
builder.Services.AddSingleton<QueueService>();
builder.Services.AddSingleton<ServiceBusService>();
builder.Services.AddHostedService<ServiceBusHostedService>();

builder.Logging.AddConsole();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseSwagger();
app.UseSwaggerUI();
//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", ([FromServices] ILogger<EventGridService> logger) =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    logger.LogInformation("test");
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/get401", () =>
{
    return Results.Unauthorized();
})
.WithName("get401")
.WithOpenApi();

app.MapGet("/get500", () =>
{
    return Results.StatusCode(500);
})
.WithName("get500")
.WithOpenApi();

app.MapGet("/getException", () =>
{
    var i = 0;
    return Results.Ok((1 / i).ToString());
})
.WithName("getException")
.WithOpenApi();

app.MapPost("/upload", async (IFormFile file, StorageService storageService) =>
{
    try
    {
        var _uploadsDirectory = "uploads";

        // Get a file path to save to
        var filePath = Path.Combine(_uploadsDirectory, file.FileName);

        // Save the file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        if (file.FileName.EndsWith("html"))
        {
            await storageService.UploadFile(file.FileName);
        }

        

        return Results.Ok(new { Message = "OK" });
    }
    catch (Exception ex)
    {
        // Handle any errors
        return Results.BadRequest(ex.Message);
    }
})
.Produces(200)
.DisableAntiforgery();


app.MapPost("/receiveEvent", async (HttpRequest request, EventGridService eventGridService,[FromServices] ILogger<EventGridService> logger) =>
{
    using  var requestStream = new StreamReader(request.Body) ;

    var bodyJson = await requestStream.ReadToEndAsync();
    logger.LogInformation(bodyJson);
    var events = JsonSerializer.Deserialize<List<EventGridEvent>>(bodyJson);

    if (request.Headers["aeg-event-type"].FirstOrDefault() ==
              "SubscriptionValidation")
    {
        var subValidationEventData = events.First().Data.ToObjectFromJson<SubscriptionValidationEventData>();
        return Results.Ok(new
        {
            ValidationResponse = subValidationEventData.ValidationCode
        });
    }
    else if (request.Headers["aeg-event-type"].FirstOrDefault() ==
           "Notification")
    {
        var notificationEvent = events.First();
        var data = notificationEvent.Data.ToObjectFromJson<DevMessage>();
        logger.LogInformation(notificationEvent.Subject+data.Message);
        return Results.Ok();
    }

    return Results.Ok(new { Message = "NA" });
})
.Produces(200)
.WithOpenApi();

app.MapGet("/sentEventGrid",async (EventGridService eventGridService) =>
{
    await eventGridService.Publish();
    return Results.Ok(new { Message = "OK" });
})
.WithName("sentEvent")
.WithOpenApi();

app.MapGet("/sentEventHub", async (EventHubService eventHubService) =>
{
    await eventHubService.SendToRandomPartition();
    return Results.Ok(new { Message = "OK" });
})
.WithName("sentEventHub")
.WithOpenApi();

app.MapGet("/sentEventHub/{key}", async (string key,EventHubService eventHubService) =>
{
    await eventHubService.SendToSamePartition(key);
    return Results.Ok(new { Message = "OK" });
})
.WithName("sentEventHubByKey")
.WithOpenApi();

app.MapGet("/EventHubPartition", async (EventHubService eventHubService) =>
{
    var list = new List<string>();
    await foreach(var a in eventHubService.GetPartitionInfo())
    {
        list.Add(a);
    }
    return Results.Ok(list);
})
.WithName("EventHubPartition")
.WithOpenApi();

app.MapGet("/EventHub/{id}", async (string id,EventHubService eventHubService) =>
{
    var list = await eventHubService.ReadFromPartition(id);
    return Results.Ok(list);
})
.WithName("EventHubById")
.WithOpenApi();


app.MapGet("/SendQueue", async ( QueueService queueService) =>
{
    await queueService.SendMessageAsync("Now:" + DateTime.Now);
    return Results.Ok();
})
.WithName("SendQueue")
.WithOpenApi();

app.MapGet("/SendServiceBus", async (ServiceBusService serviceBusService) =>
{
    await serviceBusService.SendMessageAsync("Now:" + DateTime.Now);
    return Results.Ok();
})
.WithName("SendServiceBus")
.WithOpenApi();

app.MapGet("/SendServiceBusTopic", async (ServiceBusService serviceBusService) =>
{
    await serviceBusService.SendTopicMessageAsync("Now:" + DateTime.Now);
    return Results.Ok();
})
.WithName("SendServiceBusTopic")
.WithOpenApi();

app.MapRazorPages();

app.Run();


internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

internal record DevMessage(string Message);