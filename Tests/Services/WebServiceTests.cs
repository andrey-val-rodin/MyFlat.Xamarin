using MobileFlat.Models;
using MobileFlat.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Services
{
    public class WebServiceTests
    {
        [Fact]
        public void IsSuitableTimeToLoad_9AM_False()
        {
            Assert.False(WebService.IsSuitableTimeToLoad(new DateTime(2024, 1, 1, 9, 0, 0)));
        }

        [Fact]
        public void IsSuitableTimeToLoad_11PM_False()
        {
            Assert.False(WebService.IsSuitableTimeToLoad(new DateTime(2024, 1, 1, 23, 0, 0)));
        }

        [Fact]
        public void IsSuitableTimeToLoad_12PM_True()
        {
            Assert.True(WebService.IsSuitableTimeToLoad(new DateTime(2024, 1, 1, 12, 0, 0)));
        }

        [Fact]
        public void NeedToLoad_CurrentTime_TrueOrFalse()
        {
            var service = new WebService(null);
            if (WebService.IsSuitableTimeToLoad(DateTime.Now))
                Assert.True(service.NeedToLoad);
            else
                Assert.False(service.NeedToLoad);
        }

        [Fact]
        public async Task GetTomorrowTimeSpan_CurrentTime_ValidResult()
        {
            await WaitSecondIfTimeIsNear(9, 59, 59);
            await WaitSecondIfTimeIsNear(19, 59, 59);
            await WaitSecondIfTimeIsNear(23, 59, 59);
            var today = DateTime.Now;
            if (WebService.IsSuitableTimeToLoad(today))
            {
                var actual = today + WebService.GetTomorrowTimeSpan();
                var expected = GetTomorrowStartTime(today);
                Assert.Equal(expected, actual, TimeSpan.FromMilliseconds(1));
            }
            else
            {
                if (today.Hour >= 20)
                {
                    var actual = today + WebService.GetTomorrowTimeSpan();
                    var expected = GetTomorrowStartTime(today);
                    Assert.Equal(expected, actual, TimeSpan.FromMilliseconds(1));
                }
                else
                {
                    var actual = today + WebService.GetTomorrowTimeSpan();
                    var expected = GetTodayStartTime(today);
                    Assert.Equal(expected, actual, TimeSpan.FromMilliseconds(1));
                }
            }
        }

        [Fact]
        public async Task GetOneHourTimeSpan_CurrentTime_ValidResult()
        {
            await WaitSecondIfTimeIsNear(9, 59, 59);
            await WaitSecondIfTimeIsNear(21, 59, 59);
            await WaitSecondIfTimeIsNear(23, 59, 59);
            var today = DateTime.Now;
            var actual = today + WebService.GetOneHourTimeSpan();
            var expected = today + TimeSpan.FromHours(1);
            if (!WebService.IsSuitableTimeToLoad(expected))
                expected = GetTomorrowStartTime(today);

            Assert.Equal(expected, actual, TimeSpan.FromMilliseconds(1));
        }

        [Fact]
        public async Task LoadAsync_SuccessIfCurrentTimeIsSuitable()
        {
            await WaitSecondIfTimeIsNear(9, 59, 59);
            await WaitSecondIfTimeIsNear(21, 59, 59);

            if (!WebService.IsSuitableTimeToLoad(DateTime.Now))
                return; // skip test

            var service = new WebService(null) { Config = new ConfigStub() };
            var status = await service.LoadAsync(false);

            Assert.Equal(Status.Loaded, status);
            Assert.Equal(Status.Loaded, service.Status);
            if (WebService.UseMeters)
            {
                Assert.NotNull(service.KitchenColdWater);
                Assert.NotNull(service.KitchenHotWater);
                Assert.NotNull(service.BathroomColdWater);
                Assert.NotNull(service.BathroomHotWater);
                Assert.NotNull(service.Electricity);
            }
            Assert.NotNull(service.Model);
            Assert.Equal(DateTime.Now, service.Timestamp, TimeSpan.FromMilliseconds(10));
        }

        static async Task WaitSecondIfTimeIsNear(int hour, int minute, int second)
        {
            var now = DateTime.Now;
            while (now.Hour == hour && now.Minute == minute && now.Second == second)
            {
                await Task.Delay(100);
                now = DateTime.Now;
            }
        }

        static DateTime GetTodayStartTime(DateTime today)
        {
            return new DateTime(today.Year, today.Month, today.Day, 10, 0, 0);
        }

        static DateTime GetTomorrowStartTime(DateTime today)
        {
            var nextDay = today.AddDays(1);
            return new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, 10, 0, 0);
        }
    }
}
