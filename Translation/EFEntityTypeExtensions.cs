using Microsoft.EntityFrameworkCore.Metadata;

namespace EFSqlTranslator.Translation
{
    public static class EFEntityTypeExtensions
    {
        private const string _tableNameKey = "Relational:TableName";

        public static EntityInfo ToEntityInfo(this IEntityType et)
        {
            var a = et.GetAnnotations();
            var annotation = et.FindAnnotation(_tableNameKey);
            return new EntityInfo 
            {
                Namespace = "",
                EntityName = annotation.Value.ToString()
            };
        }
    }
}