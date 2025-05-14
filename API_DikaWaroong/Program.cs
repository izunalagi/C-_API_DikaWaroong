var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutterWeb", policy =>
    {
        policy.WithOrigins("http://localhost:57307")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFlutterWeb");

app.UseHttpsRedirection();

// ✅ Tambahkan ini untuk melayani file statis dari folder wwwroot
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
