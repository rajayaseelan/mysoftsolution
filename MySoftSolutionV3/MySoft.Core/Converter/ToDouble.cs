namespace MySoft.Converter
{

    public class ToDouble : IStringConverter
    {
        object IStringConverter.ConvertTo(string value, out bool succeeded)
        {
            double num;
            succeeded = double.TryParse(value, out num);
            return num;
        }
    }
}

