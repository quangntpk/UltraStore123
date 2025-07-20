using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text.Json;
using UltraStrore.Data;
using UltraStrore.Helper;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO;
using System.Reflection.Metadata.Ecma335;
namespace UltraStrore.Services
{
    public class SanPhamServices : ISanPhamServices
    {
        private static List<MoTaSanPhamCreateModel> _tempMoTaData = new List<MoTaSanPhamCreateModel>();
        private readonly ApplicationDbContext _context;
        private readonly string _dbPath;
        public SanPhamServices(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _dbPath = Path.Combine(env.WebRootPath, "db.json");
        }
        public async Task<List<SanPhamView>> ListSanPham(string id)
        {
            List<SanPhamView> listsp = new List<SanPhamView>();
            var nhomSanPham = _context.SanPhams.GroupBy(s => s.MaSanPham.Substring(0, 6)).ToList();

            foreach (var nhom in nhomSanPham)
            {
                var sanPhamDauTien = nhom.First();
                var HinhAnhSanPhamList = _context.HinhAnhs
                    .Where(g => g.MaSanPham == sanPhamDauTien.MaSanPham.Substring(0, 6))
                    .Select(g => g.Data)
                    .ToList();
                var listMauSac = nhom.Select(sp => sp.MaSanPham.Split('_')[1]).Distinct().ToList();
                var listKichThuoc = nhom.Select(sp => sp.MaSanPham.Split('_')[2]).Distinct().ToList();
                var SoLuongDaBan = nhom.Sum(sp => sp.SoLuongDaBan);
                var tongSoLuong = nhom.Sum(sp => sp.SoLuong) - nhom.Sum(sp => sp.SoLuongDaBan);
                var TenLoai = _context.LoaiSanPhams
                    .Where(g => g.MaLoaiSanPham == sanPhamDauTien.MaLoaiSanPham)
                    .Select(g => g.TenLoaiSanPham)
                    .FirstOrDefault();
                var ThuongHieu = _context.ThuongHieus
                    .Where(g => g.MaThuongHieu == sanPhamDauTien.MaThuongHieu)
                    .Select(g => g.TenThuongHieu)
                    .FirstOrDefault();
                int GiaBan = nhom.Where(g => g.Gia.HasValue).OrderBy(g => g.Gia).Select(g => g.Gia.Value).FirstOrDefault();
                listsp.Add(new SanPhamView
                {
                    ID = sanPhamDauTien.MaSanPham.Substring(0, 6),
                    Name = sanPhamDauTien.TenSanPham,
                    MauSac = listMauSac,
                    KichThuoc = listKichThuoc,
                    Hinh = HinhAnhSanPhamList,
                    SoLuong = tongSoLuong ?? 0,
                    DonGia = GiaBan,
                    LoaiSanPham = TenLoai,
                    ThuongHieu = ThuongHieu,
                    NgayTao = sanPhamDauTien.NgayTao,
                    TrangThai = sanPhamDauTien.TrangThai,
                    ChatLieu = sanPhamDauTien.ChatLieu,
                    MoTa = sanPhamDauTien.MoTa,
                    SoLuongDaBan = SoLuongDaBan,
                    GioiTinh = sanPhamDauTien.GioiTinh == 0 ? "Nam" : "Nữ",
                    Hot = false
                });
            }

            if (string.IsNullOrEmpty(id))
            {
                var topList = listsp.OrderByDescending(g => g.SoLuongDaBan).ToList();            
                return topList;
            }

            return listsp.Where(g => g.ID.Trim() == id.Trim()).ToList();
        }

        public async Task<List<SanPhamView2>> SanPhamByID(string? id)
        {

            List<SanPhamView2>? Result = new List<SanPhamView2>();
            var ListSanPham = await _context.SanPhams
                .Include(sp => sp.MaThuongHieuNavigation) 
                .Include(sp => sp.MaLoaiSanPhamNavigation) 
                .Where(g => g.MaSanPham.Contains(id))
                .ToListAsync();
            if (ListSanPham!=null && ListSanPham.Count()>0)
            {
                foreach (var item in ListSanPham)
                {
                    Result.Add(new SanPhamView2
                    {
                        GiaNhap = item.GiaNhap??0,
                        MaSanPham = item.MaSanPham ?? "",
                        TenSanPham = item.TenSanPham ?? null,
                        SoLuong = item.SoLuong ?? null,
                        Gia = item.Gia ?? 0,
                        MaThuongHieu = item.MaThuongHieu ?? null,
                        MaLoaiSanPham = item.MaLoaiSanPham ?? null,
                        KichThuoc = item.KichThuoc ?? null,
                        NgayTao = item.NgayTao ?? null,
                        TrangThai = item.TrangThai ?? null,
                        Example = item.Example ?? null,
                        ChatLieu = item.ChatLieu ?? null,
                        MoTa = item.MoTa ?? null,
                        GioiTinh = item.GioiTinh ?? null,
                        SoLuongDaBan = item.SoLuongDaBan ?? 0,
                        ThuongHieu = item.MaThuongHieuNavigation.TenThuongHieu ?? null,
                        LoaiSanPham = item.MaLoaiSanPhamNavigation.TenLoaiSanPham ?? null,
                    });
                }
            }       
            return Result;

        }
        public async Task<List<SanPhamByIDSorted>> SanPhamByIDSorteds(string? id)
        {
            List<SanPhamByIDSorted> listsp = new List<SanPhamByIDSorted>();
            var nhomSanPham = _context.SanPhams.Where(g => g.MaSanPham.Contains(id)).GroupBy(s => s.MaSanPham.Substring(0, 13)).ToList();
            foreach (var nhom in nhomSanPham)
            {
                var sanPhamDauTien = nhom.First();
                var HinhAnhSanPhamList = _context.HinhAnhs.Where(g => g.MaSanPham == sanPhamDauTien.MaSanPham.Substring(0, 6)).Select(g => g.TenHinhAnh).ToList();
                var listMauSac = sanPhamDauTien.MaSanPham.Split('_')[1];
                List<SanPhamEditDetail> detailedit = new List<SanPhamEditDetail>();
                foreach (var item in nhom)
                {
                    SanPhamEditDetail ed = new SanPhamEditDetail();
                    ed.KichThuoc = item.KichThuoc;
                    if (item.SoLuongDaBan != null)
                        ed.SoLuong = item.SoLuong - item.SoLuongDaBan ?? 0;
                    else
                        ed.SoLuong = item.SoLuong;
                    ed.Gia = item.Gia ?? 0;
                    ed.GiaNhap = item.GiaNhap??0;
                    ed.HinhAnh = _context.HinhAnhs.Where(g => g.MaSanPham == item.MaSanPham).Select(h=>h.Data).FirstOrDefault();
                    detailedit.Add(ed);
                }
                var tongSoLuong = nhom.Sum(sp => sp.SoLuong);
                var MaLoai = _context.LoaiSanPhams.Where(g => g.MaLoaiSanPham == sanPhamDauTien.MaLoaiSanPham).Select(g => g.TenLoaiSanPham).FirstOrDefault();
                var ThuongHieu = _context.ThuongHieus.Where(g => g.MaThuongHieu == sanPhamDauTien.MaThuongHieu).Select(g => g.TenThuongHieu).FirstOrDefault();
                var HinhAnh = _context.HinhAnhs.Where(g => g.MaSanPham.Trim() == sanPhamDauTien.MaSanPham.Substring(0, 6).Trim()).Select(g => g.Data).ToList();
                string json = File.ReadAllText(_dbPath); var FullChiTiet= JsonSerializer.Deserialize<List<MoTaSanPhamCreateModel>>(json);
                var MoTaCT = FullChiTiet.Where(g => g.MaSanPham == id.Substring(0, 6)).FirstOrDefault();
                listsp.Add(new SanPhamByIDSorted
                {
                    TH = sanPhamDauTien.MaThuongHieu,
                    LSP = sanPhamDauTien.MaLoaiSanPham,
                    ID = sanPhamDauTien.MaSanPham.Substring(0, 13),
                    TenSanPham = sanPhamDauTien.TenSanPham,
                    MauSac = listMauSac,
                    LoaiSanPham = MaLoai,
                    MaThuongHieu = ThuongHieu,
                    Details = detailedit,
                    HinhAnhs = HinhAnh,
                    ChatLieu = sanPhamDauTien.ChatLieu,
                    MoTa = sanPhamDauTien.MoTa,
                    GioiTinh = sanPhamDauTien.GioiTinh,
                    MoTaChiTiet = MoTaCT,
                });
            }
            return listsp;
        }
        public async Task<APIResponse> EditSanPham(List<SanPhamEdit> data)
        {
            var SanPhamNotEdited = _context.SanPhams.Where(g => g.MaSanPham.Contains(data[0].ID)).ToList();
            List<SanPham> EditCheck = new List<SanPham>();
            List<HinhAnh> AddHinh = new List<HinhAnh>();
            APIResponse response = new APIResponse();
            using var transaction = await _context.Database.BeginTransactionAsync();
            List<MoTaSanPhamCreateModel> existingData = new List<MoTaSanPhamCreateModel>();
            if (System.IO.File.Exists(_dbPath))
            {
                var jsonContent = await System.IO.File.ReadAllTextAsync(_dbPath);
                if (!string.IsNullOrWhiteSpace(jsonContent))
                {
                    try
                    {
                        existingData = JsonSerializer.Deserialize<List<MoTaSanPhamCreateModel>>(jsonContent) ?? new List<MoTaSanPhamCreateModel>();
                    }
                    catch (JsonException ex)
                    {
                        response.ErrorMessage = ex.Message;
                        response.ResponseCode = 400;
                        return response;
                    }
                }
            }
            else
            {
                await System.IO.File.WriteAllTextAsync(_dbPath, "[]");
            }
            string maSanPham = data[0].ID.Substring(0, 6);
            var temp = _tempMoTaData;
            existingData.RemoveAll(x => x.MaSanPham.Trim() == maSanPham.Trim());

            // Save temporary MoTa data to db.json with MaSanPham and IdMoTa
            int maxIdMoTa = existingData.Any() ? existingData.Max(x => int.Parse(x.IdMoTa)) : 0;
            foreach (var moTa in _tempMoTaData)
            {
                if(moTa!=null)
                {
                    moTa.MaSanPham = maSanPham;
                    moTa.IdMoTa = (maxIdMoTa + 1).ToString();
                    existingData.Add(moTa);
                    maxIdMoTa++;
                }    
            }
            // Write updated data to db.json
            var options = new JsonSerializerOptions { WriteIndented = true };
            var updatedJson = JsonSerializer.Serialize(existingData, options);
            await System.IO.File.WriteAllTextAsync(_dbPath, updatedJson);
            _tempMoTaData.Clear();



            try
            {
                for (int i = 0; i < data.Count(); i++)
                {                   
                    for (int j = 0; j < data[i].Details.Count(); j++)
                    {
                        var tem = data[i].Details[j];
                        SanPham edit = new SanPham();
                        edit.MaSanPham = data[i].ID.Trim() + "_" + data[i].MauSac.Trim() + "_" + data[i].Details[j].KichThuoc.Trim();
                        edit.TenSanPham = data[i].TenSanPham.Trim();
                        edit.SoLuong = data[i].Details[j].SoLuong;
                        edit.Gia = data[i].Details[j].Gia;
                        edit.GiaNhap = data[i].Details[j].GiaNhap;
                        edit.MaThuongHieu = data[i].MaThuongHieu;
                        edit.MaLoaiSanPham = data[i].LoaiSanPham;
                        edit.KichThuoc = data[i].Details[j].KichThuoc.Trim();
                        edit.NgayTao = SanPhamNotEdited[0].NgayTao;
                        edit.TrangThai = 1;
                        edit.MoTa = data[i].MoTa ?? null;
                        edit.Example = true;
                        edit.ChatLieu = data[i].ChatLieu;
                        EditCheck.Add(edit);
                        HinhAnh hinh = new HinhAnh();
                        hinh.MaSanPham = edit.MaSanPham;
                        hinh.Data = data[i].Details[j].HinhAnh;
                        hinh.TenHinhAnh = edit.TenSanPham;
                        AddHinh.Add(hinh);
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
                        if (SanPhamEdited.SoLuongDaBan != null || SanPhamEdited.SoLuongDaBan == 0)
                            SanPhamEdited.SoLuong = SanPhamEdited.SoLuongDaBan + item.SoLuong;
                        else
                            SanPhamEdited.SoLuong = item.SoLuong;
                        SanPhamEdited.Gia = item.Gia;
                        SanPhamEdited.GiaNhap = item.GiaNhap;
                        SanPhamEdited.MoTa = item.MoTa;
                        SanPhamEdited.Example = item.Example;
                        SanPhamEdited.ChatLieu = item.ChatLieu;
                        _context.SanPhams.Update(SanPhamEdited);
                        var HinhCheck = _context.HinhAnhs.Where(g => g.MaSanPham == SanPhamEdited.MaSanPham).FirstOrDefault();
                        if(HinhCheck!=null)
                        {
                            var ReplaceOne = AddHinh.Where(g=>g.MaSanPham==HinhCheck.MaSanPham).FirstOrDefault();
                            if(ReplaceOne.Data!=HinhCheck.Data)
                            {
                                HinhCheck.Data = ReplaceOne.Data;
                                HinhCheck.TenHinhAnh = ReplaceOne.TenHinhAnh;
                                _context.HinhAnhs.Update(HinhCheck);
                                AddHinh.Remove(ReplaceOne);
                            }                          
                        }    
                        else
                        {
                            var NewOne = AddHinh.Where(g => g.MaSanPham == SanPhamEdited.MaSanPham).FirstOrDefault();
                            _context.HinhAnhs.Add(NewOne);
                            AddHinh.Remove(NewOne);
                        }    
                    }
                    else
                    {
                        _context.SanPhams.Add(item);
                    }
                }
                _context.HinhAnhs.AddRange(AddHinh);
                foreach (var item in SanPhamNotEdited)
                {
                    bool Found = false;
                    for (int i = 0; i < EditCheck.Count(); i++)
                    {
                        if (EditCheck[i].MaSanPham == item.MaSanPham)
                        {
                            Found = true;
                            break;
                        }
                    }
                    if (!Found)
                    {
                        item.SoLuong = item.SoLuongDaBan;
                        _context.SanPhams.Update(item);
                    }
                }
                var HinhDelete = _context.HinhAnhs.Where(g => g.MaSanPham==data[0].ID.Trim()).ToList();
                _context.HinhAnhs.RemoveRange(HinhDelete);
                foreach (var item in data[0].HinhAnhs)
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
            catch (Exception ex)
            {

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
            var KiHieu = _context.LoaiSanPhams
                .Where(g => g.MaLoaiSanPham == data[0].LoaiSanPham)
                .FirstOrDefault();
            int Max = 0;
            if (!string.IsNullOrEmpty(SanPhamCungLoaiMax) && int.TryParse(SanPhamCungLoaiMax, out int parsedMax))
            {
                Max = parsedMax + 1;
            }

            APIResponse response = new APIResponse();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                List<MoTaSanPhamCreateModel> existingData = new List<MoTaSanPhamCreateModel>();
                if (System.IO.File.Exists(_dbPath))
                {
                    var jsonContent = await System.IO.File.ReadAllTextAsync(_dbPath);
                    if (!string.IsNullOrWhiteSpace(jsonContent))
                    {
                        try
                        {
                            existingData = JsonSerializer.Deserialize<List<MoTaSanPhamCreateModel>>(jsonContent) ?? new List<MoTaSanPhamCreateModel>();
                        }
                        catch (JsonException ex)
                        {
                            response.ErrorMessage = ex.Message;
                            response.ResponseCode = 400;
                            return response;
                        }
                    }
                }
                else
                {
                    await System.IO.File.WriteAllTextAsync(_dbPath, "[]");
                }
                string maSanPham = KiHieu.KiHieu.ToString().Trim() + Max.ToString("00000").Trim();

                // Save temporary MoTa data to db.json with MaSanPham and IdMoTa
                int maxIdMoTa = existingData.Any() ? existingData.Max(x => int.Parse(x.IdMoTa)) : 0;
                foreach (var moTa in _tempMoTaData)
                {
                    moTa.MaSanPham = maSanPham;
                    moTa.IdMoTa = (maxIdMoTa+1).ToString();
                    existingData.Add(moTa);
                }

                // Write to db.json
                var options = new JsonSerializerOptions { WriteIndented = true };
                var updatedJson = JsonSerializer.Serialize(existingData, options);
                await System.IO.File.WriteAllTextAsync(_dbPath, updatedJson);
                _tempMoTaData.Clear();
                for (int i = 0; i < data.Count(); i++)
                {
                    foreach (var item in data[i].Details)
                    {
                        SanPham sp = new SanPham
                        {
                            TenSanPham = data[i].TenSanPham,
                            MaSanPham = maSanPham + "_" + data[i].MauSac.Trim() + "_" + item.KichThuoc.Trim(),
                            Gia = item.Gia,
                            GiaNhap = item.GiaNhap,
                            SoLuong = item.SoLuong,
                            ChatLieu = data[i].ChatLieu,
                            MaLoaiSanPham = KiHieu.MaLoaiSanPham,
                            MaThuongHieu = data[i].MaThuongHieu,
                            KichThuoc = item.KichThuoc,
                            NgayTao = DateOnly.FromDateTime(DateTime.Now),
                            TrangThai = 1,
                            Example = true,
                            MoTa = data[i].MoTa,
                            GioiTinh = data[i].GioiTinh,
                            SoLuongDaBan = 0
                        };
                        _context.SanPhams.Add(sp);

                        HinhAnh newHA = new HinhAnh
                        {
                            TenHinhAnh = data[i].TenSanPham,
                            MaSanPham = sp.MaSanPham,
                            Data = item.HinhAnh
                        };
                        _context.HinhAnhs.Add(newHA);
                    }
                }

                foreach (var item in data[0].HinhAnhs)
                {
                    HinhAnh newHA = new HinhAnh
                    {
                        TenHinhAnh = data[0].TenSanPham,
                        MaSanPham = maSanPham,
                        Data = item
                    };
                    _context.HinhAnhs.Add(newHA);
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
        public async Task<APIResponse> DeleteSanPham(string? id)
        {
            APIResponse response = new APIResponse();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var SanPham = _context.SanPhams.Where(g => g.MaSanPham.Contains(id)).ToList();
                foreach (var Sp in SanPham)
                {
                    Sp.TrangThai = 0;
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

        public async Task<List<SanPhamView>> ListSanPhamLQ(string? id)
        {
            List<SanPhamView> listsp = new List<SanPhamView>();
            var nhomSanPham = _context.SanPhams.GroupBy(s => s.MaSanPham.Substring(0, 6)).ToList();

            foreach (var nhom in nhomSanPham)
            {
                var sanPhamDauTien = nhom.First();
                var HinhAnhSanPhamList = _context.HinhAnhs
                    .Where(g => g.MaSanPham.Substring(0, 6) == sanPhamDauTien.MaSanPham.Substring(0, 6))
                    .Select(g => g.Data)
                    .ToList();
                var listMauSac = nhom.Select(sp => sp.MaSanPham.Split('_')[1]).Distinct().ToList();
                var listKichThuoc = nhom.Select(sp => sp.MaSanPham.Split('_')[2]).Distinct().ToList();
                var SoLuongDaBan = nhom.Sum(sp => sp.SoLuongDaBan);
                var tongSoLuong = nhom.Sum(sp => sp.SoLuong) - nhom.Sum(sp => sp.SoLuongDaBan);
                var TenLoai = _context.LoaiSanPhams
                    .Where(g => g.MaLoaiSanPham == sanPhamDauTien.MaLoaiSanPham)
                    .Select(g => g.TenLoaiSanPham)
                    .FirstOrDefault();
                var ThuongHieu = _context.ThuongHieus
                    .Where(g => g.MaThuongHieu == sanPhamDauTien.MaThuongHieu)
                    .Select(g => g.TenThuongHieu)
                    .FirstOrDefault();

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
                    ChatLieu = sanPhamDauTien.ChatLieu,
                    MoTa = sanPhamDauTien.MoTa,
                    SoLuongDaBan = SoLuongDaBan,
                    GioiTinh = sanPhamDauTien.GioiTinh == 0 ? "Nam" : "Nữ",
                    Hot = false
                });
            }           
            var tops = listsp.Where(g => g.ID.Trim() != id.Trim()).OrderByDescending(h=>h.SoLuongDaBan).Take(6).ToList();
            return tops;
        }

        public async Task<APIResponse> MoTaSanPhamCreate(List<MoTaSanPhamCreateModel>? info)
        {
            APIResponse response = new APIResponse();
            try
            {
                if (info == null || info.Count == 0)
                {
                    response.ResponseCode = 400;
                    response.ErrorMessage = "No data provided";
                    return response;
                }

                // Store data in temporary memory
                _tempMoTaData.AddRange(info);

                response.ResponseCode = 200;
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 400;
            }
            return response;
        }
    }
}
