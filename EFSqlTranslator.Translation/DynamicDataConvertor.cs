using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFSqlTranslator.Translation.Extensions;

namespace EFSqlTranslator.Translation
{
    public class DynamicDataConvertor
    {
        private readonly Type _type;
        
        private readonly IDictionary<PropertyInfo, PropertyInfo[]> _propertyInfoCache = 
            new ConcurrentDictionary<PropertyInfo, PropertyInfo[]>();

        public DynamicDataConvertor(Type type)
        {
            _type = type;
        }

        public IEnumerable<object> Convert(IEnumerable<dynamic> data)
        {
            var properties = _type.GetProperties();
            var constructor = _type.GetConstructors().Single();

            var fdList = new List<object>();
            foreach (var row in data)
            {
                var valDict = ((IDictionary<string, object>)row).ToArray();
                var objArray = GetObjArray(valDict, properties, 0);
                var obj =  constructor.Invoke(objArray);
                fdList.Add(obj);
            }

            return fdList;
        }

        private object[] GetObjArray(
            IReadOnlyList<KeyValuePair<string, object>> vals, IReadOnlyList<PropertyInfo> properties, int cIndex)
        {
            var objArray = new object[properties.Count];
            for (var i = 0; i < properties.Count; i++)
            {
                var info = properties[i];

                if (!info.PropertyType.IsValueType())
                {
                    var eProps = _propertyInfoCache.GetOrAdd(info, () =>
                    {
                        return info.PropertyType.GetProperties().Where(p => p.PropertyType.IsValueType()).ToArray();
                    });
                    
                    var objVals = GetObjArray(vals, eProps.ToArray(), cIndex);
                    var eObj = Activator.CreateInstance(info.PropertyType);
                    for (var j = 0; j < eProps.Length; j++)
                    {
                        var prop = eProps[j];
                        prop.SetValue(eObj, objVals[j]);
                    }

                    objArray[i] = eObj;
                    cIndex += objVals.Length;
                }
                else
                {
                    var kvp = vals[cIndex++];
                    if (kvp.Key != info.Name)
                        throw new Exception($"Result column name '{kvp.Key}' is not match property name '{info.Name}'.");
                    
                    var val = kvp.Value;
                    var infoType = info.PropertyType.StripNullable();

                    try
                    {
                        objArray[i] = infoType == typeof(Guid)
                            ? new Guid((byte[]) val)
                            : val == null ? null : System.Convert.ChangeType(val, infoType);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{val} {infoType.FullName}");
                        throw;
                    }
                }
            }
            return objArray;
        }
    }
}