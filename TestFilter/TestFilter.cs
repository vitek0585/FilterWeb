using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using FilterConditional;
using FilterConditional.Builder;
using FilterConditional.Container;
using FilterConditional.TypeExpression;
using Filters.Abstract;
using TestFilter.Entity;
using Xunit;

namespace TestFilter
{
    public class TestFilter
    {
        private ConditionalFilterGenerator<Employee> _fb;
        private List<Employee> _list;

        private NameValueCollection _collec = new NameValueCollection()
        {
            {"id", 1.ToString()},
            {"name", "vit"}

        };
        public TestFilter()
        {
            _fb = new ConditionalFilterGenerator<Employee>(_collec, new ConditionalFilterBuilder());
            _list = new List<Employee>()
            {
                new Employee(){Id = 1,Name = "vitek"},
                new Employee(){Id = 2,Name = "inna"},
                new Employee(){Id = 3,Name = "kristi"},
                new Employee(){Id = 4,Name = "igor"}

            };
        }
        [Fact]
        public void TestConditionalAnd()
        {
            _fb.And<int>((e, i) => e.Id == i, "id").And<string>((e, i) => e.Name.StartsWith(i), "name");
            var cond = _fb.GetConditional();
            var elems = _list.Where(cond.Compile());
            Assert.Equal(1, elems.Count());
            Assert.Equal(_collec.Get(0), elems.First().Id.ToString());
        }
        [Fact]
        public void TestConditionalOr()
        {
            _fb.Or<int>((e, i) => e.Id == i, "id");
            var cond = _fb.GetConditional();
            var elems = _list.Where(cond.Compile());
            Assert.Equal(1, elems.Count());
        }
        [Fact]
        public void TestConditionalTwo()
        {
            _fb.And<int>((e, i) => e.Id > i, "id")
                .And<int>((e, i) => e.Id < 4, "Id")
                .Or<string>((e, s) => e.Name.StartsWith(s), "name");
            var cond = _fb.GetConditional();
            var elems = _list.Where(cond.Compile());
            Assert.Equal(3, elems.Count());
            Assert.Contains(elems, (e) => e.Id == 1);
        }

        [Fact]
        public void TestConditionalContainsInArray()
        {
            _fb.And<int, IEnumerable<string>>((e, i, a) => e.Id == i && a.Any(s => e.Name.StartsWith(s)), "id", "4name")
                .Or<int>((e, i) => e.Id == 5, false, "id");
            var cond = _fb.GetConditional();

            var elems = _list.Where(cond.Compile());
            Assert.Empty(elems);

        }
        [Fact]
        public void TestConditionalNotContainsKey()
        {

            var cond = _fb.GetConditional();

            var elems = _list.Where(cond.Compile());
            Assert.Equal(4, elems.Count());

            _fb.And<int, IEnumerable<string>>((e, i, a) => e.Id == i && a.Any(s => e.Name.StartsWith(s)))
                .Or<int>((e, i) => e.Id == 5, false, "i2d");

            elems = _list.Where(cond.Compile());
            Assert.Equal(4, elems.Count());

        }
        [Fact]
        public void TestConditionalIfRequire()
        {

            _fb.And<int, IEnumerable<string>>((e, i, a) => e.Id == i && a.Any(s => e.Name.StartsWith(s)), "id", "name")
                .Or<int>((e, i) => e.Id > i, true, "id");
            var cond = _fb.GetConditional();

            var elems = _list.Where(cond.Compile());
            Assert.Equal(4, elems.Count());

        }
        [Fact]
        public void TestConditionalIfOptional()
        {

            _fb.And<int, IEnumerable<string>>((e, i, a) => e.Id == i && a.Any(s => e.Name.StartsWith(s)), "id", "name")
                .Or<int>((e, i) => e.Id > i, false, "id");
            var cond = _fb.GetConditional();

            var elems = _list.Where(cond.Compile());
            Assert.Equal(1, elems.Count());

        }
        [Fact]
        public void TestConditionalParamFail()
        {
            NameValueCollection collec = new NameValueCollection()
            {
                {"id", "sdsd"},
                {"name", "vit"}
            };
            _fb = new ConditionalFilterGenerator<Employee>(collec, new ConditionalFilterBuilder());

            _fb.And<int, IEnumerable<string>>((e, i, a) => e.Id == i && a.Any(s => e.Name.StartsWith(s)), "id", "name")
                .Or<string>((e, i) => e.Name.StartsWith(i), true, "name");
            var cond = _fb.GetConditional();

            var elems = _list.Where(cond.Compile());
            Assert.Equal(1, elems.Count());

        }
        [Fact]
        public void ConditionalTrue()
        {
            Expression<Func<Employee, int, bool>> exp = (e, i) => i > 0;
            var list = new List<ContainerExpression>()
            {
                new ContainerExpression(exp,exp.Parameters,Enumerable.Empty<string>(),BinaryExpressionType.And)
                
            };
            var cond = new ConditionalFilterBuilder().ToBuild<Employee>(list, _collec);
            var elems = _list.Where(cond.Compile());
            Assert.Equal(elems.Count(), _list.Count);
        }
    }
}
