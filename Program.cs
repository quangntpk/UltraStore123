using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using UltraStrore.Data;
using UltraStrore.Middleware;
using UltraStrore.Repository;
using UltraStrore.Services;
using UltraStrore.Utils;

namespace UltraStrore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
            builder.Services.AddScoped<IGeminiServices, GeminiServices>();
            builder.Services.AddScoped<ICartServices, CartServices>();
            builder.Services.AddScoped<ISanPhamServices, SanPhamServices>();
            builder.Services.AddScoped<INguoiDungServices, NguoiDungServices>();
            builder.Services.AddScoped<IDanhSachDiaChiServices, DanhSachDiaChiServices>();
            builder.Services.AddScoped<ICommetServices, CommetServices>();
<<<<<<< Updated upstream
            builder.Services.AddScoped<IJwtTokenServices, JwtTokenGenerator>();
            builder.Services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();
            builder.Services.AddScoped<IEmailServices, EmailServices>();
            builder.Services.AddScoped<ILienHeServices, LienHeServices>();
            builder.Services.AddScoped<ITinNhanServices, TinNhanServices>();
            builder.Services.AddTransient<EmailService>();
=======
            builder.Services.AddScoped<IComboServices, ComboServices>();
>>>>>>> Stashed changes

            /*builder.Services.AddScoped<INguoiDungServices, NguoiDungServices>();*/

            builder.Services.AddMemoryCache();

            builder.Services.AddSingleton(resolver =>
                resolver.GetRequiredService<IOptions<GeminiSettings>>().Value);
            builder.Services.Configure<GeminiSettings>(
                builder.Configuration.GetSection("Authentication"));
            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            // Định nghĩa chính sách CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // Cấu hình JWT Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });

            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Nhập token của bạn vào đây (Bearer <your_token>)"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Thêm UseCors trước UseAuthorization và MapControllers
            app.UseCors("AllowAll");

            app.UseTokenBlacklist();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run("http://0.0.0.0:5261");
        }
    }
}