using MobileFlat.Common;
using MobileFlat.Dto;
using MobileFlat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MobileFlat.Services
{
    public class WebService
    {
        private readonly IMessenger _messenger;
        private readonly MosOblEircService _mosOblEircService;
        private readonly GlobusService _globusService;
        private IList<MeterChildDto> _meters;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        // Kitchen cold water   323381, 17523577
        private MeterChildDto KitchenColdWater => GetMeter(17523577);
        // Kitchen hot water    206922, 16702145
        private MeterChildDto KitchenHotWater => GetMeter(16702145);
        // Bathroom hot water   204933, 16702144
        private MeterChildDto BathroomColdWater => GetMeter(17523578);
        // Bathroom hot water   204933, 16702144
        private MeterChildDto BathroomHotWater => GetMeter(16702144);
        // Electricity          19843385, 14680903
        private MeterChildDto Electricity => GetMeter(14680903);

        public Main Model { get; private set; }

        public DateTime Timestamp { get; private set; }

        public Status Status { get; private set; }

        public static bool UseMeters
        {
            get
            {
#if METERS
                return true;
#else
                return false;
#endif
            }
        }

        public bool NeedToLoad
        {
            get
            {
                if (!IsSuitableTimeToLoad)
                    return false;

                return Status switch
                {
                    Status.NotLoaded => true,
                    Status.Loading => false,
                    Status.Loaded => Timestamp.Day != DateTime.Now.Day,
                    _ => true
                };
            }
        }

        protected bool IsSuitableTimeToLoad
        {
            get
            {
                var now = DateTime.Now;
                return now.Hour >= 9 && now.Hour <= 18;
            }
        }

        public bool CanPassWaterMeters
        {
            get
            {
                if (!UseMeters)
                    return false;

                var now = DateTime.Now;
                if (now.Day >= 5 && now.Day <= 25)
                {
                    return
                        KitchenColdWater?.GetDate().Month != now.Month ||
                        KitchenHotWater?.GetDate().Month != now.Month ||
                        BathroomColdWater?.GetDate().Month != now.Month ||
                        BathroomHotWater?.GetDate().Month != now.Month;
                }

                return false;
            }
        }

        public bool CanPassElectricityMeter
        {
            get
            {
                if (!UseMeters)
                    return false;

                var now = DateTime.Now;
                if (now.Day >= 15 && now.Day <= 26)
                    return Electricity?.GetDate().Month != now.Month;

                return false;
            }
        }

        public WebService(IMessenger messenger)
        {
            _messenger = messenger;
            _mosOblEircService = new MosOblEircService(messenger);
            _globusService = new GlobusService(messenger);
        }

        private MeterChildDto GetMeter(int id)
        {
            return _meters?.FirstOrDefault(c => c.Id_counter == id);
        }

        public async Task<Status> LoadAsync(bool checkConditions)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (checkConditions)
                {
                    LoadState();
                    if (!NeedToLoad)
                        return Status.Skipped;
                }

                if (!await Config.IsSetAsync())
                {
                    Status = Status.NotLoaded;
                    return Status;
                }

                // МосОблЕИРЦ
                var user = await Config.GetMosOblEircUserAsync();
                var password = await Config.GetMosOblEircPasswordAsync();
                if (!await _mosOblEircService.AuthorizeAsync(user, password))
                {
                    await SetErrorAsync("Не удалось авторизоваться на сайте МосОблЕИРЦ");
                    return Status;
                }

                Tuple<string, decimal> tuple;
                try
                {
                    tuple = await _mosOblEircService.GetBalanceAsync();
                    if (tuple == null)
                        return Status;

                    if (UseMeters)
                    {
                        _meters = await _mosOblEircService.GetMetersAsync();
                        if (_meters == null)
                            return Status;

                        if (KitchenColdWater == null ||
                            KitchenHotWater == null ||
                            BathroomColdWater == null ||
                            BathroomHotWater == null ||
                            Electricity == null)
                        {
                            await SetErrorAsync("Нет показаний счётчика");
                            return Status;
                        }
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    await _mosOblEircService.LogoffAsync();
                }

                // Глобус
                user = await Config.GetGlobusUserAsync();
                password = await Config.GetGlobusPasswordAsync();
                if (!await _globusService.AuthorizeAsync(user, password))
                {
                    await SetErrorAsync("Не удалось авторизоваться на сайте Глобус");
                    return Status;
                }

                // GlobusService keeps current balance; we can logoff
                await _globusService.LogoffAsync();

                var model = new Main
                {
                    MosOblEircBalance = tuple.Item2,
                    GlobusBalance = _globusService.Balance,
                    Meters = new Meters()
                };

                // Globus site keeps invoice many days and even weeks
                // App will notify the user only one time
                if (model.GlobusBalance != 0 && model.GlobusBalance == Config.GetLastGlobusBalance(0))
                    model.GlobusBalance = 0;
                else
                    Config.SetLastGlobusBalance(model.GlobusBalance);

                if (UseMeters)
                {
                    model.MosOblEircBalance = tuple.Item2;
                    model.Meters.KitchenColdWaterMeter = KitchenColdWater.Vl_last_indication;
                    model.Meters.KitchenHotWaterMeter = KitchenHotWater.Vl_last_indication;
                    model.Meters.BathroomColdWaterMeter = BathroomColdWater.Vl_last_indication;
                    model.Meters.BathroomHotWaterMeter = BathroomHotWater.Vl_last_indication;
                    model.Meters.ElectricityMeter = Electricity.Vl_last_indication;
                }

                Model = model;
                Status = Status.Loaded;
                Timestamp = DateTime.Now;
                SaveState();
                return Status;
            }
            catch (Exception ex)
            {
                await SetErrorAsync(ex.Message);
                return Status;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task SetErrorAsync(string errorMessage)
        {
            Model = null;
            Status = Status.Error;
            if (_messenger != null)
                await _messenger.ShowErrorAsync(errorMessage);
        }

        public async Task<bool> SetMetersAsync(Meters meters)
        {
            if (meters == null)
                throw new ArgumentNullException(nameof(meters));
            if (!UseMeters)
                return false;

            if (meters.KitchenColdWaterMeter == 0 &&
                meters.KitchenHotWaterMeter == 0 &&
                meters.BathroomColdWaterMeter == 0 &&
                meters.BathroomHotWaterMeter == 0 &&
                meters.ElectricityMeter == 0)
                return false; // Nothing to send

            bool result = false;
            try
            {
                if (!await _mosOblEircService.AuthorizeAsync(
                        await Config.GetMosOblEircUserAsync(), await Config.GetMosOblEircPasswordAsync()) ||
                    !await _globusService.AuthorizeAsync(
                        await Config.GetGlobusUserAsync(), await Config.GetGlobusPasswordAsync()))
                    return false;

                if ((meters.KitchenColdWaterMeter == 0 ||
                        await _mosOblEircService.SendMeterAsync(
                            KitchenColdWater.Id_counter, meters.KitchenColdWaterMeter)) &&
                    (meters.KitchenHotWaterMeter == 0 ||
                        await _mosOblEircService.SendMeterAsync(
                            KitchenHotWater.Id_counter, meters.KitchenHotWaterMeter)) &&
                    (meters.BathroomColdWaterMeter == 0 ||
                        await _mosOblEircService.SendMeterAsync(
                            BathroomColdWater.Id_counter, meters.BathroomColdWaterMeter)) &&
                    (meters.BathroomHotWaterMeter == 0 ||
                        await _mosOblEircService.SendMeterAsync(
                            BathroomHotWater.Id_counter, meters.BathroomHotWaterMeter)) &&
                    (meters.ElectricityMeter == 0 ||
                        await _mosOblEircService.SendMeterAsync(
                            Electricity.Id_counter, meters.ElectricityMeter)) &&
                    (meters.KitchenHotWaterMeter == 0 || meters.BathroomHotWaterMeter == 0 ||
                        await _globusService.SendMetersAsync(
                            meters.KitchenHotWaterMeter, meters.BathroomHotWaterMeter)))

                    result = true;
            }
            finally
            {
                if (_mosOblEircService.IsAuthorized)
                    await _mosOblEircService.LogoffAsync();
                if (_globusService.IsAuthorized)
                    await _globusService.LogoffAsync();
            }

            return result;
        }

        public async Task<bool> CheckAccountsAsync(Settings settings)
        {
            try
            {
                if (!await _mosOblEircService.AuthorizeAsync(
                        settings.MosOblEircUser, settings.MosOblEircPassword) ||
                    !await _globusService.AuthorizeAsync(
                        settings.GlobusUser, settings.GlobusPassword))
                    return false;
            }
            finally
            {
                if (_mosOblEircService.IsAuthorized)
                    await _mosOblEircService.LogoffAsync();
                if (_globusService.IsAuthorized)
                    await _globusService.LogoffAsync();
            }

            return true;
        }

        private void LoadState()
        {
            Status = Config.GetStatus(Status.NotLoaded);
            Timestamp = Config.GetTimestamp(new DateTime(100, 1, 1));
            if (Config.GetModelIsSet(false))
            {
                Model = new Main
                {
                    MosOblEircBalance = Config.GetMosOblEircBalance(0),
                    GlobusBalance = Config.GetGlobusBalance(0),
                    Meters = new Meters()
                };

                Model.Meters.KitchenColdWaterMeter = Config.GetKitchenColdWaterMeter(0);
                Model.Meters.BathroomColdWaterMeter = Config.GetBathroomColdWaterMeter(0);
                Model.Meters.BathroomHotWaterMeter = Config.GetBathroomHotWaterMeter(0);
                Model.Meters.ElectricityMeter = Config.GetElectricityMeter(0);
            }
            else
            {
                Status = Status.NotLoaded;
                Model = null;
            }
        }

        private void SaveState()
        {
            Config.SetStatus(Status);
            Config.SetTimestamp(Timestamp);
            if (Model != null)
            {
                Config.SetMosOblEircBalance(Model.MosOblEircBalance);
                Config.SetGlobusBalance(Model.GlobusBalance);

                Config.SetKitchenColdWaterMeter(Model.Meters.KitchenColdWaterMeter);
                Config.SetKitchenHotWaterMeter(Model.Meters.KitchenHotWaterMeter);
                Config.SetBathroomColdWaterMeter(Model.Meters.BathroomColdWaterMeter);
                Config.SetBathroomHotWaterMeter(Model.Meters.BathroomHotWaterMeter);
                Config.SetElectricityMeter(Model.Meters.ElectricityMeter);

                Config.SetModelIsSet(true);
            }
            else
                Config.SetModelIsSet(false);
        }
    }
}
