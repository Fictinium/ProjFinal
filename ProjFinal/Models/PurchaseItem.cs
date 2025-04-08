namespace ProjFinal.Models
{
    public class PurchaseItem
    {
        public int Id { get; set; }

        public int Quantity { get; set; }

        public Purchase Purchase { get; set; }

        public Book Book { get; set; }

    }
}
