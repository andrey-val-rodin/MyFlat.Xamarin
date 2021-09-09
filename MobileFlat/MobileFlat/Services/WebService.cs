using MobileFlat.Common;
using MobileFlat.Dto;
using MobileFlat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MobileFlat.Services
{
    public class WebService
    {
        private readonly IMessenger _messenger;
        private MosOblEircService _mosOblEircService;
        private GlobusService _globusService;
        private IList<MeterChildDto> _meters;

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

        public bool CanPassWaterMeters
        {
            get
            {
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
                var now = DateTime.Now;
                if (now.Day >= 15 && now.Day <= 26)
                    return Electricity?.GetDate().Month != now.Month;

                return false;
            }
        }

        public WebService(IMessenger messenger)
        {
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _mosOblEircService = new MosOblEircService(_messenger);
            _globusService = new GlobusService(_messenger);
        }

        private MeterChildDto GetMeter(int id)
        {
            return _meters?.FirstOrDefault(c => c.Id_counter == id);
        }

        public async Task<Main> GetModelAsync()
        {
            if (!await _mosOblEircService.AuthorizeAsync(
                Config.MosOblEircUser, Config.MosOblEircPassword))
                return null;

            Tuple<string, decimal> tuple;
            try
            {
                tuple = await _mosOblEircService.GetBalanceAsync();
                if (tuple == null)
                    return null;

                _meters = await _mosOblEircService.GetMetersAsync();
                if (_meters == null)
                    return null;

                if (KitchenColdWater == null ||
                    KitchenHotWater == null ||
                    BathroomColdWater == null ||
                    BathroomHotWater == null ||
                    Electricity == null)
                {
                    _messenger.ShowError("Нет показаний счётчика");
                    return null;
                }
            }
            finally
            {
                await _mosOblEircService.LogoffAsync();
            }

            if (!await _globusService.AuthorizeAsync(
                Config.GlobusUser, Config.GlobusPassword))
                return null;

            // GlobusService keeps current balance; we can logoff
            await _globusService.LogoffAsync();

            var result = new Main
            {
                MosOblEircBalance = tuple.Item2,
                GlobusBalance = _globusService.Balance,
                Meters = new Meters()
            };
            result.MosOblEircBalance = tuple.Item2;
            result.Meters.KitchenColdWaterMeter = KitchenColdWater.Vl_last_indication;
            result.Meters.KitchenHotWaterMeter = KitchenHotWater.Vl_last_indication;
            result.Meters.BathroomColdWaterMeter = BathroomColdWater.Vl_last_indication;
            result.Meters.BathroomHotWaterMeter = BathroomHotWater.Vl_last_indication;
            result.Meters.ElectricityMeter = Electricity.Vl_last_indication;

            return result;
        }

        public async Task<bool> SetMetersAsync(Meters meters)
        {
            if (meters == null)
                throw new ArgumentNullException(nameof(meters));

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
                        Config.MosOblEircUser, Config.MosOblEircPassword) ||
                    !await _globusService.AuthorizeAsync(
                        Config.GlobusUser, Config.GlobusPassword))
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
    }
}
