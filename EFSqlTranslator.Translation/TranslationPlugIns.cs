using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using EFSqlTranslator.Translation.MethodTranslators;

namespace EFSqlTranslator.Translation
{
    public class TranslationPlugIns
    {
        private readonly Dictionary<string, AbstractMethodTranslator> _methodTranslators =
            new Dictionary<string, AbstractMethodTranslator>(StringComparer.CurrentCultureIgnoreCase);

        public void RegisterMethodTranslator(string methodName, AbstractMethodTranslator translator)
        {
            _methodTranslators[methodName] = translator;
        }

        internal bool TranslateMethodCall(
            MethodCallExpression m, TranslationState state, UniqueNameGenerator nameGenerator)
        {
            var method = m.Method;
            if (!_methodTranslators.ContainsKey(method.Name))
                return false;

            var translator = _methodTranslators[method.Name];
            translator.Translate(m, state, nameGenerator);
            return true;
        }
    }
}