namespace MySoft.Converter
{
    using System;

    public class ToSbyteArray : ToArray
    {
        private static Type mValueType = typeof(sbyte);

        protected override Type ValueType
        {
            get
            {
                return mValueType;
            }
        }
    }
}

