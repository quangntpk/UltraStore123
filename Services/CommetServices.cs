using UltraStrore.Data;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;

namespace UltraStrore.Services
{
    public class CommetServices : ICommetServices
    {
        private readonly ApplicationDbContext _context;

        public CommetServices(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<BinhLuanView>> ListBinhLuan()
        {
            // Đường link ngắn của ảnh mặc định
            string defaultImageUrl = "data:image/jpeg;base64,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"; // Có thể thay bằng link ảnh khác

            // Lấy danh sách bình luận từ bảng BinhLuans
            var binhLuans = await _context.BinhLuans
                .Select(bl => new BinhLuanView
                {
                    MaBlog = bl.MaBlog,
                    MaCombo = bl.MaCombo,
                    MaBinhLuan = bl.MaBinhLuan,
                    MaSanPham = bl.MaSanPham,
                    MaNguoiDung = bl.MaNguoiDung,
                    NoiDungBinhLuan = bl.NoiDungBinhLuan,
                    SoTimBinhLuan = bl.SoTimBinhLuan,
                    DanhGia = bl.DanhGia,
                    TrangThai = bl.TrangThai,
                    NgayBinhLuan = bl.NgayBinhLuan
                })
                .ToListAsync();

            // Duyệt qua từng bình luận để lấy thông tin bổ sung
            foreach (var binhLuan in binhLuans)
            {
                // Lấy thông tin người dùng
                var nguoiDung = await _context.NguoiDungs
                    .FirstOrDefaultAsync(nd => nd.MaNguoiDung == binhLuan.MaNguoiDung);

                // Gán HoTen
                binhLuan.HoTen = nguoiDung?.HoTen;

                // Gán HinhAnh
                binhLuan.HinhAnh = nguoiDung != null && nguoiDung.HinhAnh != null
                    ? $"data:image/jpeg;base64,{Convert.ToBase64String(nguoiDung.HinhAnh)}"
                    : defaultImageUrl;

                // Lấy thông tin sản phẩm (so sánh 6 ký tự đầu của MaSanPham)
                if (!string.IsNullOrEmpty(binhLuan.MaSanPham) && binhLuan.MaSanPham.Length >= 6)
                {
                    string maSanPhamShort = binhLuan.MaSanPham.Substring(0, 6); // Lấy 6 ký tự đầu
                    var sanPham = await _context.SanPhams
                        .FirstOrDefaultAsync(sp => sp.MaSanPham.StartsWith(maSanPhamShort));
                    binhLuan.TenSanPham = sanPham?.TenSanPham;
                }
                else
                {
                    binhLuan.TenSanPham = null; // Nếu MaSanPham không đủ dài, gán null
                }
            }

            return binhLuans;
        }

        public async Task<BinhLuanView> AddBinhLuan(BinhLuanCreate binhLuan)
        {
            // Danh sách từ ngữ thô tục
            var badWords = new List<string>
    {

        "lồn", "cặc", "địt", "chịch", "buồi", "đụ", "đéo", "điếm", "bitch", "fuck",
        "dick", "pussy", "asshole", "motherfucker", "cu", "chó chết", "dâm", "ngu",
        "vl", "dm", "clgt", "vcl", "phò", "đĩ", "khốn nạn", "con mẹ mày", "đéo mẹ",
        "nứng", "tổ sư", "mẹ kiếp", "bố mày", "liếm lồn", "liếm cặc", "óc chó",
        "cave", "đm", "wtf", "fucking", "shit", "cum", "slut", "whore", "tits",
        "boobs", "rape", "jerk", "suck", "balls", "blowjob", "handjob", "faggot",
        "gay", "lesbian", "dildo", "vagina", "penis", "anus", "scum", "bastard",
        "đĩ mẹ", "đụ mẹ", "đụ cha", "đụ bà", "mẹ cha", "đù", "fuck you", "đéo hiểu",
        "mẹ nó", "fuck off", "cút", "get lost", "piss off", "liếm buồi", "bú cặc",
        "bú lồn", "bố láo", "chó má", "súc vật", "mất dạy", "khốn", "khốn kiếp",
        "mẹ kiếp", "thằng khốn", "con khốn", "dickhead", "cunt", "shithead", "piss",
        "pissing", "screw you", "goddamn", "son of a bitch", "sonofabitch", "dirty",
        "mothafucka", "jackass", "douchebag", "retard", "fuckface", "cock", "shitbag",
        "fuckwit", "fuckstick", "arsehole", "tosser", "bloody hell", "cuntface",
        "ballsack", "fucker", "dickhead", "bitchface", "ho", "cumdumpster", "dickwad",
        "twat", "shitfaced", "cockface", "gobshite", "bollocks", "minger", "arse",
        "knobhead", "twatwaffle", "dumbfuck", "shitcunt", "cumslut", "wanker", "prick",
        "fucknugget", "fuckhead", "dickweasel", "cockmongler", "dickfucker", "shitweasel",
        "fucksocks", "fucksponge", "fuckbiscuit", "fuckbucket", "cumguzzler", "cockjockey",
        "shitbrick", "cumbucket", "fucktard", "dicknose", "shitstain", "craphole",
        "fuckpile", "shitstick", "fuckbunny", "fuckrag", "fuckknuckle", "shitsmear",
        "cocksucker", "cocksplat",

        "đít", "lỗ đít", "cứt", "đù má", "đéo thèm", "mẹ mày", "bố láo bố lếu",
        "con đĩ", "thằng chó", "con chó", "đồ ngu", "ngốc", "đần", "hãm", "lỗn",
        "đầu buồi", "bú buồi", "đụ con mẹ", "địt mẹ", "địt cha", "đéo cần",
        "cặc lồn", "vãi lồn", "vãi cặc", "đéo chịu", "mẹ cha mày", "đồ súc sinh",
        "thằng đần", "con đần", "đồ mất dạy", "khốn nạn kiếp", "đéo ra gì",
        "cặc gì", "lồn gì", "đù mẹ", "đồ đểu", "thằng đểu", "con đểu",
        "đéo đáng", "mẹ mày chứ", "bố mẹ mày", "đù cha", "đéo thằng nào",
        "vãi đái", "đái bậy", "ỉa bậy", "đồ khốn", "thằng ngu", "con ngu",
        "mẹ kiếp đời",

        "đồ dỏm", "hàng giả", "hàng đểu", "lừa đảo", "đồ lừa", "bán đồ rởm",
        "dịch vụ tệ", "dịch vụ như cứt", "thái độ lồi lõm", "nhân viên ngu",
        "đồ thối", "hàng thối", "chất lượng như lồn", "đồ cùi", "hàng cùi bắp",
        "lừa tiền", "ăn cắp tiền", "bán đồ như cứt", "đồ rẻ rách", "hàng rẻ tiền",
        "đồ vứt đi", "hàng như rác", "dịch vụ như hạch", "bán hàng đểu", "hàng dởm",
        "đồ giả mạo", "bán hàng lừa", "chất lượng rác", "đồ tồi", "hàng tồi",
        "dịch vụ khốn nạn", "nhân viên đểu", "thái độ như cứt", "bán đồ hỏng",
        "hàng hỏng", "đồ lỗi", "bán hàng lỗi", "lừa khách", "đéo đáng tiền",
        "đồ đắt cắt cổ", "hàng kém", "chất lượng kém", "bán đồ kém", "đồ đểu cáng",
        "hàng như hạch", "dịch vụ dởm", "nhân viên láo", "thái độ láo",
        "bán hàng giả", "đồ không xài được", "hàng vớ vẩn", "đồ vớ vẩn",
        "bán đồ vớ vẩn", "dịch vụ lừa đảo", "nhân viên khốn", "thái độ khốn",
        "bán hàng đéo ra gì", "đồ rởm rít", "hàng rởm rít", "đồ dở hơi",
        "bán đồ dở hơi", "hàng không ra gì", "đồ như đồ bỏ", "bán đồ bỏ",
        "dịch vụ chó má", "nhân viên chó má", "thái độ chó má", "bán hàng ngu",
        "đồ ngu xuẩn", "hàng ngu xuẩn", "đồ không đáng tiền", "bán đồ cắt cổ",
        "hàng cắt cổ", "đồ lừa gạt", "bán hàng lừa gạt", "dịch vụ đểu cáng",
        "nhân viên đểu cáng", "thái độ đểu cáng", "bán hàng như lồn", "đồ tào lao",
        "hàng tào lao", "đồ bèo nhèo", "bán đồ bèo nhèo", "hàng bèo nhèo",
        "đồ không giống quảng cáo", "bán hàng khác quảng cáo", "dịch vụ ngu xuẩn",
        "nhân viên ngu dốt", "thái độ ngu dốt", "bán hàng dối trá", "đồ dối trá",
        "hàng dối trá", "đồ không đúng mô tả", "bán hàng không đúng mô tả",
        "dịch vụ tào lao", "nhân viên tào lao", "thái độ tào lao"
    };

            // Hàm tiền xử lý để làm sạch nội dung bình luận
            string CleanComment(string comment)
            {
                if (string.IsNullOrEmpty(comment)) return comment;

                // Danh sách ký tự đặc biệt cần loại bỏ hoặc thay bằng khoảng trắng
                char[] specialChars = { '@', '!', '#', '$', '%', '^', '.', '?', ':', '"', '<', '>', '*', '&', '(', ')', '-', '_', '+', '=', '[', ']', '{', '}', '|', '\\', '/', ',', ';' };

                // Chuyển về chữ thường
                string cleaned = comment.ToLower();

                // Thay thế ký tự đặc biệt bằng khoảng trắng
                foreach (var c in specialChars)
                {
                    cleaned = cleaned.Replace(c.ToString(), " ");
                }

                // Thay thế nhiều khoảng trắng liên tiếp bằng một khoảng trắng
                cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\s+", " ");

                // Loại bỏ khoảng trắng ở đầu và cuối
                return cleaned.Trim();
            }

            // Kiểm tra từ ngữ thô tục
            string cleanedComment = CleanComment(binhLuan.NoiDungBinhLuan);
            bool containsBadWords = badWords.Any(word => cleanedComment.Contains(word.ToLower()));

            // Tạo một đối tượng BinhLuan từ BinhLuanCreate
            var newBinhLuan = new BinhLuan
            {
                MaCombo = binhLuan.MaCombo,
                MaBlog = binhLuan.MaBlog,
                MaSanPham = binhLuan.MaSanPham,
                MaNguoiDung = binhLuan.MaNguoiDung,
                NoiDungBinhLuan = binhLuan.NoiDungBinhLuan,
                SoTimBinhLuan = binhLuan.SoTimBinhLuan ?? 0, // Giá trị mặc định nếu null
                DanhGia = binhLuan.DanhGia,
                TrangThai = containsBadWords ? 0 : 1, // 0 nếu có từ thô tục, 1 nếu không
                NgayBinhLuan = binhLuan.NgayBinhLuan ?? DateTime.Now // Gán ngày hiện tại nếu null
            };

            _context.BinhLuans.Add(newBinhLuan);
            await _context.SaveChangesAsync();

            // Lấy thông tin bổ sung
            var nguoiDung = await _context.NguoiDungs
                .FirstOrDefaultAsync(nd => nd.MaNguoiDung == binhLuan.MaNguoiDung);
            string tenSanPham = null;
            if (!string.IsNullOrEmpty(binhLuan.MaSanPham) && binhLuan.MaSanPham.Length >= 6)
            {
                string maSanPhamShort = binhLuan.MaSanPham.Substring(0, 6);
                var sanPham = await _context.SanPhams
                    .FirstOrDefaultAsync(sp => sp.MaSanPham.StartsWith(maSanPhamShort));
                tenSanPham = sanPham?.TenSanPham;
            }

            // Trả về một BinhLuanView từ dữ liệu vừa thêm
            return new BinhLuanView
            {
                MaBlog = newBinhLuan.MaBlog,
                MaCombo = newBinhLuan.MaCombo,
                MaBinhLuan = newBinhLuan.MaBinhLuan,
                MaSanPham = newBinhLuan.MaSanPham,
                TenSanPham = tenSanPham,
                MaNguoiDung = newBinhLuan.MaNguoiDung,
                HoTen = nguoiDung?.HoTen,
                NoiDungBinhLuan = newBinhLuan.NoiDungBinhLuan,
                SoTimBinhLuan = newBinhLuan.SoTimBinhLuan,
                DanhGia = newBinhLuan.DanhGia,
                TrangThai = newBinhLuan.TrangThai,
                NgayBinhLuan = newBinhLuan.NgayBinhLuan,
                HinhAnh = nguoiDung != null && nguoiDung.HinhAnh != null
                    ? $"data:image/jpeg;base64,{Convert.ToBase64String(nguoiDung.HinhAnh)}"
                    : "data:image/jpeg;base64,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"
            };
        }

        public async Task<BinhLuanView> UpdateBinhLuan(int maBinhLuan, BinhLuanEdit binhLuan)
        {
            var existingBinhLuan = await _context.BinhLuans
                .FirstOrDefaultAsync(bl => bl.MaBinhLuan == maBinhLuan);

            if (existingBinhLuan == null)
            {
                return null; // Hoặc throw exception tùy yêu cầu
            }

            // Cập nhật các thuộc tính
            existingBinhLuan.MaSanPham = binhLuan.MaSanPham;
            existingBinhLuan.MaNguoiDung = binhLuan.MaNguoiDung;
            existingBinhLuan.NoiDungBinhLuan = binhLuan.NoiDungBinhLuan;
            existingBinhLuan.SoTimBinhLuan = binhLuan.SoTimBinhLuan;
            existingBinhLuan.DanhGia = binhLuan.DanhGia;
            existingBinhLuan.NgayBinhLuan = binhLuan.NgayBinhLuan;

            await _context.SaveChangesAsync();

            // Lấy thông tin bổ sung
            var nguoiDung = await _context.NguoiDungs
                .FirstOrDefaultAsync(nd => nd.MaNguoiDung == binhLuan.MaNguoiDung);
            string tenSanPham = null;
            if (!string.IsNullOrEmpty(binhLuan.MaSanPham) && binhLuan.MaSanPham.Length >= 6)
            {
                string maSanPhamShort = binhLuan.MaSanPham.Substring(0, 6);
                var sanPham = await _context.SanPhams
                    .FirstOrDefaultAsync(sp => sp.MaSanPham.StartsWith(maSanPhamShort));
                tenSanPham = sanPham?.TenSanPham;
            }

            // Trả về một BinhLuanView từ dữ liệu vừa cập nhật
            return new BinhLuanView
            {
                MaCombo = existingBinhLuan.MaCombo,
                MaBlog = existingBinhLuan.MaBlog,
                MaBinhLuan = existingBinhLuan.MaBinhLuan,
                MaSanPham = existingBinhLuan.MaSanPham,
                TenSanPham = tenSanPham,
                MaNguoiDung = existingBinhLuan.MaNguoiDung,
                HoTen = nguoiDung?.HoTen,
                NoiDungBinhLuan = existingBinhLuan.NoiDungBinhLuan,
                SoTimBinhLuan = existingBinhLuan.SoTimBinhLuan,
                DanhGia = existingBinhLuan.DanhGia,
                TrangThai = existingBinhLuan.TrangThai,
                NgayBinhLuan = existingBinhLuan.NgayBinhLuan,
                HinhAnh = nguoiDung != null && nguoiDung.HinhAnh != null
                    ? $"data:image/jpeg;base64,{Convert.ToBase64String(nguoiDung.HinhAnh)}"
                    : "https://via.placeholder.com/50"
            };
        }

        public async Task<bool> DeleteBinhLuan(int maBinhLuan)
        {
            var binhLuanToRemove = await _context.BinhLuans
                .FirstOrDefaultAsync(bl => bl.MaBinhLuan == maBinhLuan);

            if (binhLuanToRemove == null)
            {
                return false;
            }

            _context.BinhLuans.Remove(binhLuanToRemove);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApproveBinhLuan(int maBinhLuan)
        {
            var binhLuan = await _context.BinhLuans
                .FirstOrDefaultAsync(bl => bl.MaBinhLuan == maBinhLuan);

            if (binhLuan == null)
            {
                return false;
            }

            binhLuan.TrangThai = 1; // Cập nhật trạng thái thành "Đã Duyệt"
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnapproveBinhLuan(int maBinhLuan)
        {
            var binhLuan = await _context.BinhLuans
                .FirstOrDefaultAsync(bl => bl.MaBinhLuan == maBinhLuan);

            if (binhLuan == null)
            {
                return false;
            }

            binhLuan.TrangThai = 0; // Cập nhật trạng thái thành "Chưa Duyệt"
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LikeBinhLuan(int maBinhLuan)
        {
            var binhLuan = await _context.BinhLuans
                .FirstOrDefaultAsync(bl => bl.MaBinhLuan == maBinhLuan);

            if (binhLuan == null)
            {
                return false;
            }

            binhLuan.SoTimBinhLuan = (binhLuan.SoTimBinhLuan ?? 0) + 1; // Tăng số lượt thích
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlikeBinhLuan(int maBinhLuan)
        {
            var binhLuan = await _context.BinhLuans
                .FirstOrDefaultAsync(bl => bl.MaBinhLuan == maBinhLuan);

            if (binhLuan == null)
            {
                return false;
            }

            binhLuan.SoTimBinhLuan = Math.Max(0, (binhLuan.SoTimBinhLuan ?? 0) - 1); // Giảm số lượt thích, không âm
            await _context.SaveChangesAsync();
            return true;
        }
    }



}