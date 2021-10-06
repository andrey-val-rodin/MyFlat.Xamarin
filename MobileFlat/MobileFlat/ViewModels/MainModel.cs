using MobileFlat.Common;
using MobileFlat.Models;
using MobileFlat.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MobileFlat.ViewModels
{
    public class MainModel : NotifyPropertyChangedImpl
    {
        private readonly IMessenger _messenger;
        private readonly WebService _webService;
        private Main _model;
        bool _isBusy = false;
        bool _isEnabled = true;
        private string _mosOblEircText;
        private string _globusText;
        private string _kitchenColdWaterMeter;
        private string _kitchenHotWaterMeter;
        private string _bathroomColdWaterMeter;
        private string _bathroomHotWaterMeter;
        private string _electricityMeter;
        private string _kitchenColdWaterOldMeter;
        private string _kitchenHotWaterOldMeter;
        private string _bathroomColdWaterOldMeter;
        private string _bathroomHotWaterOldMeter;
        private string _electricityOldMeter;
        private bool _canPassWaterMeters;
        private bool _canPassElectricityMeter;

        public Command OpenMosOblEircCommand { get; }
        public Command OpenGlobusCommand { get; }
        public Command PassMetersCommand { get; }
        public Command RefreshCommand { get; }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        public string MosOblEircText
        {
            get => _mosOblEircText;
            set => SetProperty(ref _mosOblEircText, value);
        }

        public string GlobusText
        {
            get => _globusText;
            set => SetProperty(ref _globusText, value);
        }

        public string KitchenColdWaterMeter
        {
            get => _kitchenColdWaterMeter;
            set => SetProperty(ref _kitchenColdWaterMeter, value);
        }

        public string KitchenHotWaterMeter
        {
            get => _kitchenHotWaterMeter;
            set => SetProperty(ref _kitchenHotWaterMeter, value);
        }

        public string BathroomColdWaterMeter
        {
            get => _bathroomColdWaterMeter;
            set => SetProperty(ref _bathroomColdWaterMeter, value);
        }

        public string BathroomHotWaterMeter
        {
            get => _bathroomHotWaterMeter;
            set => SetProperty(ref _bathroomHotWaterMeter, value);
        }

        public string ElectricityMeter
        {
            get => _electricityMeter;
            set => SetProperty(ref _electricityMeter, value);
        }

        public string KitchenColdWaterOldMeter
        {
            get => _kitchenColdWaterOldMeter;
            set => SetProperty(ref _kitchenColdWaterOldMeter, value);
        }

        public string KitchenHotWaterOldMeter
        {
            get => _kitchenHotWaterOldMeter;
            set => SetProperty(ref _kitchenHotWaterOldMeter, value);
        }

        public string BathroomColdWaterOldMeter
        {
            get => _bathroomColdWaterOldMeter;
            set => SetProperty(ref _bathroomColdWaterOldMeter, value);
        }

        public string BathroomHotWaterOldMeter
        {
            get => _bathroomHotWaterOldMeter;
            set => SetProperty(ref _bathroomHotWaterOldMeter, value);
        }

        public string ElectricityOldMeter
        {
            get => _electricityOldMeter;
            set => SetProperty(ref _electricityOldMeter, value);
        }

        public bool CanPassWaterMeters
        {
            get =>_canPassWaterMeters;
            set => SetProperty(ref _canPassWaterMeters, value);
        }

        public bool CanPassElectricityMeter
        {
            get => _canPassElectricityMeter;
            set => SetProperty(ref _canPassElectricityMeter, value);
        }

        public bool CanPassMeters => CanPassWaterMeters || CanPassElectricityMeter;

        public MainModel(IMessenger messenger)
        {
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _webService = new WebService(messenger);

            OpenMosOblEircCommand = new Command(async () =>
                await Browser.OpenAsync("https://my.mosenergosbyt.ru/auth"));
            OpenGlobusCommand = new Command(async () =>
                await Browser.OpenAsync("https://lk.globusenergo.ru"));
            PassMetersCommand = new Command(async () =>
                await PassMetersAsync(), () => CanPassMeters);
            RefreshCommand = new Command(async () =>
                await InitializeAsync());
        }

        public async Task<bool> InitializeAsync()
        {
            _isBusy = true;
            try
            {
                MosOblEircText = "Загрузка...";
                GlobusText = "Загрузка...";

                if (!Config.IsSet)
                {
                    MosOblEircText = "Нет учётных данных";
                    GlobusText = "Нет учётных данных";
                    return false;
                }

                _model = await _webService.GetModelAsync();
                if (_model == null)
                {
                    MosOblEircText = "Ошибка";
                    GlobusText = "Ошибка";
                    return false;
                }

                MosOblEircText = _model.MosOblEircBalance == 0 ?
                    "Оплачено" : $"Выставлен счёт на {_model.MosOblEircBalance} руб";
                GlobusText = _model.GlobusBalance == 0 ?
                    "Оплачено" : $"Выставлен счёт на {_model.GlobusBalance} руб";

                KitchenColdWaterOldMeter = _model.Meters.KitchenColdWaterMeter.ToString();
                KitchenHotWaterOldMeter = _model.Meters.KitchenHotWaterMeter.ToString();
                BathroomColdWaterOldMeter = _model.Meters.BathroomColdWaterMeter.ToString();
                BathroomHotWaterOldMeter = _model.Meters.BathroomHotWaterMeter.ToString();
                ElectricityOldMeter = _model.Meters.ElectricityMeter.ToString();

                var oldCanPassMeters = CanPassMeters;
                CanPassWaterMeters = _webService.CanPassWaterMeters;
                CanPassElectricityMeter = _webService.CanPassElectricityMeter;
                // Enable button "Передать показания" if value changed
                if (oldCanPassMeters != CanPassMeters)
                    PassMetersCommand.ChangeCanExecute();

                return true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<bool> PassMetersAsync()
        {
            if (!CanPassMeters)
            {
                await _messenger.ShowErrorAsync("Ещё не время передавать показания");
                return false;
            }

            IsEnabled = false;
            try
            {
                var meters = CreateMeters();
                var error = ValidateMeters(meters);
                if (!string.IsNullOrEmpty(error))
                {
                    await _messenger.ShowErrorAsync(error);
                    return false;
                }

                if (await _webService.SetMetersAsync(meters))
                {
                    await _messenger.ShowMessageAsync("Показания успешно переданы");
                    // Reset
                    KitchenColdWaterMeter = null;
                    KitchenHotWaterMeter = null;
                    BathroomColdWaterMeter = null;
                    BathroomHotWaterMeter = null;
                    ElectricityMeter = null;
                    await InitializeAsync();
                }

                return false;
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private Meters CreateMeters()
        {
            var result = new Meters();
            if (CanPassWaterMeters)
            {
                result.KitchenColdWaterMeter = ToInt(KitchenColdWaterMeter);
                result.KitchenHotWaterMeter = ToInt(KitchenHotWaterMeter);
                result.BathroomColdWaterMeter = ToInt(BathroomColdWaterMeter);
                result.BathroomHotWaterMeter = ToInt(BathroomHotWaterMeter);
            }

            if (CanPassElectricityMeter)
                result.ElectricityMeter = ToInt(ElectricityMeter);

            return result;
        }

        private int ToInt(string value)
        {
            return int.TryParse(value, out int result) ? result : 0;
        }

        private string ValidateMeters(Meters meters)
        {
            if (IsMetersEmpty(meters))
                return "Показания не заданы";

            if (CanPassWaterMeters)
            {
                if (meters.KitchenColdWaterMeter < _model.Meters.KitchenColdWaterMeter)
                    return $"Кухня хол. вода: значение должно быть не меньше {_model.Meters.KitchenColdWaterMeter}";
                if (meters.KitchenHotWaterMeter < _model.Meters.KitchenHotWaterMeter)
                    return $"Кухня гор. вода: значение должно быть не меньше {_model.Meters.KitchenHotWaterMeter}";
                if (meters.BathroomColdWaterMeter < _model.Meters.BathroomColdWaterMeter)
                    return $"Санузел хол. вода: значение должно быть не меньше {_model.Meters.BathroomColdWaterMeter}";
                if (meters.BathroomHotWaterMeter < _model.Meters.BathroomHotWaterMeter)
                    return $"Санузел гор. вода: значение должно быть не меньше {_model.Meters.BathroomHotWaterMeter}";
            }

            if (CanPassElectricityMeter && meters.ElectricityMeter < _model.Meters.ElectricityMeter)
                return $"Электричество: значение должно быть не меньше {_model.Meters.ElectricityMeter}";

            return null;
        }

        private bool IsMetersEmpty(Meters meters)
        {
            return meters == null || (
                meters.KitchenColdWaterMeter == 0 &&
                meters.KitchenHotWaterMeter == 0 &&
                meters.BathroomColdWaterMeter == 0 &&
                meters.BathroomHotWaterMeter == 0 &&
                meters.ElectricityMeter == 0);
        }
    }
}
