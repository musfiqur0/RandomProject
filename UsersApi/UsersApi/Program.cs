using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using UsersApi.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, lc) => lc
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .ReadFrom.Configuration(builder.Configuration));
try
{

    // Add services to the container.
    // EF Core + SQLite
    builder.Services.AddDbContext<UsersDbContext>(opt =>
        opt.UseSqlite(builder.Configuration.GetConnectionString("UsersDb")));

    builder.Services.AddControllers();
    builder.Services.AddMemoryCache();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Data"));
        // Ensure the database and schema are created. Since this project does not use migrations,
        // calling EnsureCreatedAsync() will create the database and tables if they don't exist.
        await db.Database.EnsureCreatedAsync();
    }

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Faild to start applicsation.");
}
finally
{
    Log.CloseAndFlush();
}