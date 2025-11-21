using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Services
{
    public interface ISquareWebhookService
    {
        bool VerifySignature(string requestUrl, string requestBody, string signatureHeader);
    }

    public class SquareWebhookService : ISquareWebhookService
    {
        private readonly string _signatureKey;

        public SquareWebhookService(IConfiguration configuration)
        {
            _signatureKey = configuration["Square:WebhookSignatureKey"] ?? string.Empty;
        }

        // Square computes HMAC-SHA1 over: <notification_url> + <request_body>, then base64 encodes it.
        public bool VerifySignature(string requestUrl, string requestBody, string signatureHeader)
        {
            if (string.IsNullOrEmpty(_signatureKey) || string.IsNullOrEmpty(signatureHeader))
                return false;

            var payload = requestUrl + requestBody;
            byte[] keyBytes = Encoding.UTF8.GetBytes(_signatureKey);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA1(keyBytes);
            var hash = hmac.ComputeHash(payloadBytes);
            var expected = Convert.ToBase64String(hash);

            return string.Equals(expected, signatureHeader, StringComparison.Ordinal);
        }
    }
}