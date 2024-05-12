namespace api.DTO
{
    public class MeterReadingDto
    {
        public int AccountId { get; set; }
        public DateTime MeterReadingDateTime { get; set; }
        public int MeterReadingValue { get; set; }
    }
}