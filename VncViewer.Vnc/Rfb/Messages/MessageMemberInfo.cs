using System;
using System.Reflection;

#pragma warning disable CA1720 // Identifier contains type name

namespace VncViewer.Vnc
{
    public class MessageMemberInfo
    {
        public Object Value { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }
        public MessageMemberAttribute MessageMemberAttribute { get; private set; }
        public Type Type { get; private set; }


        public static MessageMemberInfo FromPropertyInfo(PropertyInfo pi, Object obj)
        {
            if (pi == null) throw new ArgumentNullException(nameof(pi));            

            return new MessageMemberInfo()
            {
                MessageMemberAttribute = pi.GetCustomAttribute<MessageMemberAttribute>(),
                Value = pi.GetValue(obj),
                PropertyInfo = pi,
                Type = pi.PropertyType
            };

        }
    }
}
