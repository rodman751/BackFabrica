using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<Services.ApkBuilderService>();
builder.Services.AddScoped<CapaDapper.DataService.IDbMetadataRepository, CapaDapper.DataService.DbMetadataRepository>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

// Extend Kestrel timeouts to accommodate long-running Flutter build operations.
builder.Services.Configure<KestrelServerOptions>(options =>
{
	options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
	options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(10);
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
app.UseSession();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
