namespace UltraStrore.Helper
{
    public class GoogleUserInfo
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string Id { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("email")]
        public string Email { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("verified_email")]
        public bool VerifiedEmail { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string Name { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("given_name")]
        public string GivenName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("family_name")]
        public string FamilyName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("picture")]
        public string Picture { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("locale")]
        public string Locale { get; set; }
    }
}
