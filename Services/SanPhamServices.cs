using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using UltraStrore.Data;
using UltraStrore.Helper;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Services
{
    public class SanPhamServices : ISanPhamServices
    {
        private readonly ApplicationDbContext _context;

        public SanPhamServices(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<SanPhamView>> ListSanPham(string id)
        {
            List<SanPhamView> listsp = new List<SanPhamView>();
            var nhomSanPham = _context.SanPhams.GroupBy(s => s.MaSanPham.Substring(0, 6)).ToList();



            foreach (var nhom in nhomSanPham)
            {
                var sanPhamDauTien = nhom.First();
                var HinhAnhSanPhamList = _context.HinhAnhs.Where(g => g.MaSanPham.Substring(0, 6) == sanPhamDauTien.MaSanPham.Substring(0, 6)).Select(g => g.Data).ToList();
                var listMauSac = nhom.Select(sp => sp.MaSanPham.Split('_')[1]).Distinct().ToList();
                var listKichThuoc = nhom.Select(sp => sp.MaSanPham.Split('_')[2]).Distinct().ToList();
                var tongSoLuong = nhom.Sum(sp => sp.SoLuong);
                var TenLoai = _context.LoaiSanPhams.Where(g => g.MaLoaiSanPham == sanPhamDauTien.MaLoaiSanPham).Select(g => g.TenLoaiSanPham).FirstOrDefault();
                var ThuongHieu = _context.ThuongHieus.Where(g => g.MaThuongHieu == sanPhamDauTien.MaThuongHieu).Select(g => g.TenThuongHieu).FirstOrDefault();
                listsp.Add(new SanPhamView
                {
                    ID = sanPhamDauTien.MaSanPham.Substring(0, 6),
                    Name = sanPhamDauTien.TenSanPham,
                    MauSac = listMauSac,
                    KichThuoc = listKichThuoc,
                    Hinh = HinhAnhSanPhamList,
                    SoLuong = tongSoLuong ?? 0,
                    DonGia = sanPhamDauTien.Gia ?? 0,
                    LoaiSanPham = TenLoai,                  
                    ThuongHieu = ThuongHieu,
                    NgayTao = sanPhamDauTien.NgayTao,
                    TrangThai = sanPhamDauTien.TrangThai,         
                    ChatLieu = sanPhamDauTien.ChatLieu
                });
            }
            if (string.IsNullOrEmpty(id))
            {
                return listsp.OrderByDescending(g => g.NgayTao).ToList();
            }
            return listsp.Where(g => g.ID.Trim() == id.Trim()).ToList();
        }
        public async Task<List<SanPham>> SanPhamByID(string? id)
        {
           
            var ListSanPham = _context.SanPhams.Where(g => g.MaSanPham.Contains(id)).ToList();
            return ListSanPham;
            
        }
        public async Task<List<SanPhamByIDSorted>> SanPhamByIDSorteds (string? id)
        {
            List<SanPhamByIDSorted> listsp = new List<SanPhamByIDSorted>();
            var nhomSanPham = _context.SanPhams.Where(g=>g.MaSanPham.Contains(id)).GroupBy(s => s.MaSanPham.Substring(0, 13)).ToList();
            foreach (var nhom in nhomSanPham)
            {
                var sanPhamDauTien = nhom.First();
                var HinhAnhSanPhamList = _context.HinhAnhs.Where(g => g.MaSanPham.Substring(0, 6) == sanPhamDauTien.MaSanPham.Substring(0, 6)).Select(g => g.TenHinhAnh).ToList();
                var listMauSac = sanPhamDauTien.MaSanPham.Split('_')[1];
                List<SanPhamEditDetail> detailedit = new List<SanPhamEditDetail>();
                foreach(var item in nhom)
                {
                    SanPhamEditDetail ed = new SanPhamEditDetail();
                    ed.KichThuoc = item.KichThuoc;
                    ed.SoLuong = item.SoLuong??0;
                    ed.Gia = item.Gia?? 0;
                    detailedit.Add(ed);
                }    
                var tongSoLuong = nhom.Sum(sp => sp.SoLuong);
                var MaLoai = sanPhamDauTien.MaLoaiSanPham;
                var ThuongHieu = sanPhamDauTien.MaThuongHieu;
                var HinhAnh = _context.HinhAnhs.Where(g => g.MaSanPham.Trim() == sanPhamDauTien.MaSanPham.Substring(0, 6).Trim()).Select(g=>g.Data).ToList();
                listsp.Add(new SanPhamByIDSorted
                {
                    ID = sanPhamDauTien.MaSanPham.Substring(0, 13),
                    TenSanPham = sanPhamDauTien.TenSanPham,
                    MauSac = listMauSac,                   
                    LoaiSanPham = MaLoai,
                    MaThuongHieu = ThuongHieu,
                    Details = detailedit,
                    HinhAnhs = HinhAnh,
                    ChatLieu = sanPhamDauTien.ChatLieu,
                    MoTa = sanPhamDauTien.MoTa
                });
            }
            return listsp;
        }
        public async Task<APIResponse> EditSanPham(List<SanPhamEdit> data)
        {
            var SanPhamNotEdited = _context.SanPhams.Where(g => g.MaSanPham.Contains(data[0].ID)).ToList();
            List<SanPham> EditCheck = new List<SanPham>();
            APIResponse response = new APIResponse();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                for (int i = 0; i < data.Count(); i++)
                {
                    for (int j = 0; j < data[i].Details.Count(); j++)
                    {
                        SanPham edit = new SanPham();
                        edit.MaSanPham = data[i].ID.Trim() + "_" + data[i].MauSac.Trim() + "_" + data[i].Details[j].KichThuoc.Trim();
                        edit.TenSanPham = data[i].TenSanPham.Trim();
                        edit.SoLuong = data[i].Details[j].SoLuong;
                        edit.Gia = data[i].Details[j].Gia;
                        edit.MaThuongHieu = data[i].MaThuongHieu;
                        edit.MaLoaiSanPham = data[i].LoaiSanPham;
                        edit.KichThuoc = data[i].Details[j].KichThuoc.Trim();
                        edit.NgayTao = SanPhamNotEdited[0].NgayTao;
                        edit.TrangThai = 1;
                        edit.MoTa = data[i].MoTa ?? null;
                        edit.Example = true;
                        edit.ChatLieu = data[i].ChatLieu;
                        EditCheck.Add(edit);
                    }
                }
                foreach (var item in EditCheck)
                {
                    var SanPhamEdited = _context.SanPhams.FirstOrDefault(g => g.MaSanPham == item.MaSanPham);
                    if (SanPhamEdited != null)
                    {
                        SanPhamEdited.TenSanPham = item.TenSanPham;
                        SanPhamEdited.MaLoaiSanPham = item.MaLoaiSanPham;
                        SanPhamEdited.MaThuongHieu = item.MaThuongHieu;
                        SanPhamEdited.KichThuoc = item.KichThuoc;
                        SanPhamEdited.SoLuong = item.SoLuong;
                        SanPhamEdited.Gia = item.Gia;
                        SanPhamEdited.MoTa = item.MoTa;
                        SanPhamEdited.Example = item.Example;
                        SanPhamEdited.ChatLieu = item.ChatLieu;
                        _context.SanPhams.Update(SanPhamEdited);
                    }
                    else
                    {
                        _context.SanPhams.Add(item);
                    }
                }
                foreach(var item in SanPhamNotEdited)
                {
                    bool Found = false;
                    for (int i = 0; i < EditCheck.Count(); i++) 
                    {
                        if(EditCheck[i].MaSanPham == item.MaSanPham)
                        {
                            Found = true;
                            break;
                        }
                    }
                    if (!Found) 
                    {
                        _context.SanPhams.Remove(item);
                    }                                             
                }
                var HinhDelete = _context.HinhAnhs.Where(g => g.MaSanPham.Contains(data[0].ID.Trim())).ToList();
                _context.HinhAnhs.RemoveRange(HinhDelete);
                foreach(var item in data[0].HinhAnhs)
                {
                    HinhAnh newHinhAnh = new HinhAnh();
                    newHinhAnh.TenHinhAnh = data[0].TenSanPham;
                    newHinhAnh.Data = item;
                    newHinhAnh.MaSanPham = data[0].ID;
                    _context.HinhAnhs.Add(newHinhAnh);
                }    
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                response.ResponseCode = 200;
            }
            catch (Exception ex) {

                response.ErrorMessage = ex.Message;
                response.ResponseCode = 400;
                await transaction.RollbackAsync();
            }
            return response;
        }
        public async Task<APIResponse> CreateSanPham(List<SanPhamCreate> data)
        {
            var SanPhamCungLoaiMax = _context.SanPhams
                .Where(g => g.MaLoaiSanPham == data[0].LoaiSanPham)
                .Select(g => g.MaSanPham.Substring(1, 5))
                .OrderByDescending(x => x)
                .FirstOrDefault();
            var KiHieu = _context.LoaiSanPhams.Where(g => g.MaLoaiSanPham == data[0].LoaiSanPham).FirstOrDefault();
            int Max = 0;
            if (!string.IsNullOrEmpty(SanPhamCungLoaiMax) && int.TryParse(SanPhamCungLoaiMax, out int parsedMax))
            {
                Max = parsedMax + 1;
            }
            APIResponse response = new APIResponse();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                for (int i=0;i <data.Count();i++) 
                {
                    foreach(var item in data[i].Details)
                    {
                        SanPham sp = new SanPham();
                        sp.TenSanPham = data[i].TenSanPham;
                        sp.MaSanPham = KiHieu.KiHieu.ToString().Trim() + Max.ToString("00000").Trim()+ "_"+data[i].MauSac.Trim()+"_"+item.KichThuoc.Trim();
                        sp.Gia = item.Gia;
                        sp.SoLuong = item.SoLuong;
                        sp.ChatLieu = data[i].ChatLieu;
                        sp.MaLoaiSanPham = KiHieu.MaLoaiSanPham;
                        sp.MaThuongHieu = data[i].MaThuongHieu;
                        sp.KichThuoc = item.KichThuoc;
                        sp.NgayTao = DateOnly.FromDateTime(DateTime.Now);
                        sp.TrangThai = 1;
                        sp.Example = true;
                        sp.MoTa = data[i].MoTa;
                        _context.SanPhams.Add(sp);
                    }
                }
                foreach(var item in data[0].HinhAnhs)
                {
                    HinhAnh newHA = new HinhAnh();
                    newHA.TenHinhAnh = data[0].TenSanPham + 1;
                    newHA.MaSanPham = KiHieu.KiHieu.ToString().Trim() + Max.ToString("00000").Trim();
                    newHA.Data = item;
                    _context.HinhAnhs.Add(newHA);
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                response.ResponseCode = 200;
            }           
            catch(Exception ex) 
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 400;
                await transaction.RollbackAsync();
            }
            return response;
        }
        public async Task<APIResponse> DeleteSanPham(string? id)
        {
            APIResponse response = new APIResponse();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {                
                var SanPham = _context.SanPhams.Where(g => g.MaSanPham.Contains(id)).ToList();
                foreach(var Sp in SanPham)
                {
                    Sp.TrangThai = 0;
                    _context.SanPhams.Update(Sp);
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                response.ResponseCode = 200;
            }
            catch(Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 400;
                await transaction.RollbackAsync();
            }
            return response;
        }
        public async Task<APIResponse> ActiveSanPham(string? id)
        {
            APIResponse response = new APIResponse();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var SanPham = _context.SanPhams.Where(g => g.MaSanPham.Contains(id)).ToList();
                foreach (var Sp in SanPham)
                {
                    Sp.TrangThai = 1;
                    _context.SanPhams.Update(Sp);
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                response.ResponseCode = 200;
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 400;
                await transaction.RollbackAsync();
            }
            return response;
        }

    }
}
