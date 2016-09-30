using System;

namespace Translation
{
    public interface IModelInfoProvider
    {
        EntityInfo FindEntityInfo(Type type);
    }
}