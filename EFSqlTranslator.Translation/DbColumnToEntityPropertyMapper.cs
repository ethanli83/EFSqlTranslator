using System;
using System.Linq;
using EFSqlTranslator.Translation.DbObjects;
using Microsoft.AspNetCore.Builder;

namespace EFSqlTranslator.Translation
{
    public static class DbColumnToEntityPropertyMapper
    {
        public static IDbSelect Map(
            IDbSelect dbSelect, Type returnType,
            IModelInfoProvider infoProvider, IDbObjectFactory dbFactory, UniqueNameGenerator nameGenerator)
        {
            // If the select returns specified columns, it means there is a constructor in Select,
            // e.g (.Select(d => new { A = d.ABC }))
            // In this case we do not need to add alias to the end result as where will be one in the select
            if (dbSelect.Selection.Any(s => !(s is IDbRefColumn)))
                return dbSelect;

            var entityInfo = infoProvider.FindEntityInfo(returnType);
            if (!entityInfo.RequirePropertyNameMapping())
                return dbSelect;

            var alias = nameGenerator.GenerateAlias(dbSelect, TranslationConstants.SubSelectPrefix, true);
            var newSelectRef = dbFactory.BuildRef(dbSelect, alias);
            var newSelect = dbFactory.BuildSelect(newSelectRef);

            foreach (var fieldInfo in entityInfo.Columns)
            {
                var column = dbFactory.BuildColumn(
                    newSelectRef, fieldInfo.DbName, fieldInfo.ValType, fieldInfo.PropertyName);

                newSelect.Selection.Add(column);
            }

            return newSelect;
        }
    }
}