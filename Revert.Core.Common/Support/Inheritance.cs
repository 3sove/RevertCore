using System;
using System.Collections.Generic;
using System.Linq;

namespace Revert.Core.Common.Support
{
    public class Inheritance
    {
        private const System.Reflection.BindingFlags FieldBindingFlags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public |
                                                                           System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        public static List<Type> GetInheritingMembers(Type baseType)
        {
            return System.Reflection.Assembly.GetAssembly(baseType).GetTypes().Where(baseType.IsAssignableFrom).ToList();
        }

        public static List<T> GetImplementingSingletonMembers<T>(string singletonFieldName = "Instance")
        {
            return GetImplementingSingletonMembers<T>(typeof(T), singletonFieldName);
        }

        public static List<T> GetImplementingSingletonMembers<T>(Type baseType, string singletonFieldName = "Instance")
        {
            List<Type> inheritingTypes = GetInheritingMembers(baseType);
            var implementingMembers = new List<T>();

            foreach (Type t in inheritingTypes)
                foreach (var field in t.GetFields(FieldBindingFlags))
                    if (field.Name == singletonFieldName)
                    {
                        implementingMembers.Add((T)field.GetValue(null));
                        break;
                    }

            return implementingMembers;
        }
    }
}
