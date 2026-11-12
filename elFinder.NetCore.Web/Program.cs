using elFinder.NetCore.Web;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(); // Register MVC services
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = int.MaxValue; // Set to unlimited
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = int.MaxValue; // Set to unlimited
});

var app = builder.Build();

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
app.UseStaticFiles(new StaticFileOptions()
{
    ServeUnknownFileTypes = true // <<<<<<
});
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

PathHelper.WebRootPath = app.Environment.WebRootPath;

app.Run();