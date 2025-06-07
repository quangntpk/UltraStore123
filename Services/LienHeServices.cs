using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UltraStrore.Services
{
    public class LienHeServices : ILienHeServices
    {
        private readonly SheetsService _sheetsService;
        private readonly string _spreadsheetId;
        private readonly string _sheetName = "LienHe";

        public LienHeServices(IConfiguration configuration)
        {
            var googleSheetConfig = configuration.GetSection("GoogleSheet");
            _spreadsheetId = googleSheetConfig["SpreadsheetId"];

            var credentialJson = JsonConvert.SerializeObject(new
            {
                type = googleSheetConfig["type"],
                project_id = googleSheetConfig["project_id"],
                private_key_id = googleSheetConfig["private_key_id"],
                private_key = googleSheetConfig["private_key"],
                client_email = googleSheetConfig["client_email"],
                client_id = googleSheetConfig["client_id"],
                auth_uri = googleSheetConfig["auth_uri"],
                token_uri = googleSheetConfig["token_uri"],
                auth_provider_x509_cert_url = googleSheetConfig["auth_provider_x509_cert_url"],
                client_x509_cert_url = googleSheetConfig["client_x509_cert_url"],
                universe_domain = googleSheetConfig["universe_domain"]
            });

            var credential = GoogleCredential.FromJson(credentialJson)
                .CreateScoped(SheetsService.Scope.Spreadsheets);

            _sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "UltraStore"
            });

            InitializeSheetAsync().Wait();
        }

        private async Task InitializeSheetAsync()
        {
            try
            {
                var spreadsheet = await _sheetsService.Spreadsheets.Get(_spreadsheetId).ExecuteAsync();
                var sheet = spreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == _sheetName);

                if (sheet == null)
                {
                    var addSheetRequest = new BatchUpdateSpreadsheetRequest()
                    {
                        Requests = new List<Request>()
                        {
                            new Request()
                            {
                                AddSheet = new AddSheetRequest()
                                {
                                    Properties = new SheetProperties()
                                    {
                                        Title = _sheetName
                                    }
                                }
                            }
                        }
                    };
                    await _sheetsService.Spreadsheets.BatchUpdate(addSheetRequest, _spreadsheetId).ExecuteAsync();
                }

                var range = $"{_sheetName}!A1:G1";
                var getRequest = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);
                var response = await getRequest.ExecuteAsync();

                if (response.Values == null || response.Values.Count == 0)
                {
                    var headers = new List<object> { "MaLienHe", "HoTen", "Sdt", "NoiDung", "Email", "TrangThai", "NgayTao" };
                    var valueRange = new ValueRange()
                    {
                        Values = new List<IList<object>> { headers }
                    };
                    var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
                    updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                    await updateRequest.ExecuteAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khởi tạo Google Sheet: {ex.Message}");
            }
        }

        public async Task<List<LienHeView>> GetLienHeList(string? searchTerm)
        {
            try
            {
                var range = $"{_sheetName}!A:G";
                var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);
                var response = await request.ExecuteAsync();

                var lienHeList = new List<LienHeView>();

                if (response.Values != null && response.Values.Count > 1)
                {
                    for (int i = 1; i < response.Values.Count; i++)
                    {
                        var row = response.Values[i];
                        if (row.Count >= 7)
                        {
                            var lienHe = new LienHeView
                            {
                                MaLienHe = int.TryParse(row[0]?.ToString(), out int maLienHe) ? maLienHe : 0,
                                HoTen = row[1]?.ToString(),
                                Sdt = row[2]?.ToString(),
                                NoiDung = row[3]?.ToString(),
                                Email = row[4]?.ToString(),
                                TrangThai = int.TryParse(row[5]?.ToString(), out int trangThai) ? trangThai : 0,
                                NgayTao = DateTime.TryParse(row[6]?.ToString(), out DateTime ngayTao) ? ngayTao : DateTime.MinValue
                            };

                            if (!string.IsNullOrEmpty(searchTerm))
                            {
                                searchTerm = searchTerm.Trim().ToLower();
                                if (lienHe.MaLienHe.ToString().Contains(searchTerm) ||
                                    (lienHe.HoTen?.ToLower().Contains(searchTerm) == true) ||
                                    (lienHe.Sdt?.ToLower().Contains(searchTerm) == true) ||
                                    (lienHe.NoiDung?.ToLower().Contains(searchTerm) == true) ||
                                    (lienHe.Email?.ToLower().Contains(searchTerm) == true))
                                {
                                    lienHeList.Add(lienHe);
                                }
                            }
                            else
                            {
                                lienHeList.Add(lienHe);
                            }
                        }
                    }
                }

                return lienHeList;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách liên hệ: {ex.Message}");
            }
        }

        public async Task<LienHeView> GetLienHeById(int id)
        {
            try
            {
                var list = await GetLienHeList(null);
                var lienHe = list.FirstOrDefault(l => l.MaLienHe == id);

                if (lienHe == null)
                    throw new Exception("Liên hệ không tồn tại.");

                return lienHe;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy thông tin liên hệ: {ex.Message}");
            }
        }

        public async Task<LienHeView> CreateLienHe(LienHeCreate model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            try
            {
                var existingList = await GetLienHeList(null);
                int newId = existingList.Count > 0 ? existingList.Max(l => l.MaLienHe) + 1 : 1;

                var newLienHe = new LienHeView
                {
                    MaLienHe = newId,
                    HoTen = model.HoTen,
                    Sdt = model.Sdt,
                    NoiDung = model.NoiDung,
                    Email = model.Email,
                    TrangThai = model.TrangThai,
                    NgayTao = DateTime.Now
                };

                var values = new List<object>
                {
                    newLienHe.MaLienHe,
                    newLienHe.HoTen,
                    newLienHe.Sdt,
                    newLienHe.NoiDung,
                    newLienHe.Email,
                    newLienHe.TrangThai,
                    newLienHe.NgayTao
                };

                var range = $"{_sheetName}!A:G";
                var valueRange = new ValueRange()
                {
                    Values = new List<IList<object>> { values }
                };

                var appendRequest = _sheetsService.Spreadsheets.Values.Append(valueRange, _spreadsheetId, range);
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
                await appendRequest.ExecuteAsync();

                return newLienHe;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tạo liên hệ: {ex.Message}");
            }
        }

        public async Task<LienHeView> AddLienHe(LienHeCreate model)
        {
            return await CreateLienHe(model);
        }

        public async Task<LienHeView> UpdateLienHe(LienHeEdit model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            try
            {
                var range = $"{_sheetName}!A:G";
                var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);
                var response = await request.ExecuteAsync();

                if (response.Values == null)
                    throw new Exception("Liên hệ không tồn tại.");

                int rowIndex = -1;
                for (int i = 1; i < response.Values.Count; i++)
                {
                    if (response.Values[i].Count > 0 &&
                        int.TryParse(response.Values[i][0]?.ToString(), out int id) &&
                        id == model.MaLienHe)
                    {
                        rowIndex = i + 1;
                        break;
                    }
                }

                if (rowIndex == -1)
                    throw new Exception("Liên hệ không tồn tại.");

                var ngayTao = DateTime.TryParse(response.Values[rowIndex - 1][6]?.ToString(), out DateTime existingDate)
                    ? existingDate : DateTime.Now;

                var updatedLienHe = new LienHeView
                {
                    MaLienHe = model.MaLienHe,
                    HoTen = model.HoTen,
                    Sdt = model.Sdt,
                    NoiDung = model.NoiDung,
                    Email = model.Email,
                    TrangThai = model.TrangThai,
                    NgayTao = ngayTao
                };

                var values = new List<object>
                {
                    updatedLienHe.MaLienHe,
                    updatedLienHe.HoTen,
                    updatedLienHe.Sdt,
                    updatedLienHe.NoiDung,
                    updatedLienHe.Email,
                    updatedLienHe.TrangThai,
                    updatedLienHe.NgayTao
                };

                var updateRange = $"{_sheetName}!A{rowIndex}:G{rowIndex}";
                var valueRange = new ValueRange()
                {
                    Values = new List<IList<object>> { values }
                };

                var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, _spreadsheetId, updateRange);
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                await updateRequest.ExecuteAsync();

                return updatedLienHe;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật liên hệ: {ex.Message}");
            }
        }
        public async Task<bool> DeleteLienHe(int id)
        {
            try
            {
                var range = $"{_sheetName}!A:G";
                var response = await _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range).ExecuteAsync();

                if (response.Values == null || response.Values.Count <= 1)
                    return false;

                for (int i = 1; i < response.Values.Count; i++)
                {
                    var row = response.Values[i];
                    if (row.Count > 0 &&
                        int.TryParse(row[0]?.ToString(), out int maLienHe) &&
                        maLienHe == id)
                    {
                        var sheetId = await GetSheetId(_sheetName);
                        if (sheetId == null)
                            throw new Exception("Không tìm thấy sheet để xoá.");

                        var deleteRequest = new BatchUpdateSpreadsheetRequest
                        {
                            Requests = new List<Request>
                    {
                        new Request
                        {
                            DeleteDimension = new DeleteDimensionRequest
                            {
                                Range = new DimensionRange
                                {
                                    SheetId = sheetId,
                                    Dimension = "ROWS",
                                    StartIndex = i,
                                    EndIndex = i + 1
                                }
                            }
                        }
                    }
                        };

                        await _sheetsService.Spreadsheets.BatchUpdate(deleteRequest, _spreadsheetId).ExecuteAsync();
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xóa liên hệ: {ex.Message}");
            }
        }

        public async Task<bool> DeleteMultipleLienHe(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
                return false;

            try
            {
                var range = $"{_sheetName}!A:G";
                var response = await _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range).ExecuteAsync();

                if (response.Values == null || response.Values.Count <= 1)
                    return false;

                var sheetId = await GetSheetId(_sheetName);
                if (sheetId == null)
                    throw new Exception("Không tìm thấy sheet để xoá.");

                var rowsToDelete = new List<int>();

                for (int i = 1; i < response.Values.Count; i++)
                {
                    var row = response.Values[i];
                    if (row.Count > 0 &&
                        int.TryParse(row[0]?.ToString(), out int maLienHe) &&
                        ids.Contains(maLienHe))
                    {
                        rowsToDelete.Add(i);
                    }
                }

                if (rowsToDelete.Count == 0)
                    return false;

                rowsToDelete.Sort((a, b) => b.CompareTo(a));

                var deleteRequests = rowsToDelete.Select(rowIndex => new Request
                {
                    DeleteDimension = new DeleteDimensionRequest
                    {
                        Range = new DimensionRange
                        {
                            SheetId = sheetId,
                            Dimension = "ROWS",
                            StartIndex = rowIndex,
                            EndIndex = rowIndex + 1
                        }
                    }
                }).ToList();

                var batchRequest = new BatchUpdateSpreadsheetRequest
                {
                    Requests = deleteRequests
                };

                await _sheetsService.Spreadsheets.BatchUpdate(batchRequest, _spreadsheetId).ExecuteAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xóa nhiều liên hệ: {ex.Message}");
            }
        }

        private async Task<int?> GetSheetId(string sheetName)
        {
            var spreadsheet = await _sheetsService.Spreadsheets.Get(_spreadsheetId).ExecuteAsync();
            var sheet = spreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == sheetName);
            return sheet?.Properties.SheetId;
        }
    }
}
