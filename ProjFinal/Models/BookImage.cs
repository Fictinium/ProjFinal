namespace ProjFinal.Models
{
    public class BookImage
    {
        public int Id { get; set; }

        public string ImageUrl { get; set; }

        public int PageNumber { get; set; }

        public Book Book { get; set; }

        
    }
}
