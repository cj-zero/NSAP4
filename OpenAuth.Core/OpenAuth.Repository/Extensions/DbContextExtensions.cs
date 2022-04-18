extern alias MySqlConnectorAlias;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OpenAuth.Repository.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class EntityFrameworkCoreExtensions
    {
        private static DbCommand CreateCommand(DatabaseFacade facade, string sql, out DbConnection connection, CommandType commandType, params object[] parameters)
        {
            var conn = facade.GetDbConnection();
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                var cmd = conn.CreateCommand();
                if (parameters != null)
                {
                    if (facade.IsSqlServer())
                    {
                        foreach (List<SqlParameter> itemp in parameters.ToArray())
                        {
                            cmd.Parameters.AddRange(itemp.ToArray());
                        }
                    }
                    else
                    {
                        foreach (List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> itemp in parameters)
                        {
                            cmd.Parameters.AddRange(itemp.ToArray());
                        }
                    }
                }
                cmd.CommandType = commandType;
                cmd.CommandText = sql;
                connection = conn;
                return cmd;
            }
            catch (Exception ex)
            {
                conn.Close();
                throw ex;
            }
        }

        public static DataTable SqlQuery(this DatabaseFacade facade, string sql, CommandType commandType, params object[] parameters)
        {
            var dt = new DataTable();
            var command = CreateCommand(facade, sql, out DbConnection conn, commandType, parameters);
            try
            {
                var reader = command.ExecuteReader();
                dt.Load(reader);
                reader.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                conn.Close();
                throw ex;
            }
            return dt;
        }

        public static List<T> SqlQuery<T>(this DatabaseFacade facade, string sql, CommandType commandType, params object[] parameters) where T : class, new()
        {
            var dt = SqlQuery(facade, sql, commandType, parameters);
            return dt.Tolist<T>();
        }
        public static DataTable SqlQueryDataTable(this DatabaseFacade facade, string sql, CommandType commandType, params object[] parameters)
        {
            var dt = SqlQuery(facade, sql, commandType, parameters);
            return dt;
        }
        public static List<T> Tolist<T>(this DataTable dt) where T : class, new()
        {
            if (dt == null)
                return null;

            DataTable p_Data = dt;
            // 返回值初始化
            List<T> result = new List<T>();
            for (int j = 0; j < p_Data.Rows.Count; j++)
            {
                T _t = (T)Activator.CreateInstance(typeof(T));
                PropertyInfo[] propertys = _t.GetType().GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    for (int i = 0; i < p_Data.Columns.Count; i++)
                    {
                        // 属性与字段名称一致的进行赋值
                        if (pi.Name.Equals(p_Data.Columns[i].ColumnName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            try
                            {
                                // 数据库NULL值单独处理
                                if (p_Data.Rows[j][i] != DBNull.Value)
                                {
                                    if (pi.PropertyType.FullName == "System.Boolean")
                                    {
                                        var val = p_Data.Rows[j][i].ToString() == "1" ? true : false;
                                        pi.SetValue(_t, val, null);
                                    }
                                    else if (pi.PropertyType == typeof(Int32))
                                    {
                                        pi.SetValue(_t, Convert.ToInt32(p_Data.Rows[j][i]), null);
                                    }
                                    else if (pi.PropertyType == typeof(Int64))
                                    {
                                        pi.SetValue(_t, Convert.ToInt64(p_Data.Rows[j][i]), null);
                                    }
                                    else if (pi.PropertyType == typeof(decimal))
                                    {
                                        pi.SetValue(_t, Convert.ToDecimal(p_Data.Rows[j][i]), null);
                                    }
                                    else
                                    {
                                        pi.SetValue(_t, p_Data.Rows[j][i], null);
                                    }

                                }
                                else
                                {
                                    pi.SetValue(_t, null, null);
                                }

                            }
                            catch (Exception ex)
                            {
                                string msg = ex.Message;
                            }
                            break;
                        }
                    }
                }
                result.Add(_t);
            }
            return result;
        }
        /// <summary>
        /// 执行仓储过程
        /// </summary>
        /// <param name="facade"></param>
        /// <param name="sql"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object ExecuteScalar(this DatabaseFacade facade, string sql, CommandType commandType, params object[] parameters)
        {
            var command = CreateCommand(facade, sql, out DbConnection conn, commandType, parameters);
            try
            {
                if (command.Connection.State != ConnectionState.Open)
                {
                    command.Connection.Open();
                }
                object result = command.ExecuteScalar();
                conn.Close();
                return result;
            }
            catch (Exception ex)
            {
                conn.Close();
                string msg = ex.Message;
                throw ex;
            }
        }
        public static int ExecuteNonQuery(this DatabaseFacade facade, CommandType commandType, string sql, params object[] parameters)
        {
            var command = CreateCommand(facade, sql, out DbConnection conn, commandType, parameters);
            try
            {
                if (command.Connection.State != ConnectionState.Open)
                {
                    command.Connection.Open();
                }
                int result = command.ExecuteNonQuery();
                conn.Close();
                return result;
            }
            catch (Exception ex)
            {
                conn.Close();
                string msg = ex.Message;
                throw ex;
            }
        }
    }
}
