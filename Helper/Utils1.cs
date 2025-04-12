namespace UltraStrore.Helper
{
    public static class Utils1
    {
        public static string GetIpAddress(HttpContext context)
        {
            string ipAddress = context.Connection.RemoteIpAddress?.ToString();

            // Nếu không lấy được IP từ RemoteIpAddress (ví dụ: chạy local), thử lấy từ header X-Forwarded-For
            if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1") // "::1" là localhost trong IPv6
            {
                ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            }

            // Nếu vẫn không có, trả về mặc định
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = "127.0.0.1"; // Địa chỉ IP mặc định (localhost)
            }

            return ipAddress;
        }
    }
}
