using System;
using System.Reflection;

namespace EFSqlTranslator.Translation
{
    public interface IModelInfoProvider
    {
        EntityInfo FindEntityInfo(Type type);
        EntityFieldInfo FindFieldInfo(MemberInfo memberInfo);
    }
}