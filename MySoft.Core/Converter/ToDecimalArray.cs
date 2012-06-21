namespace MySoft.Converter
{
    using System;

    public class ToDecimalArray : ToArray
    {
        private static Type mValueType = typeof(decimal);

        protected override Type ValueType
        {
            get
            {
                return mValueType;
            }
        }
    }
}

