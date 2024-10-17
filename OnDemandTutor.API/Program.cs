using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OnDemandTutor.API;

using OnDemandTutor.Contract.Services.Interface;
using OnDemandTutor.Repositories.Context;

using OnDemandTutor.Contract.Repositories.Interface;
using OnDemandTutor.Contract.Repositories.IUOW;
using OnDemandTutor.Contract.Services.Interface;
using OnDemandTutor.Repositories.Context;


using OnDemandTutor.Repositories.UOW;

using OnDemandTutor.Services.Service;
using OnDemandTutor.Services.Service.AccountUltil;
using ServiceLayer.Services;

var builder = WebApplication.CreateBuilder(args);


// Cấu hình DbContext

// C?u h?nh DbContext v?i SQL Server  

builder.Services.AddDbContext<DatabaseContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("OnDemandTutor.API"));
});

// Add services to the container.
builder.Services.AddScoped<IVNPayService, VnPayService>();
builder.Services.AddScoped<ITutorRepository, TutorRepository>();
builder.Services.AddScoped<ITutorService, TutorService>();
builder.Services.AddScoped<IVNPayService, VnPayService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITutorRepository, TutorRepository>();
builder.Services.AddScoped<ITutorService, TutorService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
builder.Services.AddScoped<AccountUtils>();



// Cấu hình appsettings theo môi trường


builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITutorRepository, TutorRepository>();
builder.Services.AddScoped<ITutorService, TutorService>();

// C?u h?nh appsettings theo m?i tr??ng  

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();


// Cấu hình JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    // Thêm các sự kiện cho JWT
    x.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("OnAuthenticationFailed: " + context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("OnTokenValidated: " + context.SecurityToken);
            return Task.CompletedTask;
        }
    };
});
// Thêm dịch vụ xác thực  
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
});
//.AddGoogle(options =>
//{
//    options.ClientId = "YOUR_CLIENT_ID";
//    options.ClientSecret = "YOUR_CLIENT_SECRET";
//    options.Scope.Add("email");
//    options.Scope.Add("profile");
//    options.SaveTokens = true;
//});

// Cấu hình dịch vụ xác thực  
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
});
//.AddFacebook(options =>
//{
//    options.AppId = "YOUR_APP_ID"; // Thay thế bằng App ID mà bạn đã lấy từ Facebook  
//    options.AppSecret = "YOUR_APP_SECRET"; // Thay thế bằng App Secret mà bạn đã lấy từ Facebook  
//    options.Scope.Add("email");
//    options.SaveTokens = true;

//    // Thiết lập URL redirect  
//    options.CallbackPath = "/signin-facebook"; // Thêm đường dẫn callback  
//});

//add to swagger
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Đăng ký TokenService
builder.Services.AddScoped<TokenService>();

// Thêm các dịch vụ khác
builder.Services.AddControllers();

// C?u h?nh CORS  
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// Th?m c?c d?ch v? ?i?u khi?n  
builder.Services.AddControllers();

// C?u h?nh Swagger/OpenAPI  

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Th?m c?u h?nh t?y ch?nh n?u c?n  
builder.Services.AddConfig(builder.Configuration);

var app = builder.Build();

// Cấu hình pipeline HTTP request
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();  // Đảm bảo CORS nằm trước các middleware xác thực và ủy quyền
app.UseAuthentication();  // Sử dụng Authentication middleware
app.UseAuthorization();   // Sử dụng Authorization middleware
app.MapControllers();     // Ánh xạ các controller
app.Run();