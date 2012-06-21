namespace MySoft.Converter
{
    using System;
    using System.Collections.Generic;

    public static class ConverterFactory
    {
        private static IDictionary<Type, IStringConverter> mConverters;

        private static void OnInit()
        {
            if (mConverters == null)
            {
                mConverters = new Dictionary<Type, IStringConverter>();
                mConverters.Add(typeof(byte), new ToByte());
                mConverters.Add(typeof(byte[]), new ToByteArray());
                mConverters.Add(typeof(sbyte), new ToSbyte());
                mConverters.Add(typeof(sbyte[]), new ToSbyteArray());
                mConverters.Add(typeof(short), new ToInt16());
                mConverters.Add(typeof(short[]), new ToInt16Array());
                mConverters.Add(typeof(ushort), new ToUInt16());
                mConverters.Add(typeof(ushort[]), new ToUInt16Array());
                mConverters.Add(typeof(int), new ToInt32());
                mConverters.Add(typeof(int[]), new ToInt32Array());
                mConverters.Add(typeof(uint), new ToUInt23());
                mConverters.Add(typeof(uint[]), new ToUInt16Array());
                mConverters.Add(typeof(long), new ToLong());
                mConverters.Add(typeof(long[]), new ToLongArray());
                mConverters.Add(typeof(ulong), new ToULong());
                mConverters.Add(typeof(ulong[]), new ToULongArray());
                mConverters.Add(typeof(char), new ToChar());
                mConverters.Add(typeof(char[]), new ToCharArray());
                mConverters.Add(typeof(Guid), new ToGuid());
                mConverters.Add(typeof(Guid[]), new ToGuidArray());
                mConverters.Add(typeof(DateTime), new ToDateTime());
                mConverters.Add(typeof(DateTime[]), new ToDateTimeArray());
                mConverters.Add(typeof(decimal), new ToDecimal());
                mConverters.Add(typeof(decimal[]), new ToDecimalArray());
                mConverters.Add(typeof(float), new ToFloat());
                mConverters.Add(typeof(float[]), new ToFloatArray());
                mConverters.Add(typeof(double), new ToDouble());
                mConverters.Add(typeof(double[]), new ToDoubleArray());
                mConverters.Add(typeof(string), new ToString());
                mConverters.Add(typeof(string[]), new ToStringArray());
                mConverters.Add(typeof(bool), new ToBool());
                mConverters.Add(typeof(bool[]), new ToBoolArray());
            }
        }

        public static IDictionary<Type, IStringConverter> Converters
        {
            get
            {
                if (mConverters == null)
                {
                    lock (typeof(ConverterFactory))
                    {
                        OnInit();
                    }
                }
                return mConverters;
            }
        }
    }
}

