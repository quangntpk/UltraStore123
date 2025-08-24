using System.Text.Json;
using UltraStrore.Data;
using UltraStrore.Helper;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using Microsoft.Extensions.Logging;

namespace UltraStrore.Services
{
    public class KhuyenMaiServices : IKhuyenMaiServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IKhuyenMaiServices _service;
        private readonly string _kmPath;
        private readonly ILogger<KhuyenMaiServices> _logger;
        private static MoTaKhuyenMai MoTaTempCreate;
        private static MoTaKhuyenMai MoTaTempEdit;

        public KhuyenMaiServices(ApplicationDbContext context, IWebHostEnvironment env, ILogger<KhuyenMaiServices> logger)
        {
            _context = context;
            _kmPath = Path.Combine(Directory.GetCurrentDirectory(), "DanhMuc", "KhuyenMai.json");
            _logger = logger;
        }

        public async Task<List<KhuyenMaiView>> ListKhuyenMaiUser(int? id)
        {
            _logger.LogInformation("Starting ListKhuyenMaiUser with ID: {Id}", id);
            List<KhuyenMaiView> ListKhuyenMaiView = new List<KhuyenMaiView>();
            try
            {
                var ListKhuyenMai = _context.KhuyenMais
                    .Where(g => g.TrangThai == true && g.BatDau <= DateOnly.FromDateTime(DateTime.Now) && g.KetThuc >= DateOnly.FromDateTime(DateTime.Now))
                    .ToList();
                _logger.LogInformation("Retrieved {Count} active promotions", ListKhuyenMai.Count);

                var ListChiTietKhuyenMai = _context.ChiTietKhuyenMais.ToList();
                string MoTa = File.ReadAllText(_kmPath);
                _logger.LogInformation("Read promotion description file from path: {Path}", _kmPath);

                var FullMoTa = JsonSerializer.Deserialize<List<MoTaKhuyenMaiCreateModel>>(MoTa);
                _logger.LogInformation("Deserialized {Count} promotion descriptions", FullMoTa?.Count ?? 0);

                foreach (var item in ListKhuyenMai)
                {
                    _logger.LogDebug("Processing promotion ID: {PromotionId}", item.ID);
                    KhuyenMaiView KMView = new KhuyenMaiView
                    {
                        ID = item.ID,
                        NgayBatDau = item.BatDau,
                        NgayKetThuc = item.KetThuc,
                        TenKhuyenMai = item.TenKhuyenMai,
                        PercentChung = item.PercentChung,
                        HinhAnh = new List<byte[]>(),
                        TrangThai = item.TrangThai
                    };

                    List<ChiTietKhuyenMaiView> CTKMView = new List<ChiTietKhuyenMaiView>();
                    var DataBegin = ListChiTietKhuyenMai.Where(g => g.MaKhuyenMai == item.ID).ToList();
                    _logger.LogDebug("Found {Count} promotion details for promotion ID: {PromotionId}", DataBegin.Count, item.ID);

                    foreach (var CTKM in DataBegin)
                    {
                        if (CTKM.SP != null)
                        {
                            var SanPhamInfo = _context.SanPhams
                                .Where(g => g.MaSanPham.Contains(CTKM.SP) && g.TrangThai == 1)
                                .OrderBy(g => g.Gia)
                                .ToList();
                            var sp = SanPhamInfo.FirstOrDefault();

                            if (sp != null)
                            {
                                ChiTietKhuyenMaiView CT = new ChiTietKhuyenMaiView
                                {
                                    IdSanPham = sp.MaSanPham.Substring(0, 6),
                                    TenSanPhamCombo = sp.TenSanPham,
                                    GiaGoc = sp.Gia,
                                    GiaMoi = (float)(sp.Gia * (1 - CTKM.PercentRieng / 100)),
                                    Percent = CTKM.PercentRieng,
                                    HinhAnh = _context.HinhAnhs.Where(g => g.MaSanPham.Contains(sp.MaSanPham.Substring(0, 6))).Select(g => g.Data).ToList()
                                };
                                CTKMView.Add(CT);
                                _logger.LogDebug("Added product {ProductId} to promotion view", CT.IdSanPham);
                            }
                        }
                        else
                        {
                            var ComboSummary = _context.ComBoSanPhams
                                .Where(g => g.MaComBo == CTKM.CB && g.TrangThai == true)
                                .FirstOrDefault();
                            if (ComboSummary != null)
                            {
                                ChiTietKhuyenMaiView CT = new ChiTietKhuyenMaiView
                                {
                                    IdCombo = ComboSummary.MaComBo,
                                    TenSanPhamCombo = ComboSummary.TenComBo,
                                    GiaGoc = (int)ComboSummary.TongGia,
                                    GiaMoi = (float)(ComboSummary.TongGia * (1 - CTKM.PercentRieng / 100)),
                                    Percent = CTKM.PercentRieng,
                                    HinhAnh = new List<byte[]> { ComboSummary.HinhAnh }
                                };
                                CTKMView.Add(CT);
                                _logger.LogDebug("Added combo {ComboId} to promotion view", CT.IdCombo);
                            }
                        }
                    }

                    var dataEx = FullMoTa?.Where(g => int.Parse(g.ID) == item.ID).FirstOrDefault();
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
                                        byte[] imageBytes = Convert.FromBase64String(picture.Url);
                                        KMView.HinhAnh.Add(imageBytes);
                                        _logger.LogDebug("Added image to promotion ID: {PromotionId}", item.ID);
                                    }
                                    catch (FormatException ex)
                                    {
                                        _logger.LogError(ex, "Failed to convert base64 image for promotion ID: {PromotionId}", item.ID);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "Unexpected error processing image for promotion ID: {PromotionId}", item.ID);
                                    }
                                }
                            }
                        }
                    }

                    KMView.DanhSachKhuyenMai = CTKMView;
                    ListKhuyenMaiView.Add(KMView);
                    _logger.LogDebug("Completed processing promotion ID: {PromotionId}", item.ID);
                }

                if (id.HasValue)
                {
                    _logger.LogInformation("Filtering promotions by ID: {Id}", id);
                    return ListKhuyenMaiView.Where(g => g.ID == id).ToList();
                }

                _logger.LogInformation("Returning {Count} promotions for user", ListKhuyenMaiView.Count);
                return ListKhuyenMaiView;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ListKhuyenMaiUser with ID: {Id}", id);
                throw;
            }
        }

        public async Task<List<KhuyenMaiView>> ListKhuyenMaiAdmin(int? id)
        {
            _logger.LogInformation("Starting ListKhuyenMaiAdmin with ID: {Id}", id);
            List<KhuyenMaiView> ListKhuyenMaiView = new List<KhuyenMaiView>();
            try
            {
                var ListKhuyenMai = _context.KhuyenMais.ToList();
                _logger.LogInformation("Retrieved {Count} promotions for admin", ListKhuyenMai.Count);

                var ListChiTietKhuyenMai = _context.ChiTietKhuyenMais.ToList();
                string MoTa = File.ReadAllText(_kmPath);
                _logger.LogInformation("Read promotion description file from path: {Path}", _kmPath);

                var FullMoTa = JsonSerializer.Deserialize<List<MoTaKhuyenMaiCreateModel>>(MoTa);
                _logger.LogInformation("Deserialized {Count} promotion descriptions", FullMoTa?.Count ?? 0);

                foreach (var item in ListKhuyenMai)
                {
                    _logger.LogDebug("Processing promotion ID: {PromotionId}", item.ID);
                    KhuyenMaiView KMView = new KhuyenMaiView
                    {
                        ID = item.ID,
                        NgayBatDau = item.BatDau,
                        NgayKetThuc = item.KetThuc,
                        TenKhuyenMai = item.TenKhuyenMai,
                        PercentChung = item.PercentChung,
                        HinhAnh = new List<byte[]>(),
                        TrangThai = item.TrangThai,
                    };

                    List<ChiTietKhuyenMaiView> CTKMView = new List<ChiTietKhuyenMaiView>();
                    var DataBegin = ListChiTietKhuyenMai.Where(g => g.MaKhuyenMai == item.ID).ToList();
                    _logger.LogDebug("Found {Count} promotion details for promotion ID: {PromotionId}", DataBegin.Count, item.ID);

                    foreach (var CTKM in DataBegin)
                    {
                        if (CTKM.SP != null)
                        {
                            var SanPhamInfo = _context.SanPhams
                                .Where(g => g.MaSanPham.Contains(CTKM.SP) && g.TrangThai == 1)
                                .OrderBy(g => g.Gia)
                                .ToList();
                            var sp = SanPhamInfo.FirstOrDefault();

                            if (sp != null)
                            {
                                ChiTietKhuyenMaiView CT = new ChiTietKhuyenMaiView
                                {
                                    IdSanPham = sp.MaSanPham.Substring(0, 6),
                                    TenSanPhamCombo = sp.TenSanPham,
                                    GiaGoc = sp.Gia,
                                    GiaMoi = (float)(sp.Gia * (1 - CTKM.PercentRieng / 100)),
                                    Percent = CTKM.PercentRieng,
                                    HinhAnh = _context.HinhAnhs.Where(g => g.MaSanPham.Contains(sp.MaSanPham.Substring(0, 6))).Select(g => g.Data).ToList()
                                };
                                CTKMView.Add(CT);
                                _logger.LogDebug("Added product {ProductId} to promotion view", CT.IdSanPham);
                            }
                        }
                        else
                        {
                            var ComboSummary = _context.ComBoSanPhams
                                .Where(g => g.MaComBo == CTKM.CB && g.TrangThai == true)
                                .FirstOrDefault();
                            if (ComboSummary != null)
                            {
                                ChiTietKhuyenMaiView CT = new ChiTietKhuyenMaiView
                                {
                                    IdCombo = ComboSummary.MaComBo,
                                    TenSanPhamCombo = ComboSummary.TenComBo,
                                    GiaGoc = (int)ComboSummary.TongGia,
                                    GiaMoi = (float)(ComboSummary.TongGia * (1 - CTKM.PercentRieng / 100)),
                                    Percent = CTKM.PercentRieng,
                                    HinhAnh = new List<byte[]> { ComboSummary.HinhAnh }
                                };
                                CTKMView.Add(CT);
                                _logger.LogDebug("Added combo {ComboId} to promotion view", CT.IdCombo);
                            }
                        }
                    }

                    var dataEx = FullMoTa?.Where(g => int.Parse(g.ID) == item.ID).FirstOrDefault();
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
                                        byte[] imageBytes = Convert.FromBase64String(picture.Url);
                                        KMView.HinhAnh.Add(imageBytes);
                                        _logger.LogDebug("Added image to promotion ID: {PromotionId}", item.ID);
                                    }
                                    catch (FormatException ex)
                                    {
                                        _logger.LogError(ex, "Failed to convert base64 image for promotion ID: {PromotionId}", item.ID);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "Unexpected error processing image for promotion ID: {PromotionId}", item.ID);
                                    }
                                }
                            }
                        }
                    }

                    KMView.DanhSachKhuyenMai = CTKMView;
                    ListKhuyenMaiView.Add(KMView);
                    _logger.LogDebug("Completed processing promotion ID: {PromotionId}", item.ID);
                }

                if (id.HasValue)
                {
                    _logger.LogInformation("Filtering promotions by ID: {Id}", id);
                    return ListKhuyenMaiView.Where(g => g.ID == id).ToList();
                }

                _logger.LogInformation("Returning {Count} promotions for admin", ListKhuyenMaiView.Count);
                return ListKhuyenMaiView;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ListKhuyenMaiAdmin with ID: {Id}", id);
                throw;
            }
        }

        public async Task<APIResponse> KhuyenMaiCreate(KhuyenMaiCreate data)
        {
            _logger.LogInformation("Starting KhuyenMaiCreate for promotion: {PromotionName}", data.TenKhuyenMai);
            APIResponse response = new APIResponse();
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                KhuyenMai newKM = new KhuyenMai
                {
                    TenKhuyenMai = data.TenKhuyenMai,
                    BatDau = data.BatDau,
                    KetThuc = data.KetThuc
                };
                _context.KhuyenMais.Add(newKM);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Created new promotion with ID: {PromotionId}", newKM.ID);

                if (data.All)
                {
                    newKM.All = true;
                    newKM.PercentChung = data.PercentChung;
                    _logger.LogInformation("Set promotion ID: {PromotionId} to apply to all products with discount: {Percent}", newKM.ID, data.PercentChung);
                }
                else
                {
                    foreach (var item in data.ChiTiet)
                    {
                        ChiTietKhuyenMai CTKM = new ChiTietKhuyenMai
                        {
                            PercentRieng = item.Percent,
                            CB = item.IdCombo,
                            SP = item.IdSanPham,
                            MaKhuyenMai = newKM.ID
                        };
                        _context.ChiTietKhuyenMais.Add(CTKM);
                        await _context.SaveChangesAsync();
                        _logger.LogDebug("Added promotion detail for promotion ID: {PromotionId}, Product/Combo: {Id}", newKM.ID, item.IdSanPham ?? item.IdCombo.ToString());
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
                            _logger.LogInformation("Read {Count} existing promotion descriptions from file", existingData.Count);
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogError(ex, "Failed to deserialize promotion description file");
                            response.ErrorMessage = ex.Message;
                            response.ResponseCode = 400;
                            return response;
                        }
                    }
                }
                else
                {
                    await System.IO.File.WriteAllTextAsync(_kmPath, "[]");
                    _logger.LogInformation("Created empty promotion description file at: {Path}", _kmPath);
                }

                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "DanhMuc", "KhuyenMai.json");
                int newIdMoTa = existingData.Count > 0 ? existingData.Max(x => int.Parse(x.IdMoTa)) + 1 : 1;
                int newId = newKM.ID;

                existingData.Add(new MoTaKhuyenMaiCreateModel
                {
                    ID = newId.ToString(),
                    IdMoTa = newIdMoTa.ToString(),
                    MoTa = MoTaTempCreate
                });
                _logger.LogInformation("Added new promotion description with ID: {IdMoTa} for promotion ID: {PromotionId}", newIdMoTa, newId);

                string jsonString = JsonSerializer.Serialize(existingData, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, jsonString);
                _logger.LogInformation("Updated promotion description file at: {Path}", filePath);

                await transaction.CommitAsync();
                _logger.LogInformation("Successfully committed transaction for promotion creation: {PromotionName}", data.TenKhuyenMai);
                MoTaTempCreate = new MoTaKhuyenMai();
                response.ResponseCode = 200;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create promotion: {PromotionName}", data.TenKhuyenMai);
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 400;
            }
            return response;
        }

        public async Task<APIResponse> MoTaKhuyenMaiCreate(MoTaKhuyenMai moTaKhuyenMai)
        {
            _logger.LogInformation("Starting MoTaKhuyenMaiCreate");
            APIResponse response = new APIResponse();
            try
            {
                MoTaTempCreate = moTaKhuyenMai;
                _logger.LogInformation("Stored temporary promotion description");
                response.ResponseCode = 200;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MoTaKhuyenMaiCreate");
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 400;
            }
            return response;
        }

        public async Task<APIResponse> MoTaKhuyenMaiEdit(MoTaKhuyenMai moTaKhuyenMai)
        {
            _logger.LogInformation("Starting MoTaKhuyenMaiEdit");
            APIResponse response = new APIResponse();
            try
            {
                MoTaTempEdit = moTaKhuyenMai;
                _logger.LogInformation("Stored temporary promotion description for edit");
                response.ResponseCode = 200;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MoTaKhuyenMaiEdit");
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 400;
            }
            return response;
        }

        public async Task<APIResponse> KhuyenMaiUpdate(KhuyenMaiEdit data)
        {
            _logger.LogInformation("Starting KhuyenMaiUpdate for promotion ID: {PromotionId}", data.ID);
            APIResponse response = new APIResponse();
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var EditKM = _context.KhuyenMais.FirstOrDefault(g => g.ID == data.ID);
                if (EditKM == null)
                {
                    _logger.LogWarning("Promotion ID: {PromotionId} not found", data.ID);
                    response.ErrorMessage = "Promotion not found";
                    response.ResponseCode = 404;
                    return response;
                }

                var DeleteCT = _context.ChiTietKhuyenMais.Where(g => g.MaKhuyenMai == data.ID).ToList();
                _context.ChiTietKhuyenMais.RemoveRange(DeleteCT);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted {Count} promotion details for promotion ID: {PromotionId}", DeleteCT.Count, data.ID);

                EditKM.TenKhuyenMai = data.TenKhuyenMai;
                EditKM.BatDau = data.BatDau;
                EditKM.KetThuc = data.KetThuc;
                EditKM.TrangThai = true;
                _context.KhuyenMais.Update(EditKM);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated promotion details for ID: {PromotionId}", data.ID);

                if (data.All)
                {
                    EditKM.All = data.All;
                    EditKM.PercentChung = data.PercentChung;
                    _logger.LogInformation("Set promotion ID: {PromotionId} to apply to all products with discount: {Percent}", data.ID, data.PercentChung);
                }
                else
                {
                    foreach (var item in data.ChiTiet)
                    {
                        ChiTietKhuyenMai CTKM = new ChiTietKhuyenMai
                        {
                            PercentRieng = item.Percent,
                            CB = item.IdCombo,
                            SP = item.IdSanPham,
                            MaKhuyenMai = EditKM.ID
                        };
                        _context.ChiTietKhuyenMais.Add(CTKM);
                        await _context.SaveChangesAsync();
                        _logger.LogDebug("Added promotion detail for promotion ID: {PromotionId}, Product/Combo: {Id}", EditKM.ID, item.IdSanPham ?? item.IdCombo.ToString());
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
                            _logger.LogInformation("Read {Count} existing promotion descriptions from file", existingData.Count);
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogError(ex, "Failed to deserialize promotion description file");
                            response.ErrorMessage = ex.Message;
                            response.ResponseCode = 400;
                            return response;
                        }
                    }
                }
                else
                {
                    await System.IO.File.WriteAllTextAsync(_kmPath, "[]");
                    _logger.LogInformation("Created empty promotion description file at: {Path}", _kmPath);
                }

                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "DanhMuc", "KhuyenMai.json");
                int newIdMoTa = existingData.Count > 0 ? existingData.Max(x => int.Parse(x.IdMoTa)) + 1 : 1;
                var ready = existingData.FirstOrDefault(g => int.Parse(g.ID) == EditKM.ID);
                var test = MoTaTempEdit;
                existingData.RemoveAll(g => int.Parse(g.ID) == EditKM.ID);
                _logger.LogInformation("Removed existing description for promotion ID: {PromotionId}", EditKM.ID);

                if (MoTaTempEdit != null)
                {
                    existingData.Add(new MoTaKhuyenMaiCreateModel
                    {
                        ID = data.ID.ToString(),
                        IdMoTa = newIdMoTa.ToString(),
                        MoTa = MoTaTempEdit
                    });
                    _logger.LogInformation("Added updated promotion description with ID: {IdMoTa} for promotion ID: {PromotionId}", newIdMoTa, data.ID);
                }
                else if (ready != null)
                {
                    existingData.Add(new MoTaKhuyenMaiCreateModel
                    {
                        ID = data.ID.ToString(),
                        IdMoTa = ready.IdMoTa,
                        MoTa = ready.MoTa
                    });
                    _logger.LogInformation("Reused existing promotion description with ID: {IdMoTa} for promotion ID: {PromotionId}", ready.IdMoTa, data.ID);
                }
                var test2 = existingData;
                int i = 0;
                string jsonString = JsonSerializer.Serialize(existingData, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, jsonString);
                _logger.LogInformation("Updated promotion description file at: {Path}", filePath);

                await transaction.CommitAsync();
                _logger.LogInformation("Successfully committed transaction for promotion update ID: {PromotionId}", data.ID);
                MoTaTempEdit = new MoTaKhuyenMai();
                response.ResponseCode = 200;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to update promotion ID: {PromotionId}", data.ID);
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 400;
            }
            return response;
        }

        public async Task<APIResponse> DisableKhuyenMai(int id)
        {
            _logger.LogInformation("Starting DisableKhuyenMai for promotion ID: {PromotionId}", id);
            var response = new APIResponse();
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var KM = _context.KhuyenMais.FirstOrDefault(g => g.ID == id);
                if (KM == null)
                {
                    _logger.LogWarning("Promotion ID: {PromotionId} not found", id);
                    response.ErrorMessage = "Promotion not found";
                    response.ResponseCode = 404;
                    return response;
                }

                KM.TrangThai = !KM.TrangThai;
                _context.KhuyenMais.Update(KM);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Toggled status for promotion ID: {PromotionId} to {Status}", id, KM.TrangThai);

                await transaction.CommitAsync();
                _logger.LogInformation("Successfully committed transaction for disabling promotion ID: {PromotionId}", id);
                response.ResponseCode = 200;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to toggle status for promotion ID: {PromotionId}", id);
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 400;
            }
            return response;
        }
    }
}