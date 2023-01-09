using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using WebDavFilesRepository.Server.Database;
using WebDavFilesRepository.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// отключаем CORS
builder.Services.AddCors(options =>
{
    // разрешить все
    options.AddPolicy("AllowAllPolicy",
        builder =>
        {
            builder
            .AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed(origin => true)
            .AllowAnyOrigin();
            //.AllowCredentials();
        });
    options.AddPolicy("AllowCredentialsPolicy", builder =>
    {
        builder.AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header",
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
     {
         {
               new OpenApiSecurityScheme
                 {
                     Reference = new OpenApiReference
                     {
                         Type = ReferenceType.SecurityScheme,
                         Id = "Bearer"
                     }
                 },
                 new string[] {}
         }
     });
});

// авторизация на основе jwt токена отключена намерено
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options => options.TokenValidationParameters =
//                Jwt.GetValidationParameters(builder.Configuration["Access:Issuer"],
//                                            builder.Configuration["Access:Audience"],
//                                            bool.Parse(builder.Configuration["Access:ValidateLifetime"]),
//                                            builder.Configuration["Access:Key"]));

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


// -------- настраиваем строку подключения к БД и WebDav
// в тестовой версии используем тестовую базу данных и тестовый webdav в debug и release
#if DEBUG
string connectionString = builder.Configuration.GetConnectionString("ConnectionString");
string webDAVconnectionString = "WebDAVServer:ServerLocal:";
#else
    string connectionString = builder.Configuration.GetConnectionString("ConnectionString");
    string webDAVconnectionString = "WebDAVServer:ServerLocal:";
#endif

// ------- подключаем сервисы
builder.Services.AddSingleton<IConnectionString>(new ConnectionStringService(connectionString));

builder.Services.AddDbContext<AppDbContext>();

builder.Services.AddSingleton<IWebDAVConnectionStringService>(
                            new WebDAVConnectionStringService(webDAVconnectionString));
builder.Services.AddTransient<IWebDAVService, WebDavService>();
builder.Services.AddTransient<IFoldersAccessesService, FoldersAccessesService>();
builder.Services.AddTransient<ILogsService, LogsService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "JWTAuthDemo v1"));
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseCors("AllowAllPolicy");
app.UseHttpsRedirection();
app.UseDefaultFiles();  // используется для адресации с / на index.html
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

//app.UseAuthentication();
app.UseRouting();
//app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();