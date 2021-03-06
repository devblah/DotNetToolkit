﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace DevBlah.DotNetToolkit.System.Extensions
{
    public static class ObjectExtensions
    {
        private readonly static object Lock = new object();

        /// <summary>
        /// Clones the current object via reflection - generic version
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <param name="deep">if it should create a deep copy - ATTENTION: no check for circular references here -
        ///     infinte loop possible</param>
        /// <param name="propertyExcludeList">list of properties, which shouldn't be cloned</param>
        /// <returns></returns>
        public static T Clone<T>(this T original, bool deep, List<string> propertyExcludeList) where T : class
        {
            return Clone(original, original.GetType(), deep, propertyExcludeList) as T;
        }

        /// <summary>
        /// Clones the current object via reflection
        /// </summary>
        /// <param name="original">the current object</param>
        /// <param name="type">type, which should be created </param>
        /// <param name="deep">if it should create a deep copy - ATTENTION: no check for circular references here -
        ///     infinte loop possible</param>
        /// <param name="propertyExcludeList">list of properties, which shouldn't be cloned</param>
        /// <returns>the cloned object as 'object'</returns>
        public static object Clone(this object original, Type type, bool deep, List<string> propertyExcludeList)
        {
            try
            {
                Monitor.Enter(Lock);
                object copy = Activator.CreateInstance(type);
                PropertyInfo[] piList = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (PropertyInfo pi in piList)
                {
                    if (propertyExcludeList == null || !propertyExcludeList.Contains(pi.Name))
                    {
                        if (pi.GetValue(copy, null) != pi.GetValue(original, null))
                        {
                            object value = pi.GetValue(original, null);
                            if (deep && !pi.PropertyType.IsValueType
                                && !pi.PropertyType.GetInterfaces().Any(
                                    i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                            {
                                value = value.Clone(value.GetType(), true, propertyExcludeList);
                            }

                            pi.SetValue(copy, value, null);
                        }
                    }
                }
                return copy;
            }
            finally
            {
                Monitor.Exit(Lock);
            }
        }

        /// <summary>
        /// converts the current object to a dictionary
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bindingAttr"></param>
        /// <returns></returns>
        public static IDictionary<string, object> ToDictionary(this object source,
            BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );

        }
    }
}
