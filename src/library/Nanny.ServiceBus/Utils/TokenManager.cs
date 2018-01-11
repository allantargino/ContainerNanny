using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Nanny.ServiceBus
{
    public static class TokenManager
    {
        public static Token GetToken(ConnectionStringValues values, double validFor = 3600)
        {
            var baseAddress = values.ServiceBusEndpoint;
            var SASKeyValue = values.SasKeyValue;
            var SASKeyName = values.SasKeyName;
            var expiresOn = DateTime.UtcNow.AddSeconds(validFor);

            var value = CreateToken(baseAddress, SASKeyName, SASKeyValue, validFor);

            return new Token(value, expiresOn);
        }
        
        private static string CreateToken(string resourceUri, string keyName, string key, double validFor)
        {
            TimeSpan sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
            var expiry = Convert.ToString((int)sinceEpoch.TotalSeconds + validFor);
            string stringToSign = HttpUtility.UrlEncode(resourceUri) + "\n" + expiry;
            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
            var sasToken = String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}", HttpUtility.UrlEncode(resourceUri), HttpUtility.UrlEncode(signature), expiry, keyName);
            return sasToken;
        }
    }
}
