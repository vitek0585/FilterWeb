using System;
using System.Collections.Specialized;
using System.Linq.Expressions;

namespace Filters.Abstract
{
    public abstract class FilterBase<TItem, TResult>
    {
        protected NameValueCollection KeyValue;

        protected FilterBase(NameValueCollection keyValue)
        {
            KeyValue = keyValue;
        }
        public abstract Expression<Func<TItem, TResult>> GetConditional();
    }
}
