using Microsoft.EntityFrameworkCore;
using SignLanguage.Core.Entities;
using SignLanguage.Core.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SignLanguage.Infrastracture
{
    public static class SpecificationsEvaluator<TEntity> where TEntity : BaseEntity
    {
        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecifications<TEntity> spec)
        {
            var query = inputQuery;

            if (spec.Criteria is not null)
                query=query.Where(spec.Criteria);

            query=spec.Includes.Aggregate(query, (currentQuery, includesExpression) => currentQuery.Include(includesExpression));

            return query;
        }
    }
}
