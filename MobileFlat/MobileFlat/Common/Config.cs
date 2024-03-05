using MobileFlat.Models;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace MobileFlat.Common
{
    public static class Config
    {
        #region Preferences
        public static decimal GetLastGlobusBalance()
        {
            return decimal.TryParse(Preferences.Get("LastGlobusBalance", 0.ToString()), out decimal balance)
                ? balance
                : 0;
        }

        public static void SetLastGlobusBalance(decimal value)
        {
            Preferences.Set("LastGlobusBalance", value.ToString());
        }

        public static int GetGlobusBalanceAccessCount()
        {
            return Preferences.Get("GlobusBalanceAccessCount", 0);
        }

        public static void SetGlobusBalanceAccessCount(int value)
        {
            Preferences.Set("GlobusBalanceAccessCount", value);
        }
        #endregion

        #region Secure settings
        public static async Task<string> GetMosOblEircUserAsync()
        {
            return await SecureStorage.GetAsync("MosOblEircUser");
        }

        public static async Task<string> GetMosOblEircPasswordAsync()
        {
            return await SecureStorage.GetAsync("MosOblEircPassword");
        }

        public static async Task<string> GetGlobusUserAsync()
        {
            return await SecureStorage.GetAsync("GlobusUser");
        }

        public static async Task<string> GetGlobusPasswordAsync()
        {
            return await SecureStorage.GetAsync("GlobusPassword");
        }

        public static async Task SetMosOblEircUserAsync(string value)
        {
            await SecureStorage.SetAsync("MosOblEircUser", value);
        }

        public static async Task SetMosOblEircPasswordAsync(string value)
        {
            await SecureStorage.SetAsync("MosOblEircPassword", value);
        }

        public static async Task SetGlobusUserAsync(string value)
        {
            await SecureStorage.SetAsync("GlobusUser", value);
        }

        public static async Task SetGlobusPasswordAsync(string value)
        {
            await SecureStorage.SetAsync("GlobusPassword", value);
        }

        public static async Task<bool> IsSetAsync()
        {
            return
                !string.IsNullOrWhiteSpace(await GetMosOblEircUserAsync()) &&
                !string.IsNullOrWhiteSpace(await GetMosOblEircPasswordAsync()) &&
                !string.IsNullOrWhiteSpace(await GetGlobusUserAsync()) &&
                !string.IsNullOrWhiteSpace(await GetGlobusPasswordAsync());
        }

        public static async Task SaveAsync(Settings model)
        {
            await SetMosOblEircUserAsync(model.MosOblEircUser);
            await SetMosOblEircPasswordAsync(model.MosOblEircPassword);
            await SetGlobusUserAsync(model.GlobusUser);
            await SetGlobusPasswordAsync(model.GlobusPassword);
        }
        #endregion
    }
}
