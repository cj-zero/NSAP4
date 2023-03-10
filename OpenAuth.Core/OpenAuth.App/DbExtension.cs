using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAuth.Repository;
using OpenAuth.Repository.Domain;

namespace OpenAuth.App
{
    public class DbExtension
    {
        private List<DbContext> _dbContexts;

        private IOptions<AppSetting> _appConfiguration;
        public DbExtension(IServiceProvider serviceProvider, IOptions<AppSetting> appConfiguration)
        {
            _dbContexts = new List<DbContext>();
            var types = Infrastructure.Common.ContextDir.Select(d => d.Value).Distinct().ToList();
            foreach (var type in types)
            {
                _dbContexts.Add((DbContext)serviceProvider.CreateScope().ServiceProvider.GetRequiredService(type));
            }
            _appConfiguration = appConfiguration;
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
                //throw new Exception($"未能找到{moduleName}对应的实体类");
                return null;
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


        /// <summary>
        /// 获取数据库表结构信息
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public IList<SysTableColumn> GetDbTableStructure(string tableName)
        {
            if (_appConfiguration.Value.DbType == Define.DBTYPE_MYSQL)
            {
                return GetMySqlStructure(tableName);
            }
            else
            {
                return GetSqlServerStructure(tableName);
            }
        }

        /// <summary>
        /// 获取Mysql表结构信息
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private IList<SysTableColumn> GetMySqlStructure(string tableName)
        {
            var sql = $@"SELECT  DISTINCT
                    Column_Name AS ColumnName,
                     '{ tableName}'  as tableName,
	                Column_Comment AS Comment,
                    data_type as ColumnType,
                        CASE
                          WHEN data_type IN( 'BIT', 'BOOL', 'bit', 'bool') THEN
                'bool'
		             WHEN data_type in('smallint','SMALLINT') THEN 'short'
								WHEN data_type in('tinyint','TINYINT') THEN 'bool'
                        WHEN data_type IN('MEDIUMINT','mediumint', 'int','INT','year', 'Year') THEN
                    'int'
                    WHEN data_type in ( 'BIGINT','bigint') THEN
                    'bigint'
                    WHEN data_type IN('FLOAT', 'DOUBLE', 'DECIMAL','float', 'double', 'decimal') THEN
                    'decimal'
                    WHEN data_type IN('CHAR', 'VARCHAR', 'TINY TEXT', 'TEXT', 'MEDIUMTEXT', 'LONGTEXT', 'TINYBLOB', 'BLOB', 'MEDIUMBLOB', 'LONGBLOB', 'Time','char', 'varchar', 'tiny text', 'text', 'mediumtext', 'longtext', 'tinyblob', 'blob', 'mediumblob', 'longblob', 'time') THEN
                    'string'
                    WHEN data_type IN('Date', 'DateTime', 'TimeStamp','date', 'datetime', 'timestamp') THEN
                    'DateTime' ELSE 'string'
                END AS EntityType,
	              case WHEN CHARACTER_MAXIMUM_LENGTH>8000 THEN 0 ELSE CHARACTER_MAXIMUM_LENGTH end  AS Maxlength,
            CASE
                    WHEN COLUMN_KEY <> '' THEN  
                    1 ELSE 0
                END AS IsKey,
            CASE
                    WHEN Column_Name IN( 'CreateID', 'ModifyID', '' ) 
		            OR COLUMN_KEY<> '' THEN
                        0 ELSE 1
                        END AS IsDisplay,
		            1 AS IsColumnData,
                    120 AS ColumnWidth,
                    0 AS OrderNo,
                CASE
                        WHEN IS_NULLABLE = 'NO' THEN
                        0 ELSE 1
                    END AS IsNull,
	            CASE
                        WHEN COLUMN_KEY <> '' THEN
                        1 ELSE 0
                    END AS IsReadDataset
                FROM
                    information_schema.COLUMNS
                WHERE
                    table_name = '{tableName}'"; // {GetMysqlTableSchema()}
            
            foreach (var context in _dbContexts)
            {
                var columns = context.Query<SysTableColumn>().FromSqlRaw(sql).ToList();
                if(columns.Count != 0)
                    return columns;
            }
            return new List<SysTableColumn>();
        }

        /// <summary>
        /// 获取mysql当前的数据库名称
        /// </summary>
        /// <returns></returns>
        private string GetMysqlTableSchema()
        {
            //try
            //{
            //    string dbName = _context.Database.GetDbConnection().ConnectionString.Split("Database=")[1].Split(";")[0]?.Trim();
            //    if (!string.IsNullOrEmpty(dbName))
            //    {
            //        dbName = $" and table_schema = '{dbName}' ";
            //    }
            //    return dbName;
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"获取mysql数据库名异常:{ex.Message}");
            //    return "";
            //}
            throw new NotImplementedException();
        }


        /// <summary>
        /// 获取SqlServer表结构信息
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private IList<SysTableColumn> GetSqlServerStructure(string tableName)
        {
            var sql = $@"
            SELECT TableName,
                LTRIM(RTRIM(ColumnName)) AS ColumnName,
                Comment,
                CASE WHEN ColumnType = 'uniqueidentifier' THEN 'guid'
                     WHEN ColumnType IN('smallint', 'INT') THEN 'int'
                     WHEN ColumnType = 'BIGINT' THEN 'long'
                     WHEN ColumnType IN('CHAR', 'VARCHAR', 'NVARCHAR',
                                          'text', 'xml', 'varbinary', 'image')
                     THEN 'string'
                     WHEN ColumnType IN('tinyint')
                     THEN 'byte'
                     WHEN ColumnType IN('bit')
                     THEN 'bool'

                     WHEN ColumnType IN('bit') THEN 'bool'
                     WHEN ColumnType IN('time', 'date', 'DATETIME', 'smallDATETIME')
                     THEN 'DateTime'
                     WHEN ColumnType IN('smallmoney', 'DECIMAL', 'numeric',
                                          'money') THEN 'decimal'
                     WHEN ColumnType = 'float' THEN 'float'
                     ELSE 'string '
                END as  EntityType,
                    ColumnType,
                    [Maxlength],
                IsKey,
                CASE WHEN ColumnName IN('CreateID', 'ModifyID', '')
                          OR IsKey = 1 THEN 0
                     ELSE 1
                END AS IsDisplay ,
				1 AS IsColumnData,

              CASE   
                     WHEN ColumnName IN('Modifier', 'Creator') THEN 130
                     WHEN[Maxlength] < 110 AND[Maxlength] > 60 THEN 120
                     WHEN[Maxlength] < 200 AND[Maxlength] >= 110 THEN 180
                     WHEN[Maxlength] > 200 THEN 220
                     ELSE 90
                   END AS ColumnWidth ,
                0 AS OrderNo,
                --CASE WHEN IsKey = 1 OR t.[IsNull]=0 THEN 0
                --     ELSE 1 END
                t.[IsNull] AS
                 [IsNull],
            CASE WHEN IsKey = 1 THEN 1 ELSE 0 END IsReadDataset,
            CASE WHEN IsKey!=1 AND t.[IsNull] = 0 THEN 0 ELSE NULL END AS EditColNo
        FROM    (SELECT obj.name AS TableName ,
                            col.name AS ColumnName ,
                            CONVERT(NVARCHAR(100),ISNULL(ep.[value], '')) AS Comment,
                            t.name AS ColumnType ,
                           CASE WHEN  col.length<1 THEN 0 ELSE  col.length END  AS[Maxlength],
                            CASE WHEN EXISTS (SELECT   1
                                               FROM dbo.sysindexes si
                                                        INNER JOIN dbo.sysindexkeys sik ON si.id = sik.id
                                                              AND si.indid = sik.indid
                                                        INNER JOIN dbo.syscolumns sc ON sc.id = sik.id
                                                              AND sc.colid = sik.colid
                                                        INNER JOIN dbo.sysobjects so ON so.name = si.name
                                                              AND so.xtype = 'PK'
                                               WHERE sc.id = col.id
                                                        AND sc.colid = col.colid)
                                 THEN 1
                                 ELSE 0
                            END AS IsKey ,
                            CASE WHEN col.isnullable = 1 THEN 1
                                 ELSE 0
                            END AS[IsNull],
                            col.colorder
                  FROM      dbo.syscolumns col
                            LEFT JOIN dbo.systypes t ON col.xtype = t.xusertype
                           INNER JOIN dbo.sysobjects obj ON col.id = obj.id

                                                            AND obj.xtype IN ( 'U','V')
                                                          --   AND obj.status >= 01
                            LEFT JOIN dbo.syscomments comm ON col.cdefault = comm.id
                            LEFT JOIN sys.extended_properties ep ON col.id = ep.major_id
                                                              AND col.colid = ep.minor_id
                                                              AND ep.name = 'MS_Description'
                            LEFT JOIN sys.extended_properties epTwo ON obj.id = epTwo.major_id
                                                              AND epTwo.minor_id = 0
                                                              AND epTwo.name = 'MS_Description'
                  WHERE obj.name =  '{ tableName}') AS t
            ORDER BY t.colorder";

            var context = _dbContexts.FirstOrDefault(d => d.GetType() == typeof(OpenAuthDBContext));
            var columns = context.Query<SysTableColumn>().FromSqlRaw(sql);
            return columns.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="D"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="contextType"></param>
        /// <returns></returns>
        public IEnumerable<T> GetObjectDataFromSQL<T>(string sql, object[] parameters, Type dbContextType) where T : class
        {
            var context = _dbContexts.FirstOrDefault(d => d.GetType() == dbContextType);
            var data = context?.Query<T>().FromSqlRaw(sql, parameters);
            return data;
        }
    }
}