namespace MySoft.Converter
{

    public class ToUInt16 : IStringConverter
    {
        object IStringConverter.ConvertTo(string value, out bool succeeded)
        {
            ushort num;
            succeeded = ushort.TryParse(value, out num);
            return num;
        }
    }
}

