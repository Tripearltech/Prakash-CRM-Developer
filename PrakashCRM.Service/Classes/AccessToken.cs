using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrakashCRM.Service.Classes
{
    public class AccessToken
    {
        // I usually let my token "expire" 5 minutes before it's actual expiration
        // to avoid using expired tokens and getting 401.
        private static readonly TimeSpan Threshold = new TimeSpan(0, 5, 0);

        public AccessToken(string access_token, int expires_in)
        {
            Token = access_token;
            ExpiresInSeconds = expires_in;
            Expires = DateTime.UtcNow.AddSeconds(expires_in);
        }

        public string Token { get; }
        public int ExpiresInSeconds { get; }
        public DateTime Expires { get; }
        public bool Expired => (Expires - DateTime.UtcNow).TotalSeconds <= Threshold.TotalSeconds;
    }
}