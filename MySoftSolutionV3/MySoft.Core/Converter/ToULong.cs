namespace MySoft.Converter
{

    public class ToULong : IStringConverter
    {
        object IStringConverter.ConvertTo(string value, out bool succeeded)
        {
            ulong num;
            succeeded = ulong.TryParse(value, out num);
            return num;
        }
    }
}

