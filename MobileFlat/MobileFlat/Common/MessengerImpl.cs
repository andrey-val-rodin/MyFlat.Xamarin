using System;
using System.Threading.Tasks;
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

        public async Task ShowErrorAsync(string message)
        {
            await _page.DisplayAlert("Ошибка!", message, "OK");
        }

        public async Task ShowMessageAsync(string message)
        {
            await _page.DisplayAlert("", message, "OK");
        }
    }
}
