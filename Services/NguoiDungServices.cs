
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using UltraStrore.Data;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using UltraStrore.Utils;
using BCrypt.Net;
namespace UltraStrore.Services
{
    public class NguoiDungServices : INguoiDungServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtTokenServices _jwtTokenGenerator;
        private readonly IEmailServices _emailService;

        public NguoiDungServices(ApplicationDbContext context, IJwtTokenServices jwtTokenGenerator, IEmailServices emailService)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
            _emailService = emailService;
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
                TrangThai = 0, // Active
                VaiTro = 0 // User thường
            };

            _context.NguoiDungs.Add(newUser);
            await _context.SaveChangesAsync();

            return new NguoiDungView
            {
                MaNguoiDung = newUser.MaNguoiDung,
                HoTen = model.HoTen,
                Email = newUser.Email,
                TaiKhoan = newUser.TaiKhoan,
                MatKhau = newUser.MatKhau,
                VaiTro = newUser.VaiTro,
                TrangThai = newUser.TrangThai,
                NgayTao = newUser.NgayTao
            };
        }

        public async Task<(NguoiDungView User, string Token)> DangNhap(DangNhapView model)
        {
            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(u => u.TaiKhoan == model.TaiKhoan);

            if (user == null)
                throw new Exception("Tài khoản không tồn tại.");

            if (user.TrangThai == 1)
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

        public async Task<(NguoiDungView User, string Token)> DangNhapAdmin(LoginAdmin model)
        {
            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(u => u.TaiKhoan == model.TaiKhoan && u.VaiTro == 1); // Chỉ lấy Admin (VaiTro = 1)

            if (user == null)
                throw new Exception("Tài khoản Admin không tồn tại.");

            if (user.TrangThai != 1)
                throw new Exception("Tài khoản Admin đã bị khóa hoặc chưa được kích hoạt.");

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
        public async Task<bool> IsAdminAsync(string email)
        {
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);
            return user != null && user.VaiTro == 1; // VaiTro = 1 là admin
        }

        public async Task<(NguoiDungView User, string Token)> DangNhapGoogleAdmin(string email)
        {
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                // Tạo mới người dùng với vai trò admin nếu chưa tồn tại
                user = new NguoiDung
                {
                    Email = email,
                    VaiTro = 1, // Admin
                    MaNguoiDung = GenerateMaNguoiDung(1),
                    NgayTao = DateTime.Now,
                    TrangThai = 1 // Active
                };
                _context.NguoiDungs.Add(user);
                await _context.SaveChangesAsync();
            }
            else if (user.VaiTro != 1)
            {
                throw new Exception("Không phải admin.");
            }

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
        public async Task<NguoiDungView> UpdateChiTietUser(ChiTietUser model)
        {
            // Lấy người dùng từ database dựa trên MaNguoiDung
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.MaNguoiDung == model.MaNguoiDung);

            // Kiểm tra xem người dùng có tồn tại không
            if (user == null)
            {
                throw new Exception("Người dùng không tồn tại.");
            }

            // Cập nhật các thuộc tính của người dùng (chỉ cập nhật nếu giá trị không null)
            user.HoTen = model.HoTen ?? user.HoTen;
            user.NgaySinh = model.NgaySinh ?? user.NgaySinh;
            user.Sdt = model.Sdt ?? user.Sdt;
            user.Cccd = model.Cccd ?? user.Cccd;
            user.Email = model.Email ?? user.Email;
            user.TaiKhoan = model.TaiKhoan ?? user.TaiKhoan;
            user.DiaChi = model.DiaChi ?? user.DiaChi;
            user.VaiTro = model.VaiTro ?? user.VaiTro;
            user.TrangThai = model.TrangThai ?? user.TrangThai;

            // Xử lý hình ảnh nếu có file được upload
            if (model.HinhAnhFile != null && model.HinhAnhFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await model.HinhAnhFile.CopyToAsync(memoryStream);
                    user.HinhAnh = memoryStream.ToArray(); // Chuyển file thành byte[]
                }
            }
            else
            {
                user.HinhAnh = model.HinhAnh ?? user.HinhAnh; // Giữ nguyên nếu không có file mới
            }

            user.NgayTao = model.NgayTao ?? user.NgayTao;
            user.MoTa = model.MoTa ?? user.MoTa;

            // Lưu thay đổi vào database
            await _context.SaveChangesAsync();

            // Trả về NguoiDungView của người dùng đã cập nhật
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
                VaiTro = user.VaiTro,
                TrangThai = user.TrangThai,
                HinhAnh = user.HinhAnh,
                NgayTao = user.NgayTao,
                MoTa = user.MoTa
            };
        }
    }
}