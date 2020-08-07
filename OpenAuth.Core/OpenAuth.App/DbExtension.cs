﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Repository;

namespace OpenAuth.App
{
    public class DbExtension
    {
        private List<DbContext> _dbContexts;

        public DbExtension(IServiceProvider serviceProvider)
        {
            _dbContexts = new List<DbContext>();
            var types = Common.ContextDir.Select(d => d.Value).Distinct().ToList();
            foreach (var type in types)
            {
                _dbContexts.Add((DbContext)serviceProvider.CreateScope().ServiceProvider.GetRequiredService(type));
            }
        }

        /// <summary>
        /// 获取数据库一个表的所有属性值及属性描述
        /// </summary>
        /// <param name="moduleName">模块名称/表名</param>
        /// <returns></returns>
        public List<KeyDescription> GetProperties(string moduleName)
        {
            var result = new List<KeyDescription>();
            const string domain = "openauth.repository.domain.";
            IEntityType entity = null;
            foreach (var _context in _dbContexts)
            {
                entity = _context.Model.GetEntityTypes()
                    .FirstOrDefault(u => u.Name.Equals(domain + moduleName, StringComparison.OrdinalIgnoreCase));
                if (!(entity is null))
                    break;
            }
            // var entity = _context.Model.GetEntityTypes().FirstOrDefault(u => u.Name.Contains(moduleName, StringComparison.OrdinalIgnoreCase));
            if (entity == null)
            {
                throw new Exception($"未能找到{moduleName}对应的实体类");
            }

            foreach (var property in entity.ClrType.GetProperties())
            {
                object[] objs = property.GetCustomAttributes(typeof(DescriptionAttribute), true);
                object[] browsableObjs = property.GetCustomAttributes(typeof(BrowsableAttribute), true);
                var description = objs.Length > 0 ? ((DescriptionAttribute) objs[0]).Description : property.Name;
                if (string.IsNullOrEmpty(description)) description = property.Name;
                //如果没有BrowsableAttribute或 [Browsable(true)]表示可见，其他均为不可见，需要前端配合显示
                bool browsable = browsableObjs == null || browsableObjs.Length == 0 ||
                                 ((BrowsableAttribute) browsableObjs[0]).Browsable;
                var typeName = property.PropertyType.Name;
                if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                {
                    typeName = Nullable.GetUnderlyingType(property.PropertyType).Name;
                }
                result.Add(new KeyDescription
                {
                    Key = property.Name,
                    Description = description,
                    Browsable = browsable,
                    Type = typeName
                });
            }

            return result;
        }

        /// <summary>
        /// 获取数据库所有的表名
        /// </summary>
        public List<string> GetTableNames()
        {
            var names = new List<string>();
            foreach (var _context in _dbContexts)
            {

                var model = _context.Model;

                // Get all the entity types information contained in the DbContext class, ...
                var entityTypes = model.GetEntityTypes();
                foreach (var entityType in entityTypes)
                {
                    var tableNameAnnotation = entityType.GetAnnotation("Relational:TableName");
                    names.Add(tableNameAnnotation.Value.ToString());
                }
            }

            return names;
        }
    }
}