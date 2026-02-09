using Microsoft.AspNetCore.Server.Kestrel.Core; // <--- 1. IMPORTANTE: Agrega este namespace

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<Services.ApkBuilderService>(); // Registro del servicio ApkBuilderService
builder.Services.AddScoped<CapaDapper.DataService.IDbMetadataRepository, CapaDapper.DataService.DbMetadataRepository>(); // Registro del repositorio Dapper
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

// =========================================================================
// SOLUCIÓN 1: CONFIGURACIÓN DE TIEMPO DE ESPERA (TIMEOUT)
// Esto evita que la conexión se corte mientras Flutter compila (la primera vez tarda mucho)
// =========================================================================
builder.Services.Configure<KestrelServerOptions>(options =>
{
	// Le damos hasta 10 minutos al servidor para mantener la conexión viva
	// Esto es suficiente para que Azure despierte, Gradle arranque y Flutter compile.
	options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
	options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(10);
});
// =========================================================================

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
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
