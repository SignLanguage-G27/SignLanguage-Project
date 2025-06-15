using SignLanguage.Core.Entities;
using System.Linq.Expressions;

namespace SignLanguage.Core.Specifications
{
    public interface ISpecifications<T> where T : BaseEntity
    {
        public Expression<Func<T, bool>>? Criteria { get; set; }
        public List<Expression<Func<T, object>>> Includes { get; set; }
    }
}
