using Microsoft.EntityFrameworkCore;
using UltraStrore.Data;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Services
{
    public class DanhSachDiaChiServices : IDanhSachDiaChiServices
    {
        private readonly ApplicationDbContext _context;

        public DanhSachDiaChiServices(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DanhSachDiaChiView>> GetDanhSachDiaChiList(string? searchTerm)
        {
            var query = _context.DanhSachDiaChis.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(d =>
                    (d.MaNguoiDung != null && d.MaNguoiDung.ToLower().Contains(searchTerm)) ||
                    (d.HoTen != null && d.HoTen.ToLower().Contains(searchTerm)) ||
                    (d.Sdt != null && d.Sdt.ToLower().Contains(searchTerm)) ||
                    (d.MoTa != null && d.MoTa.ToLower().Contains(searchTerm)) ||
                    (d.DiaChi != null && d.DiaChi.ToLower().Contains(searchTerm))
                );
            }
            var list = await query.ToListAsync();
            return list.Select(d => new DanhSachDiaChiView
            {
                MaDiaChi = d.MaDiaChi,
                MaNguoiDung = d.MaNguoiDung,
                HoTen = d.HoTen,
                Sdt = d.Sdt,
                MoTa = d.MoTa,
                DiaChi = d.DiaChi,
                PhuongXa = d.PhuongXa,
                QuanHuyen = d.QuanHuyen,
                Tinh = d.Tinh,
                TrangThai = d.TrangThai
            }).ToList();
        }

        public async Task<List<DanhSachDiaChiView>> GetDanhSachDiaChiByMaNguoiDung(string maNguoiDung)
        {
            var list = await _context.DanhSachDiaChis
                .Where(d => d.MaNguoiDung == maNguoiDung)
                .ToListAsync();
            return list.Select(d => new DanhSachDiaChiView
            {
                MaDiaChi = d.MaDiaChi,
                MaNguoiDung = d.MaNguoiDung,
                HoTen = d.HoTen,
                Sdt = d.Sdt,
                MoTa = d.MoTa,
                DiaChi = d.DiaChi,
                PhuongXa = d.PhuongXa,
                QuanHuyen = d.QuanHuyen,
                Tinh = d.Tinh,
                TrangThai = d.TrangThai
            }).ToList();
        }

        public async Task<DanhSachDiaChiView> GetDanhSachDiaChiById(int id)
        {
            var address = await _context.DanhSachDiaChis.FirstOrDefaultAsync(d => d.MaDiaChi == id);
            if (address == null)
                throw new Exception("Địa chỉ không tồn tại.");
            return new DanhSachDiaChiView
            {
                MaDiaChi = address.MaDiaChi,
                MaNguoiDung = address.MaNguoiDung,
                HoTen = address.HoTen,
                Sdt = address.Sdt,
                MoTa = address.MoTa,
                DiaChi = address.DiaChi,
                PhuongXa = address.PhuongXa,
                QuanHuyen = address.QuanHuyen,
                Tinh = address.Tinh,
                TrangThai = address.TrangThai
            };
        }

        public async Task<DanhSachDiaChiView> CreateDanhSachDiaChi(DanhSachDiaChiCreate model)
        {

            int isActive = model.TrangThai ?? 0;

            if (isActive == 1)
            {
                var existingAddresses = await _context.DanhSachDiaChis
                    .Where(d => d.MaNguoiDung == model.MaNguoiDung)
                    .ToListAsync();
                foreach (var addr in existingAddresses)
                {
                    addr.TrangThai = 0;
                }
            }

            var newAddress = new DanhSachDiaChi
            {
                MaDiaChi = model.MaDiaChi,
                MaNguoiDung = model.MaNguoiDung,
                HoTen = model.HoTen,
                Sdt = model.Sdt,
                MoTa = model.MoTa,
                DiaChi = model.DiaChi,
                PhuongXa = model.PhuongXa,
                QuanHuyen = model.QuanHuyen,
                Tinh = model.Tinh,
                TrangThai = isActive
            };

            _context.DanhSachDiaChis.Add(newAddress);
            await _context.SaveChangesAsync();

            return new DanhSachDiaChiView
            {
                MaDiaChi = newAddress.MaDiaChi,
                MaNguoiDung = newAddress.MaNguoiDung,
                HoTen = newAddress.HoTen,
                Sdt = newAddress.Sdt,
                MoTa = newAddress.MoTa,
                DiaChi = newAddress.DiaChi,
                PhuongXa = newAddress.PhuongXa,
                QuanHuyen = newAddress.QuanHuyen,
                Tinh = newAddress.Tinh,
                TrangThai = newAddress.TrangThai
            };
        }

        public async Task<DanhSachDiaChiView> UpdateDanhSachDiaChi(DanhSachDiaChiEdit model)
        {
            var address = await _context.DanhSachDiaChis.FirstOrDefaultAsync(d => d.MaDiaChi == model.MaDiaChi);
            if (address == null)
                throw new Exception("Địa chỉ không tồn tại.");

            int isActive = model.TrangThai ?? 0;

            if (isActive == 1)
            {
                var otherAddresses = await _context.DanhSachDiaChis
                    .Where(d => d.MaNguoiDung == model.MaNguoiDung && d.MaDiaChi != model.MaDiaChi)
                    .ToListAsync();
                foreach (var addr in otherAddresses)
                {
                    addr.TrangThai = 0;
                }
            }

            address.MaNguoiDung = model.MaNguoiDung;
            address.HoTen = model.HoTen;
            address.Sdt = model.Sdt;
            address.MoTa = model.MoTa;
            address.DiaChi = model.DiaChi;
            address.PhuongXa = model.PhuongXa;
            address.QuanHuyen = model.QuanHuyen;
            address.Tinh = model.Tinh;
            address.TrangThai = isActive;

            await _context.SaveChangesAsync();

            return new DanhSachDiaChiView
            {
                MaDiaChi = address.MaDiaChi,
                MaNguoiDung = address.MaNguoiDung,
                HoTen = address.HoTen,
                Sdt = address.Sdt,
                MoTa = address.MoTa,
                DiaChi = address.DiaChi,
                PhuongXa = address.PhuongXa,
                QuanHuyen = address.QuanHuyen,
                Tinh = address.Tinh,
                TrangThai = address.TrangThai
            };
        }

        public async Task<bool> DeleteDanhSachDiaChi(int id)
        {
            var address = await _context.DanhSachDiaChis.FirstOrDefaultAsync(d => d.MaDiaChi == id);
            if (address == null)
                return false;

            _context.DanhSachDiaChis.Remove(address);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}