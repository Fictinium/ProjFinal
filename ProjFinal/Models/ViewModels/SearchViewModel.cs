using ProjFinal.Models;

namespace ProjFinal.Models.ViewModels
{
    public class SearchViewModel
    {
        public string? SearchTerm { get; set; }
        public int? SelectedCategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public List<Category> Categories { get; set; } = new();
        public List<Book> Results { get; set; } = new();
    }
}

