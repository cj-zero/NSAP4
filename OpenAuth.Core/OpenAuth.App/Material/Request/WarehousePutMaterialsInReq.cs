using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    public class WarehousePutMaterialsInReq
    {
        public int ReturnNoteId { get; set; }
        public string ExpressageId { get; set; }
        public List<QuotationMergeMaterialReq> putInMaterials { get; set; }
    }
}
