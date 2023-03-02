using System;

namespace Utils.Others
{
    public static class EnumerationExtensions
    {
        public static bool Has(this long type, long value)
        {
            try
            {
                return (type & value) == value;
            }
            catch
            {
                return false;
            }
        }

        public static bool Is(this Enum type, long value)
        {
            try
            {
                return (long)(object)type == value;
            }
            catch
            {
                return false;
            }
        }

        public static T Add<T>(this long type, T value)
        {
            try
            {
                return (T)(object)(type | (long)(object)value);
            }
            catch (Exception ex)
            {
                throw new AppException(
                    $"Could not append value from enumerated type '{typeof(T).Name}'.", ex.Message);
            }
        }

        public static string ToString(Enum value)
        {
            return value.ToString();
        }

        public static T Remove<T>(this Enum type, T value)
        {
            try
            {
                return (T)(object)((int)(object)type & ~(int)(object)value);
            }
            catch (Exception ex)
            {
                throw new AppException(
                    $"Could not remove value from enumerated type '{typeof(T).Name}'.", ex.Message);
            }
        }
    }
}