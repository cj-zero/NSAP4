using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Clue.Request
{
    /// <summary>
    /// 编辑线索model
    /// </summary>
    public class UpdateClueReq:AddClueReq
    {
        public int Id { get; set; }

    }

    public class CluePatternReq
    {
       public string pattern { get; set; }
    }

}
