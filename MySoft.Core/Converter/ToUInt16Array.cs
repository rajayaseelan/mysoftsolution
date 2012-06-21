namespace MySoft.Converter
{
    using System;

    public class ToUInt16Array : ToArray
    {
        private static Type mValueType = typeof(ushort);

        protected override Type ValueType
        {
            get
            {
                return mValueType;
            }
        }
    }
}

