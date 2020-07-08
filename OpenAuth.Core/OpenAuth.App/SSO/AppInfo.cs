// ***********************************************************************
// Assembly         : SmartSSO
// Author           : yubaolee
// Created          : 06-08-2016
//
// Last Modified By : yubaolee
// Last Modified On : 07-06-2016
// Contact : 
// File: AppInfo.cs
// ***********************************************************************

using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;

namespace OpenAuth.App.SSO
{
    /// <summary>
    /// 应用程序信息
    /// </summary>
    [AutoMapTo(typeof(Application))]
    public class AppInfo
    {
        public string AppKey { get; set; }

        public string AppSecret { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Icon { get; set; }

        public string ReturnUrl { get; set; }

        public bool Disable { get; set; }

        public DateTime CreateTime { get; set; }
        public DateTime CreateUser { get; set; }
    }
}