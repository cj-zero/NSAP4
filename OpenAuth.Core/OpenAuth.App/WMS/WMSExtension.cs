using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SAPbobsCOM;


namespace OpenAuth.App.WMS
{
    public static class WMSExtension
    {
        public static IServiceCollection AddSap(this IServiceCollection services, IConfiguration configuration)
        {
            CompanySettings settings = new CompanySettings();
            configuration.GetSection("CompanySettings").Bind(settings);
            Company oCompany = new Company();
            oCompany.Server = settings.Server; //dRow[0].ToString();
            oCompany.DbServerType = BoDataServerTypes.dst_MSSQL2016;
            oCompany.UseTrusted = false;
            oCompany.DbUserName = settings.DbUserName; //dRow[4].ToString();
            oCompany.DbPassword = settings.DbPassword; //dRow[5].ToString();
            oCompany.LicenseServer = settings.LicenseServer;//dell-t30:40000 //dRow[1].ToString();
            oCompany.UserName = settings.UserName; //dRow[2].ToString();
            oCompany.Password = settings.Password; //dRow[3].ToString();Aa789123@
            oCompany.CompanyDB = settings.CompanyDB; //dRow[6].ToString();
            oCompany.language = BoSuppLangs.ln_Chinese;
            int connecti = oCompany.Connect();
            if (connecti != 0)
            {
                int temp_int;
                string temp_string;
                oCompany.GetLastError(out temp_int, out temp_string);
                throw new Exception(temp_string);
            }
            else
            {
                //Console.WriteLine("OK"); ;//oCompany;
                services.AddSingleton(oCompany);
                //services.AddTransient<ServiceWorkOrderAPI>();
            }
            return services;
        }
    }
}
