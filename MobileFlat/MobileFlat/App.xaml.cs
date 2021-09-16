using MobileFlat.Common;
using MobileFlat.Views;
using System;
using Xamarin.Forms;

namespace MobileFlat
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            var mainPage = (MainPage)Shell.Current.CurrentPage;
            var mainModel = mainPage?.viewModel;
            if (mainModel == null)
                throw new InvalidOperationException("Missing MainPage or MainModel");

            await Config.LoadAsync();
            if (!Config.IsSet)
            {
                mainModel.MosOblEircText = "Нет учётных данных";
                mainModel.GlobusText = "Нет учётных данных";
                await Shell.Current.GoToAsync("//SettingsPage");
                return;
            }

            await mainModel.InitializeAsync();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
