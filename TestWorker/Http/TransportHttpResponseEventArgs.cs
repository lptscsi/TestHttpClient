using System;

namespace TestWorker.Http
{
    public class TransportHttpResponseEventArgs : EventArgs
    {
        public TransportHttpResponseEventArgs(string message)
        {
            this.Message = message;
        }

        public string Message { get; private set; }
    }
}