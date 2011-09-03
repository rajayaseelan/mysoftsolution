namespace MySoft.Converter
{
    using System;

    public class ToUInt32Array : ToArray
    {
        private static Type mValueType = typeof(uint);

        protected override Type ValueType
        {
            get
            {
                return mValueType;
            }
        }
    }
}

