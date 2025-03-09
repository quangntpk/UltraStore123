using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UltraStrore.Data;
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


            /*builder.Services.AddScoped<INguoiDungServices, NguoiDungServices>();*/

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

            app.UseAuthorization();

            app.MapControllers();

            app.Run("http://0.0.0.0:5261");
        }
    }
}