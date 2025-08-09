using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using UltraStrore.Repository;

namespace UltraStrore.Services
{
    public class QRCodeService : IQRCodeService
    {
        /// <summary>
        /// Tạo mã QR và trả về byte array
        /// </summary>
        public byte[] GenerateQRCode(string text, int pixelsPerModule = 10)
        {
            try
            {
                Console.WriteLine($"[DEBUG] Generating QR code for: {text}");

                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
                    using (var qrCode = new PngByteQRCode(qrCodeData))
                    {
                        var result = qrCode.GetGraphic(pixelsPerModule);
                        Console.WriteLine($"[DEBUG] QR code generated successfully, size: {result.Length} bytes");
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to generate QR code: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Tạo mã QR và trả về Base64 string
        /// </summary>
        public string GenerateQRCodeBase64(string text, int pixelsPerModule = 10)
        {
            try
            {
                byte[] qrCodeBytes = GenerateQRCode(text, pixelsPerModule);
                return Convert.ToBase64String(qrCodeBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to generate QR code Base64: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Tạo mã QR có logo ở giữa - Simplified version
        /// </summary>
        public byte[] GenerateQRCodeWithLogo(string text, string logoPath, int pixelsPerModule = 10)
        {
            // ✅ FALLBACK: Nếu QRCode class không work, chỉ trả về QR đơn giản
            try
            {
                Console.WriteLine($"[WARNING] Logo feature not available, generating simple QR code instead");
                return GenerateQRCode(text, pixelsPerModule);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to generate QR code with logo: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Phương pháp thay thế đơn giản
        /// </summary>
        public byte[] GenerateQRCodeAlternative(string text, int pixelsPerModule = 10)
        {
            try
            {
                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
                    using (var qrCode = new PngByteQRCode(qrCodeData))
                    {
                        return qrCode.GetGraphic(pixelsPerModule);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to generate QR code alternative: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Tạo QR code với màu tùy chỉnh - Simplified
        /// </summary>
        public byte[] GenerateQRCodeWithColors(string text, int pixelsPerModule = 10, string darkColor = "#000000", string lightColor = "#FFFFFF")
        {
            // ✅ FALLBACK: Trả về QR đơn giản nếu không support màu
            try
            {
                Console.WriteLine($"[WARNING] Custom colors not available, generating simple QR code instead");
                return GenerateQRCode(text, pixelsPerModule);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to generate QR code with colors: {ex.Message}");
                throw;
            }
        }
    }
}