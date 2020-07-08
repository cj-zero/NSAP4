using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenAuth.Repository.Core;
using OpenAuth.Repository.Interface;
using Z.EntityFramework.Plus;

namespace OpenAuth.Repository
{
    public class UnitWork : IUnitWork
    {

        private readonly IDictionary<Type, Type> ContextTypes = new Dictionary<Type, Type>();

        private readonly IDictionary<Type,DbContext> DbContexts = new Dictionary<Type, DbContext>();

        private readonly IServiceProvider _serviceProvider;

        public UnitWork(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private DbContext GetDbContext<T>()
        {
            if(ContextTypes.TryGetValue(typeof(T), out Type contextType))
            {
                if (DbContexts.TryGetValue(contextType, out DbContext context))
                    return context;
            }
            else
            {
                if (Common.ContextDir.TryGetValue(typeof(T), out Type ctType))
                {
                    if (DbContexts.TryGetValue(ctType, out DbContext ct))
                        return ct;
                    ct = (DbContext)_serviceProvider.CreateScope().ServiceProvider.GetRequiredService(ctType);
                    ContextTypes.TryAdd(typeof(T), ctType);
                    DbContexts.TryAdd(ctType, ct);
                    return ct;
                }
            }
            throw new Exception("当前实体未包含在任一DbCotext.");
        }
        private DbContext GetDbContext(Type contextType)
        {
            if (DbContexts.TryGetValue(contextType, out DbContext context))
                return context;
            else
            {
                var ct = (DbContext)_serviceProvider.CreateScope().ServiceProvider.GetRequiredService(contextType);
                DbContexts.TryAdd(contextType, ct);
                return ct;
            }
        }

        /// <summary>
        /// 根据过滤条件，获取记录
        /// </summary>
        /// <param name="exp">The exp.</param>
        public IQueryable<T> Find<T>(Expression<Func<T, bool>> exp = null) where T : class
        {
            return Filter(exp);
        }

        public bool IsExist<T>(Expression<Func<T, bool>> exp) where T : class
        {
            return GetDbContext<T>().Set<T>().Any(exp);
        }

        /// <summary>
        /// 查找单个
        /// </summary>
        public T FindSingle<T>(Expression<Func<T, bool>> exp) where T : class
        {
            return GetDbContext<T>().Set<T>().AsNoTracking().FirstOrDefault(exp);
        }

        /// <summary>
        /// 得到分页记录
        /// </summary>
        /// <param name="pageindex">The pageindex.</param>
        /// <param name="pagesize">The pagesize.</param>
        /// <param name="orderby">排序，格式如："Id"/"Id descending"</param>
        public IQueryable<T> Find<T>(int pageindex, int pagesize, string orderby = "", Expression<Func<T, bool>> exp = null) where T : class
        {
            if (pageindex < 1) pageindex = 1;
            if (string.IsNullOrEmpty(orderby))
                orderby = "Id descending";

            return Filter(exp).OrderBy(orderby).Skip(pagesize * (pageindex - 1)).Take(pagesize);
        }

        /// <summary>
        /// 根据过滤条件获取记录数
        /// </summary>
        public int GetCount<T>(Expression<Func<T, bool>> exp = null) where T : class
        {
            return Filter(exp).Count();
        }

        public void Add<T>(T entity) where T : class
        {
            //if (string.IsNullOrEmpty(entity.Id))
            //{
            //    entity.Id = Guid.NewGuid().ToString();
            //}
            GetDbContext<T>().Set<T>().Add(entity);
        }

        /// <summary>
        /// 批量添加
        /// </summary>
        /// <param name="entities">The entities.</param>
        public void BatchAdd<T>(T[] entities) where T : class
        {
            //foreach (var entity in entities)
            //{
            //    entity.Id = Guid.NewGuid().ToString();
            //}
            GetDbContext<T>().Set<T>().AddRange(entities);
        }

        public void Update<T>(T entity) where T : class
        {
            var entry = GetDbContext<T>().Entry(entity);
            entry.State = EntityState.Modified;

            //如果数据没有发生变化
            if (!GetDbContext<T>().ChangeTracker.HasChanges())
            {
                entry.State = EntityState.Unchanged;
            }

        }

        public void Delete<T>(T entity) where T : class
        {
            GetDbContext<T>().Set<T>().Remove(entity);
        }

        /// <summary>
        /// 实现按需要只更新部分更新
        /// <para>如：Update(u =>u.Id==1,u =>new User{Name="ok"});</para>
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="entity">The entity.</param>
        public void Update<T>(Expression<Func<T, bool>> where, Expression<Func<T, T>> entity) where T : class
        {
            GetDbContext<T>().Set<T>().Where(where).Update(entity);
        }

        public virtual void Delete<T>(Expression<Func<T, bool>> exp) where T : class
        {
            GetDbContext<T>().Set<T>().RemoveRange(Filter(exp));
        }

        public void Save()
        {
            //try
            //{
            // _context.SaveChanges();
            //}
            //catch (DbEntityValidationException e)
            //{
            //    throw new Exception(e.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage);
            //}
            foreach (var context in DbContexts)
            {
                context.Value.SaveChanges();
            }
        }

        private IQueryable<T> Filter<T>(Expression<Func<T, bool>> exp) where T : class
        {
            var dbSet = GetDbContext<T>().Set<T>().AsNoTracking().AsQueryable();
            if (exp != null)
                dbSet = dbSet.Where(exp);
            return dbSet;
        }

        public int ExecuteSql(string sql, Type contextType)
        {
            return GetDbContext(contextType).Database.ExecuteSqlRaw(sql);
        }

        public IQueryable<T> FromSql<T>(string sql, params object[] parameters) where T : class
        {
            return GetDbContext<T>().Set<T>().FromSqlRaw(sql, parameters);
        }

        public IQueryable<T> Query<T>(string sql, params object[] parameters) where T : class
        {
            return GetDbContext<T>().Query<T>().FromSqlRaw(sql, parameters);
        }

        public async Task<T> FindSingleAsync<T>(Expression<Func<T, bool>> exp = null, CancellationToken cancellationToken = default) where T : class
        {
            return await GetDbContext<T>().Set<T>().AsNoTracking().FirstOrDefaultAsync(exp, cancellationToken);
        }

        public async Task<bool> IsExistAsync<T>(Expression<Func<T, bool>> exp, CancellationToken cancellationToken = default) where T : class
        {
            return await GetDbContext<T>().Set<T>().AnyAsync(exp, cancellationToken);
        }

        public async Task<int> GetCountAsync<T>(Expression<Func<T, bool>> exp = null, CancellationToken cancellationToken = default) where T : class
        {
            return await Filter(exp).CountAsync();
        }

        public async Task AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
        {
            //if (string.IsNullOrEmpty(entity.Id))
            //{
            //    entity.Id = Guid.NewGuid().ToString();
            //}
            await GetDbContext<T>().Set<T>().AddAsync(entity, cancellationToken);
        }

        public async Task BatchAddAsync<T>(T[] entities, CancellationToken cancellationToken = default) where T : class
        {
            //foreach (var entity in entities)
            //{
            //    entity.Id = Guid.NewGuid().ToString();
            //}
            await GetDbContext<T>().Set<T>().AddRangeAsync(entities, cancellationToken);
        }

        public Task UpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
        {
            Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
        {
            Delete(entity);
            return Task.CompletedTask;
        }

        public async Task UpdateAsync<T>(Expression<Func<T, bool>> where, Expression<Func<T, T>> entity, CancellationToken cancellationToken = default) where T : class
        {
            await GetDbContext<T>().Set<T>().Where(where).UpdateAsync(entity, cancellationToken);
        }

        public Task DeleteAsync<T>(Expression<Func<T, bool>> exp, CancellationToken cancellationToken = default) where T : class
        {
            Delete(exp);
            return Task.CompletedTask;
        }

        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            foreach (var context in DbContexts)
            {
                await context.Value.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<int> ExecuteSqlAsync(string sql, Type contextType, CancellationToken cancellationToken = default)
        {
            return await GetDbContext(contextType).Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }
    }
}
