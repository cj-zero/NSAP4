using Microsoft.AspNetCore.SignalR;
using OpenAuth.App.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.SignalR
{
    public class NameUserIdProvider : IUserIdProvider
    {
        private readonly IAuth _authUtil;

        public NameUserIdProvider(IAuth authUtil)
        {
            _authUtil = authUtil;
        }

        public string GetUserId(HubConnectionContext connection)
        {
            return _authUtil.GetUserName();
            // return  _authUtil.GetCurrentUser().User.Name;
        }
    }
}
