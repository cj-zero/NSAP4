using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Infrastructure.Helpers
{
    /// <summary>
    /// List转成Tree
    /// <para>李玉宝新增于2016-10-09 19:54:07</para>
    /// </summary>
    public static class GenericHelpers
    {
        /// <summary>
        /// Generates tree of items from item list
        /// </summary>
        /// 
        /// <typeparam name="T">Type of item in collection</typeparam>
        /// <typeparam name="K">Type of parent_id</typeparam>
        /// 
        /// <param name="collection">Collection of items</param>
        /// <param name="idSelector">Function extracting item's id</param>
        /// <param name="parentIdSelector">Function extracting item's parent_id</param>
        /// <param name="rootId">Root element id</param>
        /// 
        /// <returns>Tree of items</returns>
        public static IEnumerable<TreeItem<T>> GenerateTree<T, K>(
            this IEnumerable<T> collection,
            Func<T, K> idSelector,
            Func<T, K> parentIdSelector,
            K rootId = default(K))
        {
            foreach (var c in collection.Where(u =>
            {
                var selector = parentIdSelector(u);
                return (rootId == null && selector == null)  
                || (rootId != null &&rootId.Equals(selector));
            }))
            {
                yield return new TreeItem<T>
                {
                    Item = c,
                    Children = collection.GenerateTree(idSelector, parentIdSelector, idSelector(c))
                };
            }
        }
        /// <summary>
        /// 把数组转为逗号连接的字符串
        /// </summary>
        /// <param name="data"></param>
        /// <param name="Str"></param>
        /// <returns></returns>
        public static string ArrayToString(dynamic data, string Str)
        {
            string resStr = Str;
            foreach (var item in data)
            {
                if (resStr != "")
                {
                    resStr += ",";
                }

                if (item is string)
                {
                    resStr += item;
                }
                else
                {
                    resStr += item.Value;

                }
            }
            return resStr;
        }
        public static string FirstRowToJSON(this DataTable dTable) {
            return DataRowByIndexToJSON(dTable, 0);
        }
        public static string DataRowByIndexToJSON(this DataTable dTable, int index) {
            StringBuilder sBuilder = new StringBuilder();
            if (dTable.Rows.Count == 0) return "{}";

            sBuilder.Append("{");
            for (int i = 0; i < dTable.Columns.Count; i++) {
                sBuilder.Append("\"");
                sBuilder.Append(dTable.Columns[i].ColumnName.ToString().FilterString());
                sBuilder.Append("\":\"");
                sBuilder.Append(dTable.Rows[index][i].ToString().FilterString());
                sBuilder.Append("\",");
            }
            sBuilder.Remove(sBuilder.Length - 1, 1);
            sBuilder.Append("}");

            return sBuilder.ToString();
        }
        public static string DataTableToJSON(this DataTable dTable) {
            StringBuilder sBuilder = new StringBuilder();
            if (dTable.Rows.Count == 0) return "[]";
            sBuilder.Append("[");
            for (int i = 0; i < dTable.Rows.Count; i++) {
                sBuilder.Append("{");
                for (int j = 0; j < dTable.Columns.Count; j++) {
                    sBuilder.Append("\"");
                    sBuilder.Append(dTable.Columns[j].ColumnName);
                    sBuilder.Append("\":\"");
                    sBuilder.Append(dTable.Rows[i][j].ToString().FilterString());
                    sBuilder.Append("\",");
                }
                sBuilder.Remove(sBuilder.Length - 1, 1);
                sBuilder.Append("},");
            }
            sBuilder.Remove(sBuilder.Length - 1, 1);
            sBuilder.Append("]");
            return sBuilder.ToString();
        }
        public static string FelxgridDataToJSON(this DataTable dTable, string page, string total) {
            if (dTable.Rows.Count == 0) return "{page:1,total:0,rows:[]}";
            StringBuilder sBuilder = new StringBuilder("{");
            sBuilder.AppendFormat("page:{0},total:{1},", page, total);
            sBuilder.Append("rows:[");
            for (int i = 0; i < dTable.Rows.Count; i++) {
                sBuilder.Append("{");
                sBuilder.AppendFormat("id:\"{0}\",cell:[", dTable.Rows[i][0].ToString().FilterString());
                for (int j = 0; j < dTable.Columns.Count; j++)
                    sBuilder.AppendFormat("\"{0}\",", dTable.Rows[i][j].ToString().FilterString());
                sBuilder.Remove(sBuilder.Length - 1, 1);
                sBuilder.Append("]");
                sBuilder.Append("},");
            }
            sBuilder.Remove(sBuilder.Length - 1, 1);
            sBuilder.Append("]}");
            return sBuilder.ToString();
        }
        public static DataTable Join(this DataTable dTable, DataTable sTable, DataColumn[] dColumn, DataColumn[] sColumn)
        {
            DataTable aTable = new DataTable("Join");
            using (DataSet dSet = new DataSet())
            {
                dSet.Tables.AddRange(new DataTable[] { dTable.Copy(), sTable.Copy() });
                DataColumn[] pColumns = new DataColumn[dColumn.Length];
                for (int i = 0; i < pColumns.Length; i++)
                {
                    pColumns[i] = dSet.Tables[0].Columns[dColumn[i].ColumnName];
                }
                DataColumn[] cColumns = new DataColumn[sColumn.Length];
                for (int i = 0; i < cColumns.Length; i++)
                {
                    cColumns[i] = dSet.Tables[1].Columns[sColumn[i].ColumnName];
                }
                DataRelation dRelation = new DataRelation(string.Empty, pColumns, cColumns, false);
                dSet.Relations.Add(dRelation);
                for (int i = 0; i < dTable.Columns.Count; i++)
                {
                    aTable.Columns.Add(dTable.Columns[i].ColumnName, dTable.Columns[i].DataType);
                }
                for (int i = 0; i < sTable.Columns.Count; i++)
                {
                    if (!aTable.Columns.Contains(sTable.Columns[i].ColumnName))
                        aTable.Columns.Add(sTable.Columns[i].ColumnName, sTable.Columns[i].DataType);
                    else
                        aTable.Columns.Add(sTable.Columns[i].ColumnName + "1", sTable.Columns[i].DataType);
                }
                aTable.BeginLoadData();
                foreach (DataRow fRow in dSet.Tables[0].Rows)
                {
                    DataRow[] cRows = fRow.GetChildRows(dRelation);
                    if (cRows != null && cRows.Length > 0)
                    {
                        object[] pArray = fRow.ItemArray;
                        foreach (DataRow sRow in cRows)
                        {
                            object[] sArray = sRow.ItemArray;
                            object[] jArray = new object[pArray.Length + sArray.Length];
                            Array.Copy(pArray, 0, jArray, 0, pArray.Length);
                            Array.Copy(sArray, 0, jArray, pArray.Length, sArray.Length);
                            aTable.LoadDataRow(jArray, true);
                        }
                    }
                }
                aTable.EndLoadData();
            }
            return aTable;
        }

    }
}