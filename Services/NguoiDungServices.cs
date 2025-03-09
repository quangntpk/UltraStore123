
using Microsoft.EntityFrameworkCore;
using UltraStrore.Data;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using UltraStrore.Utils;
namespace UltraStrore.Services
{
    public class NguoiDungServices : INguoiDungServices
    {
        private readonly ApplicationDbContext _context;

        public NguoiDungServices(ApplicationDbContext context)
        {
            _context = context;
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

    }
}