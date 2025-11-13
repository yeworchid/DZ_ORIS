namespace MiniHttpServer.Models
{
    public class TourDate
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public DateTime DepartureDate { get; set; }
        public int CityId { get; set; }
        public decimal AdultPrice { get; set; }
        public decimal ChildPrice { get; set; }
        public string DepartureTime { get; set; }
        public string ArrivalTime { get; set; }
    }
}
