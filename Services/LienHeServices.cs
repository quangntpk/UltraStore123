using Microsoft.EntityFrameworkCore;
using UltraStrore.Data;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Services
{
    public class LienHeServices : ILienHeServices
    {
        private readonly ApplicationDbContext _context;

        public LienHeServices(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<LienHeView>> GetLienHeList(string? searchTerm)
        {
            var query = _context.LienHes.AsNoTracking(); // Tối ưu hiệu suất bằng cách không theo dõi thực thể

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(l =>
                    EF.Functions.Like(l.MaLienHe.ToString(), $"%{searchTerm}%") ||
                    (l.HoTen != null && EF.Functions.Like(l.HoTen.ToLower(), $"%{searchTerm}%")) ||
                    (l.Sdt != null && EF.Functions.Like(l.Sdt.ToLower(), $"%{searchTerm}%")) ||
                    (l.NoiDung != null && EF.Functions.Like(l.NoiDung.ToLower(), $"%{searchTerm}%")) ||
                    (l.Email != null && EF.Functions.Like(l.Email.ToLower(), $"%{searchTerm}%"))
                );
            }

            return await query
                .Select(l => new LienHeView
                {
                    MaLienHe = l.MaLienHe,
                    HoTen = l.HoTen,
                    Sdt = l.Sdt,
                    NoiDung = l.NoiDung,
                    Email = l.Email,
                    TrangThai = l.TrangThai
                })
                .ToListAsync();
        }

        public async Task<LienHeView> GetLienHeById(int id)
        {
            var lienHe = await _context.LienHes
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.MaLienHe == id);

            if (lienHe == null)
                throw new Exception("Liên hệ không tồn tại.");

            return new LienHeView
            {
                MaLienHe = lienHe.MaLienHe,
                HoTen = lienHe.HoTen,
                Sdt = lienHe.Sdt,
                NoiDung = lienHe.NoiDung,
                Email = lienHe.Email,
                TrangThai = lienHe.TrangThai
            };
        }

        public async Task<LienHeView> CreateLienHe(LienHeCreate model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var newLienHe = new LienHe
            {
                MaLienHe = model.MaLienHe, // Giả sử MaLienHe được tự động sinh nếu là identity
                HoTen = model.HoTen,
                Sdt = model.Sdt,
                NoiDung = model.NoiDung,
                Email = model.Email,
                TrangThai = model.TrangThai
            };

            _context.LienHes.Add(newLienHe);
            await _context.SaveChangesAsync();

            return new LienHeView
            {
                MaLienHe = newLienHe.MaLienHe,
                HoTen = newLienHe.HoTen,
                Sdt = newLienHe.Sdt,
                NoiDung = newLienHe.NoiDung,
                Email = newLienHe.Email,
                TrangThai = newLienHe.TrangThai
            };
        }

        public async Task<LienHeView> UpdateLienHe(LienHeEdit model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var lienHe = await _context.LienHes.FirstOrDefaultAsync(l => l.MaLienHe == model.MaLienHe);
            if (lienHe == null)
                throw new Exception("Liên hệ không tồn tại.");

            lienHe.HoTen = model.HoTen;
            lienHe.Sdt = model.Sdt;
            lienHe.NoiDung = model.NoiDung;
            lienHe.Email = model.Email;
            lienHe.TrangThai = model.TrangThai;

            await _context.SaveChangesAsync();

            return new LienHeView
            {
                MaLienHe = lienHe.MaLienHe,
                HoTen = lienHe.HoTen,
                Sdt = lienHe.Sdt,
                NoiDung = lienHe.NoiDung,
                Email = lienHe.Email,
                TrangThai = lienHe.TrangThai
            };
        }

        public async Task<bool> DeleteLienHe(int id)
        {
            var lienHe = await _context.LienHes.FirstOrDefaultAsync(l => l.MaLienHe == id);
            if (lienHe == null)
                return false;

            _context.LienHes.Remove(lienHe);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}