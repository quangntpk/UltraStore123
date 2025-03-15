using UltraStrore.Data;
using UltraStrore.Data.Temp;
using UltraStrore.Helper;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Services
{
    public class CartServices : ICartServices
    {
        private readonly ApplicationDbContext _context;

        public CartServices(ApplicationDbContext context)
        {
            _context = context;
        }    
        public async Task<GioHangView> GioHangViews(string? MaKhachHang)
        {
            GioHangView GioHangView = new GioHangView();
            var GioHang = _context.GioHangs.Where(g => g.MaNguoiDung == MaKhachHang).FirstOrDefault();
            if (GioHang != null)
            {
                var CTGHSanPham = _context.ChiTietGioHangs.Where(g => g.MaGioHang == GioHang.MaGioHang && !string.IsNullOrEmpty(g.MaSanPham)).ToList();
                List<ChiTietGioHangSanPhamView> DetailSanPhamView = new List<ChiTietGioHangSanPhamView>();
                foreach (var item in CTGHSanPham)
                {
                    var sp = _context.SanPhams.Where(g => g.MaSanPham == item.MaSanPham).FirstOrDefault();
                    ChiTietGioHangSanPhamView spview = new ChiTietGioHangSanPhamView();
                    spview.IDSanPham = item.MaSanPham;
                    spview.TenSanPham = sp.TenSanPham;
                    spview.TienSanPham = sp.Gia * item.SoLuong ?? 0;
                    spview.MauSac = sp.MaSanPham.Split('_')[1];
                    spview.KickThuoc = sp.MaSanPham.Split('_')[2];
                    spview.SoLuong = item.SoLuong ?? 0;
                    spview.HinhAnh = _context.HinhAnhs.Where(g => g.MaSanPham.Trim() == sp.MaSanPham.Substring(0,6).Trim()).Select(g=>g.Data).FirstOrDefault();
                    DetailSanPhamView.Add(spview);
                }
                GioHangView.CTGHSanPhamView = DetailSanPhamView;
                var CTGHCombo = _context.ChiTietGioHangs.Where(g => g.MaGioHang == GioHang.MaGioHang && g.MaCombo != null).ToList();
                List<ChiTietGioHangComboView> DetailComboView = new List<ChiTietGioHangComboView>();
                foreach(var item in CTGHCombo)
                {
                    ChiTietGioHangComboView cbview = new ChiTietGioHangComboView();
                    List<SanPhamInGioHangCombo> splist = new List<SanPhamInGioHangCombo>();
                    var sanpham = _context.GioHangSupports.Where(g => g.ChiTietGioHang == item.MaCtgh).ToList();
                    for(int i = 0; i < sanpham.Count; i++)
                    {
                        SanPhamInGioHangCombo newsp = new SanPhamInGioHangCombo();
                        newsp.MaSanPham = sanpham[i].MaSanPham;
                        newsp.SoLuong = sanpham[i].SoLuong;
                        newsp.Version = sanpham[i].Version;
                        splist.Add(newsp);
                    }
                    cbview.SanPhamList = splist;
                    cbview.IDCombo = item.MaCombo??0;
                    var ComboName = _context.ComBoSanPhams.Where(g => g.MaComBo == item.MaCombo).FirstOrDefault();
                    cbview.TenCombo = ComboName.TenComBo;
                    cbview.SoLuong = item.SoLuong??0;
                    cbview.HinhAnh = ComboName.HinhAnh;
                    cbview.Gia = (int)ComboName.TongGia;
                    DetailComboView.Add(cbview);                      
                }
                GioHangView.CTGHComboView = DetailComboView;
                if(MaKhachHang!=null)
                    GioHangView.IDNguoiDung = MaKhachHang;
                GioHangView.ID = GioHang.MaGioHang;
                return GioHangView;
            }
            return GioHangView;
        }

        public async Task<APIResponse> ThemSanPham(ChiTietGioHangSanPhamCreate info)
        {
            APIResponse response = new APIResponse();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
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
                                if (check.MaSanPham.Trim() == MaSanPham)
                                {
                                    check.SoLuong += info.SoLuong;
                                    response.ResponseCode = 201;
                                    response.Result = "Thêm sản phẩm vào giỏ hàng thành công";
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
                        ThanhTien = info.SoLuong * SanPham.Gia
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
                var Combo = _context.ComBoSanPhams.Where(g => g.MaComBo == info.IDCombo).FirstOrDefault();
                if (Combo == null)
                {
                    response.ResponseCode = 404;
                    response.Result = "Combo không tồn tại.";
                    return response;
                }

                var GioHang = _context.GioHangs.Where(g => g.MaNguoiDung == info.IDKhachHang).FirstOrDefault();
                if (GioHang == null) // Chưa có giỏ hàng
                {
                    GioHang newGH = new GioHang
                    {
                        MaNguoiDung = info.IDKhachHang
                    };
                    _context.GioHangs.Add(newGH);
                    await _context.SaveChangesAsync(); // Save to get MaGioHang
                }

                // Reload GioHang to ensure we have the latest data
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
                    int support = _context.GioHangSupports.Where(g => g.ChiTietGioHang == ChiTietGioHangs.MaCtgh).OrderByDescending(g=>g.Version).Select(g=>g.ID).FirstOrDefault();
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
                else // Chưa có Combo trong giỏ hàng
                {
                    ChiTietGioHang newCTGH = new ChiTietGioHang
                    {
                        MaGioHang = GioHang.MaGioHang,
                        MaCombo = info.IDCombo,
                        SoLuong = info.SoLuong,
                        Gia = (int)Combo.TongGia,
                        ThanhTien = (int)Combo.TongGia * info.SoLuong
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
                            ChiTietGioHang = newCTGH.MaCtgh, // Use the newly generated MaCtgh
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
    }
}

