using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace PrakashCRM.Security
{
    public static class UserTokenStore
    {
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> ActiveTokensByUser
            = new ConcurrentDictionary<string, ConcurrentDictionary<string, byte>>();

        public static void RegisterToken(string userNo, string token)
        {
            if (string.IsNullOrWhiteSpace(userNo) || string.IsNullOrWhiteSpace(token))
                return;

            var userTokens = ActiveTokensByUser.GetOrAdd(userNo, _ => new ConcurrentDictionary<string, byte>());
            userTokens[token] = 1;
        }

        public static bool IsTokenActive(string userNo, string token)
        {
            if (string.IsNullOrWhiteSpace(userNo) || string.IsNullOrWhiteSpace(token))
                return false;

            return ActiveTokensByUser.TryGetValue(userNo, out var userTokens) && userTokens.ContainsKey(token);
        }

        public static void InvalidateToken(string userNo, string token)
        {
            if (string.IsNullOrWhiteSpace(userNo) || string.IsNullOrWhiteSpace(token))
                return;

            if (!ActiveTokensByUser.TryGetValue(userNo, out var userTokens))
                return;

            userTokens.TryRemove(token, out _);
            if (userTokens.IsEmpty)
                ActiveTokensByUser.TryRemove(userNo, out _);
        }

        public static void InvalidateAllTokensForUsers(IEnumerable<string> userNos)
        {
            if (userNos == null)
                return;

            foreach (var userNo in userNos.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
                ActiveTokensByUser.TryRemove(userNo, out _);
        }
    }
}
