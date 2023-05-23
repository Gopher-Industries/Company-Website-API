using System.Text.Json.Serialization;

namespace Database.Models
{
    internal sealed record GCPServiceAccount
    {

        [JsonPropertyName("type")]
        public string Type { get; init; }

        [JsonPropertyName("project_id")]
        public string ProjectId { get; init; }

        [JsonPropertyName("private_key_id")]
        public string PrivateKeyId { get; init; }

        [JsonPropertyName("private_key")]
        public string PrivateKey { get; init; }

        [JsonPropertyName("client_email")]
        public string ClientEmail { get; init; }

        [JsonPropertyName("client_id")]
        public string ClientId { get; init; }

        [JsonPropertyName("auth_uri")]
        public string AuthenticationUri { get; init; }

        [JsonPropertyName("token_uri")]
        public string TokenUri { get; init; }

        [JsonPropertyName("auth_provider_x509_cert_url")]
        public string AuthProviderCertificateUrl { get; init; }

        [JsonPropertyName("client_x509_cert_url")]
        public string ClientCertificateUrl { get; init; }

    }
}
