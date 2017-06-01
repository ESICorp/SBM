using System;

namespace SBM.Schedule
{
    /// <summary>
    /// Represents the method that will handle an <see cref="Exception"/> object.
    /// </summary>
    public delegate void ExceptionHandler(Exception ex);

    /// <summary>
    /// Represents the method that will generate an <see cref="Exception"/> object.
    /// </summary>
    public delegate Exception ExceptionProvider();

    /// <summary>
    /// Defines error handling strategies.
    /// </summary>

    internal static class ErrorHandling
    {
        /// <summary>
        /// A stock <see cref="ExceptionHandler"/> that throws.
        /// </summary>
        public static readonly ExceptionHandler Throw = e => { throw e; };

        internal static ExceptionProvider OnError(ExceptionProvider provider, ExceptionHandler handler)
        {
            if (handler != null)
            {
                handler(provider());
            }

            return provider;
        }
    }
}
