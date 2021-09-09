using System;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace MobileFlat.Common
{
    static public class Config
    {
        public static string MosOblEircUser { get; set; }
        public static string MosOblEircPassword { get; set; }
        public static string GlobusUser { get; set; }
        public static string GlobusPassword { get; set; }

        public static bool IsSet
        {
            get =>
                !string.IsNullOrWhiteSpace(MosOblEircUser) &&
                !string.IsNullOrWhiteSpace(MosOblEircPassword) &&
                !string.IsNullOrWhiteSpace(GlobusUser) &&
                !string.IsNullOrWhiteSpace(GlobusPassword);
        }

        public static async Task SaveAsync(Models.Settings model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            MosOblEircUser = model.MosOblEircUser;
            MosOblEircPassword = model.MosOblEircPassword;
            GlobusUser = model.GlobusUser;
            GlobusPassword = model.GlobusPassword;

            await SecureStorage.SetAsync("MosOblEircUser", MosOblEircUser);
            await SecureStorage.SetAsync("MosOblEircPassword", MosOblEircPassword);
            await SecureStorage.SetAsync("GlobusUser", GlobusUser);
            await SecureStorage.SetAsync("GlobusPassword", GlobusPassword);
        }

        public static async Task LoadAsync()
        {
            MosOblEircUser = await SecureStorage.GetAsync("MosOblEircUser");
            MosOblEircPassword = await SecureStorage.GetAsync("MosOblEircPassword");
            GlobusUser = await SecureStorage.GetAsync("GlobusUser");
            GlobusPassword = await SecureStorage.GetAsync("GlobusPassword");
        }
    }
}
