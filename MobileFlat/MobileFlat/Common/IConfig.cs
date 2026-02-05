using MobileFlat.Models;
using System.Threading.Tasks;

namespace MobileFlat.Common
{
    public interface IConfig
    {
        decimal GetLastGlobusBalance();
        void SetLastGlobusBalance(decimal value);
        int GetGlobusBalanceAccessCount();
        void SetGlobusBalanceAccessCount(int value);
        Task<string> GetMosOblEircUserAsync();
        Task<string> GetMosOblEircPasswordAsync();
        Task<string> GetGlobusUserAsync();
        Task<string> GetGlobusPasswordAsync();
        Task SetMosOblEircUserAsync(string value);
        Task SetMosOblEircPasswordAsync(string value);
        Task SetGlobusUserAsync(string value);
        Task SetGlobusPasswordAsync(string value);
        Task<bool> IsSetAsync();
        Task SaveAsync(Settings model);
    }
}