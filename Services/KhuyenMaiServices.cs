using System.Text.Json;
using UltraStrore.Data;
using UltraStrore.Helper;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Services
{
    public class KhuyenMaiServices : IKhuyenMaiServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IKhuyenMaiServices _service;
        private readonly string _kmPath;
        private static MoTaKhuyenMai MoTaTempCreate;
        private static MoTaKhuyenMai MoTaTempEdit;
        public KhuyenMaiServices(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _kmPath = Path.Combine(env.WebRootPath, "KhuyenMai.json");
        }

        public async Task<List<KhuyenMaiView>> ListKhuyenMaiUser(int? id)
        {
            List<KhuyenMaiView> ListKhuyenMaiView = new List<KhuyenMaiView>();
            var ListKhuyenMai = _context.KhuyenMais.Where(g=>g.TrangThai==true &&g.BatDau<=DateOnly.FromDateTime(DateTime.Now)&&g.KetThuc>=DateOnly.FromDateTime(DateTime.Now)).ToList();
            var ListChiTietKhuyenMai = _context.ChiTietKhuyenMais.ToList();
            string MoTa = File.ReadAllText(_kmPath);
            var FullMoTa = JsonSerializer.Deserialize<List<MoTaKhuyenMaiCreateModel>>(MoTa);

            foreach (var item in ListKhuyenMai)
            {
                KhuyenMaiView KMView = new KhuyenMaiView();
                KMView.ID = item.ID;
                KMView.NgayBatDau = item.BatDau;
                KMView.NgayKetThuc = item.KetThuc;
                KMView.TenKhuyenMai = item.TenKhuyenMai;
                KMView.PercentChung = item.PercentChung;
                KMView.HinhAnh = new List<byte[]>();

                List<ChiTietKhuyenMaiView> CTKMView = new List<ChiTietKhuyenMaiView>();
                var DataBegin = ListChiTietKhuyenMai.Where(g => g.MaKhuyenMai == item.ID).ToList();

                foreach (var CTKM in DataBegin)
                {
                    if (CTKM.SP != null)
                    {
                        var SanPhamInfo = _context.SanPhams.Where(g => g.MaSanPham.Contains(CTKM.SP) && g.TrangThai == 1).OrderBy(g => g.Gia).ToList();
                        var sp = SanPhamInfo.FirstOrDefault();

                        if (sp != null)
                        {
                            ChiTietKhuyenMaiView CT = new ChiTietKhuyenMaiView();
                            CT.IdSanPham = sp.MaSanPham.Substring(0, 6);
                            CT.TenSanPhamCombo = sp.TenSanPham;
                            CT.GiaGoc = sp.Gia;
                            CT.GiaMoi = (float)(sp.Gia * (1 - CTKM.PercentRieng / 100));
                            CT.Percent = CTKM.PercentRieng;
                            CT.HinhAnh = _context.HinhAnhs.Where(g => g.MaSanPham.Contains(CT.IdSanPham)).Select(g => g.Data).ToList();
                            CTKMView.Add(CT);
                        }
                    }
                    else
                    {
                        ChiTietKhuyenMaiView CT = new ChiTietKhuyenMaiView();
                        var ComboSummary = _context.ComBoSanPhams.Where(g => g.MaComBo == CTKM.CB && g.TrangThai == true).FirstOrDefault();
                        CT.IdCombo = ComboSummary.MaComBo;
                        CT.TenSanPhamCombo = ComboSummary.TenComBo;
                        CT.GiaGoc = (int)ComboSummary.TongGia;
                        CT.GiaMoi = (float)(ComboSummary.TongGia * (1 - CTKM.PercentRieng / 100));
                        CT.Percent = CTKM.PercentRieng;
                        List<byte[]> Hinh = new List<byte[]>();
                        Hinh.Add(ComboSummary.HinhAnh);
                        CT.HinhAnh = Hinh;
                        CTKMView.Add(CT);
                    }
                }

                var dataEx = FullMoTa?.Where(g => int.Parse(g.IdMoTa) == item.ID).FirstOrDefault();
                if (dataEx != null && dataEx.MoTa != null)
                {
                    KMView.MoTa = dataEx.MoTa;
                    if (dataEx.MoTa.Pictures != null)
                    {
                        foreach (var picture in dataEx.MoTa.Pictures)
                        {
                            if (!string.IsNullOrEmpty(picture?.Url))
                            {
                                try
                                {
                                    string base64String = picture.Url;
                                    byte[] imageBytes = Convert.FromBase64String(base64String);
                                    KMView.HinhAnh.Add(imageBytes);
                                }
                                catch (FormatException ex)
                                {
                                    Console.WriteLine($"Error converting base64 image: {ex.Message}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Unexpected error: {ex.Message}");
                                }
                            }
                        }
                    }
                }

                KMView.DanhSachKhuyenMai = CTKMView;
                ListKhuyenMaiView.Add(KMView);
            }
            if (id.HasValue)
                return ListKhuyenMaiView.Where(g => g.ID == id).ToList();
            return ListKhuyenMaiView;
        }



        public async Task<List<KhuyenMaiView>> ListKhuyenMaiAdmin(int? id)
        {
            List<KhuyenMaiView> ListKhuyenMaiView = new List<KhuyenMaiView>();
            var ListKhuyenMai = _context.KhuyenMais.ToList();
            var ListChiTietKhuyenMai = _context.ChiTietKhuyenMais.ToList();
            string MoTa = File.ReadAllText(_kmPath);
            var FullMoTa = JsonSerializer.Deserialize<List<MoTaKhuyenMaiCreateModel>>(MoTa);

            foreach (var item in ListKhuyenMai)
            {
                KhuyenMaiView KMView = new KhuyenMaiView();
                KMView.ID = item.ID;
                KMView.NgayBatDau = item.BatDau;
                KMView.NgayKetThuc = item.KetThuc;
                KMView.TenKhuyenMai = item.TenKhuyenMai;
                KMView.PercentChung = item.PercentChung;
                KMView.HinhAnh = new List<byte[]>();

                List<ChiTietKhuyenMaiView> CTKMView = new List<ChiTietKhuyenMaiView>();
                var DataBegin = ListChiTietKhuyenMai.Where(g => g.MaKhuyenMai == item.ID).ToList();

                foreach (var CTKM in DataBegin)
                {
                    if (CTKM.SP != null)
                    {
                        var SanPhamInfo = _context.SanPhams.Where(g => g.MaSanPham.Contains(CTKM.SP) && g.TrangThai == 1).OrderBy(g => g.Gia).ToList();
                        var sp = SanPhamInfo.FirstOrDefault();

                        if (sp != null) 
                        {
                            ChiTietKhuyenMaiView CT = new ChiTietKhuyenMaiView();
                            CT.IdSanPham = sp.MaSanPham.Substring(0, 6);
                            CT.TenSanPhamCombo = sp.TenSanPham;
                            CT.GiaGoc = sp.Gia;
                            CT.GiaMoi = (float)(sp.Gia * (1 - CTKM.PercentRieng / 100));
                            CT.Percent = CTKM.PercentRieng;
                            CT.HinhAnh = _context.HinhAnhs.Where(g => g.MaSanPham.Contains(CT.IdSanPham)).Select(g => g.Data).ToList();
                            CTKMView.Add(CT);
                        }
                    }
                    else
                    {
                        ChiTietKhuyenMaiView CT = new ChiTietKhuyenMaiView();
                        var ComboSummary = _context.ComBoSanPhams.Where(g => g.MaComBo == CTKM.CB && g.TrangThai == true).FirstOrDefault();
                        CT.IdCombo = ComboSummary.MaComBo;
                        CT.TenSanPhamCombo = ComboSummary.TenComBo;
                        CT.GiaGoc = (int)ComboSummary.TongGia;
                        CT.GiaMoi = (float)(ComboSummary.TongGia * (1 - CTKM.PercentRieng / 100));
                        CT.Percent = CTKM.PercentRieng;
                        List<byte[]> Hinh = new List<byte[]>();
                        Hinh.Add(ComboSummary.HinhAnh);
                        CT.HinhAnh = Hinh;
                        CTKMView.Add(CT);
                    }
                }

                var dataEx = FullMoTa?.Where(g => int.Parse(g.IdMoTa) == item.ID).FirstOrDefault();
                if (dataEx != null && dataEx.MoTa != null)
                {
                    KMView.MoTa = dataEx.MoTa;
                    if (dataEx.MoTa.Pictures != null)
                    {
                        foreach (var picture in dataEx.MoTa.Pictures)
                        {
                            if (!string.IsNullOrEmpty(picture?.Url))
                            {
                                try
                                {
                                    string base64String = picture.Url;
                                    byte[] imageBytes = Convert.FromBase64String(base64String);
                                    KMView.HinhAnh.Add(imageBytes);
                                }
                                catch (FormatException ex)
                                {
                                    Console.WriteLine($"Error converting base64 image: {ex.Message}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Unexpected error: {ex.Message}");
                                }
                            }
                        }
                    }
                }

                KMView.DanhSachKhuyenMai = CTKMView;
                ListKhuyenMaiView.Add(KMView);
            }
            if (id.HasValue)
                return ListKhuyenMaiView.Where(g => g.ID == id).ToList();
            return ListKhuyenMaiView;
        }

        public async Task<APIResponse> KhuyenMaiCreate(KhuyenMaiCreate data)
        {
            APIResponse response = new APIResponse();
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                KhuyenMai newKM = new KhuyenMai();
                newKM.TenKhuyenMai = data.TenKhuyenMai;
                newKM.BatDau = data.BatDau;
                newKM.KetThuc = data.KetThuc;
                _context.KhuyenMais.Add(newKM);
                await _context.SaveChangesAsync();
                if(data.All)
                {
                    newKM.All = true;
                    newKM.PercentChung = data.PercentChung;
                }    
                else
                {
                    foreach(var item in data.ChiTiet)
                    {
                        ChiTietKhuyenMai CTKM = new ChiTietKhuyenMai();
                        CTKM.PercentRieng = item.Percent;
                        if (item.IdCombo.HasValue)
                        {
                            CTKM.CB = item.IdCombo;
                        }
                        else
                            CTKM.SP = item.IdSanPham;
                        CTKM.MaKhuyenMai = newKM.ID;
                        _context.ChiTietKhuyenMais.Add(CTKM);
                        await _context.SaveChangesAsync();
                    }    
                }
                List<MoTaKhuyenMaiCreateModel> existingData = new List<MoTaKhuyenMaiCreateModel>();
                if (System.IO.File.Exists(_kmPath))
                {
                    
                    var jsonContent = await System.IO.File.ReadAllTextAsync(_kmPath);
                    if (!string.IsNullOrWhiteSpace(jsonContent))
                    {
                        try
                        {
                            existingData = JsonSerializer.Deserialize<List<MoTaKhuyenMaiCreateModel>>(jsonContent) ?? new List<MoTaKhuyenMaiCreateModel>();
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
                    await System.IO.File.WriteAllTextAsync(_kmPath, "[]");
                }
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "KhuyenMai.json");
                string jsonString = File.Exists(filePath) ? File.ReadAllText(filePath) : "[]";
                int newIdMoTa = existingData.Count > 0 ? existingData.Max(x => int.Parse(x.IdMoTa)) + 1 : 1;
                int newId = newKM.ID;

                existingData.Add(new MoTaKhuyenMaiCreateModel
                {
                    ID = newId.ToString(),
                    IdMoTa = newIdMoTa.ToString(),
                    MoTa = MoTaTempCreate
                });

                jsonString = JsonSerializer.Serialize(existingData, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, jsonString);
                await transaction.CommitAsync();
                MoTaTempCreate = new MoTaKhuyenMai();
                response.ResponseCode = 200;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 400;
                
            }
            return response;
            
        }

        public async Task<APIResponse> MoTaKhuyenMaiCreate(MoTaKhuyenMai moTaKhuyenMai)
        {
            APIResponse response = new APIResponse();
            MoTaTempCreate = moTaKhuyenMai;
            response.ResponseCode = 200;
            return response;

        }
        public async Task<APIResponse> MoTaKhuyenMaiEdit(MoTaKhuyenMai moTaKhuyenMai)
        {
            APIResponse response = new APIResponse();
            MoTaTempEdit = moTaKhuyenMai;
            response.ResponseCode = 200;
            return response;
        }

        public async Task<APIResponse> KhuyenMaiUpdate(KhuyenMaiEdit data)
        {
            APIResponse response = new APIResponse();
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var EditKM = _context.KhuyenMais.Where(g => g.ID == data.ID).FirstOrDefault();
                var DeleteCT = _context.ChiTietKhuyenMais.Where(g => g.MaKhuyenMai == data.ID).ToList();
                _context.ChiTietKhuyenMais.RemoveRange(DeleteCT);
                await _context.SaveChangesAsync();
                EditKM.TenKhuyenMai = data.TenKhuyenMai;

                EditKM.BatDau = data.BatDau;
                EditKM.KetThuc = data.KetThuc;
                EditKM.TrangThai = true;
                _context.KhuyenMais.Update(EditKM);
                await _context.SaveChangesAsync();               
                if (data.All)
                {
                    EditKM.All = data.All;
                    EditKM.PercentChung = data.PercentChung;
                }
                else
                {
                    foreach (var item in data.ChiTiet)
                    {
                        ChiTietKhuyenMai CTKM = new ChiTietKhuyenMai();
                        CTKM.PercentRieng = item.Percent;
                        if (item.IdCombo.HasValue)
                        {
                            CTKM.CB = item.IdCombo;
                        }
                        else
                            CTKM.SP = item.IdSanPham;
                        CTKM.MaKhuyenMai = EditKM.ID;
                        _context.ChiTietKhuyenMais.Add(CTKM);
                        await _context.SaveChangesAsync();
                    }
                }
                List<MoTaKhuyenMaiCreateModel> existingData = new List<MoTaKhuyenMaiCreateModel>();
                if (System.IO.File.Exists(_kmPath))
                {

                    var jsonContent = await System.IO.File.ReadAllTextAsync(_kmPath);
                    if (!string.IsNullOrWhiteSpace(jsonContent))
                    {
                        try
                        {
                            existingData = JsonSerializer.Deserialize<List<MoTaKhuyenMaiCreateModel>>(jsonContent) ?? new List<MoTaKhuyenMaiCreateModel>();
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
                    await System.IO.File.WriteAllTextAsync(_kmPath, "[]");
                }
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "KhuyenMai.json");
                string jsonString = File.Exists(filePath) ? File.ReadAllText(filePath) : "[]";
                int newIdMoTa = existingData.Count > 0 ? existingData.Max(x => int.Parse(x.IdMoTa)) + 1 : 1;
                existingData.RemoveAll(g=>int.Parse(g.ID)==EditKM.ID);
                var MoTaEdit = MoTaTempEdit;
                if (MoTaTempEdit != null ) {
                    existingData.Add(new MoTaKhuyenMaiCreateModel
                    {
                        ID = data.ID.ToString(),
                        IdMoTa = newIdMoTa.ToString(),
                        MoTa = MoTaEdit,

                    });
                }
                jsonString = JsonSerializer.Serialize(existingData, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, jsonString);
                await transaction.CommitAsync();
                MoTaTempEdit = new MoTaKhuyenMai();
                response.ResponseCode = 200;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 400;

            }
            return response;
        }

        public async Task<APIResponse> DisableKhuyenMai(int id)
        {
            var response = new APIResponse();
            using var transation = _context.Database.BeginTransaction();
            try
            {
                var KM = _context.KhuyenMais.Where(g => g.ID == id).FirstOrDefault();
                if (KM.TrangThai)
                    KM.TrangThai = false;
                else
                    KM.TrangThai = true;
                _context.KhuyenMais.Update(KM);
                await _context.SaveChangesAsync();
                await transation.CommitAsync();
                response.ResponseCode = 200;                
            }
            catch (Exception ex)
            {
                await transation.RollbackAsync();
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 400;
            }
            return response;
        }
    }
}

