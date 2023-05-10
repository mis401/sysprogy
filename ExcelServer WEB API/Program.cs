

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
//builder.Services.AddSingleton<Microsoft.Extensions.Caching.Memory.MemoryCache>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(p =>
{
    p.AddPolicy("AllowAngularOrigins", builder =>
    {
        builder.AllowAnyOrigin()
                    // {
                    //     "http://localhost:4200",
                    //     "http://127.0.0.1:4200",
                    //     "https://localhost:4200",
                    //     "https://127.0.0.1:4200",
                    //     "http://localhost:5555",
                    //     "http://127.0.0.1:5555",
                    //     "http://localhost:8080",
                    //     "http://127.0.0.1:8080",
                    //     "https://localhost:5555",
                    //     "https://127.0.0.1:5555",
                    //     "https://localhost:8080",
                    //     "https://127.0.0.1:8080",
                    //     "http://localhost:5050",
                    //     "http://127.0.0.1:5050",
                    //     "https://localhost:5050",
                    //     "https://127.0.0.1:5050",
                    // })
                .AllowAnyHeader()
                .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseCors("AllowAngularOrigins");

app.Run();
