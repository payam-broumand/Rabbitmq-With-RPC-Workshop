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

		public TEntity Create(TEntity entity)
		{
			_dbSet.Add(entity);

			_dbContext.SaveChanges();

			return entity;
		}

		public TEntity? Delete(int id)
		{
			TEntity? entityToDelete = GetById(id);
			if (entityToDelete == null) return null; 
			 
			_dbSet.Remove(entityToDelete);
			_dbContext.SaveChanges();

			return entityToDelete;
		}

		public TEntity? Edit(int id, TEntity model)
		{
			TEntity? entity = GetById(id);
			if (entity is null) return null;

			_dbSet.Entry(entity).State = EntityState.Detached;

			_dbSet.Update(model);
			_dbContext.SaveChanges();

			return model;
		}

		public IReadOnlyList<TEntity> GetAll() => _dbSet.ToList();

		public TEntity? GetById(int id) => _dbSet.Find(id);

		public int CountEntites() => _dbSet.Count();
	}
}
