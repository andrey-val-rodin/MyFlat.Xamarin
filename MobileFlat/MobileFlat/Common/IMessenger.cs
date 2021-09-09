namespace MobileFlat.Common
{
    public interface IMessenger
    {
        void ShowMessage(string message);
        void ShowError(string message);
    }
}
