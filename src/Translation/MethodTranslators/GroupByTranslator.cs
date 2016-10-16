using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Translation.DbObjects;

namespace Translation.MethodTranslators
{
    public class GroupByTranslator : AbstractMethodTranslator
    {
        public GroupByTranslator(IModelInfoProvider infoProvider, IDbObjectFactory dbFactory) 
            : base(infoProvider, dbFactory)
        {
        }

        public override void Register(TranslationPlugIns plugIns)
        {
            plugIns.RegisterMethodTranslator("groupby", this);
        }


        public override void Translate(
            MethodCallExpression m, TranslationState state, UniqueNameGenerator nameGenerator)
        {
            // group by can be a column, a expression, or a list of columns / expressions
            var arguments = state.ResultStack.Pop();
            var dbSelect = (IDbSelect)state.ResultStack.Pop();
            
            // if the selection is not empty
            // we need to wrap the select in another select
            
            state.ResultStack.Push(dbSelect);
        }
    }
}