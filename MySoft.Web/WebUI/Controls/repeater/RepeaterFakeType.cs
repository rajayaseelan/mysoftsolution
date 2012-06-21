using System;
using System.Reflection;
using System.Web.UI;

namespace MySoft.Web.UI.Controls
{
    internal class RepeaterFakeType : TypeDelegator
    {
        #region FakePropertyInfo Class

        private class FakePropertyInfo : PropertyInfoDelegator
        {
            private Type templateContainerType;

            public FakePropertyInfo(PropertyInfo real, Type templateContainerType)
                : base(real)
            {
                this.templateContainerType = templateContainerType;
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                if (attributeType == typeof(TemplateContainerAttribute))
                    return new Attribute[] { new TemplateContainerAttribute(templateContainerType) };

                return base.GetCustomAttributes(attributeType, inherit);
            }
        }

        #endregion

        private Type repeaterItemType;

        public RepeaterFakeType(Type dataItemType)
            : base(typeof(Repeater<>).MakeGenericType(dataItemType))
        {
            this.repeaterItemType = typeof(RepeaterItem<>).MakeGenericType(dataItemType);
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            PropertyInfo info = base.GetPropertyImpl(name, bindingAttr, binder, returnType, types, modifiers);

            if (name == "ItemTemplate" || name == "AlternatingItemTemplate")
                info = new FakePropertyInfo(info, this.repeaterItemType);

            return info;
        }
    }
}