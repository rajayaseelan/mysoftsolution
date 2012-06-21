namespace MySoft.Converter
{

    public class ToChar : IStringConverter
    {
        public object ConvertTo(string value, out bool succeeded)
        {
            char ch;
            succeeded = char.TryParse(value, out ch);
            return ch;
        }
    }
}

