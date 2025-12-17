using CapaDapper.Cadena;
using CapaDapper.DataService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<Services.IAuthRepository, Services.AuthRepository>();
builder.Services.AddScoped<Services.IAuthService, Services.AuthService>();
builder.Services.AddScoped<IDbMetadataRepository, DbMetadataRepository>();

builder.Services.AddScoped<IProductosRepository, ProductosRepository>();
builder.Services.AddScoped<IEducacionRepository, EducacionRepository>();
builder.Services.AddScoped<ISaludRepository, SaludRepository>();


builder.Services.AddScoped<IDatabaseContext, DatabaseContext>();
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

//  fin services

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
