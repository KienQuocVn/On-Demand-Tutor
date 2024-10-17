namespace OnDemandTutor.ModelViews.AuthModelViews
{
    public class PaymentInfo
    {
        public int OrderId { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}