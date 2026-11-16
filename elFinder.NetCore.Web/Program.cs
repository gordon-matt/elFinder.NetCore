using elFinder.NetCore.Web;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(); // Register MVC services

// Set upload size limit to maximum value. Change this if you want to limit the upload size.
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = int.MaxValue);
builder.Services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = int.MaxValue);

var app = builder.Build();

var ffmpegLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("AppLogger");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

PathHelper.WebRootPath = app.Environment.WebRootPath;

app.Run();