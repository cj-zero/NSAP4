// ***********************************************************************
// Assembly         : OpenAuth.Domain
// Author           : yubaolee
// Created          : 04-29-2016
//
// Last Modified By : yubaolee
// Last Modified On : 04-29-2016
// Contact : Microsoft
// File: IUnitWork.cs
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Interface
{
    /// <summary>
    /// 工作单元接口
    /// <para> 适合在一下情况使用:</para>
    /// <para>1 在同一事务中进行多表操作</para>
    /// <para>2 需要多表联合查询</para>
    /// <para>因为架构采用的是EF访问数据库，暂时可以不用考虑采用传统Unit Work的注册机制</para>
    /// </summary>
    public interface IUnitWork
    {
        DbContext GetDbContext<T>() where T : class;
        T FindSingle<T>(Expression<Func<T, bool>> exp = null) where T : class;
        Task<T> FindSingleAsync<T>(Expression<Func<T, bool>> exp = null, CancellationToken cancellationToken = default) where T : class;
        bool IsExist<T>(Expression<Func<T, bool>> exp) where T : class;
        Task<bool> IsExistAsync<T>(Expression<Func<T, bool>> exp, CancellationToken cancellationToken = default) where T : class;
        IQueryable<T> Find<T>(Expression<Func<T, bool>> exp = null) where T : class;
        IQueryable<T> FindTrack<T>(Expression<Func<T, bool>> exp = null) where T : class;

        IQueryable<T> Find<T>(int pageindex = 1, int pagesize = 10, string orderby = "",
             Expression<Func<T, bool>> exp = null) where T : class;

        int GetCount<T>(Expression<Func<T, bool>> exp = null) where T : class;
        Task<int> GetCountAsync<T>(Expression<Func<T, bool>> exp = null, CancellationToken cancellationToken = default) where T : class;

        void Add<T>(T entity) where T : BaseEntity;
        T Add<T, TKey>(T entity) where T : class;
        Task<T> AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity;
        Task<T> AddAsync<T, Tkey>(T entity, CancellationToken cancellationToken = default) where T : class;

        void BatchAdd<T>(T[] entities) where T : BaseEntity;
        void BatchAdd<T, TKey>(T[] entities) where T : class;
        Task BatchAddAsync<T>(T[] entities, CancellationToken cancellationToken = default) where T : BaseEntity;
        Task BatchAddAsync<T, TKey>(T[] entities, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// 更新一个实体的所有属性
        /// </summary>
        void Update<T>(T entity) where T : class;
        Task UpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;

        void BatchUpdate<T>(T[] entity) where T : class;

        Task BatchUpdateAsync<T>(T[] entity, CancellationToken cancellationToken = default) where T : class;


        void Delete<T>(T entity) where T : class;
        Task DeleteAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;

        void BatchDelete<T>(T[] entity) where T : class;
        Task BatchDeleteAsync<T>(T[] entity, CancellationToken cancellationToken = default) where T : class;


        /// <summary>
        /// 实现按需要只更新部分更新
        /// <para>如：Update<T>(u =>u.Id==1,u =>new User{Name="ok"}) where T:class;</para>
        /// </summary>
        /// <param name="where">更新条件</param>
        /// <param name="entity">更新后的实体</param>
        void Update<T>(Expression<Func<T, bool>> where, Expression<Func<T, T>> entity) where T : class;
        Task UpdateAsync<T>(Expression<Func<T, bool>> where, Expression<Func<T, T>> entity, CancellationToken cancellationToken = default) where T : class;
        /// <summary>
        /// 批量删除
        /// </summary>
        void Delete<T>(Expression<Func<T, bool>> exp) where T : class;
        Task DeleteAsync<T>(Expression<Func<T, bool>> exp, CancellationToken cancellationToken = default) where T : class;

        void Save();
        Task SaveAsync(CancellationToken cancellationToken = default);

        int ExecuteSql(string sql, Type contextType);
        Task<int> ExecuteSqlAsync(string sql, Type contextType, CancellationToken cancellationToken = default);

        /// <summary>
        /// 使用SQL脚本查询
        /// </summary>
        /// <typeparam name="T"> T为数据库实体</typeparam>
        /// <returns></returns>
        IQueryable<T> FromSql<T>(string sql, params object[] parameters) where T : class;
        /// <summary>
        /// 使用SQL脚本查询
        /// </summary>
        /// <typeparam name="T"> T为非数据库实体，需要在DbContext中增加对应的DbQuery</typeparam>
        /// <returns></returns>
        IQueryable<T> Query<T>(string sql, params object[] parameters) where T : class;
        /// <summary>
        ///  执行sql语句不需要(建立实体)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="contextType"></param>
        /// <param name="sql"></param>
        /// <param name="commandType"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        List<T> ExcuteSql<T>(Type contextType, string sql, CommandType commandType, params object[] param) where T : class, new();
        /// <summary>
        ///  执行sql语句不需要(建立实体)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="contextType"></param>
        /// <param name="sql"></param>
        /// <param name="commandType"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        DataTable ExcuteSqlTable(Type contextType, string sql, CommandType commandType, params object[] param);
        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="contextType"></param>
        /// <param name="sql"></param>
        /// <param name="commandType"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        object ExecuteScalar(Type contextType, string sql, CommandType commandType, params object[] param);

        int ExecuteNonQuery(Type contextType, CommandType commandType, string sql, params object[] param);

    }
}
