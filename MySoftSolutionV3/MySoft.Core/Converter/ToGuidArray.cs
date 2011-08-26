namespace MySoft.Converter
{
    using System;

    public class ToGuidArray : ToArray
    {
        private static Type mValueType = typeof(Guid);

        protected override Type ValueType
        {
            get
            {
                return mValueType;
            }
        }
    }
}

