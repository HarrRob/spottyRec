using spottyRec.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services for Razor Pages and MVC Controllers
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddDistributedMemoryCache();
builder.Logging.AddConsole();



builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register HttpClient for dependency injection
builder.Services.AddHttpClient();

var app = builder.Build();
app.UseSession();
app.UseRouting();

// Enable endpoint routing for both controllers and Razor Pages
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // Enables controllers
    endpoints.MapRazorPages();  // Keeps Razor Pages working
});
app.UseStaticFiles();


app.Run();
