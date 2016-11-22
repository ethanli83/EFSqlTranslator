using System;

namespace EFSqlTranslator.Translation
{
    public interface IModelInfoProvider
    {
        EntityInfo FindEntityInfo(Type type);
    }
}