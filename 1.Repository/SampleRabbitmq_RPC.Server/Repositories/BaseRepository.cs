using Microsoft.EntityFrameworkCore;
using SampleRabbitmq_RPC.Repository.Contracts;
using SampleRabbitmq_RPC.Repository.Model;

namespace SampleRabbitmq_RPC.Repository.Repositories
{
	public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity>
		where TEntity : BaseEntity
	{
		protected readonly AcademyDbContext _dbContext;
		protected readonly DbSet<TEntity> _dbSet;

		protected BaseRepository(AcademyDbContext dbContext)
		{
			_dbContext = dbContext;
			_dbSet = _dbContext.Set<TEntity>();

			_dbContext.Database.EnsureCreated();
		}

		public void Delete(int id)
		{
			TEntity? entity = GetById(id);
			if (entity is null) return;

			_dbSet.Remove(entity);
			_dbContext.SaveChanges();
		}

		public TEntity? Edit(int id, TEntity model)
		{
			TEntity? entity = GetById(id);
			if (entity is null) return null;

			_dbSet.Update(entity);
			_dbContext.SaveChanges();

			return entity;
		}

		public IReadOnlyList<TEntity> GetAll() => _dbSet.ToList();

		public TEntity? GetById(int id) => _dbSet.Find(id);
	}
}
