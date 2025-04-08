namespace ProjFinal.Models
{
    public class Purchase
    {
        public int Id { get; set; }

        public decimal TotalPrice { get; set; }

        public DateTime OrderDate { get; set; }

        public PurchaseStatus Status { get; set; }

        public ICollection<PurchaseItem> Items { get; set; }
    }
}
