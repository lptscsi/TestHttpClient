using System;

namespace TestWorker.Extensions
{
    public static class ExceptionExt
	{
        /// <summary>
        /// Получение полной строки об ошибке (включая InnerException)
        /// </summary>
        /// <param name="exception">Ошибка</param>
        /// <returns>Сообщение об ошибке</returns>
		public static string GetFullMessage(this Exception exception)
		{
			string result = $"{exception.GetType().Name}: {exception.Message}";
			
            while (exception.InnerException != null)
			{
                result = $"{exception.InnerException.GetType().Name}: {exception.InnerException.Message}{Environment.NewLine}{result}";
				exception = exception.InnerException;
			}

#if DEBUG
            result = $"{result}{Environment.NewLine}{(exception.InnerException != null ? exception.InnerException.StackTrace : exception.StackTrace)}";
#endif

  		    return result;
		}
	}
}
