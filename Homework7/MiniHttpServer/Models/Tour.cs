namespace MiniHttpServer.Models
{
    public class Tour
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string AdditionalDescription { get; set; }
        public string IncludedInPrice { get; set; }
        public string NotIncludedInPrice { get; set; }
        public decimal BasePrice { get; set; }
        public int Duration { get; set; }
        public string TourType { get; set; }
        public int CountryId { get; set; }
        public string CityName { get; set; }
    }
}
