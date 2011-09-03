namespace MySoft.Converter
{
    using System;

    public class ToDoubleArray : ToArray
    {
        private static Type mValueType = typeof(double);

        protected override Type ValueType
        {
            get
            {
                return mValueType;
            }
        }
    }
}

