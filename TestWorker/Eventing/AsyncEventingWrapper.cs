using System;
using System.Threading.Tasks;

namespace TestWorker.Eventing
{
    #nullable enable

    /// <summary>
    /// Структура (обертка) для вызова асинхронных событий (скопирована из кода клиента C# для rabbitMQ).
    /// Оптимизирована (результат GetInvocationList() кэшируется в поле _handlers) и охвачена тестами (в исходном коде)
    /// Плюс, не дает вызвать асинхронное событие - не асинхронно. (т.е. Не сможете вызвать Invoke вместо InvokeAsync)
    /// </summary>
    /// <typeparam name="T">Тип аргумента события</typeparam>
    public struct AsyncEventingWrapper<T>
    {
        private event AsyncWrapperEventHandler<T>? _event;
        private Delegate[]? _handlers;

        public bool IsEmpty => _event is null;

        public void AddHandler(AsyncWrapperEventHandler<T>? handler)
        {
            _event += handler;
            _handlers = null;
        }

        public void RemoveHandler(AsyncWrapperEventHandler<T>? handler)
        {
            _event -= handler;
            _handlers = null;
        }

        // Не делайте эту функцию async! (Этот тип структура, и копируется в начале асинхронного метода => при этом копируются неинициализированные _handlers)
        public Task InvokeAsync(object sender, T parameter)
        {
            var handlers = _handlers;
            if (handlers is null)
            {
                handlers = _event?.GetInvocationList();
                if (handlers is null)
                {
                    return Task.CompletedTask;
                }

                _handlers = handlers;
            }

            if (handlers.Length == 1)
            {
                return ((AsyncWrapperEventHandler<T>)handlers[0])(sender, parameter);
            }
            return InternalInvoke(handlers, sender, parameter);
        }

        private static async Task InternalInvoke(Delegate[] handlers, object sender, T parameter)
        {
            foreach (AsyncWrapperEventHandler<T> action in handlers)
            {
                await action(sender, parameter).ConfigureAwait(false);
            }
        }

        public void Takeover(in AsyncEventingWrapper<T> other)
        {
            _event = other._event;
            _handlers = other._handlers;
        }
    }
}
