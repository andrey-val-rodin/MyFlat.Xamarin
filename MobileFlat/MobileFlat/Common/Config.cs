using MobileFlat.Dto;
using MobileFlat.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace MobileFlat.Common
{
    public static class Config
    {
        #region Preferences
        public static Status GetStatus(Status defaultValue)
        {
            return (Status)Preferences.Get("Status", (int)defaultValue);
        }

        public static void SetStatus(Status value)
        {
            Preferences.Set("Status", (int)value);
        }

        public static DateTime GetTimestamp(DateTime defaultValue)
        {
            return Preferences.Get("Timestamp", defaultValue);
        }

        public static void SetTimestamp(DateTime value)
        {
            Preferences.Set("Timestamp", value);
        }

        public static bool GetModelIsSet(bool defaultValue)
        {
            return Preferences.Get("ModelIsSet", defaultValue);
        }

        public static void SetModelIsSet(bool value)
        {
            Preferences.Set("ModelIsSet", value);
        }

        public static decimal GetMosOblEircBalance(decimal defaultValue)
        {
            return decimal.TryParse(Preferences.Get("MosOblEircBalance", defaultValue.ToString()), out decimal balance)
                ? balance
                : defaultValue;
        }

        public static void SetMosOblEircBalance(decimal value)
        {
            Preferences.Set("MosOblEircBalance", value.ToString());
        }

        public static decimal GetGlobusBalance(decimal defaultValue)
        {
            return decimal.TryParse(Preferences.Get("GlobusBalance", defaultValue.ToString()), out decimal balance)
                ? balance
                : defaultValue;
        }

        public static void SetGlobusBalance(decimal value)
        {
            Preferences.Set("GlobusBalance", value.ToString());
        }

        public static void SaveMeters(IList<MeterChildDto> meters)
        {
            if (meters == null)
                throw new ArgumentNullException(nameof(meters));

            Preferences.Set("MeterCount", meters.Count);
            for (int i = 0; i < meters.Count; i++)
            {
                var meter = meters[i];
                var prefix = $"Meter{i}";

                Preferences.Set($"{prefix}Id", meter.Id_counter);
                Preferences.Set($"{prefix}Name", meter.Nm_counter);
                Preferences.Set($"{prefix}Value", meter.Vl_last_indication);
                Preferences.Set($"{prefix}Date", meter.Dt_last_indication);
            }
        }

        public static IList<MeterChildDto> LoadMeters()
        {
            int count = Preferences.Get("MeterCount", 0);
            if (count == 0)
                return null;

            var result = new List<MeterChildDto>();
            for (int i = 0; i < count; i++)
            {
                var meter = new MeterChildDto();
                var prefix = $"Meter{i}";

                meter.Id_counter = Preferences.Get($"{prefix}Id", 0);
                meter.Nm_counter = Preferences.Get($"{prefix}Name", "");
                meter.Vl_last_indication = Preferences.Get($"{prefix}Value", 0);
                meter.Dt_last_indication = Preferences.Get($"{prefix}Date", "");
                result.Add(meter);
            }

            return result;
        }

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
