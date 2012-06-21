namespace MySoft.Converter
{
    using System;

    public class ToCharArray : ToArray
    {
        private static Type mValueType = typeof(char);

        protected override Type ValueType
        {
            get
            {
                return mValueType;
            }
        }
    }
}

