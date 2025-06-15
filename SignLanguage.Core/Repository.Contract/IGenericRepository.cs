using SignLanguage.Core.Entities;
using SignLanguage.Core.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignLanguage.Core.Repository.Contract
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T?> GetAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();

        Task<T?> GetWithSpecAsync(ISpecifications<T> spec);

        Task<IEnumerable<T>> GetAllWithSpecAsync(ISpecifications<T> spec);
    }
}
