using MobileFlat.Common;
using MobileFlat.ViewModels;
using Xamarin.Forms;

namespace MobileFlat.Views
{
    public partial class SettingsPage : ContentPage
    {
        private readonly SettingsModel _viewModel;

        public SettingsPage()
        {
            InitializeComponent();
            _viewModel = new SettingsModel(new MessengerImpl(this));
            BindingContext = _viewModel;

            _viewModel.MosOblEircUser = Config.MosOblEircUser;
            _viewModel.MosOblEircPassword = Config.MosOblEircPassword;
            _viewModel.GlobusUser = Config.GlobusUser;
            _viewModel.GlobusPassword = Config.GlobusPassword;
        }
    }
}