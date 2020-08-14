using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class CertVerificationReq
    {
        /// <summary>
        /// 校准Id
        /// </summary>
        public string CertInfoId { get; set; }

        public VerificationReq Verification { get; set; }
    }
}
