using MobileFlat.Common;
using MobileFlat.Models;
using MobileFlat.Services;
using MobileFlat.Views;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MobileFlat.ViewModels
{
    public class SettingsModel : NotifyPropertyChangedImpl
    {
        private readonly IMessenger _messenger;
        private readonly WebService _webService;

        private string _mosOblEircUser;
        private string _mMosOblEircPassword;
        private string _globusUser;
        private string _globusPassword;

        public string MosOblEircUser
        {
            get => _mosOblEircUser;
            set => SetProperty(ref _mosOblEircUser, value);
        }

        public string MosOblEircPassword
        {
            get => _mMosOblEircPassword;
            set => SetProperty(ref _mMosOblEircPassword, value);
        }

        public string GlobusUser
        {
            get => _globusUser;
            set => SetProperty(ref _globusUser, value);
        }

        public string GlobusPassword
        {
            get => _globusPassword;
            set => SetProperty(ref _globusPassword, value);
        }

        public ICommand CheckAndSaveCommand { get; }

        public SettingsModel(IMessenger messenger)
        {
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _webService = new WebService(messenger);

            CheckAndSaveCommand = new Command(async () =>
                await CheckAndSaveAsync());
        }

        private async Task<bool> CheckAndSaveAsync()
        {
            var model = CreateModel();
            if (!model.IsSet)
            {
                await _messenger.ShowErrorAsync("Заполните все поля");
                return false;
            }

            if (!await _webService.CheckAccountsAsync(model))
                return false;

            await Config.SaveAsync(model);
            await _messenger.ShowMessageAsync("Учётные данные успешно сохранены");
            await Shell.Current.GoToAsync("//MainPage");
            var mainPage = Shell.Current.CurrentPage as MainPage;
            var mainModel = mainPage?.viewModel;
            if (mainModel == null)
                throw new InvalidOperationException("Missing MainPage or MainModel");

            // Trigger initialization
            mainModel.IsBusy = true;

            return true;
        }

        private Settings CreateModel()
        {
            return new Settings
            {
                MosOblEircUser = MosOblEircUser,
                MosOblEircPassword = MosOblEircPassword,
                GlobusUser = GlobusUser,
                GlobusPassword = GlobusPassword
            };
        }
    }
}