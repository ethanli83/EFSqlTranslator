using System;
using System.Collections.Generic;
using EFSqlTranslator.Translation.DbObjects;

namespace EFSqlTranslator.Translation
{
    public class UniqueNameGenerator
    {
        private readonly Dictionary<IDbSelect, Dictionary<string, int>> _uniqueAliasNames =
            new Dictionary<IDbSelect, Dictionary<string, int>>();

        private readonly Dictionary<string, int> _globalUniqueAliasNames =
            new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);

        public string GenerateAlias(IDbSelect dbSelect, string name, bool fullName = false)
        {
            name = name.StartsWith("#") ? name.Remove(0, 1) : name;

            var alias = fullName ? name : name.Substring(0, 1).ToLower();

            int count;
            if (dbSelect == null)
            {
                count = _globalUniqueAliasNames.ContainsKey(alias)
                    ? ++_globalUniqueAliasNames[alias]
                    : _globalUniqueAliasNames[alias] = 0;
            }
            else
            {
                var uniqueNames = _uniqueAliasNames.ContainsKey(dbSelect)
                    ? _uniqueAliasNames[dbSelect]
                    : _uniqueAliasNames[dbSelect] = new Dictionary<string, int>();

                
                count = uniqueNames.ContainsKey(alias)
                    ? ++uniqueNames[alias]
                    : uniqueNames[alias] = 0;
            }

            return $"{alias}{count}";
        }
    }
}