using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Services;
using SchoolProject.Controllers;
using SchoolProject.Services.Education;
using SchoolProject.Services.Education.Resolvers;
using SchoolProject.Services.Search.Interfaces;
using SchoolProject.Services.Search.Implementations;

var builder = WebApplication.CreateBuilder(args);

// ===== CONFIGURATION =====
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables();

// ===== SERVICES =====
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IListingService, ListingService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<ISchoolSearchService, SchoolSearchService>();
builder.Services.AddScoped<ICollegeSearchService, CollegeSearchService>();
builder.Services.AddScoped<ISpecializationSearchService, SpecializationSearchService>();
builder.Services.AddScoped<ICourseSearchService, CourseSearchService>();
builder.Services.AddScoped<ICourseAliasResolver, CourseAliasResolver>();
builder.Services.AddScoped<ISearchIntentParser, SearchIntentParser>();
builder.Services.AddScoped<IQueryNormalizer, QueryNormalizer>();
builder.Services.AddScoped<ICourseResolver, CourseResolver>();
builder.Services.AddScoped<ICityResolver, CityResolver>();
builder.Services.AddScoped<IListingCourseResolver, ListingCourseResolver>();
builder.Services.AddScoped<IDisplayNameService, DisplayNameService>();
builder.Services.AddSingleton<CollegeListingUrlBuilder>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();



builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<NavService>();
builder.Services.AddScoped<SidebarService>();
builder.Services.AddScoped<ContentService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ReCaptchaService>();
builder.Services.AddScoped<SchoolSearchController>();

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("EditorOrAdmin", policy => policy.RequireRole("Admin", "Editor"));
});

// Elasticsearch 9.x client
var esSettings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
    .DefaultIndex("be_search_v1");

builder.Services.AddSingleton(new ElasticsearchClient(esSettings));

// ===== BUILD APP =====
var app = builder.Build();

// ===== MIDDLEWARE PIPELINE =====
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ===== SEO ROUTES =====
app.MapControllerRoute(
    name: "coaching-list",
    pattern: "{course}-coaching-in-{city}",
    defaults: new
    {
        controller = "College",
        action = "CoachingList"
    });

app.MapControllerRoute(
    name: "college-list",
    pattern: "{course}-colleges-in-{city}",
    defaults: new { controller = "College", action = "List" });

app.MapControllerRoute(
    name: "college-detail",
    pattern: "college/{slug}",
    defaults: new { controller = "College", action = "Details" });

app.MapControllerRoute(
    name: "school-list",
    pattern: "{board}-schools-in-{city}",
    defaults: new { controller = "School", action = "List" });

app.MapControllerRoute(
    name: "school-detail",
    pattern: "school/{slug}",
    defaults: new { controller = "School", action = "Details" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "study-abroad",
    pattern: "study-abroad/{countrySlug?}/{instituteSlug?}",
    defaults: new { controller = "StudyAbroad", action = "Index" });

// ===== Reindex code for ElasticSearch =====
using (var scope = app.Services.CreateScope())
{
    var searchController = scope.ServiceProvider.GetRequiredService<SchoolSearchController>();
    await searchController.ReindexAll();
}

app.Run();