using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace NestorHub.Sentinels.Api.Class
{
    public static class ObjectExtension
    {
        public static T GetPropertyValue<T>(this object argument, string propertyName)
        {
            var arg = (JObject)argument;
            var property = arg.Property(propertyName);
            return property != null ? property.Value.Value<T>() : default(T);
        }

        public static List<T> GetListPropertyValue<T>(this object argument, string propertyName)
        {
            var list = new List<T>();
            var arg = (JObject)argument;
            var property = arg.Property(propertyName);
            if (property != null)
            {
                foreach (var value in property.Value)
                {
                    list.Add(value.ToObject<T>());
                }
            }
            return list;
        }
    }
}
