using HNOne.API.Repositories.Interfaces;

namespace HNOne.API.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        public Task<bool> Add(T entity)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> All()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(string id)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetById(string id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Upsert(T entity)
        {
            throw new NotImplementedException();
        }
    }
}
