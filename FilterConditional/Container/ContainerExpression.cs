using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FilterConditional.TypeExpression;

namespace FilterConditional.Container
{
    public class ContainerExpression
    {
        internal Expression Expression { get; set; }
        internal IEnumerable<string> Keys { get;  set; }
        internal Dictionary<string, Type> KeyType { get; set; }
        internal bool IsRequire { get; set; }
        internal BinaryExpressionType ExpType { get;  set; }
        public ContainerExpression(Expression expression, IEnumerable<ParameterExpression> type,
            IEnumerable<string> keys, BinaryExpressionType tExp, bool require = true)

        {
            ExpType = tExp;
            IsRequire = require;
            Expression = expression;
            Keys = keys;
            var types = type.Select(t => t.Type).Skip(1);
            KeyType = types.Zip(keys, (t, k) => new { t, k }).ToDictionary(a => a.k, a => a.t);
        }

        
       
    }
}