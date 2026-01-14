using static Craciun_Adriana_Lab4.PricePredictionModel;

namespace Craciun_Adriana_Lab4.Models
{
    public class BatchPredictionViewModel
    {
        public IFormFile? File { get; set; }
        // Rezultatele (predicțiile) pentru fiecare rând din fișier
        public List<ModelOutput>? Predictions { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
