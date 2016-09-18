using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace EFSqlTranslator.Translation
{
    public class ModelInfoProvider
    {
        private readonly DbContext _ctx;
        private readonly Dictionary<Type, EntityInfo> _entityInfos = new Dictionary<Type, EntityInfo>();
        public ModelInfoProvider(DbContext context)
        {
            _ctx = context;

            var eqgType = typeof(EntityQueryable<>); 
            var entityTypes = _ctx.Model.GetEntityTypes();
            foreach(var et in entityTypes)
            {
                var type = et.ClrType;
                var gType = typeof(EntityQueryable<>).MakeGenericType(type);
                
                var entityInfo = et.ToEntityInfo();
                _entityInfos[type] = entityInfo;
                _entityInfos[gType] = entityInfo;
            }
        }

        public EntityInfo FindEntityInfo(Type type)
        {
            if (_entityInfos.ContainsKey(type))
                return _entityInfos[type];

            return null;
        }
    }
}