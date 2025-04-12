namespace UltraStrore.Helper
{
    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string Message { get; set; }
        public int? OrderId { get; set; }
    }
}
