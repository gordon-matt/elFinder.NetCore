using elFinder.NetCore.Ffmpeg;
using elFinder.NetCore.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(); // Register MVC services

var app = builder.Build();

// Video thumbnails need ffmpeg and ffprobe. An official build is downloaded into ./ffmpeg
// only when neither is already installed. That folder is gitignored.
ILogger ffmpegLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("FFmpeg");
await FfmpegBootstrap.EnsureAsync(
    new FfmpegBootstrapOptions
    {
        PreferLinuxSystemDirectory = true,
        SearchWellKnownLocations = true,
        SearchApplicationDirectories = true,
        DownloadDirectory = Path.Combine(app.Environment.ContentRootPath, "ffmpeg"),
        DownloadIfMissing = true,
        RequireFfprobe = true
    },
    ffmpegLogger);

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