namespace MySoft.Converter
{
    using System;

    public abstract class ToArray : IStringConverter
    {
        protected ToArray()
        {
        }

        protected object ConverItem(string value, out bool succeeded)
        {
            succeeded = ConverterFactory.Converters.ContainsKey(this.ValueType);
            if (!succeeded)
            {
                return null;
            }
            IStringConverter converter = ConverterFactory.Converters[this.ValueType];
            return converter.ConvertTo(value, out succeeded);
        }

        object IStringConverter.ConvertTo(string value, out bool succeeded)
        {
            succeeded = (value != null) && (value != string.Empty);
            if (!succeeded)
            {
                return null;
            }
            string[] strArray = value.Split(new char[] { ',' });
            Array array = Array.CreateInstance(this.ValueType, strArray.Length);
            for (int i = 0; i < strArray.Length; i++)
            {
                object obj2 = this.ConverItem(strArray[i], out succeeded);
                if (!succeeded)
                {
                    return null;
                }
                array.SetValue(obj2, i);
            }
            return array;
        }

        protected abstract Type ValueType { get; }
    }
}

