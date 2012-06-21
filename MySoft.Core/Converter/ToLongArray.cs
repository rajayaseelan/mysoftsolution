namespace MySoft.Converter
{
    using System;

    public class ToLongArray : ToArray
    {
        private static Type mValueType = typeof(long);

        protected override Type ValueType
        {
            get
            {
                return mValueType;
            }
        }
    }
}

