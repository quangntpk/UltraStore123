namespace UltraStrore.Repository
{
    public interface IQRCodeService
    {
        byte[] GenerateQRCode(string text, int pixelsPerModule = 10);
        string GenerateQRCodeBase64(string text, int pixelsPerModule = 10);
        byte[] GenerateQRCodeWithLogo(string text, string logoPath, int pixelsPerModule = 10);
        byte[] GenerateQRCodeAlternative(string text, int pixelsPerModule = 10);
        byte[] GenerateQRCodeWithColors(string text, int pixelsPerModule = 10, string darkColor = "#000000", string lightColor = "#FFFFFF");
    }
}