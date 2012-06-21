namespace MySoft.Converter
{
    using System;

    public class ToBoolArray : ToArray
    {
        private static Type mValueType = typeof(bool);

        protected override Type ValueType
        {
            get
            {
                return mValueType;
            }
        }
    }
}

