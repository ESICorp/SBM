using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SBM.Schedule
{
    public delegate ExceptionProvider Accumulator(int start, int end, int interval, ExceptionHandler onError);

    internal sealed class Parser 
    {
        private static Parser Minute = new Parser(Part.Minute, 0, 59, null);
        private static Parser Hour = new Parser(Part.Hour, 0, 23, null);
        private static Parser Day = new Parser(Part.Day, 1, 31, null);
        private static Parser Month = new Parser(Part.Month, 1, 12, new[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" });
        private static Parser DayOfWeek = new Parser(Part.DayOfWeek, 0, 6, new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" });

        private static readonly Parser[] _fieldByKind = new[] { Minute, Hour, Day, Month, DayOfWeek };

        private static readonly CompareInfo _comparer = CultureInfo.InvariantCulture.CompareInfo;
        private static readonly char[] _comma = new[] { ',' };

        private readonly Part _kind;
        private readonly int _minValue;
        private readonly int _maxValue;
        private readonly string[] _names;

        public static Parser FromKind(Part kind)
        {
            if (!Enum.IsDefined(typeof(Part), kind))
            {
                throw new ArgumentException(
                    "Invalid crontab field kind. Valid values are " + string.Join(", ", Enum.GetNames(typeof(Part))), "kind");
            }

            return _fieldByKind[(int)kind];
        }

        private Parser(Part kind, int minValue, int maxValue, string[] names)
        {
            _kind = kind;
            _minValue = minValue;
            _maxValue = maxValue;
            _names = names;
        }

        public Part Kind
        {
            get { return _kind; }
        }

        public int MinValue
        {
            get { return _minValue; }
        }

        public int MaxValue
        {
            get { return _maxValue; }
        }

        public int ValueCount
        {
            get { return _maxValue - _minValue + 1; }
        }

        //public void Format(IField field, TextWriter writer)
        //{
        //    Format(field, writer, false);
        //}

        public void Format(IField field, TextWriter writer, bool noNames)
        {
            if (field == null)
                throw new ArgumentNullException("field");

            if (writer == null)
                throw new ArgumentNullException("writer");

            var next = field.First;
            var count = 0;

            while (next != -1)
            {
                var first = next;
                int last;

                do
                {
                    last = next;
                    next = field.GetNext(last + 1);
                }
                while (next - last == 1);

                if (count == 0
                    && first == _minValue && last == _maxValue)
                {
                    writer.Write('*');
                    return;
                }

                if (count > 0)
                    writer.Write(',');

                if (first == last)
                {
                    FormatValue(first, writer, noNames);
                }
                else
                {
                    FormatValue(first, writer, noNames);
                    writer.Write('-');
                    FormatValue(last, writer, noNames);
                }

                count++;
            }
        }

        private void FormatValue(int value, TextWriter writer, bool noNames)
        {
            if (noNames || _names == null)
            {
                if (value >= 0 && value < 100)
                {
                    FastFormatNumericValue(value, writer);
                }
                else
                {
                    writer.Write(value.ToString(CultureInfo.InvariantCulture));
                }
            }
            else
            {
                var index = value - _minValue;
                writer.Write(_names[index]);
            }
        }

        private static void FastFormatNumericValue(int value, TextWriter writer)
        {
            if (value >= 10)
            {
                writer.Write((char)('0' + (value / 10)));
                writer.Write((char)('0' + (value % 10)));
            }
            else
            {
                writer.Write((char)('0' + value));
            }
        }

        //public void Parse(string @value, Accumulator accumulator)
        //{
        //    TryParse(@value, accumulator, ErrorHandling.Throw);
        //}

        public ExceptionProvider TryParse(string @value, Accumulator accumulator, ExceptionHandler onError)
        {
            if (accumulator == null)
                throw new ArgumentNullException("accumulator", "Accumulator cannot be a null reference");

            if (string.IsNullOrEmpty(@value)) 
                return null;

            try
            {
                return InternalParse(@value, accumulator, onError);
            }
            catch (FormatException e)
            {
                return OnParseException(e, @value, onError);
            }
            catch (ParseException e)
            {
                return OnParseException(e, @value, onError);
            }
        }

        private ExceptionProvider OnParseException(Exception innerException, string @value, ExceptionHandler onError)
        {
            return ErrorHandling.OnError(
                       () => new ParseException(innerException, "{0} is not a valid {1} expression", @value, Kind), onError);
        }

        private ExceptionProvider InternalParse(string @value, Accumulator accumulator, ExceptionHandler onError)
        {
            if (@value.Length == 0)
            {
                return ErrorHandling.OnError(() => new ParseException("Value cannot be empty."), onError);
            }

            //
            // Next, look for a list of values (e.g. 1,2,3).
            //

            var commaIndex = @value.IndexOf(',');

            if (commaIndex > 0)
            {
                ExceptionProvider e = null;
                var token = ((IEnumerable<string>) @value.Split(_comma)).GetEnumerator();
                while (token.MoveNext() && e == null)
                    e = InternalParse(token.Current, accumulator, onError);
                return e;
            }

            var every = 1;

            //
            // Look for stepping first (e.g. */2 = every 2nd).
            // 

            var slashIndex = @value.IndexOf('/');

            if (slashIndex > 0)
            {
                every = int.Parse(@value.Substring(slashIndex + 1), CultureInfo.InvariantCulture);
                @value = @value.Substring(0, slashIndex);
            }

            //
            // Next, look for wildcard (*).
            //

            if (@value.Length == 1 && @value[0] == '*')
            {
                return accumulator(-1, -1, every, onError);
            }

            //
            // Next, look for a range of values (e.g. 2-10).
            //

            var dashIndex = @value.IndexOf('-');

            if (dashIndex > 0)
            {
                var first = ParseValue(@value.Substring(0, dashIndex));
                var last = ParseValue(@value.Substring(dashIndex + 1));

                return accumulator(first, last, every, onError);
            }

            //
            // Finally, handle the case where there is only one number.
            //

            var parsed = ParseValue(@value);

            if (every == 1)
                return accumulator(parsed, parsed, 1, onError);

            return accumulator(parsed, _maxValue, every, onError);
        }

        private int ParseValue(string @value)
        {
            if (@value.Length == 0)
                throw new ParseException("Value cannot be empty");

            var firstChar = @value[0];

            if (firstChar >= '0' && firstChar <= '9')
                return int.Parse(@value, CultureInfo.InvariantCulture);

            if (_names == null)
            {
                throw new ParseException("{0} is not a valid {3}. It must be a numeric value between {1} and {2} (inclusive).",
                    @value, _minValue, _maxValue, _kind);
            }

            for (var i = 0; i < _names.Length; i++)
            {
                if (_comparer.IsPrefix(_names[i], @value, CompareOptions.IgnoreCase))
                    return i + _minValue;
            }

            throw new ParseException("{0} is not a known value name. Use one of the following: {1}",
                @value, string.Join(", ", _names));
        }
    }
}
