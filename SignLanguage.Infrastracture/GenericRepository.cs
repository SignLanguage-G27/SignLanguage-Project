using Microsoft.EntityFrameworkCore;
using SignLanguage.Core.Entities;
using SignLanguage.Core.Repository.Contract;
using SignLanguage.Core.Service.Contract;
using SignLanguage.Core.Specifications;
using SignLanguage.Infrastracture.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignLanguage.Infrastracture
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly StoreContext _dbContext;
        private readonly IAttachmentService _attachment;

        public GenericRepository(StoreContext dbContext,IAttachmentService attachment)
        {
            _dbContext=dbContext;
            _attachment=attachment;
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbContext.Set<T>().ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllWithSpecAsync(ISpecifications<T> spec)
        {
           return await ApplySpecification(spec).ToListAsync();
        } 

        public async Task<T?> GetAsync(int id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task<T?> GetWithSpecAsync(ISpecifications<T> spec)
        {
            return await ApplySpecification(spec).FirstOrDefaultAsync();
        }

        private IQueryable<T> ApplySpecification(ISpecifications<T> spec)
        {
            return SpecificationsEvaluator<T>.GetQuery(_dbContext.Set<T>(), spec);
        }
    }
}
