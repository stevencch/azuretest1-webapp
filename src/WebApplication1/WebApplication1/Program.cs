var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
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

app.MapPost("/upload", async (IFormFile file) =>
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


app.MapRazorPages();

app.Run();


internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}