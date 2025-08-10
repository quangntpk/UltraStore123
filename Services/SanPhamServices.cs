using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text.Json;
using UltraStrore.Data;
using UltraStrore.Helper;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Threading.Tasks.Dataflow;

namespace UltraStrore.Services
{
    public class SanPhamServices : ISanPhamServices
    {
        private static int EditOrCreate = 0;
        private static List<MoTaSanPhamCreateModel> _tempMoTaData = new List<MoTaSanPhamCreateModel>();
        private readonly ApplicationDbContext _context;
        private readonly string _dbPath;
        private readonly string _htPath;
        private readonly string _loaiSanPhamPath;
        private readonly IKhuyenMaiServices _serviceKM;

        public SanPhamServices(ApplicationDbContext context, IWebHostEnvironment env, IKhuyenMaiServices services)
        {
            _context = context;
            _dbPath = Path.Combine(env.WebRootPath, "db.json");
            _htPath = Path.Combine(Directory.GetCurrentDirectory(), "DanhMuc", "attachHashTag.json");
            _loaiSanPhamPath = Path.Combine(Directory.GetCurrentDirectory(), "DanhMuc", "loaisanpham.json");
            _serviceKM = services;
        }

        private async Task<List<LoaiSanPham>> LoadLoaiSanPhamAsync()
        {
            if (File.Exists(_loaiSanPhamPath))
            {
                var jsonContent = await File.ReadAllTextAsync(_loaiSanPhamPath);
                if (!string.IsNullOrWhiteSpace(jsonContent))
                {
                    return JsonSerializer.Deserialize<List<LoaiSanPham>>(jsonContent) ?? new List<LoaiSanPham>();
                }
            }
            return new List<LoaiSanPham>();
        }

        public async Task<List<SanPhamView>> ListSanPham(string id)
        {
            var KhuyenMaiView = (await _serviceKM.ListKhuyenMaiAdmin(null)).ToList();
            int KhuyenMaiChung = KhuyenMaiView.Where(g => g.PercentChung.HasValue).OrderByDescending(g => g.PercentChung).Select(g => g.PercentChung).FirstOrDefault() ?? 0;
            List<SanPhamView> listsp = new List<SanPhamView>();
            var nhomSanPham = _context.SanPhams.GroupBy(s => s.MaSanPham.Substring(0, 6)).ToList();
            string hashtag = File.ReadAllText(_htPath);
            var FullHashTag = JsonSerializer.Deserialize<List<HashTagSp>>(hashtag);
            var loaiSanPhams = await LoadLoaiSanPhamAsync();

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
                var TenLoai = loaiSanPhams
                    .Where(g => g.MaLoaiSanPham == sanPhamDauTien.MaLoaiSanPham)
                    .Select(g => g.TenLoaiSanPham)
                    .FirstOrDefault();
                var ThuongHieu = _context.ThuongHieus
                    .Where(g => g.MaThuongHieu == sanPhamDauTien.MaThuongHieu)
                    .Select(g => g.TenThuongHieu)
                    .FirstOrDefault();
                int GiaBan = nhom.Where(g => g.Gia.HasValue).OrderBy(g => g.Gia).Select(g => g.Gia.Value).FirstOrDefault();
                var ListHashTag = FullHashTag.Where(g => sanPhamDauTien.MaSanPham.Contains(g.IDSanPham.Trim())).SelectMany(g => g.ListHashTag).ToList();
                int MaxKM = 0;
                var KhuyenMaiRieng = KhuyenMaiView.Where(g => !g.PercentChung.HasValue).ToList();
                foreach (var KM in KhuyenMaiRieng)
                {
                    foreach (var dis in KM.DanhSachKhuyenMai)
                    {
                        if (dis.IdSanPham != null && dis.IdSanPham.Trim() == sanPhamDauTien.MaSanPham.Substring(0, 6).Trim())
                        {
                            if (dis.Percent > MaxKM)
                                MaxKM = dis.Percent ?? MaxKM;
                        }
                    }
                }
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
                    GioiTinh = sanPhamDauTien.GioiTinh == 0
                        ? "Nam"
                        : sanPhamDauTien.GioiTinh == 1
                            ? "Nữ"
                            : "Unisex",
                    Hot = SoLuongDaBan > 10 ? true : false,
                    ListHashTag = ListHashTag,
                    KhuyenMaiMax = KhuyenMaiChung > MaxKM ? KhuyenMaiChung : MaxKM
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
            List<SanPhamView2> Result = new List<SanPhamView2>();
            var ListSanPham = await _context.SanPhams
                .Include(sp => sp.MaThuongHieuNavigation)
                .Where(g => g.MaSanPham.Contains(id))
                .ToListAsync();
            string hashtag = File.ReadAllText(_htPath);
            var FullHashTag = JsonSerializer.Deserialize<List<HashTagSp>>(hashtag);
            var loaiSanPhams = await LoadLoaiSanPhamAsync();

            if (ListSanPham != null && ListSanPham.Count() > 0)
            {
                foreach (var item in ListSanPham)
                {
                    var ListHashTag = FullHashTag.Where(g => item.MaSanPham.Contains(g.IDSanPham.Trim())).SelectMany(g => g.ListHashTag).ToList() ?? new List<DetailHashTagSP>();
                    var TenLoai = loaiSanPhams
                        .Where(g => g.MaLoaiSanPham == item.MaLoaiSanPham)
                        .Select(g => g.TenLoaiSanPham)
                        .FirstOrDefault();
                    Result.Add(new SanPhamView2
                    {
                        GiaNhap = item.GiaNhap ?? 0,
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
                        LoaiSanPham = TenLoai,
                        ListHashTag = ListHashTag,
                    });
                }
            }
            return Result;
        }

        public async Task<List<SanPhamByIDSorted>> SanPhamByIDSorteds(string? id)
        {
            var KhuyenMaiView = (await _serviceKM.ListKhuyenMaiAdmin(null)).ToList();
            int KhuyenMaiChung = KhuyenMaiView.Where(g => g.PercentChung.HasValue).OrderByDescending(g => g.PercentChung).Select(g => g.PercentChung).FirstOrDefault() ?? 0;
            List<SanPhamByIDSorted> listsp = new List<SanPhamByIDSorted>();
            var nhomSanPham = _context.SanPhams.Where(g => g.MaSanPham.Contains(id)).GroupBy(s => s.MaSanPham.Substring(0, 13)).ToList();
            var loaiSanPhams = await LoadLoaiSanPhamAsync();

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
                    ed.GiaNhap = item.GiaNhap ?? 0;
                    ed.HinhAnh = _context.HinhAnhs.Where(g => g.MaSanPham == item.MaSanPham).Select(h => h.Data).FirstOrDefault();
                    detailedit.Add(ed);
                }
                var tongSoLuong = nhom.Sum(sp => sp.SoLuong);
                var MaLoai = loaiSanPhams.Where(g => g.MaLoaiSanPham == sanPhamDauTien.MaLoaiSanPham).Select(g => g.TenLoaiSanPham).FirstOrDefault();
                var ThuongHieu = _context.ThuongHieus.Where(g => g.MaThuongHieu == sanPhamDauTien.MaThuongHieu).Select(g => g.TenThuongHieu).FirstOrDefault();
                var HinhAnh = _context.HinhAnhs.Where(g => g.MaSanPham.Trim() == sanPhamDauTien.MaSanPham.Substring(0, 6).Trim()).Select(g => g.Data).ToList();
                string json = File.ReadAllText(_dbPath);
                var FullChiTiet = JsonSerializer.Deserialize<List<MoTaSanPhamCreateModel>>(json);
                string hashtag = File.ReadAllText(_htPath);
                var FullHashTag = JsonSerializer.Deserialize<List<HashTagSp>>(hashtag);
                List<DetailHashTagSP> DHTSP = FullHashTag.Where(g => sanPhamDauTien.MaSanPham.Contains(g.IDSanPham)).SelectMany(g => g.ListHashTag).ToList();
                var MoTaCT = FullChiTiet.Where(g => g.MaSanPham == id.Substring(0, 6)).FirstOrDefault();
                int MaxKM = 0;
                var KhuyenMaiRieng = KhuyenMaiView.Where(g => !g.PercentChung.HasValue).ToList();
                foreach (var KM in KhuyenMaiRieng)
                {
                    foreach (var dis in KM.DanhSachKhuyenMai)
                    {
                        if (dis.IdSanPham != null && dis.IdSanPham.Trim() == sanPhamDauTien.MaSanPham.Substring(0, 6).Trim())
                        {
                            if (dis.Percent > MaxKM)
                                MaxKM = dis.Percent ?? MaxKM;
                        }
                    }
                }
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
                    ListHashTag = DHTSP,
                    KhuyenMaiMax = KhuyenMaiChung > MaxKM ? KhuyenMaiChung : MaxKM
                });
            }
            return listsp;
        }

        public async Task<APIResponse> EditSanPham(FullInfoSanPhamEdit info)
        {
            var SanPhamNotEdited = _context.SanPhams.Where(g => g.MaSanPham.Contains(info.data[0].ID)).ToList();
            List<SanPham> EditCheck = new List<SanPham>();
            List<HinhAnh> AddHinh = new List<HinhAnh>();
            APIResponse response = new APIResponse();
            using var transaction = await _context.Database.BeginTransactionAsync();
            List<MoTaSanPhamCreateModel> existingData = new List<MoTaSanPhamCreateModel>();
            if (File.Exists(_dbPath))
            {
                var jsonContent = await File.ReadAllTextAsync(_dbPath);
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
                await File.WriteAllTextAsync(_dbPath, "[]");
            }
            List<HashTagSp> ListHT = new List<HashTagSp>();
            if (File.Exists(_htPath))
            {
                var jsonContent = await File.ReadAllTextAsync(_htPath);
                if (!string.IsNullOrWhiteSpace(jsonContent))
                {
                    try
                    {
                        ListHT = JsonSerializer.Deserialize<List<HashTagSp>>(jsonContent) ?? new List<HashTagSp>();
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
                await File.WriteAllTextAsync(_htPath, "[]");
            }
            string maSanPham = info.data[0].ID.Substring(0, 6);
            var temp = _tempMoTaData;
            if (temp.Count > 0 && temp != null && (temp[0].MaSanPham != null && temp[0].MaSanPham.Trim().Length > 0))
            {
                existingData.RemoveAll(g => g.MaSanPham.Trim() == maSanPham.Trim());
            }
            ListHT.RemoveAll(g => g.IDSanPham.Trim() == maSanPham.Trim());
            ListHT.Add(new HashTagSp
            {
                IDSanPham = maSanPham,
                ListHashTag = info.hashtaglist
            });
            int maxIdMoTa = existingData.Any() ? existingData.Max(x => int.Parse(x.IdMoTa)) : 0;
            foreach (var moTa in _tempMoTaData)
            {
                if (moTa != null)
                {
                    moTa.MaSanPham = maSanPham;
                    moTa.IdMoTa = (maxIdMoTa + 1).ToString();
                    existingData.Add(moTa);
                    maxIdMoTa++;
                }
            }
            var options = new JsonSerializerOptions { WriteIndented = true };
            var updatedJson = JsonSerializer.Serialize(existingData, options);
            await File.WriteAllTextAsync(_dbPath, updatedJson);
            var options2 = new JsonSerializerOptions { WriteIndented = true };
            var updatedJson2 = JsonSerializer.Serialize(ListHT, options2);
            await File.WriteAllTextAsync(_htPath, updatedJson2);
            _tempMoTaData.Clear();
            try
            {
                for (int i = 0; i < info.data.Count(); i++)
                {
                    for (int j = 0; j < info.data[i].Details.Count(); j++)
                    {
                        var tem = info.data[i].Details[j];
                        SanPham edit = new SanPham();
                        edit.MaSanPham = info.data[i].ID.Trim() + "_" + info.data[i].MauSac.Trim() + "_" + info.data[i].Details[j].KichThuoc.Trim();
                        edit.TenSanPham = info.data[i].TenSanPham.Trim();
                        edit.SoLuong = info.data[i].Details[j].SoLuong;
                        edit.Gia = info.data[i].Details[j].Gia;
                        edit.GiaNhap = info.data[i].Details[j].GiaNhap;
                        edit.MaThuongHieu = info.data[i].MaThuongHieu;
                        edit.MaLoaiSanPham = info.data[i].LoaiSanPham;
                        edit.KichThuoc = info.data[i].Details[j].KichThuoc.Trim();
                        edit.NgayTao = SanPhamNotEdited[0].NgayTao;
                        edit.TrangThai = 1;
                        edit.MoTa = info.data[i].MoTa ?? null;
                        edit.Example = true;
                        edit.ChatLieu = info.data[i].ChatLieu;
                        EditCheck.Add(edit);
                        HinhAnh hinh = new HinhAnh();
                        hinh.MaSanPham = edit.MaSanPham;
                        hinh.Data = info.data[i].Details[j].HinhAnh;
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
                        if (HinhCheck != null)
                        {
                            var ReplaceOne = AddHinh.Where(g => g.MaSanPham == HinhCheck.MaSanPham).FirstOrDefault();
                            if (ReplaceOne.Data != HinhCheck.Data)
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
                var HinhDelete = _context.HinhAnhs.Where(g => g.MaSanPham == info.data[0].ID.Trim()).ToList();
                _context.HinhAnhs.RemoveRange(HinhDelete);
                foreach (var item in info.data[0].HinhAnhs)
                {
                    HinhAnh newHinhAnh = new HinhAnh();
                    newHinhAnh.TenHinhAnh = info.data[0].TenSanPham;
                    newHinhAnh.Data = item;
                    newHinhAnh.MaSanPham = info.data[0].ID;
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

        public async Task<APIResponse> CreateSanPham(FullCreateSanPham info)
        {
            var loaiSanPhams = await LoadLoaiSanPhamAsync();
            var SanPhamCungLoaiMax = _context.SanPhams
                .Where(g => g.MaLoaiSanPham == info.data[0].LoaiSanPham)
                .Select(g => g.MaSanPham.Substring(1, 5))
                .OrderByDescending(x => x)
                .FirstOrDefault();
            var KiHieu = loaiSanPhams
                .Where(g => g.MaLoaiSanPham == info.data[0].LoaiSanPham)
                .Select(g => g.KiHieu)
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
                if (File.Exists(_dbPath))
                {
                    var jsonContent = await File.ReadAllTextAsync(_dbPath);
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
                    await File.WriteAllTextAsync(_dbPath, "[]");
                }
                List<HashTagSp> ListHT = new List<HashTagSp>();
                if (File.Exists(_htPath))
                {
                    var jsonContent = await File.ReadAllTextAsync(_htPath);
                    if (!string.IsNullOrWhiteSpace(jsonContent))
                    {
                        try
                        {
                            ListHT = JsonSerializer.Deserialize<List<HashTagSp>>(jsonContent) ?? new List<HashTagSp>();
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
                    await File.WriteAllTextAsync(_htPath, "[]");
                }

                string maSanPham = KiHieu?.ToString().Trim() + Max.ToString("00000").Trim();
                ListHT.Add(new HashTagSp
                {
                    IDSanPham = maSanPham,
                    ListHashTag = info.ListHashTag
                });
                int maxIdMoTa = existingData.Any() ? existingData.Max(x => int.Parse(x.IdMoTa)) : 0;
                foreach (var moTa in _tempMoTaData)
                {
                    moTa.MaSanPham = maSanPham;
                    moTa.IdMoTa = (maxIdMoTa + 1).ToString();
                    existingData.Add(moTa);
                }

                var options = new JsonSerializerOptions { WriteIndented = true };
                var updatedJson = JsonSerializer.Serialize(existingData, options);
                await File.WriteAllTextAsync(_dbPath, updatedJson);
                var options2 = new JsonSerializerOptions { WriteIndented = true };
                var updatedJson2 = JsonSerializer.Serialize(ListHT, options2);
                await File.WriteAllTextAsync(_htPath, updatedJson2);
                _tempMoTaData.Clear();

                for (int i = 0; i < info.data.Count(); i++)
                {
                    foreach (var item in info.data[i].Details)
                    {
                        SanPham sp = new SanPham
                        {
                            TenSanPham = info.data[i].TenSanPham,
                            MaSanPham = maSanPham + "_" + info.data[i].MauSac.Trim() + "_" + item.KichThuoc.Trim(),
                            Gia = item.Gia,
                            GiaNhap = item.GiaNhap,
                            SoLuong = item.SoLuong,
                            ChatLieu = info.data[i].ChatLieu,
                            MaLoaiSanPham = info.data[0].LoaiSanPham,
                            MaThuongHieu = info.data[i].MaThuongHieu,
                            KichThuoc = item.KichThuoc,
                            NgayTao = DateOnly.FromDateTime(DateTime.Now),
                            TrangThai = 1,
                            Example = true,
                            MoTa = info.data[i].MoTa,
                            GioiTinh = info.data[i].GioiTinh,
                            SoLuongDaBan = 0
                        };
                        _context.SanPhams.Add(sp);

                        HinhAnh newHA = new HinhAnh
                        {
                            TenHinhAnh = info.data[i].TenSanPham,
                            MaSanPham = sp.MaSanPham,
                            Data = item.HinhAnh
                        };
                        _context.HinhAnhs.Add(newHA);
                    }
                }

                foreach (var item in info.data[0].HinhAnhs)
                {
                    HinhAnh newHA = new HinhAnh
                    {
                        TenHinhAnh = info.data[0].TenSanPham,
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
            var KhuyenMaiView = (await _serviceKM.ListKhuyenMaiAdmin(null)).ToList();
            int KhuyenMaiChung = KhuyenMaiView.Where(g => g.PercentChung.HasValue).OrderByDescending(g => g.PercentChung).Select(g => g.PercentChung).FirstOrDefault() ?? 0;
            List<SanPhamView> listsp = new List<SanPhamView>();
            var nhomSanPham = _context.SanPhams.GroupBy(s => s.MaSanPham.Substring(0, 6)).ToList();
            string hashtag = File.ReadAllText(_htPath);
            var FullHashTag = JsonSerializer.Deserialize<List<HashTagSp>>(hashtag);
            var loaiSanPhams = await LoadLoaiSanPhamAsync();

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
                var TenLoai = loaiSanPhams
                    .Where(g => g.MaLoaiSanPham == sanPhamDauTien.MaLoaiSanPham)
                    .Select(g => g.TenLoaiSanPham)
                    .FirstOrDefault();
                var ThuongHieu = _context.ThuongHieus
                    .Where(g => g.MaThuongHieu == sanPhamDauTien.MaThuongHieu)
                    .Select(g => g.TenThuongHieu)
                    .FirstOrDefault();
                int GiaBan = nhom.Where(g => g.Gia.HasValue).OrderBy(g => g.Gia).Select(g => g.Gia.Value).FirstOrDefault();
                var ListHashTag = FullHashTag.Where(g => sanPhamDauTien.MaSanPham.Contains(g.IDSanPham.Trim())).SelectMany(g => g.ListHashTag).ToList();
                int MaxKM = 0;
                var KhuyenMaiRieng = KhuyenMaiView.Where(g => !g.PercentChung.HasValue).ToList();
                foreach (var KM in KhuyenMaiRieng)
                {
                    foreach (var dis in KM.DanhSachKhuyenMai)
                    {
                        if (dis.IdSanPham != null && dis.IdSanPham.Trim() == sanPhamDauTien.MaSanPham.Substring(0, 6).Trim())
                        {
                            if (dis.Percent > MaxKM)
                                MaxKM = dis.Percent ?? MaxKM;
                        }
                    }
                }
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
                    GioiTinh = sanPhamDauTien.GioiTinh == 0
                        ? "Nam"
                        : sanPhamDauTien.GioiTinh == 1
                            ? "Nữ"
                            : "Unisex",
                    Hot = SoLuongDaBan > 10 ? true : false,
                    ListHashTag = ListHashTag,
                    KhuyenMaiMax = KhuyenMaiChung > MaxKM ? KhuyenMaiChung : MaxKM
                });
            }

            if (string.IsNullOrEmpty(id))
            {
                var topList = listsp.OrderByDescending(g => g.SoLuongDaBan).ToList();
                return topList;
            }
            var tops = listsp.Where(g => g.ID.Trim() != id.Trim()).OrderByDescending(h => h.SoLuongDaBan).Take(6).ToList();
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

        public async Task<List<ProductByDateReport>> ReportByDate(SelectDateProductView? info)
        {
            APIResponse response = new APIResponse();
            List<ProductByDateReport> ListProduct = new List<ProductByDateReport>();
            var SpInTime = await (from dhs in _context.DonHangSupports
                                  join ctdh in _context.ChiTietDonHangs on dhs.ChiTietGioHang equals ctdh.MaCtdh
                                  join ctc in _context.ChiTietComBos on dhs.MaChiTietCombo equals ctc.MaChiTietComBo
                                  join cb in _context.ComBoSanPhams on ctc.MaComBo equals cb.MaComBo
                                  join donHang in _context.DonHangs on ctdh.MaDonHang equals donHang.MaDonHang
                                  where (ctdh.MaCombo == cb.MaComBo
                                      && donHang.TrangThaiDonHang == TrangThaiDonHang.DaGiaoHang
                                      && donHang.NgayDat.HasValue
                                      && DateOnly.FromDateTime(donHang.NgayDat.Value) >= info.BatDau
                                      && DateOnly.FromDateTime(donHang.NgayDat.Value) <= info.KetThuc)
                                  select dhs).ToListAsync();
            var SpOutTime = await (from dhs in _context.DonHangSupports
                                   join ctdh in _context.ChiTietDonHangs on dhs.ChiTietGioHang equals ctdh.MaCtdh
                                   join ctc in _context.ChiTietComBos on dhs.MaChiTietCombo equals ctc.MaChiTietComBo
                                   join cb in _context.ComBoSanPhams on ctc.MaComBo equals cb.MaComBo
                                   join donHang in _context.DonHangs on ctdh.MaDonHang equals donHang.MaDonHang
                                   where (ctdh.MaCombo == cb.MaComBo
                                       && donHang.TrangThaiDonHang == TrangThaiDonHang.DaGiaoHang
                                       && donHang.NgayDat.HasValue
                                       && DateOnly.FromDateTime(donHang.NgayDat.Value) <= info.BatDau)
                                   select dhs).ToListAsync();
            var loaiSanPhams = await LoadLoaiSanPhamAsync();

            try
            {
                foreach (var item in info.ID)
                {
                    var ListSp = await _context.SanPhams
                        .Include(sp => sp.MaThuongHieuNavigation)
                        .Where(g => g.MaSanPham.Contains(item))
                        .ToListAsync();

                    foreach (var sp in ListSp)
                    {
                        ProductByDateReport sanpham = new ProductByDateReport();
                        sanpham.ID = sp.MaSanPham;
                        sanpham.TenSanPham = sp.TenSanPham;
                        sanpham.GiaNhap = sp.GiaNhap ?? 0;
                        sanpham.GiaXuat = sp.Gia ?? 0;
                        sanpham.ThuongHieu = sp.MaThuongHieuNavigation.TenThuongHieu;
                        sanpham.ChatLieu = sp.ChatLieu;
                        sanpham.LoaiSanPham = loaiSanPhams
                            .Where(g => g.MaLoaiSanPham == sp.MaLoaiSanPham)
                            .Select(g => g.TenLoaiSanPham)
                            .FirstOrDefault();
                        sanpham.Dvt = "Chiếc";
                        int SoLuongBanDau = sp.SoLuong ?? 0;
                        var maSanPham = sp.MaSanPham;

                        var chiTietDonHangs = _context.ChiTietDonHangs
                            .Join(_context.DonHangs.Where(dh => dh.NgayDat.HasValue),
                                  ct => ct.MaDonHang,
                                  dh => dh.MaDonHang,
                                  (ct, dh) => new { ChiTiet = ct, DonHang = dh })
                            .Where(x => x.ChiTiet.MaSanPham == maSanPham
                                && DateOnly.FromDateTime(x.DonHang.NgayDat.Value) >= info.BatDau
                                && DateOnly.FromDateTime(x.DonHang.NgayDat.Value) <= info.KetThuc
                                && x.DonHang.TrangThaiDonHang == TrangThaiDonHang.DaGiaoHang)
                            .Select(x => x.ChiTiet)
                            .ToList();
                        int SoLuongBan = 0;
                        foreach (var SumOn in chiTietDonHangs)
                        {
                            SoLuongBan = SoLuongBan + SumOn.SoLuong.GetValueOrDefault();
                        }
                        var ChiTietDonHangsBF = _context.ChiTietDonHangs
                            .Join(_context.DonHangs.Where(dh => dh.NgayDat.HasValue),
                                  ct => ct.MaDonHang,
                                  dh => dh.MaDonHang,
                                  (ct, dh) => new { ChiTiet = ct, DonHang = dh })
                            .Where(x => x.ChiTiet.MaSanPham == maSanPham
                                && DateOnly.FromDateTime(x.DonHang.NgayDat.Value) <= info.BatDau
                                && x.DonHang.TrangThaiDonHang == TrangThaiDonHang.DaGiaoHang)
                            .Select(x => x.ChiTiet)
                            .ToList();
                        int SoLuongBanTrcDo = 0;
                        foreach (var SumOff in ChiTietDonHangsBF)
                        {
                            SoLuongBanTrcDo = SoLuongBanTrcDo + SumOff.SoLuong.GetValueOrDefault();
                        }
                        List<int> SoLuongDaBanCombo = SpInTime.Where(g => g.MaSanPham == maSanPham).Select(g => g.SoLuong).ToList();
                        List<int> SoLuongDaBanComboBF = SpOutTime.Where(g => g.MaSanPham == maSanPham).Select(g => g.SoLuong).ToList();
                        foreach (var cbSold in SoLuongDaBanCombo)
                        {
                            SoLuongBan = cbSold != null ? SoLuongBan + cbSold : SoLuongBan;
                        }
                        foreach (var cbSold in SoLuongDaBanComboBF)
                        {
                            SoLuongBanTrcDo = cbSold != null ? SoLuongBanTrcDo + cbSold : SoLuongBan;
                        }
                        sanpham.SLNhap = SoLuongBanDau - SoLuongBanTrcDo;
                        sanpham.SLXuat = SoLuongBan;
                        sanpham.TonCuoi = SoLuongBanDau - SoLuongBan - SoLuongBanTrcDo;
                        sanpham.GiaTonCuoi = sanpham.GiaNhap;
                        ListProduct.Add(sanpham);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception if needed
            }
            return ListProduct;
        }
    }

 
}