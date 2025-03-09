namespace UltraStrore.Utils
{
    public class GeminiAPI
    {
        public string GoogleAPIUrl { get; set; }
        public string GoogleAPIKey { get; set; }
    }

    public class GeminiSettings
    {
        public GeminiAPI Google { get; set; }
    }
}
