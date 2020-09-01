using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository.Core;
using OpenAuth.Repository.Interface;
using Z.EntityFramework.Plus;

namespace OpenAuth.Repository
{
    public class BaseRepository<T> : IRepository<T> where T : Entity
    {
        private DbContext _context;

        //private DBContextFactory _factory;
        public BaseRepository(DBContextFactory factory)
        {
            _context = factory.GetDbContext<T>();
        }


        /// <summary>
        /// 根据过滤条件，获取记录
        /// </summary>
        /// <param name="exp">The exp.</param>
        public IQueryable<T> Find(Expression<Func<T, bool>> exp = null)
        {
            return Filter(exp);
        }

        public bool IsExist(Expression<Func<T, bool>> exp)
        {
            return _context.Set<T>().Any(exp);
        }

        /// <summary>
        /// 查找单个，且不被上下文所跟踪
        /// </summary>
        public T FindSingle(Expression<Func<T, bool>> exp)
        {
            return _context.Set<T>().AsNoTracking().FirstOrDefault(exp);
        }

        /// <summary>
        /// 得到分页记录
        /// </summary>
        /// <param name="pageindex">The pageindex.</param>
        /// <param name="pagesize">The pagesize.</param>
        /// <param name="orderby">排序，格式如："Id"/"Id descending"</param>
        public IQueryable<T> Find(int pageindex, int pagesize, string orderby = "", Expression<Func<T, bool>> exp = null)
        {
            if (pageindex < 1) pageindex = 1;
            if (string.IsNullOrEmpty(orderby))
                orderby = "Id descending";

            return Filter(exp).OrderBy(orderby).Skip(pagesize * (pageindex - 1)).Take(pagesize);
        }

        /// <summary>
        /// 根据过滤条件获取记录数
        /// </summary>
        public int GetCount(Expression<Func<T, bool>> exp = null)
        {
            return Filter(exp).Count();
        }

        public T Add(T entity)
        {
            if (string.IsNullOrEmpty(entity.Id))
            {
                entity.Id = Guid.NewGuid().ToString();
            }
            var o = _context.Set<T>().Add(entity);
            Save();
            _context.Entry(entity).State = EntityState.Detached;
            return o.Entity;
        }

        /// <summary>
        /// 批量添加
        /// </summary>
        /// <param name="entities">The entities.</param>
        public void BatchAdd(T[] entities)
        {
            foreach (var entity in entities)
            {
                entity.Id = Guid.NewGuid().ToString();
            }
            _context.Set<T>().AddRange(entities);
            Save();
        }

        public void Update(T entity)
        {
            var entry = this._context.Entry(entity);
            entry.State = EntityState.Modified;

            //如果数据没有发生变化
            if (!this._context.ChangeTracker.HasChanges())
            {
                return;
            }

            Save();
        }

        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
            Save();
        }


        /// <summary>
        /// 实现按需要只更新部分更新
        /// <para>如：Update(u =>u.Id==1,u =>new User{Name="ok"});</para>
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="entity">The entity.</param>
        public void Update(Expression<Func<T, bool>> where, Expression<Func<T, T>> entity)
        {
            _context.Set<T>().Where(where).Update(entity);
        }

        public virtual void Delete(Expression<Func<T, bool>> exp)
        {
            _context.Set<T>().Where(exp).Delete();
        }

        public void Save()
        {
            try
            {
                var entities = _context.ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added
                                || e.State == EntityState.Modified)
                    .Select(e => e.Entity);

                foreach (var entity in entities)
                {
                    var validationContext = new ValidationContext(entity);
                    Validator.ValidateObject(entity, validationContext, validateAllProperties: true);
                }

                _context.SaveChanges();
            }
            catch (ValidationException exc)
            {
                Console.WriteLine($"{nameof(Save)} validation exception: {exc?.Message}");
                throw (exc.InnerException as Exception ?? exc);
            }
            catch (Exception ex) //DbUpdateException 
            {
                throw (ex.InnerException as Exception ?? ex);
            }
        }

        private IQueryable<T> Filter(Expression<Func<T, bool>> exp)
        {
            var dbSet = _context.Set<T>().AsNoTracking().AsQueryable();
            if (exp != null)
                dbSet = dbSet.Where(exp);
            return dbSet;
        }

        public int ExecuteSql(string sql)
        {
            return _context.Database.ExecuteSqlRaw(sql);
        }

        public IQueryable<T> FromSql(string sql, params object[] parameters)
        {
            return _context.Set<T>().FromSqlRaw(sql, parameters);
        }

        public IQueryable<T> Query(string sql, params object[] parameters)
        {
            return _context.Query<T>().FromSqlRaw(sql, parameters);
        }

        public async Task<T> FindSingleAsync(Expression<Func<T, bool>> exp = null, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().AsNoTracking().FirstOrDefaultAsync(exp, cancellationToken);
        }

        public async Task<bool> IsExistAsync(Expression<Func<T, bool>> exp, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().AnyAsync(exp, cancellationToken);
        }

        public async Task<int> GetCountAsync(Expression<Func<T, bool>> exp = null, CancellationToken cancellationToken = default)
        {
            return await Filter(exp).CountAsync(cancellationToken);
        }

        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(entity.Id))
            {
                entity.Id = Guid.NewGuid().ToString();
            }
            var o = await _context.Set<T>().AddAsync(entity, cancellationToken);
            await SaveAsync(cancellationToken);
            _context.Entry(entity).State = EntityState.Detached;
            return o.Entity;
        }

        public async Task BatchAddAsync(T[] entities, CancellationToken cancellationToken = default)
        {
            foreach (var entity in entities)
            {
                entity.Id = Guid.NewGuid().ToString();
            }
            await _context.Set<T>().AddRangeAsync(entities, cancellationToken);
            await SaveAsync(cancellationToken);
        }

        public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            var entry = this._context.Entry(entity);
            entry.State = EntityState.Modified;

            //如果数据没有发生变化
            if (!this._context.ChangeTracker.HasChanges())
            {
                return;
            }

            await SaveAsync(cancellationToken);
        }

        public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            _context.Set<T>().Remove(entity);
            await SaveAsync(cancellationToken);
        }

        public async Task UpdateAsync(Expression<Func<T, bool>> where, Expression<Func<T, T>> entity, CancellationToken cancellationToken = default)
        {
            await _context.Set<T>().Where(where).UpdateAsync(entity, cancellationToken);
        }

        public async Task DeleteAsync(Expression<Func<T, bool>> exp, CancellationToken cancellationToken = default)
        {
            await _context.Set<T>().Where(exp).DeleteAsync(cancellationToken);
        }

        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var entities = _context.ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added
                                || e.State == EntityState.Modified)
                    .Select(e => e.Entity);

                foreach (var entity in entities)
                {
                    var validationContext = new ValidationContext(entity);
                    Validator.ValidateObject(entity, validationContext, validateAllProperties: true);
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (ValidationException exc)
            {
                Console.WriteLine($"{nameof(SaveAsync)} validation exception: {exc?.Message}");
                throw (exc.InnerException as Exception ?? exc);
            }
            catch (Exception ex) //DbUpdateException 
            {
                throw (ex.InnerException as Exception ?? ex);
            }
        }

        public async Task<int> ExecuteSqlAsync(string sql, CancellationToken cancellationToken = default)
        {
            return await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }
    }
}
