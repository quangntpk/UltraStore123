using Microsoft.AspNetCore.Http.HttpResults;
using UltraStrore.Data;
using UltraStrore.Helper;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Services
{
    public class ComboServices : IComboServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ISanPhamServices _sanphamServices;
        private readonly IKhuyenMaiServices _serviceKM;
        public ComboServices(ApplicationDbContext context, ISanPhamServices sanPhamServices, IKhuyenMaiServices services)
        {
            _context = context;
            _sanphamServices = sanPhamServices;
            _serviceKM = services;
        }
        public async Task<List<ComboAdminView>> ComboViews(int? id)
        {
            var KhuyenMaiView = (await _serviceKM.ListKhuyenMaiAdmin(null)).ToList();
            int KhuyenMaiChung = KhuyenMaiView.Where(g => g.PercentChung.HasValue).OrderByDescending(g => g.PercentChung).Select(g => g.PercentChung).FirstOrDefault() ?? 0;
            List<ComboAdminView> list = new List<ComboAdminView>();
            var data = id == null
                ? _context.ComBoSanPhams.ToList()
                : _context.ComBoSanPhams.Where(g => g.MaComBo == id).ToList();
            foreach (var item in data)
            {
                ComboAdminView cbv = new ComboAdminView();
                cbv.MaCombo = item.MaComBo;
                cbv.Name = item.TenComBo;
                cbv.HinhAnh = item.HinhAnh;
                var _chitiet = _context.ChiTietComBos.Where(g => g.MaComBo == item.MaComBo).ToList();
                List<SanPhamViewIncombo> sp = new List<SanPhamViewIncombo>();
                for (int i = 0; i < _chitiet.Count(); i++)
                {
                    var InfoSanPham = await _sanphamServices.ListSanPham(_chitiet[i].MaSanPham.Trim());
                    SanPhamViewIncombo view = new SanPhamViewIncombo();
                    view.ID = _chitiet[i].MaChiTietComBo;
                    view.IDSanPham = _chitiet[i].MaSanPham;
                    view.Name = InfoSanPham[0].Name;
                    view.MoTa = InfoSanPham[0].MoTa;
                    view.MauSac = InfoSanPham[0].MauSac;
                    view.KichThuoc = InfoSanPham[0].KichThuoc;
                    view.SoLuong = _chitiet[i].SoLuong ?? 0;
                    view.DonGia = InfoSanPham[0].DonGia;
                    view.ChatLieu = InfoSanPham[0].ChatLieu;
                    view.Hinh = InfoSanPham[0].Hinh;
                    view.ThuongHieu = InfoSanPham[0].ThuongHieu;
                    view.LoaiSanPham = InfoSanPham[0].LoaiSanPham;
                    sp.Add(view);
                }
                cbv.SanPhams = sp;
                cbv.MoTa = item.MoTa;
                cbv.Gia = (int)item.TongGia;
                if (item.TrangThai != null && item.TrangThai == true)
                {
                    cbv.TrangThai = true;
                }
                else
                    cbv.TrangThai = false;
                cbv.SoLuong = item.SoLuong ?? 0;
                cbv.NgayTao = item.NgayTao;
                int MaxKM = 0;
                var KhuyenMaiRieng = KhuyenMaiView.Where(g => !g.PercentChung.HasValue).ToList();
                foreach (var KM in KhuyenMaiRieng)
                {
                    foreach (var dis in KM.DanhSachKhuyenMai)
                    {
                        if (dis.IdCombo.HasValue && dis.IdCombo == cbv.MaCombo)
                        {
                            if (dis.Percent > MaxKM)
                                MaxKM = dis.Percent ?? MaxKM;
                        }
                    }
                }
                cbv.KhuyenMaiMax = KhuyenMaiChung > MaxKM ? KhuyenMaiChung : MaxKM;
                list.Add(cbv);
            }
            return list;
        }

        public async Task<APIResponse> EditCombo(ComboEdit info)
        {
            APIResponse response = new APIResponse();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var ComboNotEditted = _context.ComBoSanPhams.Where(g => g.MaComBo == info.ID).FirstOrDefault();
                ComboNotEditted.MoTa = info.MoTa;
                ComboNotEditted.TenComBo = info.TenCombo;
                ComboNotEditted.SoLuong = info.SoLuong;
                ComboNotEditted.Discount = info.Discount;
                ComboNotEditted.HinhAnh = info.HinhAnh;
                var ChiTietSanPham = _context.ChiTietComBos.Where(g => g.MaComBo == info.ID).ToList();
                foreach (var item in ChiTietSanPham)
                {
                    bool found = false;
                    for (int i = 0; i < info.SanPham.Count(); i++)
                    {
                        if (item.MaSanPham.Trim() == info.SanPham[i].MaSanPham.Trim())
                        {
                            item.SoLuong = info.SanPham[i].SoLuong;
                            _context.ChiTietComBos.Update(item);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        item.SoLuong = 0;
                        _context.ChiTietComBos.Update(item);
                    }
                }
                foreach (var check in info.SanPham)
                {
                    bool found = false;
                    for (int i = 0; i < ChiTietSanPham.Count(); i++)
                    {
                        if (check.MaSanPham.Trim() == ChiTietSanPham[i].MaSanPham.Trim())
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        ChiTietComBo newct = new ChiTietComBo();
                        newct.MaSanPham = check.MaSanPham;
                        newct.MaComBo = info.ID;
                        newct.SoLuong = check.SoLuong;
                        _context.ChiTietComBos.Add(newct);
                    }
                }
                float TongTien = 0;
                foreach (var item in info.SanPham)
                {
                    float TongTienSanPham = 0;
                    var ListTienSanPhamChooose = _context.SanPhams
                        .Where(g => g.MaSanPham.Contains(item.MaSanPham))
                        .Select(h => h.Gia)
                        .ToList();

                    foreach (var SItem in ListTienSanPhamChooose)
                    {
                        if (SItem.HasValue)
                        {
                            TongTienSanPham += SItem.Value;
                        }
                    }
                    if (item.SoLuong.HasValue && item.SoLuong.Value != 0)
                    {
                        TongTien += TongTienSanPham * item.SoLuong.Value/ListTienSanPhamChooose.Count();
                    }
                }
                ComboNotEditted.TongGia = TongTien * (100 - info.Discount) / 100;
                _context.ComBoSanPhams.Update(ComboNotEditted);
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

        public async Task<APIResponse> AddCombo(ComboCreate info)
        {
            APIResponse response = new APIResponse();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                float TongTien = 0;
                ComBoSanPham newCB = new ComBoSanPham
                {
                    TenComBo = info.TenCombo.Trim(),
                    MoTa = info.MoTa.Trim(),
                    SoLuong = info.SoLuong,
                    HinhAnh = info.HinhAnh,                   
                    TrangThai = true,
                    Discount = info.Discount,
                    NgayTao =  DateOnly.FromDateTime(DateTime.Now),
                };
                foreach (var item in info.SanPham)
                {
                    float TongTienSanPham = 0;
                    var ListTienSanPhamChooose = _context.SanPhams
                        .Where(g => g.MaSanPham.Contains(item.MaSanPham))
                        .Select(h => h.Gia)
                        .ToList();

                    foreach (var SItem in ListTienSanPhamChooose)
                    {
                        if (SItem.HasValue)
                        {
                            TongTienSanPham += SItem.Value;
                        }
                    }
                    if (item.SoLuong.HasValue && item.SoLuong.Value != 0)
                    {
                        TongTien += TongTienSanPham * item.SoLuong.Value / ListTienSanPhamChooose.Count();
                    }
                }
                newCB.TongGia=TongTien*(100-info.Discount)/100;
                _context.ComBoSanPhams.Add(newCB);
                await _context.SaveChangesAsync();
                int MaComBo = newCB.MaComBo;

                foreach (var item in info.SanPham)
                {
                    ChiTietComBo newCTCB = new ChiTietComBo
                    {
                        MaSanPham = item.MaSanPham,
                        SoLuong = item.SoLuong,
                        MaComBo = MaComBo
                    };
                    _context.ChiTietComBos.Add(newCTCB);
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

        public async Task<APIResponse> XoaCombo(int id)
        {
            APIResponse response = new APIResponse();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var combo = _context.ComBoSanPhams.Where(g => g.MaComBo == id).FirstOrDefault();
                if (combo.TrangThai == true)
                    combo.TrangThai = false;
                else
                    combo.TrangThai = true;
                _context.ComBoSanPhams.Update(combo);
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
