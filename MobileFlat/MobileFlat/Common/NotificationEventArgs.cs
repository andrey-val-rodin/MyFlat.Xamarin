using System;

namespace MobileFlat.Common
{
    public class NotificationEventArgs : EventArgs
    {
        public NotificationEventArgs(string title, string message)
        {
            Title = title;
            Message = message;
        }

        public string Title { get; set; }
        public string Message { get; set; }
    }
}
