using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace MobileFlat.Common
{
    static public class Config
    {
        private static readonly SemaphoreSlim _mutex = new SemaphoreSlim(1);
        private static string _mosOblEircUser;
        private static string _mosOblEircPassword;
        private static string _globusUser;
        private static string _globusPassword;

        public static string MosOblEircUser
        {
            get
            {
                _mutex.Wait();
                var result = _mosOblEircUser;
                _mutex.Release();
                return result;
            }
        }

        public static string MosOblEircPassword
        {
            get
            {
                _mutex.Wait();
                var result = _mosOblEircPassword;
                _mutex.Release();
                return result;
            }
        }

        public static string GlobusUser
        {
            get
            {
                _mutex.Wait();
                var result = _globusUser;
                _mutex.Release();
                return result;
            }
        }

        public static string GlobusPassword
        {
            get
            {
                _mutex.Wait();
                var result = _globusPassword;
                _mutex.Release();
                return result;
            }
        }


        public static bool IsSet
        {
            get
            {
                _mutex.Wait();
                var result =
                    !string.IsNullOrWhiteSpace(_mosOblEircUser) &&
                    !string.IsNullOrWhiteSpace(_mosOblEircPassword) &&
                    !string.IsNullOrWhiteSpace(_globusUser) &&
                    !string.IsNullOrWhiteSpace(_globusPassword);
                _mutex.Release();
                return result;
            }
        }

        public static async Task SaveAsync(Models.Settings model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            await _mutex.WaitAsync();
            try
            {
                _mosOblEircUser = model.MosOblEircUser;
                _mosOblEircPassword = model.MosOblEircPassword;
                _globusUser = model.GlobusUser;
                _globusPassword = model.GlobusPassword;

                await SecureStorage.SetAsync("MosOblEircUser", _mosOblEircUser);
                await SecureStorage.SetAsync("MosOblEircPassword", _mosOblEircPassword);
                await SecureStorage.SetAsync("GlobusUser", _globusUser);
                await SecureStorage.SetAsync("GlobusPassword", _globusPassword);
            }
            finally
            {
                _mutex.Release();
            }
        }

        public static async Task LoadAsync()
        {
            await _mutex.WaitAsync();
            try
            {
                _mosOblEircUser = await SecureStorage.GetAsync("MosOblEircUser");
                _mosOblEircPassword = await SecureStorage.GetAsync("MosOblEircPassword");
                _globusUser = await SecureStorage.GetAsync("GlobusUser");
                _globusPassword = await SecureStorage.GetAsync("GlobusPassword");
            }
            finally
            {
                _mutex.Release();
            }
        }
    }
}
