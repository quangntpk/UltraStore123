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
        public static List<KhachHang> KhachHangaaa = new List<KhachHang>
        {
            new KhachHang
            {
                MaNguoiDung= "KH001",
                TenTaiKhoan="User",
                MatKhau="123456"
            }
        };      
        public async Task<GioHangView> GioHangViews(string? MaKhachHang)
        {
            GioHangView GioHangView = new GioHangView();
            var GioHang = _context.GioHangs.Where(g => g.MaNguoiDung == MaKhachHang).FirstOrDefault();
            if (GioHang != null)
            {
                var CTGH = _context.ChiTietGioHangs.Where(g => g.MaGioHang == GioHang.MaGioHang).ToList();
                List<ChiTietGioHangSanPhamView> DetailSanPhamView = new List<ChiTietGioHangSanPhamView>();
                foreach (var item in CTGH)
                {
                    var sp = _context.SanPhams.Where(g => g.MaSanPham == item.MaSanPham).FirstOrDefault();
                    ChiTietGioHangSanPhamView spview = new ChiTietGioHangSanPhamView();
                    spview.IDSanPham = item.MaSanPham;
                    spview.TenSanPham = sp.TenSanPham;
                    spview.TienSanPham = sp.Gia * item.SoLuong ?? 0;
                    spview.MauSac = sp.MaSanPham.Split('_')[1];
                    spview.KickThuoc = sp.MaSanPham.Split('_')[2];
                    spview.SoLuong = item.SoLuong ?? 0;
                    DetailSanPhamView.Add(spview);
                }
                GioHangView.CTGHSanPhamView = DetailSanPhamView;
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
                    response.ResponseCode = 201;
                    response.Result = "Thêm vào giỏ hàng thành công";
                }
            }
            catch (Exception ex)
            {
                response.ResponseCode = 500;
                response.Result = $"Lỗi: {ex.Message}";
            }
            return response;
        }
    }
}

