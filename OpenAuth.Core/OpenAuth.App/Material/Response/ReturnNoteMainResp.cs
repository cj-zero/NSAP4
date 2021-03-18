namespace OpenAuth.App.Material.Response
{
    public class ReturnNoteMainResp
    {
        public string ExpressNumber { get; set; }
        public string ExpressId { get; set; }

        public int Id { get; set; }

        public int ServiceOrderId { get; set; }

        public string CreateUser { get; set; }

        public string CreateDate { get; set; }

        public int ServiceOrderSapId { get; set; }

        public int IsCanClear { get; set; }

        public string Remark { get; set; }

        public decimal TotalMoney { get; set; }

        public string CustomerId { get; set; }

        public string CustomerName { get; set; }
    }
}
