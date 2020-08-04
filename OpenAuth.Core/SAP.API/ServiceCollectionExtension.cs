using Microsoft.Extensions.DependencyInjection;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Text;

namespace SAP.API
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddSap(this IServiceCollection services)
        {
            Company oCompany = new Company();
            oCompany.Server = "192.168.1.207"; //dRow[0].ToString();
            oCompany.DbServerType = BoDataServerTypes.dst_MSSQL2016;
            oCompany.UseTrusted = false;
            oCompany.DbUserName = "sa"; //dRow[4].ToString();
            oCompany.DbPassword = "SAPB1Admin"; //dRow[5].ToString();
            oCompany.LicenseServer = "192.168.1.207:30000";//dell-t30:40000 //dRow[1].ToString();
            oCompany.UserName = "manager"; //dRow[2].ToString();
            oCompany.Password = "XinWei123&"; //dRow[3].ToString();Aa789123@
            oCompany.CompanyDB = "newareDemo3"; //dRow[6].ToString();
            oCompany.language = BoSuppLangs.ln_Chinese;
            oCompany.Connect();
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
                services.AddTransient<ServiceWorkOrderAPI>();
            }
            return services;
        }
    }
}
