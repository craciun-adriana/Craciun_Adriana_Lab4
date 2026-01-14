namespace Craciun_Adriana_Lab4.Models
{
    public class DashboardViewModel
    {
        public int TotalPredictions { get; set; }

        public List<PaymentTypeStat> PaymentTypeStats { get; set; } = new();

        public List<PriceBucketStat> PriceBuckets { get; set; } = new();

        public DateTime? fromDate { get; set; }
        public DateTime? toDate { get; set}
    }
}
