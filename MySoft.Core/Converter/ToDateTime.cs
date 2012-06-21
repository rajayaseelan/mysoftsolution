namespace MySoft.Converter
{
    using System;

    public class ToDateTime : IStringConverter
    {
        object IStringConverter.ConvertTo(string value, out bool succeeded)
        {
            DateTime time;
            succeeded = DateTime.TryParse(value, out time);
            return time;
        }
    }
}

