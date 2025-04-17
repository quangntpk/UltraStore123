using Newtonsoft.Json;
using UltraStrore.Helper;
using System.Text;
using UltraStrore.Repository;
using UltraStrore.Utils;
using UltraStrore.Models.CreateModels;
using UltraStrore.Data;
using Azure;

namespace UltraStrore.Services
{
    public class GeminiServices : IGeminiServices   
    {
        private readonly GeminiSettings _authSettings;
        private readonly ApplicationDbContext _context;
        public ISanPhamServices _SanPhamServicesAddOn;
        public GeminiServices(GeminiSettings authSettings,ApplicationDbContext context, ISanPhamServices SanPhamServicesAddOn)
        {
            _authSettings = authSettings;
            _context = context;
            _SanPhamServicesAddOn = SanPhamServicesAddOn;
        }
        public static string MaKhachHang = "";
        public static string MaSanPham = "";
        public static int SoLuong = 0;
        public async Task<APIResponse> TraLoi(string userInput)
        {
            APIResponse response1 = new APIResponse();
            try
            {
                string Openning = "";
                var GoogleAPIKey = _authSettings.Google.GoogleAPIKey;
                    var GoogleAPIUrl = _authSettings.Google.GoogleAPIUrl;

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new
                                {
                                    text = $"{Openning} + {userInput}.\n"

                                }
                            }
                        }
                    }
                };

                var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");
                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={GoogleAPIKey}", content);
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                    string answer = responseObject?.candidates[0].content?.parts[0]?.text ?? "Xin lỗi, câu hỏi của bạn đã vi phạm chính sách của Google hoặc câu trở lời quá dài nên Rem không hiển thị cho bạn được";
                    response1.ResponseCode = 201;
                    response1.Result = answer.ToString();
                }
            }
            catch (Exception ex)
            {
                response1.ResponseCode = 400;
                response1.ErrorMessage = ex.Message;
            }
            return response1;
        }
        public async Task<APIResponse> Response(RequestGeminiHinhAnh? info)
        {
            APIResponse response1 = new APIResponse();
            try
            {
                var GoogleAPIKey = _authSettings.Google.GoogleAPIKey;
                var GoogleAPIUrl = _authSettings.Google.GoogleAPIUrl;

                var parts = new List<object>
                {
                    new { text = info.CauHoi } 
                };  
                if (info.HinhAnh != null && info.HinhAnh.Count > 0)
                {
                    foreach (var imageBytes in info.HinhAnh)
                    {
                        string base64Image = Convert.ToBase64String(imageBytes);
                        parts.Add(new
                        {
                            inline_data = new
                            {
                                mime_type = "image/png", 
                                data = base64Image
                            }
                        });
                    }
                }
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = parts.ToArray()
                        }
                    },
                    generationConfig = new
                    {
                        responseModalities = new[] { "Text", "Image" } 
                    }
                };
                var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-exp-image-generation:generateContent?key={GoogleAPIKey}", content);
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);

                    string imageBase64 = responseObject?.candidates[0]?.content?.parts[0]?.inlineData?.data;
                    if (string.IsNullOrEmpty(imageBase64))
                    {
                        response1.ResponseCode = 400;
                        response1.ErrorMessage = "Không nhận được hình ảnh từ API.";
                    }
                    else
                    {
                        response1.ResponseCode = 201;
                        response1.Result = imageBase64; 
                    }
                }
            }
            catch (Exception ex)
            {
                response1.ResponseCode = 400;
                response1.ErrorMessage = ex.Message;
            }
            return response1;
        }

        public async Task<APIResponse> PhanLoaiGopY(string noiDung)
        {
            APIResponse response = new APIResponse();
            try
            {
                var GoogleAPIKey = _authSettings.Google.GoogleAPIKey;
                var prompt = $"Phân loại nội dung sau thành 'tích cực', 'tiêu cực' hoặc 'bình thường': {noiDung}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    }
                };

                var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    var apiResponse = await client.PostAsync(
                        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={GoogleAPIKey}",
                        content
                    );
                    var responseString = await apiResponse.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);

                    string result = responseObject?.candidates[0].content?.parts[0]?.text ?? "bình thường";
                    response.ResponseCode = 201;
                    response.Result = result.Trim().ToLower();
                }
            }
            catch (Exception ex)
            {
                response.ResponseCode = 400;
                response.ErrorMessage = ex.Message;
            }
            return response;
        }
        public async Task<APIResponse> TraLoiUpgrade(string userInput, string MaKhachHang)
        {
            string FinalAnswer="";
            APIResponse response1 = new APIResponse();
            try
            {

                string Openning = "\nHãy dựa vào câu hỏi của khách hàng mà hãy trả lời lại ngắn gọn giúp tôi theo yêu cầu dưới đây để tôi truyền câu trả lời của bạn vào hàm thực thi:\n1. " +
                    "Nếu câu hỏi là về thông tin của 1 sản phẩm nào đó," +
                    " hãy trả lời ngắn gọn là có định dạng là \"SP,Thông tin mà người dùng hỏi\". Ví dụ như: \"SP,Quần có màu hồng của hãng Gucci \"" +
                    "\n2. Nếu câu hỏi là về việc mua sản phẩm nào đó, hãy kiểm tra lại 3 xem khách hàng đã cung cấp đủ 4 dữ liệu sau về sản phẩm đã đủ chưa bao gồm: Mã Sản Phẩm, Màu Sắc, Kích Thước và Số lượng" +
                    "\n - Nếu đã đầy đủ thông tin thì trả lời ngắn gọn là \"CART,Mã Sản phẩm, Màu Sắc, Kích Thước, Số Lượng\" với màu sắc đã chuyển qua mã Hex. Ví dụ, khách hàng muốn mua sản phẩm mã A00001, màu đen, số lượng là 10 thì sẽ trả lời : \"CART,A00001,000000,10\" " +
                    "\n - Nếu còn thiếu thông tin thì hãy trả ngắn gọn là \"CART!,null,null,null\" ứng với mỗi thông tin còn thiếu. Ví dụ: \"CART!,null,0000ff,null\" là câu trả lời khi mã sản phẩm trống nhưng có màu xanh và chưa rõ số lượng" ;

                var GoogleAPIKey = _authSettings.Google.GoogleAPIKey;
                var GoogleAPIUrl = _authSettings.Google.GoogleAPIUrl;

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new
                                {
                                    text = $"Đây là câu hỏi của khách hàng: {userInput} {Openning}\n"

                                }
                            }
                        }
                    }
                };

                var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");
                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={GoogleAPIKey}", content);
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                    string answer = responseObject?.candidates[0].content?.parts[0]?.text ?? "Xin lỗi, câu hỏi của bạn đã vi phạm chính sách của Google hoặc câu trở lời quá dài nên Rem không hiển thị cho bạn được";
                    if (answer.StartsWith("SP"))
                        FinalAnswer = await TimKiemSanPham(answer);
                    else if (answer.StartsWith("GH"))
                        FinalAnswer = await ThemVaoGioHang(answer);
                    response1.ResponseCode = 201;
                    response1.Result = FinalAnswer.ToString();
                }
            }
            catch (Exception ex)
            {
                response1.ResponseCode = 400;
                response1.ErrorMessage = ex.Message;
            }
            return response1;
        }
        public async Task<string> TimKiemSanPham(string name)
        {
            string CauTraLoi = "";
            string Search = "";
            try 
            {
                Search = name.Split(',')[1].ToLower().Trim();
            }
            catch
            {
                Search = "Toàn bộ sản phẩm";
            }
            var ListSanPham = await _SanPhamServicesAddOn.ListSanPham(null);
            var data = ListSanPham.ToList();
            if (data.Count > 0)
            {
                string NewOpen = "Đây là danh sách sản phẩm mà khách hàng cần tìm: ";
                string DataShow = "";
                for (int i = 0; i < data.Count(); i++)
                {
                    DataShow = DataShow + $"\n ${i + 1}. Tên Sản Phẩm: " + data[i].Name + "Màu sắc: ";
                    for (int j = 0; j < data[i].MauSac.Count(); j++)
                    {
                        DataShow += $"#{data[i].MauSac[j]},";
                    }
                    DataShow += " Kích thước: ";
                    for (int k = 0; k < data[i].KichThuoc.Count(); k++)
                    {
                        DataShow += $"{data[i].KichThuoc[k]},";
                    }
                    DataShow += $" Thương hiệu : {data[i].ThuongHieu}";
                    DataShow += $" Link Sản Phẩm: http://localhost:8080/product/{data[i].ID}";
                }
                try
                {
                    var GoogleAPIKey = _authSettings.Google.GoogleAPIKey;
                    var GoogleAPIUrl = _authSettings.Google.GoogleAPIUrl;

                    var requestBody = new
                    {
                        contents = new[]
                        {
                            new
                            {
                                parts = new[]
                            {
                                    new
                                    {
                                        text = $"Dựa trên {DataShow}, lọc ra những sản phẩm phù hợp với nội dung tìm kiếm là {Search},sau đó đóng vai nhân viên bán hàng. Trả về nội dung với các yêu cầu sau:\r\n1. Sử dụng thẻ <br> để ngắt dòng, tuyệt đối không sử dụng ký tự xuống dòng \\n hoặc \\n\\n.\r\n2. Mỗi sản phẩm chỉ được gắn một liên kết trong thẻ <a href=\"...\">, với văn bản liên kết là \"Xem chi tiết sản phẩm\". Thẻ <a> phải có màu xanh (#0000FF) và hiệu ứng hover đổi màu thành xanh đậm (#000099), sử dụng thuộc tính style và thẻ <style> để định nghĩa.\r\n3. Định dạng danh sách sản phẩm theo cấu trúc:\r\n   - Tiêu đề sản phẩm in đậm bằng thẻ <strong>.\r\n   - Mô tả sản phẩm, màu sắc (kèm mã màu hex), và kích thước.\r\n   - Liên kết sản phẩm trong thẻ <a> với style như yêu cầu.\r\n4. Đảm bảo nội dung HTML sạch sẽ, không chứa ký tự escape thừa (như \\\") và đúng cú pháp HTML.\r\n5. Bao gồm thẻ <style> để định nghĩa hiệu ứng hover cho các thẻ <a>.\r\n6. Kết thúc bằng câu hỏi mời gọi khách hàng tương tác.\r\n\r\nVí dụ:<style>\r\n  .product-link:hover {{ color: #000099; }}\r\n</style>\r\n" +
                                        $"Ví dụ: Chào anh/chị! Em xin tư vấn một số sản phẩm theo yêu cầu của anh chị:<br><br>\r\n1. <strong>Áo thun nam:</strong> Chất liệu cao cấp, thoáng mát. Màu sắc: đỏ (#ff0000), tím (#ff00ff). Size: S, M, XL, XXL.<br>\r\n<a href=\"http://localhost:8080/products/A00001\" style=\"color: #0000FF; text-decoration: underline;\" class=\"product-link\">Xem chi tiết sản phẩm</a><br><br>\r\n2. <strong>Quần thun nam:</strong> Thiết kế năng động, thoải mái. Màu sắc: đen (#000000), xanh dương (#0C06F5). Size: XL, XXL.<br>\r\n<a href=\"http://localhost:8080/products/Q00001\" style=\"color: #0000FF; text-decoration: underline;\" class=\"product-link\">Xem chi tiết sản phẩm</a><br><br>\r\n3. <strong>Áo khoác nữ:</strong> Sang trọng, ấm áp. Màu sắc: đỏ (#ff0000), tím (#ff00ff). Size: M, XL, XXL.<br>\r\n<a href=\"http://localhost:8080/products/A00002\" style=\"color: #0000FF; text-decoration: underline;\" class=\"product-link\">Xem chi tiết sản phẩm</a><br><br>\r\nAnh/chị quan tâm đến sản phẩm nào ạ? Em sẵn sàng hỗ trợ thêm!\r\n\r\n"

                                    }
                                }
                            }
                        }
                    };

                    var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
                    var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");
                    using (var client = new HttpClient())
                    {
                        var response = await client.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={GoogleAPIKey}", content);
                        var responseString = await response.Content.ReadAsStringAsync();
                        var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                        CauTraLoi = responseObject?.candidates[0].content?.parts[0]?.text ?? "Xin lỗi, câu hỏi của bạn đã vi phạm chính sách của Google hoặc câu trở lời quá dài nên Rem không hiển thị cho bạn được";
                    }
                }
                catch (Exception ex)
                {
                    CauTraLoi = "Không thể lấy dữ liệu về sản phẩm";
                }
            }
            else
                CauTraLoi = "Cửa hàng chúng tôi không bán sản phẩm này";
            
            return CauTraLoi;
        }
        public async Task<string> ThemVaoGioHang(string name)
        {
            return "";
        }
    }
}
