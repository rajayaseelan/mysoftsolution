using System;
using System.Globalization;
using System.Reflection;

namespace MySoft.Web.UI.Controls
{
    internal class PropertyInfoDelegator : PropertyInfo
    {
        private PropertyInfo real;

        public PropertyInfoDelegator(PropertyInfo real)
        {
            this.real = real;
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            return this.real.GetValue(obj, invokeAttr, binder, index, culture);
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            this.real.SetValue(obj, value, invokeAttr, binder, index, culture);
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            return this.real.GetIndexParameters();
        }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            return this.real.GetAccessors(nonPublic);
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            return this.real.GetGetMethod(nonPublic);
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            return this.real.GetSetMethod(nonPublic);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return this.real.IsDefined(attributeType, true);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return this.real.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return this.real.GetCustomAttributes(attributeType, true);
        }

        public override string Name
        {
            get { return this.real.Name; }
        }

        public override Type PropertyType
        {
            get { return this.real.PropertyType; }
        }

        public override bool CanRead
        {
            get { return this.real.CanRead; }
        }

        public override bool CanWrite
        {
            get { return this.real.CanWrite; }
        }

        public override Type DeclaringType
        {
            get { return this.real.DeclaringType; }
        }

        public override Type ReflectedType
        {
            get { return this.real.ReflectedType; }
        }

        public override PropertyAttributes Attributes
        {
            get { return this.real.Attributes; }
        }
    }
}