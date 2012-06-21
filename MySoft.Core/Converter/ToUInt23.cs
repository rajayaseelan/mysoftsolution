namespace MySoft.Converter
{

    public class ToUInt23 : IStringConverter
    {
        object IStringConverter.ConvertTo(string value, out bool succeeded)
        {
            uint num;
            succeeded = uint.TryParse(value, out num);
            return num;
        }
    }
}

