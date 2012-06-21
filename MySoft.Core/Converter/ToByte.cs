namespace MySoft.Converter
{

    public class ToByte : IStringConverter
    {
        object IStringConverter.ConvertTo(string value, out bool succeeded)
        {
            byte num;
            succeeded = byte.TryParse(value, out num);
            return num;
        }
    }
}

