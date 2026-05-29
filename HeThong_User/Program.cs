using Microsoft.EntityFrameworkCore;
using HeThong_User.Models;
var builder = WebApplication.CreateBuilder(args);

// Giảm log output trong terminal
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Warning); // Chỉ hiển thị Warning và Error

// Add services to the container.
builder.Services.AddControllersWithViews();

// Cấu hình giới hạn kích thước upload file (60MB để hỗ trợ file tối đa 50MB của người dùng)
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 62914560; // 60 MB
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 62914560; // 60 MB
});
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 62914560; // 60 MB
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<HeThong_User.Services.AzureBlobService>();
builder.Services.AddScoped<HeThong_User.Services.NLPService>();
builder.Services.AddDbContext<HeThongChiaSeTaiLieu_V1>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".HeThongUser.Session"; // Tên riêng biệt cho phía User
});

var app = builder.Build();

// Test database connection
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<HeThongChiaSeTaiLieu_V1>();
    
    try
    {
        var canConnect = await context.Database.CanConnectAsync();
        Console.WriteLine(canConnect ? "✅ Database: Kết nối thành công" : "❌ Database: Không thể kết nối");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database: {ex.InnerException?.Message ?? ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Tạm thời comment dòng này để test với HTTP
// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
