namespace MySoft.Converter
{
    using System;

    public class ToByteArray : ToArray
    {
        private static Type mValueType = typeof(byte[]);

        protected override Type ValueType
        {
            get
            {
                return mValueType;
            }
        }
    }
}

