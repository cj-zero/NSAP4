// ***********************************************************************
// Assembly         : FundationAdmin
// Author           : yubaolee
// Created          : 03-09-2016
//
// Last Modified By : yubaolee
// Last Modified On : 03-09-2016
// ***********************************************************************
// <copyright file="TableData.cs" company="Microsoft">
//     版权所有(C) Microsoft 2015
// </copyright>
// <summary>layui datatable数据返回</summary>
// ***********************************************************************

using System.Collections.Generic;
using Infrastructure;

namespace OpenAuth.App.Response
{
    /// <summary>
    /// table的返回数据
    /// </summary>
    public class TableData<T>
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public int Code;
        /// <summary>
        /// 操作消息
        /// </summary>
        public string Message;

        /// <summary>
        /// 总记录条数
        /// </summary>
        public int Count;

        /// <summary>
        ///  返回的列表头信息
        /// </summary>
        public List<KeyDescription> columnHeaders;

        /// <summary>
        /// 数据内容
        /// </summary>
        public T Data;

        public TableData()
        {
            Code = 200;
            Message = "加载成功";
            columnHeaders = new List<KeyDescription>();
        }
    }
    /// <summary>
    /// table的返回数据
    /// </summary>
    public class TableData : TableData<dynamic>
    {
    }
}