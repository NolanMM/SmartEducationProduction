using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartEducation.dbContext;
using SmartEducation.Entities;
using SmartEducation.Services;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.AppendTrailingSlash = true;
});
builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SmartEduDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SmartEduDb"),
    sqlServerOptions => {
        sqlServerOptions.CommandTimeout(120);
    }));

builder.Services.AddIdentity<User, IdentityRole>(options => {
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireDigit = true;
}).AddEntityFrameworkStores<SmartEduDbContext>().AddDefaultTokenProviders();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<ActivityRecommendationService>();
builder.Services.AddScoped<IGradeStandardService, GradeStandardService>();
builder.Services.AddScoped<IDetailedNGSSStandardService, DetailedNGSSStandardService>();

builder.Services.AddCors(options => {
    options.AddPolicy("AllowTaskClients", policy => {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseSwagger();
    app.UseSwaggerUI();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

await SeedDataAsync(app);

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
//using (var scope = scopeFactory.CreateScope())
//{
//    await SmartEduDbContext.SeedRolesAndAdminUser(scope.ServiceProvider);
//}

app.Run();

async Task SeedDataAsync(IHost app)
{
    var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
    using (var scope = scopeFactory.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            await SmartEduDbContext.SeedRolesAndAdminUser(services);

            var gradeStandardSeeder = services.GetRequiredService<IGradeStandardService>();
            await gradeStandardSeeder.SeedOrUpdateStandardsAsync("./Static_Data/ngss_grade_standards.json");

            var detailedStandardSeeder = services.GetRequiredService<IDetailedNGSSStandardService>();
            await detailedStandardSeeder.SeedOrUpdateStandardsAsync("./Static_Data/ngss_detailed_standards.json");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred during data seeding.");
        }
    }
}
