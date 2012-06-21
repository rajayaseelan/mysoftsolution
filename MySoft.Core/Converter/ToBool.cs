namespace MySoft.Converter
{

    public class ToBool : IStringConverter
    {
        object IStringConverter.ConvertTo(string value, out bool succeeded)
        {
            bool flag;
            succeeded = bool.TryParse(value, out flag);
            return flag;
        }
    }
}

