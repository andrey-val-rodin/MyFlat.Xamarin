using MobileFlat.Models;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace MobileFlat.Common
{
    public class ConfigImpl : IConfig
    {
        #region Preferences
        public decimal GetLastGlobusBalance()
        {
            return decimal.TryParse(Preferences.Get("LastGlobusBalance", 0.ToString()), out decimal balance)
                ? balance
                : 0;
        }

        public void SetLastGlobusBalance(decimal value)
        {
            Preferences.Set("LastGlobusBalance", value.ToString());
        }

        public int GetGlobusBalanceAccessCount()
        {
            return Preferences.Get("GlobusBalanceAccessCount", 0);
        }

        public void SetGlobusBalanceAccessCount(int value)
        {
            Preferences.Set("GlobusBalanceAccessCount", value);
        }
        #endregion

        #region Secure settings
        public async Task<string> GetMosOblEircUserAsync()
        {
            return await SecureStorage.GetAsync("MosOblEircUser");
        }

        public async Task<string> GetMosOblEircPasswordAsync()
        {
            return await SecureStorage.GetAsync("MosOblEircPassword");
        }

        public async Task<string> GetGlobusUserAsync()
        {
            return await SecureStorage.GetAsync("GlobusUser");
        }

        public async Task<string> GetGlobusPasswordAsync()
        {
            return await SecureStorage.GetAsync("GlobusPassword");
        }

        public async Task SetMosOblEircUserAsync(string value)
        {
            await SecureStorage.SetAsync("MosOblEircUser", value);
        }

        public async Task SetMosOblEircPasswordAsync(string value)
        {
            await SecureStorage.SetAsync("MosOblEircPassword", value);
        }

        public async Task SetGlobusUserAsync(string value)
        {
            await SecureStorage.SetAsync("GlobusUser", value);
        }

        public async Task SetGlobusPasswordAsync(string value)
        {
            await SecureStorage.SetAsync("GlobusPassword", value);
        }

        public async Task<bool> IsSetAsync()
        {
            return
                !string.IsNullOrWhiteSpace(await GetMosOblEircUserAsync()) &&
                !string.IsNullOrWhiteSpace(await GetMosOblEircPasswordAsync()) &&
                !string.IsNullOrWhiteSpace(await GetGlobusUserAsync()) &&
                !string.IsNullOrWhiteSpace(await GetGlobusPasswordAsync());
        }

        public async Task SaveAsync(Settings model)
        {
            await SetMosOblEircUserAsync(model.MosOblEircUser);
            await SetMosOblEircPasswordAsync(model.MosOblEircPassword);
            await SetGlobusUserAsync(model.GlobusUser);
            await SetGlobusPasswordAsync(model.GlobusPassword);
        }
        #endregion
    }
}
