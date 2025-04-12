using Microsoft.EntityFrameworkCore;
using UltraStrore.Data;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UltraStrore.Helper;

namespace UltraStrore.Services
{
    public class KichThuocServices : IKichThuocServices
    {
        private readonly ApplicationDbContext _context;

        public KichThuocServices(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<KichThuocView>> GetAllKichThuocAsync()
        {
            var list = await _context.KichThuocs.AsNoTracking().ToListAsync();
            List<KichThuocView> List = new List<KichThuocView>();
            foreach(var item in list)
            {
                var TenLoai = _context.LoaiSanPhams.Where(g => g.MaLoaiSanPham == item.MaLoai).Select(g => g.TenLoaiSanPham).FirstOrDefault();
                KichThuocView newKT = new KichThuocView
                {
                    MaKichThuoc = item.MaKichThuoc,
                    TenLoai = TenLoai,
                    TenKichThuoc = item.TenKichThuoc
                };
                List.Add(newKT);
            } 
            return List;
        }

        public async Task<KichThuocView> GetKichThuocAsync(int maKichThuoc)
        {
            var kichThuoc = await _context.KichThuocs.AsNoTracking()
                .FirstOrDefaultAsync(k => k.MaKichThuoc == maKichThuoc)
                ?? throw new KeyNotFoundException("Kích thước không tồn tại.");
            var TenLoai =  _context.LoaiSanPhams.Where(g => g.MaLoaiSanPham == kichThuoc.MaLoai).Select(g => g.TenLoaiSanPham).FirstOrDefault();
            return new KichThuocView
            {
                MaKichThuoc = kichThuoc.MaKichThuoc,
                TenLoai = TenLoai,
                TenKichThuoc = kichThuoc.TenKichThuoc
            };
        }

        public async Task<APIResponse> CreateKichThuocAsync(KichThuocCreate model)
        {
            APIResponse response = new APIResponse();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in model.TenKichThuoc)
                {
                    KichThuoc newKT = new KichThuoc();
                    newKT.TenKichThuoc = item;
                    newKT.MaLoai = model.MaLoai;
                    _context.KichThuocs.Add(newKT);
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                response.ResponseCode = 201;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.ResponseCode = 500;
                response.Result = $"Lỗi: {ex.Message}";
            }
            return response;
        }

        public async Task<KichThuocView> UpdateKichThuocAsync(KichThuocEdit model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var kichThuoc = await _context.KichThuocs
                .FirstOrDefaultAsync(k => k.MaKichThuoc == model.MaKichThuoc)
                ?? throw new KeyNotFoundException("Kích thước không tồn tại.");

            kichThuoc.TenKichThuoc = model.TenKichThuoc ?? kichThuoc.TenKichThuoc;

            await _context.SaveChangesAsync();

            return new KichThuocView
            {
                MaKichThuoc = kichThuoc.MaKichThuoc,
                TenKichThuoc = kichThuoc.TenKichThuoc
            };
        }

        public async Task<bool> DeleteKichThuocAsync(int maKichThuoc)
        {
            var kichThuoc = await _context.KichThuocs
                .FirstOrDefaultAsync(k => k.MaKichThuoc == maKichThuoc);

            if (kichThuoc == null)
                return false;

            _context.KichThuocs.Remove(kichThuoc);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<KichThuocView>> SearchKichThuocAsync(string tenKichThuoc)
        {
            var query = _context.KichThuocs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(tenKichThuoc))
            {
                query = query.Where(k => k.TenKichThuoc.Contains(tenKichThuoc, StringComparison.OrdinalIgnoreCase));
            }

            var list = await query.ToListAsync();
            return list.Select(k => new KichThuocView
            {
                MaKichThuoc = k.MaKichThuoc,
                TenKichThuoc = k.TenKichThuoc
            }).ToList();
        }
    }
}