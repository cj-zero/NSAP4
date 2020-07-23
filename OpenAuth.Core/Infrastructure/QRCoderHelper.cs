using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Infrastructure
{
    /// <summary>
    /// 二维码辅助类
    /// </summary>
    public class QRCoderHelper
    {
        #region 普通二维码
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">存储内容</param>
        /// <param name="pixel">像素大小</param>
        /// <returns></returns>
        public static Bitmap GetPTQRCode(string url, int pixel)
        {
            QRCodeGenerator generator = new QRCodeGenerator();
            QRCodeData codeData = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.M, true);
            QRCoder.QRCode qrcode = new QRCoder.QRCode(codeData);
            Bitmap qrImage = qrcode.GetGraphic(pixel, Color.Black, Color.White, true);
            return qrImage;
        }
        #endregion

        #region 带logo的二维码
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">存储内容</param>
        /// <param name="pixel">像素大小</param>
        /// <returns></returns>
        public static Bitmap GetLogoQRCode(string url, string logoPath, int pixel)
        {
            QRCodeGenerator generator = new QRCodeGenerator();
            QRCodeData codeData = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.M, true);
            QRCoder.QRCode qrcode = new QRCoder.QRCode(codeData);
            Bitmap icon = new Bitmap(logoPath);
            Bitmap qrImage = qrcode.GetGraphic(pixel, Color.Black, Color.White, icon, 15, 6, true);
            #region 参数介绍
            //GetGraphic方法参数介绍
            //pixelsPerModule //生成二维码图片的像素大小 ，我这里设置的是5
            //darkColor       //暗色   一般设置为Color.Black 黑色
            //lightColor      //亮色   一般设置为Color.White  白色
            //icon             //二维码 水印图标 例如：Bitmap icon = new Bitmap(context.Server.MapPath("~/images/zs.png")); 默认为NULL ，加上这个二维码中间会显示一个图标
            //iconSizePercent  //水印图标的大小比例 ，可根据自己的喜好设置
            //iconBorderWidth  // 水印图标的边框
            //drawQuietZones   //静止区，位于二维码某一边的空白边界,用来阻止读者获取与正在浏览的二维码无关的信息 即是否绘画二维码的空白边框区域 默认为true
            #endregion
            return qrImage;
        }
        #endregion

        /// <summary>
        /// 生成随机数
        /// </summary>
        /// <param name="VcodeNum"></param>
        /// <returns></returns>
        public static string RndNum(int VcodeNum)
        {
            //验证码可以显示的字符集合  
            string Vchar = "0,1,2,3,4,5,6,7,8,9,a,b,c,d,e,f,g,h,i,j,k,l,m,n,p" +
                ",q,r,s,t,u,v,w,x,y,z,A,B,C,D,E,F,G,H,I,J,K,L,M,N,P,P,Q" +
                ",R,S,T,U,V,W,X,Y,Z";
            string[] VcArray = Vchar.Split(new Char[] { ',' });//拆分成数组   
            string code = "";//产生的随机数  
            int temp = -1;//记录上次随机数值，尽量避避免生产几个一样的随机数  

            Random rand = new Random();
            //采用一个简单的算法以保证生成随机数的不同  
            for (int i = 1; i < VcodeNum + 1; i++)
            {
                if (temp != -1)
                {
                    rand = new Random(i * temp * unchecked((int)DateTime.Now.Ticks));//初始化随机类  
                }
                int t = rand.Next(61);//获取随机数  
                if (temp != -1 && temp == t)
                {
                    return RndNum(VcodeNum);//如果获取的随机数重复，则递归调用  
                }
                temp = t;//把本次产生的随机数记录起来  
                code += VcArray[t];//随机数的位数加一  
            }
            return code + Guid
                .NewGuid().ToString("N").Substring(2, 8);
        }
    }

    /// <summary>
    /// 二维码参数
    /// </summary>
    public class Qrcode
    {
        public string scene { get; set; }
        public string parameter { get; set; }
    }
}
