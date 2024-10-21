namespace HNOne.API.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> All();
        Task<T> GetById(string id);
        Task<bool> Add(T entity);
        Task<bool> Delete(string id);
        Task<bool> Upsert(T entity);
    }
}
