using System;

namespace TestWorker.Http
{
    public class TransportHttpFailEventArgs : EventArgs
    {
        public TransportHttpFailEventArgs(string error)
        {
            this.Error = error;
        }

        public String Error { get; }
    }
}