using Microsoft.AspNetCore.Mvc;

namespace ClientApp.DTO
{
    [BindProperties]
    public class SearchFilterDTO
    {
        public int CategoryId { get; set; }

        public string Name { get; set; }
    }
}
