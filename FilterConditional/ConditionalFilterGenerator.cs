using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;
using FilterConditional.Builder.Interfaces;
using FilterConditional.Container;
using FilterConditional.TypeExpression;



namespace FilterConditional
{
    public class ConditionalFilterGenerator<TItem>
    {
        protected NameValueCollection KeyValue;
        private readonly Lazy<List<ContainerExpression>> _expressions;
        private readonly IExpressionBuilder<ContainerExpression, bool> _builder;
        public ConditionalFilterGenerator(NameValueCollection dic, IExpressionBuilder<ContainerExpression, bool> builder)
        {
            KeyValue = dic;
            _builder = builder;
            _expressions = new Lazy<List<ContainerExpression>>(() => new List<ContainerExpression>());
        }

        #region Set up conditional
        /// <summary>
        /// Generate container and is adding to list that contains all the expressions
        /// </summary>
        /// <typeparam name="TConst">value filter that goes comparison</typeparam>
        /// <param name="expr">expression</param>
        /// <param name="require">if exists the expression with the same key (false - the expression will not be use)
        /// (true - the expression will be use). The order plays role</param>
        /// <param name="key">key from query string</param>
        public ConditionalFilterGenerator<TItem> And<TConst>(Expression<Func<TItem, TConst, bool>> expr, bool require, params string[] key)
        {
            AddExpression(expr, expr.Parameters, require, key, BinaryExpressionType.And);
            return this;
        }
        public ConditionalFilterGenerator<TItem> And<TConst, TConst1>(Expression<Func<TItem, TConst, TConst1, bool>> expr,
            bool require, params string[] key)
        {
            AddExpression(expr, expr.Parameters, require, key, BinaryExpressionType.And);
            return this;

        }
        public ConditionalFilterGenerator<TItem> And<TConst>(Expression<Func<TItem, TConst, bool>> expr, params string[] key)
        {
            AddExpression(expr, expr.Parameters, true, key, BinaryExpressionType.And);
            return this;
        }
        public ConditionalFilterGenerator<TItem> And<TConst, TConst1>(Expression<Func<TItem, TConst, TConst1, bool>> expr,
            params string[] key)
        {
            AddExpression(expr, expr.Parameters, true, key, BinaryExpressionType.And);
            return this;
        }
        public ConditionalFilterGenerator<TItem> Or<TConst>(Expression<Func<TItem, TConst, bool>> expr, bool require, params string[] key)
        {
            AddExpression(expr, expr.Parameters, require, key, BinaryExpressionType.Or);
            return this;
        }
        public ConditionalFilterGenerator<TItem> Or<TConst, TConst1>(Expression<Func<TItem, TConst, TConst1, bool>> expr,
            bool require, params string[] key)
        {
            AddExpression(expr, expr.Parameters, require, key, BinaryExpressionType.Or);
            return this;
        }
        public ConditionalFilterGenerator<TItem> Or<TConst>(Expression<Func<TItem, TConst, bool>> expr, params string[] key)
        {
            AddExpression(expr, expr.Parameters, true, key, BinaryExpressionType.Or);
            return this;
        }
        public ConditionalFilterGenerator<TItem> Or<TConst, TConst1>(Expression<Func<TItem, TConst, TConst1, bool>> expr,
            params string[] key)
        {
            AddExpression(expr, expr.Parameters, true, key, BinaryExpressionType.Or);
            return this;
        }
        private void AddExpression(Expression expr, IEnumerable<ParameterExpression> param, bool require, string[] key,
            BinaryExpressionType type)
        {
            _expressions.Value.Add(new ContainerExpression(expr, param, key, type, require));
        }
        #endregion
        public Expression<Func<TItem, bool>> GetConditional()
        {
            try
            {
                return _builder.ToBuild<TItem>(_expressions.Value, KeyValue);
            }
            catch (Exception e)
            {
                return t => false;
            }
        }



    }
}
