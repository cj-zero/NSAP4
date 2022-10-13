using Infrastructure;

namespace OpenAuth.App.SSO
{
    public class LoginResult :Response<string>
    {
        public string ReturnUrl;
        public string Token;
        public string Name;
        public bool? ChangePassword;
        public bool? isQuality;
    }
}