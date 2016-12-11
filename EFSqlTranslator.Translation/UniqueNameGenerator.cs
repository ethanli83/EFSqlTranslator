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
            int count;
            if (dbSelect == null)
            {
                count = _globalUniqueAliasNames.ContainsKey(name)
                    ? ++_globalUniqueAliasNames[name]
                    : _globalUniqueAliasNames[name] = 0;
            }
            else
            {
                var uniqueNames = _uniqueAliasNames.ContainsKey(dbSelect)
                    ? _uniqueAliasNames[dbSelect]
                    : _uniqueAliasNames[dbSelect] = new Dictionary<string, int>();

                count = uniqueNames.ContainsKey(name)
                    ? ++uniqueNames[name]
                    : uniqueNames[name] = 0;
            }

            return !fullName
                ? $"{name.Substring(0, 1).ToLower()}{count}"
                : $"{name}{count}";
        }
    }
}