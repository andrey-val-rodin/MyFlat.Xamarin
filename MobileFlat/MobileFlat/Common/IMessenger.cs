using System.Threading.Tasks;

namespace MobileFlat.Common
{
    public interface IMessenger
    {
        Task ShowMessageAsync(string message);
        Task ShowErrorAsync(string message);
    }
}
