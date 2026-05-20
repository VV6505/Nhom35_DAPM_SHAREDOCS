using Microsoft.EntityFrameworkCore;
using HeThong_User.Models;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Thay dòng cũ bằng đoạn này
builder.Services.AddControllersWithViews()
    .ConfigureApplicationPartManager(manager =>
    {
        // Tìm và loại bỏ các Controller "ké" từ project HeThong_User
        var userPart = manager.ApplicationParts
            .FirstOrDefault(p => p.Name == "HeThong_User");
        if (userPart != null)
        {
            manager.ApplicationParts.Remove(userPart);
        }
    });

builder.Services.AddScoped<HeThong_Admin.Services.AzureBlobService>();

builder.Services.AddDbContext<HeThongChiaSeTaiLieu_V1>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".HeThongAdmin.Session"; // Tên riêng biệt để không bị trùng
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
