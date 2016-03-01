using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using FilterConditional.Builder.Interfaces;
using FilterConditional.Container;
using FilterConditional.TypeExpression;

namespace FilterConditional.Builder
{
    public class ConditionalFilterBuilder : IExpressionBuilder<ContainerExpression, bool>
    {
        public Expression<Func<TItem, bool>> ToBuild<TItem>(IEnumerable<ContainerExpression> expressions,
            NameValueCollection keyValue)
        {

            var items = Matching(expressions, keyValue);

            if (!items.Any())
            {
                return t => true;
            }
            var query = GetActualListExpression(items);

            return GetPredicate<TItem>(query);

        }

        private Expression<Func<TItem, bool>> GetPredicate<TItem>(IEnumerable<ContainerResult> query)
        {
            Expression<Func<TItem, bool>> predicate = t => false;

            var prFirst = query.First();

            var coll = Enumerable.Concat(new[] {predicate.Parameters[0]}, prFirst.Parameters);

            predicate = ConcatExpression(predicate, Expression.Invoke(prFirst.Expression, coll), BinaryExpressionType.Or);


            predicate = query.Skip(1).Aggregate(predicate, (pr, item) =>
                //pr - predicate that will have included to self the other expressions
                ConcatExpression(pr,
                    //Expression.Invoke takes actual expression and his parameters (one or more)
                    Expression.Invoke(item.Expression,
                        //Enumerable.Concat create array with parameters. Always first parameter is generic type TItem
                        Enumerable.Concat(new[] {predicate.Parameters[0]}, item.Parameters)),
                    item.ExprType)
                );
            return predicate;
        }

        #region Matching
        private IEnumerable<ContainerResult> Matching(IEnumerable<ContainerExpression> expressions,
            NameValueCollection keyValue)
        {
            var items = from item in expressions
                //check on exists keys that has been defined in collection keys ContainerExpression
                //if exists then select this keys
                where
                    item.Keys.Any() &&
                    item.Keys.All(k => !string.IsNullOrWhiteSpace(k) && keyValue[k] != null && keyValue[k].Any())
                let values = (from key in item.Keys
                    select new
                    {
                        key,
                        parameters = ConvertValue(keyValue[key], item.KeyType[key]),
                    })
                where values.All(v => v.parameters != null)
                select new ContainerResult()
                {
                    Expression = item.Expression,
                    ExprType = item.ExpType,
                    Parameters = values.Select(v => v.parameters),
                    IsRequire = item.IsRequire,
                    Keys = values.Select(v => v.key)
                };
            return items;
        }

        #endregion
        #region actual list expressions

        private IEnumerable<ContainerResult> GetActualListExpression(IEnumerable<ContainerResult> items)
        {
            //select only expression where is require and select their keys
            var keys = items.Where(i => i.IsRequire).SelectMany(i => i.Keys);
            //filter expression where is require or no contains keys in previos query
            var query = items.Where(i => i.IsRequire || !i.Keys.Any(k => keys.Contains(k)));
            //for concat expression
            return query;
        }

        #endregion
        #region combines right and left body by specific scenario
        protected Expression<Func<T, bool>> ConcatExpression<T>(Expression<Func<T, bool>> expr, InvocationExpression invokedExpr, BinaryExpressionType expType)
        {
            return Expression.Lambda<Func<T, bool>>
                (expType == BinaryExpressionType.And ? Expression.AndAlso(expr.Body, invokedExpr)
                : Expression.Or(expr.Body, invokedExpr), expr.Parameters);
        }

        #endregion

        protected Expression ConvertValue(string val, Type type)
        {
            try
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    var array = val.Split(',').Select(e => Expression.Constant(Convert.ChangeType(e, type.GenericTypeArguments[0])));
                    return Expression.NewArrayInit(type.GenericTypeArguments[0], array);
                }
                var result = Convert.ChangeType(val, type);
                return Expression.Constant(result);
            }
            catch (Exception e)
            {
                return null;
            }
        }
        private struct ContainerResult
        {
            public IEnumerable<string> Keys { get; set; }
            public Expression Expression { get; set; }           
            public IEnumerable<Expression> Parameters { get; set; }       
            public bool IsRequire { get; set; }
            public BinaryExpressionType ExprType { get; set; }
        }
    }
}