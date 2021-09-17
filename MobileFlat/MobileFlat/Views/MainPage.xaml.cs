using MobileFlat.Common;
using MobileFlat.ViewModels;
using Xamarin.Forms;

namespace MobileFlat.Views
{
    public partial class MainPage : ContentPage
    {
        public readonly MainModel viewModel;

        public MainPage()
        {
            InitializeComponent();
            viewModel = new MainModel(new MessengerImpl(Shell.Current));
            BindingContext = viewModel;
        }

        public void OnEntryFocused(object sender, FocusEventArgs e)
        {
            viewModel.IsEnabled = false;
        }
    }
}
