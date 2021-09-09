using System;
using Xamarin.Forms;

namespace MobileFlat.Common
{
    public sealed class MessengerImpl : IMessenger
    {
        private readonly Page _page;

        public MessengerImpl(Page page)
        {
            _page = page ?? throw new ArgumentNullException(nameof(page));
        }

        public void ShowError(string message)
        {
            _page.DisplayAlert("Ошибка!", message, "OK");
        }

        public void ShowMessage(string message)
        {
            _page.DisplayAlert("", message, "OK");
        }
    }
}
