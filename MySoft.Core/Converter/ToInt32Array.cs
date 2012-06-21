namespace MySoft.Converter
{
    using System;

    public class ToInt32Array : ToArray
    {
        private static Type mValueType = typeof(int);

        protected override Type ValueType
        {
            get
            {
                return mValueType;
            }
        }
    }
}

