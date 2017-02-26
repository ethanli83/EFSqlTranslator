using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EFSqlTranslator.Translation.MethodTranslators;

namespace EFSqlTranslator.Translation
{
    public static class FillFunctionMaker
    {
        public static LambdaExpression Make(IncludeNode graphNode, IModelInfoProvider infoProvider)
        {
            // b => b.Posts
            var unaryExpr = (UnaryExpression)graphNode.Expression;
            var lambdaExpr = (LambdaExpression)unaryExpr.Operand;
            return Make(lambdaExpr, infoProvider);
        }

        public static LambdaExpression Make(LambdaExpression lambdaExpr, IModelInfoProvider infoProvider)
        {
            var memberExpr = (MemberExpression)lambdaExpr.Body;
            return Make(memberExpr, infoProvider);
        }

        public static LambdaExpression Make(MemberExpression memberExpr, IModelInfoProvider infoProvider)
        {
            // b => b.Posts
            // from prop Posts, to prop Blog
            // so If I go:
            // b(fromEntity).Posts(fromProp)
            // p(toEntity).Blog(toProp)
            var fromEntity = infoProvider.FindEntityInfo(memberExpr.Expression.Type);
            var relation = fromEntity.GetRelation(memberExpr.Member.Name);
            var toEntity = relation.ToEntity;

            var fromKey = relation.FromKeys.Single();
            var toKey = relation.ToKeys.Single();

            var fromProp = relation.FromProperty;
            var toProp = relation.ToProperty;

            var fromEnumType = typeof(IEnumerable<>).MakeGenericType(fromEntity.Type);
            var toEnumType = typeof(IEnumerable<>).MakeGenericType(toEntity.Type);

            if (!toEnumType.IsAssignableFrom(memberExpr.Type))
            {
                fromEntity = relation.ToEntity;
                toEntity = relation.FromEntity;

                fromKey = relation.ToKeys.Single();
                toKey = relation.FromKeys.Single();

                fromProp = relation.ToProperty;
                toProp = relation.FromProperty;

                var ft = fromEnumType;
                fromEnumType = toEnumType;
                toEnumType = ft;
            }

            var fromParam = Expression.Parameter(typeof(List<object>), "fs");
            var toParam = Expression.Parameter(typeof(List<object>), "ts");

            var varFromArr = Expression.Variable(fromEnumType, "fromArr");
            var varToArr = Expression.Variable(toEnumType, "toArr");

            var fromCastCall = Expression.Call(typeof(Enumerable), "Cast", new[] { fromEntity.Type }, fromParam);
            var toCastCall = Expression.Call(typeof(Enumerable), "Cast", new[] { toEntity.Type }, toParam);

            var varFromArrAssign = Expression.Assign(varFromArr, fromCastCall);
            var varToArrAssign = Expression.Assign(varToArr, toCastCall);

            // create dictionary for toEntities
            // var dicts = ps.GroupBy(p => p.BlogId).ToDictionary(bid => bid.Key);
            var tdCall = GetToDictCall(toEntity, toKey, varToArr);
            var varDict = Expression.Variable(tdCall.Type, "toDict");
            var varDictAssignExpr = Expression.Assign(varDict, tdCall);

            var varFromEntity = Expression.Parameter(fromEntity.Type, "f");

            var ifExpr = BuildRelationUpdateBlock(fromKey, fromProp, toEntity, toProp, varFromEntity, varDict);

            var forEachExpr = MakeForEach(varFromArr, varFromEntity, ifExpr);

            var codeBlock = Expression.Block(
                new [] { varFromArr, varToArr, varDict },
                varFromArrAssign, varToArrAssign, varDictAssignExpr, forEachExpr);

            return toEnumType.IsAssignableFrom(memberExpr.Type)
                ? Expression.Lambda(codeBlock, fromParam, toParam)
                : Expression.Lambda(codeBlock, toParam, fromParam);
        }

        private static Expression BuildRelationUpdateBlock(
            EntityFieldInfo fromKey, PropertyInfo fromProperty, EntityInfo toEntity,  PropertyInfo toProperty, Expression varFromEntity, Expression varDict)
        {
            var toEnumType = typeof(IEnumerable<>).MakeGenericType(toEntity.Type);
            var fkExpr = Expression.Property(varFromEntity, fromKey.ClrProperty);

            //var posts = dicts[blog.BlogId];
            var varTs = Expression.Variable(toEnumType, "rts");
            var dictItemExpr = Expression.Property(varDict, "Item", fkExpr);
            var varTsAssignExpr = Expression.Assign(varTs, dictItemExpr);

            var fmExpr = Expression.Property(varFromEntity, fromProperty);

            // blog.Posts.AddRange(posts);
            var arcExpr = Expression.Call(fmExpr, "AddRange", null, varTs);

            /**
            foreach (var post in posts)
            {
                post.Blog = blog;
            }
             */
            var varToEntity = Expression.Parameter(toEntity.Type, "t");
            var tfmExpr = Expression.Property(varToEntity, toProperty);
            var tfmAssignExpr = Expression.Assign(tfmExpr, varFromEntity);
            var tkLoop = MakeForEach(varTs, varToEntity, tfmAssignExpr);

            var updateBlockExpr = Expression.Block(new[] {varTs}, varTsAssignExpr, arcExpr, tkLoop);

            // ContainsKey(TKey key)
            var ccExpr = Expression.Call(varDict, "ContainsKey", null, fkExpr);
            var ifExpr = Expression.IfThen(ccExpr, updateBlockExpr);
            return ifExpr;
        }

        private static MethodCallExpression GetToDictCall(EntityInfo toEntity, EntityFieldInfo toKey, Expression toParam)
        {
            //  GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
            var gParam = Expression.Parameter(toEntity.Type, "t");
            var gBody = Expression.Property(gParam, toKey.ClrProperty);
            var gLambda = Expression.Lambda(gBody, gParam);
            var gCall = Expression.Call(typeof(Enumerable), "GroupBy", new[] {gParam.Type, gBody.Type}, toParam, gLambda);

            // ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
            var tdParam = Expression.Parameter(typeof(IGrouping<,>).MakeGenericType(toKey.ValType, toEntity.Type));
            var tdBody = Expression.Property(tdParam, "Key");
            var tdLambda = Expression.Lambda(tdBody, tdParam);
            var tdCall = Expression.Call(typeof(Enumerable), "ToDictionary", new[] {tdParam.Type, tdBody.Type}, gCall, tdLambda);

            return tdCall;
        }

        private static Expression MakeForEach(Expression collection, ParameterExpression loopVar, Expression loopContent)
        {
            var elementType = loopVar.Type;
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(elementType);

            var enumeratorVar = Expression.Variable(enumeratorType, "enumerator");
            var getEnumeratorCall = Expression.Call(collection, enumerableType.GetMethod("GetEnumerator"));
            var enumeratorAssign = Expression.Assign(enumeratorVar, getEnumeratorCall);

            // The MoveNext method's actually on IEnumerator, not IEnumerator<T>
            var moveNextCall = Expression.Call(enumeratorVar, typeof(IEnumerator).GetMethod("MoveNext"));

            var breakLabel = Expression.Label("LoopBreak");
            var breakExpression = Expression.Break(breakLabel);
            var disposeCall = Expression.Call(enumeratorVar, typeof(IDisposable).GetMethod("Dispose"));
            var elseBlock = Expression.Block(disposeCall, breakExpression);

            new List<int>().GetEnumerator().Dispose();

            var loop = Expression.Block(new[] { enumeratorVar },
                enumeratorAssign,
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.Equal(moveNextCall, Expression.Constant(true)),
                        Expression.Block(new[] { loopVar },
                            Expression.Assign(loopVar, Expression.Property(enumeratorVar, "Current")),
                            loopContent
                        ),
                        elseBlock
                    ),
                    breakLabel)
            );

            return loop;
        }
    }
}