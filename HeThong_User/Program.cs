using Microsoft.EntityFrameworkCore;
using HeThong_User.Models;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<HeThongChiaSeTaiLieu_V1>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

<<<<<<< Updated upstream
=======
// Test database connection với timeout
var testTask = Task.Run(async () =>
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var context = services.GetRequiredService<HeThongChiaSeTaiLieu_V1>();
        
        try
        {
            logger.LogInformation("========================================");
            logger.LogInformation("🔍 ĐANG KIỂM TRA KẾT NỐI DATABASE...");
            logger.LogInformation("========================================");
            
            // Test connection với timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var canConnect = await context.Database.CanConnectAsync(cts.Token);
            
            if (canConnect)
            {
                logger.LogInformation("✅ KẾT NỐI DATABASE THÀNH CÔNG!");
                
                // Lấy thông tin database
                var connectionString = context.Database.GetConnectionString();
                logger.LogInformation($"📊 Connection String: {connectionString}");
                
                // Đếm số tài khoản
                var soTaiKhoan = await context.TaiKhoans.CountAsync(cts.Token);
                logger.LogInformation($"👤 Số tài khoản trong database: {soTaiKhoan}");
                
                // Đếm số sinh viên
                var soSinhVien = await context.SinhViens.CountAsync(cts.Token);
                logger.LogInformation($"🎓 Số sinh viên: {soSinhVien}");
                
                // Lấy danh sách username
                var usernames = await context.TaiKhoans
                    .Select(t => t.TenTk)
                    .Take(5)
                    .ToListAsync(cts.Token);
                logger.LogInformation($"📝 Một số username có sẵn: {string.Join(", ", usernames)}");
                
                logger.LogInformation("========================================");
            }
            else
            {
                logger.LogError("❌ KHÔNG THỂ KẾT NỐI DATABASE!");
                logger.LogError("Vui lòng kiểm tra:");
                logger.LogError("1. SQL Server đang chạy");
                logger.LogError("2. Connection string trong appsettings.json");
                logger.LogError("3. Database đã được tạo");
                logger.LogInformation("========================================");
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogError("❌ TIMEOUT KHI KẾT NỐI DATABASE!");
            logger.LogError("Kết nối mất quá nhiều thời gian. Vui lòng kiểm tra:");
            logger.LogError("1. SQL Server có đang chạy không");
            logger.LogError("2. Firewall có chặn kết nối không");
            logger.LogError("3. Connection string có đúng không");
            logger.LogInformation("========================================");
        }
        catch (Exception ex)
        {
            logger.LogError("❌ LỖI KHI KẾT NỐI DATABASE!");
            logger.LogError($"Chi tiết lỗi: {ex.Message}");
            logger.LogError($"Inner Exception: {ex.InnerException?.Message}");
            
            // Hiển thị toàn bộ exception chain
            var innerEx = ex.InnerException;
            int level = 1;
            while (innerEx != null)
            {
                logger.LogError($"Inner Exception Level {level}: {innerEx.Message}");
                innerEx = innerEx.InnerException;
                level++;
            }
            
            logger.LogInformation("========================================");
        }
    }
});

// Chờ test hoàn thành hoặc timeout sau 15 giây
if (!testTask.Wait(TimeSpan.FromSeconds(15)))
{
    Console.WriteLine("⚠️ CẢNH BÁO: Kiểm tra database mất quá nhiều thời gian, bỏ qua và tiếp tục khởi động...");
}

>>>>>>> Stashed changes
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
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
