using System;

namespace SBM.Schedule
{

    /// <summary>
    /// A generic type that either represents a value or an error condition.
    /// </summary>

    [Serializable]
    public struct ValueOrError<T>
    {
        private readonly bool _hasValue;
        private readonly T _value;
        private readonly ExceptionProvider _ep;

        private static readonly ExceptionProvider _dep = () => new ParseException("Value is undefined.");

        /// <summary>
        /// Initializes the object with a defined value.
        /// </summary>

        public ValueOrError(T value)
            : this()
        {
            _hasValue = true;
            _value = value;
        }

        /// <summary>
        /// Initializes the object with an error.
        /// </summary>

        public ValueOrError(Exception error) : this(CheckError(error)) { }

        private static ExceptionProvider CheckError(Exception error)
        {
            if (error == null) throw new ArgumentNullException("error");
            return () => error;
        }

        /// <summary>
        /// Initializes the object with a handler that will provide
        /// the error result when needed.
        /// </summary>

        public ValueOrError(ExceptionProvider provider)
            : this()
        {
            if (provider == null) throw new ArgumentNullException("provider");
            _ep = provider;
        }

        /// <summary>
        /// Determines if object holds a defined value or not.
        /// </summary>

        public bool HasValue { get { return _hasValue; } }

        /// <summary>
        /// Gets the value otherwise throws an error if undefined.
        /// </summary>

        public T Value { get { if (!HasValue) throw ErrorProvider(); return _value; } }

        /// <summary>
        /// Determines if object identifies an error condition or not.
        /// </summary>

        public bool IsError { get { return ErrorProvider != null; } }

        /// <summary>
        /// Gets the <see cref="Exception"/> object if this object
        /// represents an error condition otherwise it returns <c>null</c>.
        /// </summary>

        public Exception Error { get { return IsError ? ErrorProvider() : null; } }

        /// <summary>
        /// Gets the <see cref="ExceptionProvider"/> object if this 
        /// object represents an error condition otherwise it returns <c>null</c>.
        /// </summary>

        public ExceptionProvider ErrorProvider { get { return HasValue ? null : _ep ?? _dep; } }

        /// <summary>
        /// Attempts to get the defined value or another in case
        /// of an error.
        /// </summary>

        public T TryGetValue(T errorValue)
        {
            return IsError ? errorValue : Value;
        }

        /// <summary>
        /// Implicitly converts a <typeparamref name="T"/> value to
        /// an object of this type.
        /// </summary>

        public static implicit operator ValueOrError<T>(T value) { return new ValueOrError<T>(value); }

        /// <summary>
        /// Implicitly converts an <see cref="Exception"/> object to
        /// an object of this type that represents the error condition.
        /// </summary>

        public static implicit operator ValueOrError<T>(Exception error) { return new ValueOrError<T>(error); }

        /// <summary>
        /// Implicitly converts an <see cref="ExceptionProvider"/> object to
        /// an object of this type that represents the error condition.
        /// </summary>

        public static implicit operator ValueOrError<T>(ExceptionProvider provider) { return new ValueOrError<T>(provider); }

        /// <summary>
        /// Explicits converts this object to a <typeparamref name="T"/> value.
        /// </summary>

        public static explicit operator T(ValueOrError<T> valueOrError) { return valueOrError.Value; }

        /// <summary>
        /// Explicits converts this object to an <see cref="Exception"/> object
        /// if it represents an error condition. The conversion yields <c>null</c>
        /// if this object does not represent an error condition.
        /// </summary>

        public static explicit operator Exception(ValueOrError<T> valueOrError) { return valueOrError.Error; }

        /// <summary>
        /// Explicits converts this object to an <see cref="ExceptionProvider"/> object
        /// if it represents an error condition. The conversion yields <c>null</c>
        /// if this object does not represent an error condition.
        /// </summary>

        public static explicit operator ExceptionProvider(ValueOrError<T> valueOrError) { return valueOrError.ErrorProvider; }

        public override string ToString()
        {
            var error = Error;
            return IsError
                 ? error.GetType().FullName + ": " + error.Message
                 : _value != null
                 ? _value.ToString() : string.Empty;
        }

        public override bool Equals(object obj)
        {
            return this == (ValueOrError<T>)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(ValueOrError<T> even, ValueOrError<T> odd)
        {
            return even.IsError && odd.IsError ? even.Error.Equals(odd.Error) :
                even.HasValue && odd.HasValue ? even.Value.Equals(odd.Value) : false;
        }

        public static bool operator !=(ValueOrError<T> even, ValueOrError<T> odd)
        {
            return !(even == odd);
        }
    }
}
