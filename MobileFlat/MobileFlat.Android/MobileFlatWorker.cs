using Android.Content;
using Android.Util;
using AndroidX.Concurrent.Futures;
using AndroidX.Work;
using Google.Common.Util.Concurrent;
using Java.Lang;
using MobileFlat.Common;
using MobileFlat.Services;
using System.Threading.Tasks;

namespace MobileFlat.Droid
{
    public class MobileFlatWorker : ListenableWorker, CallbackToFutureAdapter.IResolver
    {
        public const string TAG = "MobileFlatWorker";

        public MobileFlatWorker(Context context, WorkerParameters workerParams) :
            base(context, workerParams)
        {
        }

        public override IListenableFuture StartWork()
        {
            Log.Debug(TAG, "Started.");
            return CallbackToFutureAdapter.GetFuture(this);
        }

        public Object AttachCompleter(CallbackToFutureAdapter.Completer p0)
        {
            Log.Debug(TAG, $"Executing.");

            Task.Run(async () =>
            {
                await DoWorkAsync();

                Log.Debug(TAG, "Completed.");

                // Set a Success Result on the completer and return it
                return p0.Set(Result.InvokeSuccess());
            });

            return TAG;
        }

        private async Task DoWorkAsync()
        {
            var messenger = new Messenger();
            WebService service = new WebService(messenger);

            var status = await service.LoadAsync(true);
            if (status != Models.Status.Loaded)
            {
                if (status == Models.Status.Error)
                    SendNotification("Ошибка", messenger.ErrorMessage);
                return;
            }

            var model = service.Model;

            if (model.MosOblEircBalance != 0)
                SendNotification("Выставлен счёт МосОблЕИРЦ", $"{model.MosOblEircBalance} руб");
            if (model.GlobusBalance != 0)
                SendNotification("Выставлен счёт Глобус", $"{model.GlobusBalance} руб");

            if (WebService.UseMeters)
            {
                if (service.CanPassWaterMeters)
                    SendNotification("Пора вводить значения счётчиков воды", string.Empty);
                if (service.CanPassElectricityMeter)
                    SendNotification("Пора вводить значения электросчётчика", string.Empty);
            }
        }

        void SendNotification(string title, string message)
        {
            AndroidNotificationManager notificationManager = AndroidNotificationManager.Instance ?? new AndroidNotificationManager();
            notificationManager.SendNotification(title, message);
        }

        class Messenger : IMessenger
        {
            public string ErrorMessage { get; set; }

            public Task ShowErrorAsync(string message)
            {
                ErrorMessage += message;
                return Task.FromResult(0);
            }

            public Task ShowMessageAsync(string message)
            {
                return Task.FromResult(0);
            }
        }
    }
}