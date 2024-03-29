﻿using System;

namespace TestWorker.Http
{
    public class TransportHttpRequestEventArgs : EventArgs
    {
        public TransportHttpRequestEventArgs(string message)
        {
            this.Message = message;
        }

        public string Message { get; private set; }
    }
}