using Azure;
using Google.Apis.Sheets.v4.Data;
using Microsoft.EntityFrameworkCore;
using UltraStrore.Data;
using UltraStrore.Data.Temp;
using UltraStrore.Helper;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Services
{
    public class CartServices : ICartServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IKhuyenMaiServices _servicesKM;

        public CartServices(ApplicationDbContext context, IKhuyenMaiServices services)
        {
            _servicesKM = services;
            _context = context;
        }    
        public async Task<GioHangView> GioHangViews(string? MaKhachHang)
        {
            GioHangView GioHangView = new GioHangView();
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var KhuyenMaiView = (await _servicesKM.ListKhuyenMaiUser(null)).ToList();
                var KhuyenMaiChung = KhuyenMaiView.Where(g => g.PercentChung.HasValue).OrderByDescending(g => g.PercentChung).FirstOrDefault();
                int MaxPercentChung = 0;
                int MaKhuyenMaiChung = -1;
                if(KhuyenMaiChung != null)
                {
                    MaxPercentChung = KhuyenMaiChung.PercentChung??0;
                    MaKhuyenMaiChung = KhuyenMaiChung.ID;
                }    

                var KhuyenMaiRieng = KhuyenMaiView.Where(g => !g.PercentChung.HasValue).ToList();
                if(MaKhachHang!=null)
                {
                    var GioHang = _context.GioHangs.Where(g => g.MaNguoiDung == MaKhachHang).FirstOrDefault();
                    var CTGH = _context.ChiTietGioHangs.Where(g => g.MaGioHang == GioHang.MaGioHang).ToList();
                    int MaKhuyenMai = MaKhuyenMaiChung;
                    int MaxPercent = 0;
                    if (GioHang != null)
                    {
                        var CTGHSanPham = _context.ChiTietGioHangs.Where(g => g.MaGioHang == GioHang.MaGioHang && !string.IsNullOrEmpty(g.MaSanPham)).ToList();
                        foreach (var item in CTGH)
                        {
                            if (item.MaKhuyenMai.HasValue && item.MaKhuyenMai != null && item.MaKhuyenMai!=-1)
                            {                               
                                var _check = _context.KhuyenMais.Where(g => g.ID == item.MaKhuyenMai).FirstOrDefault();
                                if (_check != null)
                                {
                                    if (_check.KetThuc <= DateOnly.FromDateTime(DateTime.Now))
                                    {
                                        item.DeadTime = true;
                                        item.Percent = 0;
                                        item.MaKhuyenMai = MaxPercentChung;
                                        if (item.MaSanPham != null)
                                        {
                                            MaxPercent = MaxPercentChung;
                                            foreach (var item3 in KhuyenMaiRieng)
                                            {
                                                foreach (var item4 in item3.DanhSachKhuyenMai)
                                                {
                                                    if (item4.IdSanPham != null && item.MaSanPham != null && item.MaSanPham.Contains(item4.IdSanPham))
                                                    {
                                                        if (item3.NgayKetThuc <= DateOnly.FromDateTime(DateTime.Now))
                                                        {

                                                        }
                                                        else
                                                        {
                                                            if (item4.Percent >= MaxPercent)
                                                            {
                                                                MaxPercent = item4.Percent ?? MaxPercent;
                                                                MaKhuyenMai = item3.ID;
                                                                item.DeadTime = false;
                                                            }

                                                        }
                                                    }
                                                }
                                            }
                                            item.MaKhuyenMai = MaKhuyenMai;
                                            item.Percent = MaxPercent;
                                            item.ThanhTien = item.ThanhTien * (100 - item.Percent) / 100;
                                            _context.ChiTietGioHangs.Update(item);
                                            await _context.SaveChangesAsync();
                                        }
                                        else
                                        {
                                            MaxPercent = MaxPercentChung;
                                            foreach (var item3 in KhuyenMaiRieng)
                                            {
                                                foreach (var item4 in item3.DanhSachKhuyenMai)
                                                {
                                                    if (item4.IdCombo != null && item.MaCombo != null && item4.IdCombo == item.MaCombo)
                                                    {
                                                        if (item3.NgayKetThuc <= DateOnly.FromDateTime(DateTime.Now))
                                                        {

                                                        }
                                                        else
                                                        {
                                                            if (item4.Percent >= MaxPercent)
                                                            {
                                                                MaxPercent = item4.Percent ?? MaxPercent;
                                                                MaKhuyenMai = item3.ID;
                                                                item.DeadTime = false;
                                                            }

                                                        }
                                                    }
                                                }
                                            }
                                            item.MaKhuyenMai = MaKhuyenMai;
                                            item.Percent = MaxPercent;
                                            item.ThanhTien = item.ThanhTien * (100 - item.Percent) / 100;
                                            _context.ChiTietGioHangs.Update(item);
                                            await _context.SaveChangesAsync();
                                        }
                                    }
                                }    
                            }
                            else
                            {
                                item.DeadTime = true;
                                item.Percent = 0;
                                item.MaKhuyenMai = MaxPercentChung;
                                if (item.MaSanPham != null)
                                {
                                    MaxPercent =MaxPercentChung;
                                    foreach (var item3 in KhuyenMaiRieng)
                                    {
                                        foreach (var item4 in item3.DanhSachKhuyenMai)
                                        {
                                            if (item4.IdSanPham != null && item.MaSanPham != null && item.MaSanPham.Contains(item4.IdSanPham))
                                            {
                                                if (item3.NgayKetThuc <= DateOnly.FromDateTime(DateTime.Now))
                                                {

                                                }
                                                else
                                                {
                                                    if (item4.Percent >= MaxPercent)
                                                    {
                                                        MaxPercent = item4.Percent ?? MaxPercent;
                                                        MaKhuyenMai = item3.ID;
                                                        item.DeadTime = false;
                                                    }

                                                }
                                            }
                                        }
                                    }
                                    if (MaKhuyenMai != -1)
                                    {
                                        item.MaKhuyenMai = MaKhuyenMai;
                                        item.Percent = MaxPercent;
                                        int SL = item.SoLuong ?? 0;
                                        int Gia = item.Gia ?? 0;
                                        int NewTien = (SL * Gia) * (100 - MaxPercent) / 100;
                                        item.ThanhTien = NewTien != 0 ? NewTien : item.SoLuong * item.Gia;
                                    }
                                }
                                else
                                {
                                    MaxPercent = MaxPercentChung;
                                    foreach (var item3 in KhuyenMaiRieng)
                                    {
                                        foreach (var item4 in item3.DanhSachKhuyenMai)
                                        {
                                            if (item4.IdCombo != null && item.MaCombo != null && item4.IdCombo == item.MaCombo)
                                            {
                                                if (item3.NgayKetThuc <= DateOnly.FromDateTime(DateTime.Now))
                                                {

                                                }
                                                else
                                                {
                                                    if (item4.Percent >= MaxPercent)
                                                    {
                                                        MaxPercent = item4.Percent ?? MaxPercent;
                                                        MaKhuyenMai = item3.ID;
                                                        item.DeadTime = false;
                                                    }

                                                }
                                            }
                                        }
                                    }
                                    item.MaKhuyenMai = MaKhuyenMai;
                                    item.Percent = MaxPercent;
                                    item.ThanhTien = item.ThanhTien * (100 - item.Percent) / 100;
                                    _context.ChiTietGioHangs.Update(item);
                                    await _context.SaveChangesAsync();
                                }
                            }
                            var test = item;
                            int j = 0;
                            if(item.MaKhuyenMai==-1)
                            {
                                item.MaKhuyenMai = null;
                            }    
                            _context.ChiTietGioHangs.Update(item);
                            await _context.SaveChangesAsync();
                        }
                        var CTGHSanPham2 = _context.ChiTietGioHangs.Where(g => g.MaGioHang == GioHang.MaGioHang && !string.IsNullOrEmpty(g.MaSanPham)).ToList();
                        List<ChiTietGioHangSanPhamView> DetailSanPhamView = new List<ChiTietGioHangSanPhamView>();
                        foreach (var item in CTGHSanPham2)
                        {
                            var sp = _context.SanPhams.Where(g => g.MaSanPham == item.MaSanPham).FirstOrDefault();
                            var test = item;
                            ChiTietGioHangSanPhamView spview = new ChiTietGioHangSanPhamView();
                            spview.ChiTietGioHangSanPham = item.MaCtgh;
                            spview.IDSanPham = item.MaSanPham;
                            spview.TenSanPham = sp.TenSanPham;
                            spview.TienSanPham = (100 - test.Percent) * test.Gia / 100;
                            spview.MauSac = sp.MaSanPham.Split('_')[1];
                            spview.KickThuoc = sp.MaSanPham.Split('_')[2];
                            spview.SoLuong = item.SoLuong ?? 0;
                            spview.HinhAnh = _context.HinhAnhs.Where(g => g.MaSanPham.Trim() == sp.MaSanPham.Substring(0, 6).Trim()).Select(g => g.Data).FirstOrDefault();
                            DetailSanPhamView.Add(spview);
                        }
                        GioHangView.CTGHSanPhamView = DetailSanPhamView;
                        var CTGHCombo = _context.ChiTietGioHangs.Where(g => g.MaGioHang == GioHang.MaGioHang && g.MaCombo != null).ToList();
                        List<ChiTietGioHangComboView> DetailComboView = new List<ChiTietGioHangComboView>();
                        foreach (var item in CTGHCombo)
                        {
                            ChiTietGioHangComboView cbview = new ChiTietGioHangComboView();
                            List<SanPhamInGioHangCombo> splist = new List<SanPhamInGioHangCombo>();
                            var sanpham = _context.GioHangSupports.Where(g => g.ChiTietGioHang == item.MaCtgh).ToList();
                            for (int i = 0; i < sanpham.Count; i++)
                            {
                                SanPhamInGioHangCombo newsp = new SanPhamInGioHangCombo();
                                newsp.HinhAnh = _context.HinhAnhs.Where(g => sanpham[i].MaSanPham.Contains(g.MaSanPham)).Select(g => g.Data).AsNoTracking().FirstOrDefault();
                                newsp.MaSanPham = sanpham[i].MaSanPham;
                                newsp.TenSanPham = _context.SanPhams.Where(g => g.MaSanPham.Trim() == sanpham[i].MaSanPham.Trim()).Select(g => g.TenSanPham).FirstOrDefault();
                                newsp.SoLuong = sanpham[i].SoLuong;
                                newsp.Version = sanpham[i].Version;
                                splist.Add(newsp);
                            }
                            cbview.ChiTietGioHangCombo = item.MaCtgh;
                            cbview.SanPhamList = splist;
                            cbview.IDCombo = item.MaCombo ?? 0;
                            var ComboName = _context.ComBoSanPhams.Where(g => g.MaComBo == item.MaCombo).FirstOrDefault();
                            cbview.TenCombo = ComboName.TenComBo;
                            cbview.SoLuong = item.SoLuong ?? 0;
                            cbview.HinhAnh = ComboName.HinhAnh;
                            cbview.Gia = item.Gia * (100 - item.Percent) / 100;
                            DetailComboView.Add(cbview);
                        }
                        GioHangView.CTGHComboView = DetailComboView;
                        if (MaKhachHang != null)
                            GioHangView.IDNguoiDung = MaKhachHang;
                        GioHangView.ID = GioHang.MaGioHang;
                        await transaction.CommitAsync();

                    }
                    return GioHangView;
                }
                return GioHangView;
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                return GioHangView;
            }

        }

        public async Task<APIResponse> ThemSanPham(ChiTietGioHangSanPhamCreate info)
        {
            APIResponse response = new APIResponse();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int MaKhuyenMai = -1;
                bool DeadTime=false;
                int Percent = 0;
                var KhuyenMaiView = (await _servicesKM.ListKhuyenMaiAdmin(null)).ToList();
                var KhuyenMaiThis = KhuyenMaiView.Where(g => g.ID == info.MaKhuyenMai).FirstOrDefault(); 
                string MaSanPham = info.IDSanPham.Trim() + "_" + info.MauSac.Trim() + "_" + info.KichThuoc.Trim();
                var SanPham = _context.SanPhams.Where(g => g.MaSanPham == MaSanPham).FirstOrDefault();
                var item = _context.NguoiDungs.Where(g => g.MaNguoiDung == info.IDNguoiDung).FirstOrDefault();
                if (item == null)
                {
                    response.Result = "Lỗi";
                    response.ResponseCode = 401;
                }
                else
                {
                    var GioHangCustomer = _context.GioHangs.Where(g => g.MaNguoiDung == item.MaNguoiDung).FirstOrDefault();
                    int IDGioHang = -1;
                    if (GioHangCustomer == null)
                    {
                        GioHang gioHang = new GioHang()
                        {
                            MaNguoiDung = info.IDNguoiDung
                        };
                        _context.GioHangs.Add(gioHang);
                        await _context.SaveChangesAsync();
                        IDGioHang = gioHang.MaGioHang;
                    }
                    else
                    {
                        IDGioHang = GioHangCustomer.MaGioHang;
                        var Checked = _context.ChiTietGioHangs.Where(g => g.MaGioHang == IDGioHang).ToList();
                        if (Checked.Count > 0)
                        {
                            foreach (var check in Checked)
                            {
                                if (check.MaSanPham!=null && check.MaSanPham.Trim() == MaSanPham.Trim())
                                {
                                    check.SoLuong += info.SoLuong;
                                    _context.ChiTietGioHangs.Update(check);
                                    response.ResponseCode = 201;
                                    response.Result = "Thêm sản phẩm vào giỏ hàng thành công";
                                    await _context.SaveChangesAsync();
                                    await transaction.CommitAsync();
                                    return response;
                                }
                            }
                        }
                    }
                    var MaxIDCTGH = _context.ChiTietGioHangs.OrderByDescending(g => g.MaCtgh).Select(g => g.MaCtgh).FirstOrDefault();
                    MaxIDCTGH++;
                    ChiTietGioHang ctgh = new ChiTietGioHang()
                    {
                        MaGioHang = IDGioHang,
                        MaSanPham = MaSanPham,
                        MaCombo = null,
                        SoLuong = info.SoLuong,
                        Gia = SanPham.Gia,
                        ThanhTien = info.SoLuong * SanPham.Gia,
                    };
                    _context.ChiTietGioHangs.Add(ctgh);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    response.ResponseCode = 201;
                    response.Result = "Thêm vào giỏ hàng thành công";
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.ResponseCode = 500;
                response.Result = $"Lỗi: {ex.Message}";
            }
            return response;
        }
        public async Task<APIResponse> ThemCombo(ChiTietGioHangComboCreate info)
        {
            APIResponse response = new APIResponse();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int MaKhuyenMai = -1;
                bool DeadTime = false;
                int Percent = 0;
                var KhuyenMaiView = (await _servicesKM.ListKhuyenMaiAdmin(null)).ToList();
                var KhuyenMaiThis = KhuyenMaiView.Where(g => g.ID == info.MaKhuyenMai).FirstOrDefault();
                var Combo = _context.ComBoSanPhams.Where(g => g.MaComBo == info.IDCombo).FirstOrDefault();
                if (Combo == null)
                {
                    response.ResponseCode = 404;
                    response.Result = "Combo không tồn tại.";
                    return response;
                }

                var GioHang = _context.GioHangs.Where(g => g.MaNguoiDung == info.IDKhachHang).FirstOrDefault();
                if (GioHang == null) 
                {
                    GioHang newGH = new GioHang
                    {
                        MaNguoiDung = info.IDKhachHang
                    };
                    _context.GioHangs.Add(newGH);
                    await _context.SaveChangesAsync(); 
                }

                GioHang = _context.GioHangs.Where(g => g.MaNguoiDung == info.IDKhachHang).FirstOrDefault();
                if (GioHang == null)
                {
                    throw new Exception("Không thể tạo giỏ hàng mới.");
                }

                var ChiTietGioHangs = _context.ChiTietGioHangs
                    .Where(g => g.MaGioHang == GioHang.MaGioHang && g.MaCombo == info.IDCombo)
                    .FirstOrDefault();

                if (ChiTietGioHangs != null) // Đã có combo trong giỏ hàng
                {
                    int support = _context.GioHangSupports.Where(g => g.ChiTietGioHang == ChiTietGioHangs.MaCtgh).OrderByDescending(g=>g.Version).Select(g=>g.ID).FirstOrDefault() +1;
                    var CTComboList = _context.ChiTietComBos.Where(g => g.MaComBo == info.IDCombo).ToList();
                    foreach (var item in info.Detail)
                    {
                        if (item.MauSac.StartsWith("#"))
                            item.MauSac = item.MauSac.Substring(1);
                        var ChiTietCombo = CTComboList.FirstOrDefault(g => g.MaSanPham.Trim() == item.MaSanPham.Trim());
                        if (ChiTietCombo == null) continue;

                        string EndPointID = $"{item.MaSanPham.Trim()}_{item.MauSac.Trim()}_{item.KichThuoc.Trim()}";
                        bool Found = false;

                        ChiTietGioHangSupport sp = new ChiTietGioHangSupport
                        {
                            ChiTietGioHang = ChiTietGioHangs.MaCtgh,
                            MaSanPham = EndPointID,
                            MaChiTietCombo = ChiTietCombo.MaChiTietComBo,
                            SoLuong = info.SoLuong * ChiTietCombo.SoLuong ?? 0,
                            Version = support
                        };
                        _context.GioHangSupports.Add(sp);
                    }
                    ChiTietGioHangs.SoLuong += info.SoLuong;
                    ChiTietGioHangs.ThanhTien = ChiTietGioHangs.Gia * ChiTietGioHangs.SoLuong;
                    _context.ChiTietGioHangs.Update(ChiTietGioHangs);
                }
                else 
                {
                    ChiTietGioHang newCTGH = new ChiTietGioHang
                    {
                        MaGioHang = GioHang.MaGioHang,
                        MaCombo = info.IDCombo,
                        SoLuong = info.SoLuong,
                        Gia = (int)Combo.TongGia,
                        ThanhTien = (int)Combo.TongGia * info.SoLuong,
                    };
                    _context.ChiTietGioHangs.Add(newCTGH);
                    await _context.SaveChangesAsync(); // Save to get MaCtgh

                    var CTComboList = _context.ChiTietComBos.Where(g => g.MaComBo == info.IDCombo).ToList();
                    foreach (var item in info.Detail)
                    {
                        if (item.MauSac.StartsWith("#"))
                            item.MauSac = item.MauSac.Substring(1);
                        string MaSanPhamEndPoint = $"{item.MaSanPham.Trim()}_{item.MauSac.Trim()}_{item.KichThuoc.Trim()}";
                        var CTCombo = CTComboList.FirstOrDefault(g => g.MaSanPham.Trim() == item.MaSanPham.Trim());
                        if (CTCombo == null) continue;

                        ChiTietGioHangSupport support = new ChiTietGioHangSupport
                        {
                            ChiTietGioHang = newCTGH.MaCtgh,
                            MaSanPham = MaSanPhamEndPoint,
                            MaChiTietCombo = CTCombo.MaChiTietComBo,
                            SoLuong = newCTGH.SoLuong * CTCombo.SoLuong ?? 0,
                            Version = 1
                        };
                        _context.GioHangSupports.Add(support);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                response.ResponseCode = 200;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.ResponseCode = 500;
                response.Result = $"Lỗi: {ex.Message}";
            }
            return response;
        }

        public async Task<APIResponse> GiamSoLuongSanPham(TangGiamSoLuongGioHang info)
        {
            APIResponse response = new APIResponse();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var GioHang = _context.GioHangs.Where(g => g.MaNguoiDung.Trim() == info.MaKhachHang).FirstOrDefault();
                var ChiTietGiohang = _context.ChiTietGioHangs.Where(g => g.MaGioHang == GioHang.MaGioHang && g.MaSanPham == info.IDSanPham).FirstOrDefault();
                if(ChiTietGiohang.SoLuong<= 1)
                {
                    response.ResponseCode = 400;
                    response.Result = "Số lượng sản phẩm không thể giảm xuống dưới 1.";
                    return response;
                }
                else
                {
                    ChiTietGiohang.SoLuong--;
                    _context.ChiTietGioHangs.Update(ChiTietGiohang);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    response.ResponseCode = 200;
                }    
            }
            catch (Exception ex) 
            {
                await transaction.RollbackAsync();
                response.ResponseCode = 500;
                response.Result = $"Lỗi: {ex.Message}";
            }
            return response;
        }

        public async Task<APIResponse> TangSoLuongSanPham(TangGiamSoLuongGioHang info)
        {
            APIResponse response = new APIResponse();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var GioHang = _context.GioHangs.Where(g => g.MaNguoiDung.Trim() == info.MaKhachHang).FirstOrDefault();
                var ChiTietGiohang = _context.ChiTietGioHangs.Where(g => g.MaGioHang == GioHang.MaGioHang && g.MaSanPham == info.IDSanPham).FirstOrDefault();
                var SanPham = _context.SanPhams.Where(g => g.MaSanPham == info.IDSanPham).FirstOrDefault();
                if (SanPham.SoLuong < ChiTietGiohang.SoLuong + 1)
                {
                    response.ResponseCode = 400;
                    response.Result = "Số lượng sản phẩm không đủ để tăng.";
                    return response;
                }
                else 
                {
                    ChiTietGiohang.SoLuong++;
                    _context.ChiTietGioHangs.Update(ChiTietGiohang);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    response.ResponseCode = 200;
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.ResponseCode = 500;
                response.Result = $"Lỗi: {ex.Message}";
            }
            return response;
        }
        public async Task<APIResponse> XoaChiTietGioHang(TangGiamSoLuongGioHang info)
        {
            APIResponse response = new APIResponse();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var GioHang = _context.GioHangs.Where(g => g.MaNguoiDung.Trim() == info.MaKhachHang).FirstOrDefault();
                var ChiTietGiohang = _context.ChiTietGioHangs.Where(g => g.MaGioHang == GioHang.MaGioHang && g.MaSanPham == info.IDSanPham).FirstOrDefault();
                _context.ChiTietGioHangs.Remove(ChiTietGiohang);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                response.ResponseCode = 200;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.ResponseCode = 500;
                response.Result = $"Lỗi: {ex.Message}";
            }
            return response;
        }
        public async Task<APIResponse> XoaVersionComboGioHang(GioHangComboVersion info)
        {
            APIResponse response = new APIResponse();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var ctgh = _context.ChiTietGioHangs.Where(g => g.MaCtgh == info.ChiTietGioHang).FirstOrDefault();
                var ghSupport = _context.GioHangSupports.Where(g => g.ChiTietGioHang == info.ChiTietGioHang && g.Version == info.Version).ToList();
                var ctCombo = _context.ChiTietComBos.Where(g => g.MaChiTietComBo == ghSupport[0].MaChiTietCombo).FirstOrDefault();
                ctgh.SoLuong = ctgh.SoLuong - ghSupport[0].SoLuong/ctCombo.SoLuong;
                _context.GioHangSupports.RemoveRange(ghSupport);
                if (ctgh.SoLuong == 0)
                    _context.ChiTietGioHangs.Remove(ctgh);
                else
                    _context.ChiTietGioHangs.Update(ctgh);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                response.ResponseCode = 200;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.ResponseCode = 500;
                response.Result = $"Lỗi: {ex.Message}";
            }
            return response;
        }

        public async Task<APIResponse> XoaChiTietGiohangCombo(TangGiamSoLuongGioHang info)
        {
            APIResponse response = new APIResponse();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var GioHang = _context.GioHangs.Where(g => g.MaNguoiDung.Trim() == info.MaKhachHang).FirstOrDefault();
                var ChiTietGiohang = _context.ChiTietGioHangs.Where(g => g.MaGioHang == GioHang.MaGioHang && g.MaCombo == info.IDCombo).FirstOrDefault();
                var ChiTietComboSupportGioHang = _context.GioHangSupports.Where(g => g.MaChiTietCombo == ChiTietGiohang.MaGioHang).ToList();
                _context.GioHangSupports.RemoveRange(ChiTietComboSupportGioHang);
                _context.ChiTietGioHangs.Remove(ChiTietGiohang);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                response.ResponseCode = 200;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.ResponseCode = 500;
                response.Result = $"Lỗi: {ex.Message}";
            }
            return response;
        }
        public async Task<APIResponse> CopyGioHang(CopyGHModel info)
        {
            APIResponse response = new APIResponse();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int MaGH = -1;
                var GioHangUser = _context.GioHangs.Where(g => g.MaNguoiDung.Trim() == info.UserID.Trim()).FirstOrDefault();
                var CopyUser = _context.GioHangs.Where(g => g.MaNguoiDung.Trim() == info.CopyID.Trim()).FirstOrDefault();
                if (GioHangUser == null)
                {
                    int MaxGH = _context.GioHangs.OrderByDescending(g => g.MaGioHang).Select(g => g.MaGioHang).FirstOrDefault();
                    MaxGH++;
                    GioHang newGH = new GioHang
                    {
                        MaGioHang = MaxGH,
                        MaNguoiDung = info.UserID
                    };
                    _context.GioHangs.Add(newGH);
                    MaGH = newGH.MaGioHang;
                }
                else
                    MaGH = GioHangUser.MaGioHang;
                var ChiTietGH1List = _context.ChiTietGioHangs.Where(g => g.MaGioHang == MaGH).ToList();
                var ChiTietGH2List = _context.ChiTietGioHangs.Where(g => g.MaGioHang == CopyUser.MaGioHang).ToList();
                if (ChiTietGH1List.Count > 0)
                {
                    for (int n = 0; n < ChiTietGH2List.Count(); n++)
                    {
                        if (ChiTietGH2List[n].MaSanPham != null || !string.IsNullOrEmpty(ChiTietGH2List[n].MaSanPham))
                        {
                            bool Found = false;
                            for (int i = 0; i < ChiTietGH1List.Count(); i++)
                            {
                                if (ChiTietGH1List[i].MaSanPham != null)
                                {
                                    if (ChiTietGH1List[i].MaSanPham == ChiTietGH2List[n].MaSanPham)
                                    {
                                        Found = true;
                                        ChiTietGH1List[i].SoLuong += ChiTietGH2List[n].SoLuong;
                                        var Test = ChiTietGH1List[i];
                                        _context.ChiTietGioHangs.Update(Test);
                                        await _context.SaveChangesAsync();
                                        break;
                                    }                                                                
                                }                                  
                            }
                            if (!Found)
                            {
                                ChiTietGioHang newCTGH = new ChiTietGioHang();
                                newCTGH.MaSanPham = ChiTietGH2List[n].MaSanPham;
                                newCTGH.SoLuong = ChiTietGH2List[n].SoLuong;
                                newCTGH.Gia = ChiTietGH2List[n].Gia;
                                newCTGH.ThanhTien = ChiTietGH2List[n].ThanhTien;
                                newCTGH.MaGioHang = MaGH;
                                _context.ChiTietGioHangs.Add(newCTGH);
                                await _context.SaveChangesAsync();
                            }
                        }
                        else
                        {
                                if (ChiTietGH2List[n].MaCombo!=null)
                                {
                                    bool Found = false;
                                    for (int m = 0; m < ChiTietGH1List.Count(); m++)
                                    {
                                        if (ChiTietGH1List[m].MaCombo!=null)
                                        {
                                            if (ChiTietGH1List[m].MaCombo == ChiTietGH2List[n].MaCombo)
                                            {
                                                Found = true;
                                                ChiTietGH1List[m].SoLuong += ChiTietGH2List[n].SoLuong;
                                                var Test = ChiTietGH1List[m];               
                                                var CTCB = _context.ChiTietComBos.Where(g => g.MaComBo == ChiTietGH2List[n].MaCombo).ToList();
                                                var CTCBUser = _context.ChiTietComBos.Where(g => g.MaComBo == ChiTietGH1List[m].MaCombo).OrderByDescending(g => g.MaChiTietComBo).FirstOrDefault();
                                                int GioHangSupportUser = _context.GioHangSupports.Where(g => g.ChiTietGioHang == ChiTietGH1List[m].MaCtgh && g.MaChiTietCombo == CTCBUser.MaChiTietComBo).OrderByDescending(g => g.Version).Select(g => g.Version).FirstOrDefault() + 1;

                                                for (int k = 0; k < CTCB.Count(); k++)
                                                {
                                                    int Variant = GioHangSupportUser;
                                                    var GioHangSupportCopy = _context.GioHangSupports.Where(g => g.ChiTietGioHang == ChiTietGH2List[n].MaCtgh && g.MaChiTietCombo == CTCB[k].MaChiTietComBo).ToList();
                                                    int Temp = -1;
                                                    for (int l = 0; l < GioHangSupportCopy.Count(); l++)
                                                    {
                                                        if (l == 0)
                                                        {
                                                            Temp = GioHangSupportCopy[l].Version;
                                                            ChiTietGioHangSupport newCTGHSP = new ChiTietGioHangSupport();
                                                            newCTGHSP.ChiTietGioHang = ChiTietGH1List[m].MaCtgh;
                                                            newCTGHSP.MaSanPham = GioHangSupportCopy[l].MaSanPham;
                                                            newCTGHSP.SoLuong = GioHangSupportCopy[l].SoLuong;
                                                            newCTGHSP.MaChiTietCombo = CTCB[k].MaChiTietComBo;
                                                            newCTGHSP.Version = Variant;
                                                            _context.GioHangSupports.Add(newCTGHSP);
                                                        }
                                                        else
                                                        {
                                                            if (Temp == GioHangSupportCopy[l].Version)
                                                            {
                                                                ChiTietGioHangSupport newCTGHSP = new ChiTietGioHangSupport();
                                                                newCTGHSP.ChiTietGioHang = ChiTietGH1List[m].MaCtgh;
                                                                newCTGHSP.MaSanPham = GioHangSupportCopy[l].MaSanPham;
                                                                newCTGHSP.SoLuong = GioHangSupportCopy[l].SoLuong;
                                                                newCTGHSP.MaChiTietCombo = CTCB[k].MaChiTietComBo;
                                                                newCTGHSP.Version = Variant;
                                                                _context.GioHangSupports.Add(newCTGHSP);
                                                            }
                                                            else
                                                            {
                                                                Variant++;
                                                                Temp = GioHangSupportCopy[l].Version;
                                                                ChiTietGioHangSupport newCTGHSP = new ChiTietGioHangSupport();
                                                                newCTGHSP.ChiTietGioHang = ChiTietGH1List[m].MaCtgh;
                                                                newCTGHSP.MaSanPham = GioHangSupportCopy[l].MaSanPham;
                                                                newCTGHSP.SoLuong = GioHangSupportCopy[l].SoLuong;
                                                                newCTGHSP.MaChiTietCombo = CTCB[k].MaChiTietComBo;
                                                                newCTGHSP.Version = Variant;
                                                                _context.GioHangSupports.Add(newCTGHSP);

                                                            }
                                                        }
                                                    }
                                                }
                                                break;
                                            }
                                        }    
                                    }
                                    if (!Found)
                                    {
                                        ChiTietGioHang newCTGH = new ChiTietGioHang();
                                        newCTGH.MaCombo = ChiTietGH2List[n].MaCombo;
                                        newCTGH.SoLuong = ChiTietGH2List[n].SoLuong;
                                        newCTGH.Gia = ChiTietGH2List[n].Gia;
                                        newCTGH.ThanhTien = ChiTietGH2List[n].ThanhTien;
                                        newCTGH.MaGioHang = MaGH;
                                        _context.ChiTietGioHangs.Add(newCTGH);
                                        await _context.SaveChangesAsync();
                                        int MaChiTietGioHangCombo = newCTGH.MaCtgh;
                                        var GioHangSupport = _context.GioHangSupports.Where(g => g.ChiTietGioHang == ChiTietGH2List[n].MaCtgh).ToList();
                                        foreach (var item in GioHangSupport)
                                        {
                                            ChiTietGioHangSupport CTGHSp = new ChiTietGioHangSupport();
                                            CTGHSp.MaChiTietCombo = item.MaChiTietCombo;
                                            CTGHSp.ChiTietGioHang = MaChiTietGioHangCombo;
                                            CTGHSp.SoLuong = item.SoLuong;
                                            CTGHSp.Version = item.Version;
                                            CTGHSp.MaSanPham = item.MaSanPham;
                                            _context.GioHangSupports.Add(CTGHSp);
                                            await _context.SaveChangesAsync();
                                        }
                                    }
                                }                       
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < ChiTietGH2List.Count(); j++)
                    {
                     
                        ChiTietGioHang newCTGH = new ChiTietGioHang();

                        newCTGH.SoLuong = ChiTietGH2List[j].SoLuong;
                        newCTGH.Gia = ChiTietGH2List[j].Gia;
                        newCTGH.ThanhTien = ChiTietGH2List[j].ThanhTien;
                        newCTGH.MaGioHang = MaGH;
                        _context.ChiTietGioHangs.Add(newCTGH);
                        await _context.SaveChangesAsync();
                        int MaChiTietGioHangCombo = newCTGH.MaCtgh;
                        if (ChiTietGH2List[j].MaCombo != null)
                        {
                            newCTGH.MaCombo = ChiTietGH2List[j].MaCombo;
                            _context.ChiTietGioHangs.Update(newCTGH);
                            var GioHangSupport = _context.GioHangSupports.Where(g => g.ChiTietGioHang == ChiTietGH2List[j].MaCtgh).ToList();
                            foreach (var item in GioHangSupport)
                            {
                                ChiTietGioHangSupport CTGHSp = new ChiTietGioHangSupport();
                                CTGHSp.MaChiTietCombo = item.MaChiTietCombo;
                                CTGHSp.ChiTietGioHang = MaChiTietGioHangCombo;
                                CTGHSp.SoLuong = item.SoLuong;
                                CTGHSp.Version = item.Version;
                                CTGHSp.MaSanPham = item.MaSanPham;
                                _context.GioHangSupports.Add(CTGHSp);
                                await _context.SaveChangesAsync();
                            }
                        }  
                        else
                        {
                            newCTGH.MaSanPham = ChiTietGH2List[j].MaSanPham;
                            _context.ChiTietGioHangs.Update(newCTGH);
                            await _context.SaveChangesAsync();
                        }    
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                response.ResponseCode = 200;
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                response.ResponseCode = 500;
                response.Result = $"Lỗi: {ex.Message}";
            }
            return response;
        }
    }
}

