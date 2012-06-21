namespace MySoft.Converter
{
    using System;

    public class ToInt16Array : ToArray
    {
        private static Type mValueType = typeof(short);

        protected override Type ValueType
        {
            get
            {
                return mValueType;
            }
        }
    }
}

