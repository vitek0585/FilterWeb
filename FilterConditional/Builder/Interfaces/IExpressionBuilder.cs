using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;

namespace FilterConditional.Builder.Interfaces
{
    public interface IExpressionBuilder<in TContainer,TResult> 
    {
        Expression<Func<TItem, TResult>> ToBuild<TItem>(IEnumerable<TContainer> expressions,
            NameValueCollection keyValue);
    }
}