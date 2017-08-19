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
        
        private readonly IDictionary<PropertyInfo, Func<object, object>> _convertFuncCache = 
            new ConcurrentDictionary<PropertyInfo, Func<object, object>>();

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
                var valDict = (IDictionary<string, object>)row;
                var objArray = GetObjArray(valDict, properties);
                var obj =  constructor.Invoke(objArray);
                fdList.Add(obj);
            }

            return fdList;
        }

        private object[] GetObjArray(IDictionary<string, object> valDict, IReadOnlyList<PropertyInfo> properties)
        {
            var objArray = new object[properties.Count];
            for (var i = 0; i < properties.Count; i++)
            {
                var info = properties[i];

                if (!info.PropertyType.IsValueType())
                {
                    var eProps = _propertyInfoCache.GetOrAdd(info, () =>
                    {
                        return info.PropertyType.GetProperties()
                            .Where(p => p.PropertyType.IsValueType() && valDict.ContainsKey(p.Name))
                            .ToArray();
                    });
                    
                    var objVals = GetObjArray(valDict, eProps.ToArray());
                    var eObj = Activator.CreateInstance(info.PropertyType);
                    for (var j = 0; j < eProps.Length; j++)
                    {
                        var prop = eProps[j];
                        prop.SetValue(eObj, objVals[j]);
                    }

                    objArray[i] = eObj;
                }
                else
                {
                    var val = valDict[info.Name];
                    var infoType = info.PropertyType.StripNullable();
                    
                    objArray[i] = infoType == typeof(Guid)
                        ? new Guid((byte[]) val)
                        : System.Convert.ChangeType(val, infoType);
                }
            }
            return objArray;
        }
    }
}