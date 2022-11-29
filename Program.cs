using ADODB;
using Leadtools;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Smead.RecordsManagement.Imaging.Export;
using System.IO.Compression;
using TabFusionRMS.RepositoryVB;


RasterSupport.SetLicense(TabFusionRMS.WebCS.Properties.Resources.LEADTOOLSLICENSE, TabFusionRMS.WebCS.Properties.Resources.LeadToolsKey);
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews()
    .AddSessionStateTempDataProvider()
    .AddJsonOptions(op =>
    {
        op.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
        op.JsonSerializerOptions.PropertyNamingPolicy = null;
        op.JsonSerializerOptions.IncludeFields = true;
    });

// compresslevel
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;

});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.SmallestSize;
});
// compresslevel

builder.Services.AddDistributedMemoryCache();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repositories<>));
//builder.Services.AddSingleton<TabFusionRMS.Resource.Languages>();
//builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
//builder.Services.AddScoped<IHostEnvironment, IHostEnvironment>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// set static path
var env = app.Services.GetRequiredService<IWebHostEnvironment>();
AppDomain.CurrentDomain.SetData("ContentRootPath", env.ContentRootPath);
AppDomain.CurrentDomain.SetData("WebRootPath", env.WebRootPath);
// end static path

// compresslevel
app.UseResponseCompression();

// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//}

app.UseStaticFiles();
//app.UseCustomMiddleware();
app.UseDeveloperExceptionPage();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
   name: "default",
   pattern: "{controller=Login}/{action=Index}/{id?}");


//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllerRoute(
//    name: "dashboard/",
//    pattern: "/dashboard/",
//    defaults: new { controller = "Dashboard", action = "Index" });

//    endpoints.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Login}/{action=Index}/{id?}");
//});


app.Run();
