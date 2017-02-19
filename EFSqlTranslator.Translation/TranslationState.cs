using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using EFSqlTranslator.Translation.DbObjects;

namespace EFSqlTranslator.Translation
{
    public class TranslationState
    {
        private readonly List<IDbObject> _preScripts = new List<IDbObject>();

        private readonly List<IDbObject> _postScripts = new List<IDbObject>();

        public Stack<IDbObject> ResultStack { get; } = new Stack<IDbObject>();

        public Stack<Dictionary<ParameterExpression, DbReference>> ParamterStack { get; } =
            new Stack<Dictionary<ParameterExpression, DbReference>>();

        public Dictionary<Tuple<IDbSelect, EntityRelation>, IDbJoin> CreatedJoins { get; } =
            new Dictionary<Tuple<IDbSelect, EntityRelation>, IDbJoin>();

        public void AddPreScript(IDbObject script)
        {
            _preScripts.Add(script);
        }

        public void AddPostScript(IDbObject script)
        {
            _postScripts.Add(script);
        }

        public IDbSelect GetLastSelect()
        {
            IDbSelect dbSelect = null;

            var results = new Stack<IDbObject>();
            while(ResultStack.Count > 0)
            {
                var dbObject = ResultStack.Pop();
                results.Push(dbObject);

                dbSelect = dbObject as IDbSelect;
                if (dbSelect != null)
                    break;
            }

            while(results.Count > 0)
                ResultStack.Push(results.Pop());

            return dbSelect;
        }

        public IDbScript GetScript(IDbObjectFactory dbFactory)
        {
            var script = dbFactory.BuildScript();

            script.PreScripts.AddRange(_preScripts);

            var dbSelect = GetLastSelect();
            dbSelect = dbSelect.Optimize();
            script.Scripts.Add(dbSelect);

            script.PostScripts.AddRange(_postScripts);

            return script;
        }
    }
}