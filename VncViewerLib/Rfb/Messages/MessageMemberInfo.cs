using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace VncViewerLib
{
    public class MessageMemberInfo
    {
        public Object Value { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }
        public MessageMemberAttribute MessageMemberAttribute { get; private set; }
        public Type Type { get; private set; }

        public static MessageMemberInfo FromPropertyInfo(PropertyInfo pi, Object obj)
        {
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
