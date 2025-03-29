
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using UltraStrore.Data;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using UltraStrore.Utils;
using BCrypt.Net;
using System.Security.Claims;
using System.Text;
namespace UltraStrore.Services
{
    public class NguoiDungServices : INguoiDungServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtTokenServices _jwtTokenGenerator;
        private readonly IEmailServices _emailService;
        private readonly IWebHostEnvironment _environment;

        public NguoiDungServices(ApplicationDbContext context, IJwtTokenServices jwtTokenGenerator, IEmailServices emailService, IWebHostEnvironment environment)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
            _emailService = emailService;
            _environment = environment;
        }

        private string GenerateMaNguoiDung(int? vaiTro)
        {
            string prefix = "ND";
            if (vaiTro.HasValue)
            {
                if (vaiTro.Value == 1)
                    prefix = "AD";
                else if (vaiTro.Value == 2)
                    prefix = "NV";
                else if (vaiTro.Value == 0)
                    prefix = "ND";
            }

            // Lấy người dùng có mã lớn nhất với tiền tố tương ứng
            var lastUser = _context.NguoiDungs
                .Where(u => u.MaNguoiDung != null && u.MaNguoiDung.StartsWith(prefix))
                .OrderByDescending(u => u.MaNguoiDung)
                .FirstOrDefault();

            int newNumber = 1;
            if (lastUser != null)
            {
                string numericPart = lastUser.MaNguoiDung.Substring(prefix.Length);
                if (int.TryParse(numericPart, out int lastNumber))
                {
                    newNumber = lastNumber + 1;
                }
            }
            return $"{prefix}{newNumber:D5}";
        }

        public async Task<List<NguoiDungView>> GetNguoiDungList(string? searchTerm)
        {
            var query = _context.NguoiDungs.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(u =>
                    (u.MaNguoiDung != null && u.MaNguoiDung.ToLower().Contains(searchTerm)) ||
                    (u.HoTen != null && u.HoTen.ToLower().Contains(searchTerm)) ||
                    (u.Sdt != null && u.Sdt.ToLower().Contains(searchTerm)) ||
                    (u.Cccd != null && u.Cccd.ToLower().Contains(searchTerm)) ||
                    (u.Email != null && u.Email.ToLower().Contains(searchTerm))
                );
            }
            var list = await query.ToListAsync();
            return list.Select(u => new NguoiDungView
            {
                MaNguoiDung = u.MaNguoiDung,
                HoTen = u.HoTen,
                NgaySinh = u.NgaySinh,
                Sdt = u.Sdt,
                Cccd = u.Cccd,
                Email = u.Email,
                TaiKhoan = u.TaiKhoan,
                DiaChi = u.DiaChi,
                MatKhau = u.MatKhau,
                VaiTro = u.VaiTro,
                TrangThai = u.TrangThai,
                HinhAnh = u.HinhAnh,
                NgayTao = u.NgayTao,
                MoTa = u.MoTa,
                CancelConunt = u.CancelConunt,
                LockoutEndDate = u.LockoutEndDate
            }).ToList();
        }

        public async Task<NguoiDungView> GetNguoiDungById(string id)
        {
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.MaNguoiDung == id);
            if (user == null)
                throw new Exception("Người dùng không tồn tại.");

            return new NguoiDungView
            {
                MaNguoiDung = user.MaNguoiDung,
                HoTen = user.HoTen,
                NgaySinh = user.NgaySinh,
                Sdt = user.Sdt,
                Cccd = user.Cccd,
                Email = user.Email,
                TaiKhoan = user.TaiKhoan,
                DiaChi = user.DiaChi,
                MatKhau = user.MatKhau,
                VaiTro = user.VaiTro,
                TrangThai = user.TrangThai,
                HinhAnh = user.HinhAnh,
                NgayTao = user.NgayTao,
                MoTa = user.MoTa,
                CancelConunt = user.CancelConunt,
                LockoutEndDate = user.LockoutEndDate
            };
        }

        public async Task<NguoiDung> GetNguoiDungByEmailAsync(string email)
        {
            return await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<NguoiDungView> CreateNguoiDung(NguoiDungCreate model)
        {
            // Nếu MaNguoiDung chưa được cung cấp, tự sinh mã dựa trên vai trò
            if (string.IsNullOrEmpty(model.MaNguoiDung))
            {
                model.MaNguoiDung = GenerateMaNguoiDung(model.VaiTro);
            }

            // Nếu mật khẩu được cung cấp, băm mật khẩu bằng PasswordHasher
            string hashedPassword = string.Empty;
            if (!string.IsNullOrEmpty(model.MatKhau))
            {
                hashedPassword = PasswordHasher.HashPassword(model.MatKhau);
            }

            var newUser = new NguoiDung
            {
                MaNguoiDung = model.MaNguoiDung,
                HoTen = model.HoTen,
                Email = model.Email,
                TaiKhoan = model.TaiKhoan,
                MatKhau = hashedPassword, // Lưu mật khẩu đã băm
                TrangThai = model.TrangThai,
                NgayTao = model.NgayTao ?? DateTime.Now,
                VaiTro = model.VaiTro
                // Các thuộc tính khác có thể được cập nhật thêm nếu cần (như Sdt, DiaChi, …)
            };

            _context.NguoiDungs.Add(newUser);
            await _context.SaveChangesAsync();

            return new NguoiDungView
            {
                MaNguoiDung = newUser.MaNguoiDung,
                HoTen = newUser.HoTen,
                NgaySinh = newUser.NgaySinh,
                Sdt = newUser.Sdt,
                Cccd = newUser.Cccd,
                Email = newUser.Email,
                TaiKhoan = newUser.TaiKhoan,
                DiaChi = newUser.DiaChi,
                MatKhau = newUser.MatKhau,
                VaiTro = newUser.VaiTro,
                TrangThai = newUser.TrangThai,
                HinhAnh = newUser.HinhAnh,
                NgayTao = newUser.NgayTao,
                MoTa = newUser.MoTa,
                CancelConunt = newUser.CancelConunt,
                LockoutEndDate = newUser.LockoutEndDate
            };
        }

        public async Task<NguoiDungView> UpdateNguoiDung(NguoiDungEdit model)
        {
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.MaNguoiDung == model.MaNguoiDung);
            if (user == null)
                throw new Exception("Người dùng không tồn tại.");

            // Cập nhật các trường có thể thay đổi
            user.HoTen = model.HoTen;
            user.Sdt = model.Sdt;
            user.Cccd = model.Cccd;
            user.Email = model.Email;
            user.TaiKhoan = model.TaiKhoan;
            user.DiaChi = model.DiaChi;
            user.TrangThai = model.TrangThai;
            user.VaiTro = model.VaiTro;
            user.CancelConunt = model.CancelConunt;
            user.LockoutEndDate = model.LockoutEndDate;

            await _context.SaveChangesAsync();

            return new NguoiDungView
            {
                MaNguoiDung = user.MaNguoiDung,
                HoTen = user.HoTen,
                NgaySinh = user.NgaySinh,
                Sdt = user.Sdt,
                Cccd = user.Cccd,
                Email = user.Email,
                TaiKhoan = user.TaiKhoan,
                DiaChi = user.DiaChi,
                MatKhau = user.MatKhau,
                VaiTro = user.VaiTro,
                TrangThai = user.TrangThai,
                HinhAnh = user.HinhAnh,
                NgayTao = user.NgayTao,
                MoTa = user.MoTa,
                CancelConunt = user.CancelConunt,
                LockoutEndDate = user.LockoutEndDate
            };
        }

        public async Task<bool> DeleteNguoiDung(string id)
        {
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.MaNguoiDung == id);
            if (user == null)
                return false;
            _context.NguoiDungs.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<NguoiDungView> DangKy(DangKyView model)
        {

            if (await _context.NguoiDungs.AnyAsync(u => u.Email == model.Email))
                throw new Exception("Email đã được sử dụng.");


            if (await _context.NguoiDungs.AnyAsync(u => u.TaiKhoan == model.TaiKhoan))
                throw new Exception("Tài khoản đã tồn tại.");


            string hashedPassword = PasswordHasher.HashPassword(model.MatKhau);
            var newUser = new NguoiDung
            {
                MaNguoiDung = GenerateMaNguoiDung(0),
                HoTen = model.HoTen,
                Email = model.Email,
                TaiKhoan = model.TaiKhoan,
                MatKhau = hashedPassword,
                NgayTao = DateTime.Now,
                TrangThai = 1, // Active
                VaiTro = 0, // User thường
                Isveryfied = false
            };

            _context.NguoiDungs.Add(newUser);
            await _context.SaveChangesAsync();

            await GenerateAndSendOtpAccountAsync(model.Email);

            return new NguoiDungView
            {
                MaNguoiDung = newUser.MaNguoiDung,
                HoTen = model.HoTen,
                Email = newUser.Email,
                TaiKhoan = newUser.TaiKhoan,
                MatKhau = newUser.MatKhau,
                VaiTro = newUser.VaiTro,
                TrangThai = newUser.TrangThai,
                NgayTao = newUser.NgayTao,
                Isveryfied = false
            };
        }

        public async Task<(NguoiDungView User, string Token)> DangNhap(DangNhapView model)
        {
            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(u => u.TaiKhoan == model.TaiKhoan);

            if (user == null)
                throw new Exception("Tài khoản không tồn tại.");

            if (user.TrangThai != 1)
                throw new Exception("Tài khoản đã bị khóa hoặc chưa được kích hoạt.");

            if (!PasswordHasher.VerifyPassword(model.MatKhau, user.MatKhau))
                throw new Exception("Mật khẩu không đúng.");

            var userView = new NguoiDungView
            {
                MaNguoiDung = user.MaNguoiDung,
                HoTen = user.HoTen,
                Email = user.Email,
                TaiKhoan = user.TaiKhoan,
                VaiTro = user.VaiTro,
                TrangThai = user.TrangThai,
                NgayTao = user.NgayTao
            };

            var token = _jwtTokenGenerator.GenerateToken(userView);
            return (userView, token);
        }

        public async Task<NguoiDung> CreateUserFromGoogleAsync(ClaimsPrincipal principal)
        {
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = principal.FindFirst(ClaimTypes.Name)?.Value;

            var exitsingUser = await GetNguoiDungByEmailAsync(email);
            if (exitsingUser != null)
            {
                return exitsingUser;
            }

            var newUser = new NguoiDung
            {
                MaNguoiDung = GenerateMaNguoiDung(0),
                Email = email,
                HoTen = name,
                NgayTao = DateTime.Now,
                TrangThai = 1,
                VaiTro = 0
            };

            _context.NguoiDungs.Add(newUser);
            await _context.SaveChangesAsync();
            return newUser;

        }

        public async Task<bool> GenerateAndSendOtpAsync(string email)
        {
            var user = await GetNguoiDungByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            var otp = new Random().Next(100000, 999999).ToString();
            var otpExpiry = DateTime.UtcNow.AddMinutes(10);

            user.Otp = otp;
            user.OtpExpiry = otpExpiry;
            await _context.SaveChangesAsync();

            // Gửi email chứa OTP
            await _emailService.SendOtpEmailAsync(user.Email, otp);
            return true;
        }

        public async Task<bool> GenerateAndSendOtpAccountAsync(string email)
        {
            var user = await GetNguoiDungByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            var otp = new Random().Next(100000, 999999).ToString();
            var otpExpiry = DateTime.UtcNow.AddMinutes(10);

            user.Otp = otp;
            user.OtpExpiry = otpExpiry;
            await _context.SaveChangesAsync();

            // Gửi email chứa OTP
            await _emailService.SendOtpEmailAccountAsync(user.Email, otp);
            return true;
        }

        public async Task<bool> ActivateAccountAsync(string email, string otp)
        {
            var isValidOtp = await VerifyOtpAsync(email, otp);
            if (!isValidOtp)
            {
                return false;
            }

            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return false;
            }

            user.Isveryfied = true;
            await _context.SaveChangesAsync();  

            return true;
        }

        public async Task<bool> VerifyOtpAsync(string email, string otp)
        {
            var user = await GetNguoiDungByEmailAsync(email);
            if (user == null || user.Otp != otp || user.OtpExpiry == null)
            {
                return false; 
            }

            // Kiểm tra OTP có hết hạn không
            if (DateTime.UtcNow > user.OtpExpiry)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email, string otp, string newPassword)
        {
            // Xác minh OTP trước
            if (!await VerifyOtpAsync(email, otp))
            {
                return false; // OTP không hợp lệ hoặc hết hạn
            }

            var user = await GetNguoiDungByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            string hashedPassword = PasswordHasher.HashPassword(newPassword);

            // Cập nhật mật khẩu mới
            user.MatKhau = hashedPassword;
            user.Otp = null; // Xóa OTP sau khi sử dụng
            user.OtpExpiry = null;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateUserProfileAsync(string maNguoiDung, UpdateProfileView model)
        {
            try
            {
                var user = await _context.NguoiDungs
                    .FirstOrDefaultAsync(u => u.MaNguoiDung == maNguoiDung);

                if (user == null)
                    return false;

                user.HoTen = model.HoTen ?? user.HoTen;
                user.Email = model.Email ?? user.Email;
                user.Sdt = model.Sdt ?? user.Sdt;
                user.NgaySinh = model.NgaySinh ?? user.NgaySinh;
                user.Cccd = model.CCCD ?? user.Cccd;
                user.DiaChi = model.DiaChi ?? user.DiaChi;

                if(model.HinhAnh != null)
                {
                    if (model.HinhAnh.Length > 5 * 1024 * 1024)
                        throw new Exception("Kích thước ảnh không vượt quá 5MB");

                    var allowedExtensions = new[] { ".jpg", "jpeg", ".png" };
                    var extension = Path.GetExtension(model.HinhAnh.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        throw new Exception("Chỉ chấp nhận file ảnh .jpg, .jpeg, .png");
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        await model.HinhAnh.CopyToAsync(memoryStream);
                        var imageBytes = memoryStream.ToArray();
                        if (imageBytes.Length == 0)
                            throw new Exception("Ảnh không hợp lệ hoặc rỗng.");

                        user.HinhAnh = imageBytes;
                    }
                }          

                _context.NguoiDungs.Update(user);   
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) 
            {
                throw;
            }
        }

        public async Task<bool> UpdateUserPassword(string maNguoiDung, UpdateProfileView model)
        {
            try
            {
                var user = await _context.NguoiDungs
                    .FirstOrDefaultAsync(u => u.MaNguoiDung == maNguoiDung);

                if (user == null)
                    return false;

                if (!string.IsNullOrEmpty(model.MatKhauMoi) && !string.IsNullOrEmpty(model.MatKhauCu))
                {
                    if (PasswordHasher.VerifyPassword(model.MatKhauCu, user.MatKhau))
                    {
                        user.MatKhau = PasswordHasher.HashPassword(model.MatKhauMoi);
                    }
                    else
                    {
                        throw new Exception("Mật khẩu cũ không đúng");
                    }
                }

                _context.NguoiDungs.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}