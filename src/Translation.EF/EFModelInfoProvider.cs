using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Concurrent;

namespace Translation.EF
{
    public class EFModelInfoProvider : IModelInfoProvider
    {
        private readonly DbContext _ctx;
        private readonly ConcurrentDictionary<Type, EntityInfo> _entityInfos = 
            new ConcurrentDictionary<Type, EntityInfo>();

        public EFModelInfoProvider(DbContext context)
        {
            _ctx = context;
            foreach(var entityType in _ctx.Model.GetEntityTypes())
            {
                var entityInfo = GetOrAddEntityInfo(entityType);
                UpdateKeysAndColumns(entityType, entityInfo);
                UpdateParentRelations(entityType, entityInfo);
                UpdateChildRelations(entityType, entityInfo);
            }
        }

        private void UpdateKeysAndColumns(IEntityType et, EntityInfo ei)
        {
            ei.Keys = et.GetKeys().First(k => k.IsPrimaryKey()).ToEntityFieldInfo(ei).ToList();
            ei.Columns = et.GetProperties().Select(p => p.ToEntityFieldInfo(ei)).ToList();
        }

        private void UpdateParentRelations(IEntityType et, EntityInfo ei)
        {
            foreach(var relation in et.GetForeignKeys())
            {
                var relationKey = relation.DependentToPrincipal.Name;

                // child entity in parent relation is the current entity
                var declaringEntity = GetOrAddEntityInfo(relation.DeclaringEntityType);
                var declaringKeys = relation.Properties.Select(p => p.ToEntityFieldInfo(declaringEntity));
                
                // parent entity in parent relation is the referring entity 
                var principalEntity = GetOrAddEntityInfo(relation.PrincipalEntityType); 
                var principalKeys = relation.PrincipalKey.Properties.Select(p => p.ToEntityFieldInfo(principalEntity));

                var entityRelation = new EntityRelation
                {
                    FromEntity = declaringEntity,
                    ToEntity = principalEntity,
                    FromKeys = declaringKeys.ToList(),
                    ToKeys = principalKeys.ToList(), 
                };

                ei.AddRelation(relationKey, entityRelation);
            }
        }

        private void UpdateChildRelations(IEntityType et, EntityInfo ei)
        {
            foreach(var relation in et.GetReferencingForeignKeys())
            {
                var relationKey = relation.PrincipalToDependent.Name;

                // child entity in child relation is the referring entity
                var declaringEntity = GetOrAddEntityInfo(relation.DeclaringEntityType); 
                var declaringKeys = relation.Properties.Select(p => p.ToEntityFieldInfo(declaringEntity));

                // parent entity in child relation is the current entity
                var principalEntity = GetOrAddEntityInfo(relation.PrincipalEntityType);
                var principalKeys = relation.PrincipalKey.Properties.Select(p => p.ToEntityFieldInfo(principalEntity));

                var entityRelation = new EntityRelation
                {
                    FromEntity = principalEntity,
                    ToEntity = declaringEntity,
                    FromKeys = principalKeys.ToList(),
                    ToKeys = declaringKeys.ToList(), 
                    IsChildRelation = true
                };

                ei.AddRelation(relationKey, entityRelation);
            }
        }

        public EntityInfo GetOrAddEntityInfo(IEntityType et)
        {
            var type = et.ClrType;
            if (!_entityInfos.ContainsKey(type))
                _entityInfos[type] = et.ToEntityInfo();
            return _entityInfos[type];
        }

        public EntityInfo FindEntityInfo(Type type)
        {
            type = type.GenericTypeArguments.FirstOrDefault() ?? type;
            if (_entityInfos.ContainsKey(type))
                return _entityInfos[type];

            return null;
        }
    }
}