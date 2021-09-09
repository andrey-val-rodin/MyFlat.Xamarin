using MobileFlat.Common;
using MobileFlat.ViewModels;
using Xamarin.Forms;

namespace MobileFlat.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly MainModel _viewModel;

        public MainPage()
        {
            InitializeComponent();
            _viewModel = new MainModel(new MessengerImpl(this));
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await _viewModel.InitializeAsync();
        }
    }
}
