using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using static Craciun_Adriana_Lab4.PricePredictionModel;
using System.IO;
using Craciun_Adriana_Lab4.Models;
using Craciun_Adriana_Lab4.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Craciun_Adriana_Lab4.Controllers
{
    public class PredictionController : Controller
    {
        private readonly AppDbContext _context;
        public PredictionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Price()
        {
            return View(new ModelInput());
        }

        [HttpPost]
        public async Task<IActionResult> Price(ModelInput input)
        {
            try
            {
                MLContext mlContext = new MLContext();
                var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PricePredictionModel.mlnet");
                ITransformer mlModel = mlContext.Model.Load(modelPath, out var modelInputSchema);
                var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
                ModelOutput result = predEngine.Predict(input);
                ViewBag.Price = result.Score;
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
            }

            var history = new PredictionHistory
            {
                PassengerCount = input.Passenger_count,
                TripTimeInSecs = input.Trip_time_in_secs,
                TripDistance = input.Trip_distance,
                PaymentType = input.Payment_type,
                PredictedPrice = ViewBag.Price,
                CreatedAt = DateTime.Now
            };
            _context.PredictionHistories.Add(history);
            await _context.SaveChangesAsync();
            return View(input);
        }

        public IActionResult Time(ModelInput input)
        {
            try
            {
                MLContext mlContext = new MLContext();
                var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TimePredictionModel.mlnet");
                ITransformer mlModel = mlContext.Model.Load(modelPath, out var modelInputSchema);
                var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
                ModelOutput result = predEngine.Predict(input);
                ViewBag.Time = result.Score;
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
            }
            return View(input);
        }

        [HttpGet]
        public async Task<IActionResult> History(string? paymentType, float? minPrice, float? maxPrice, string? sortOrder, string? sortTime)
        {
            var query = _context.PredictionHistories.AsQueryable();

            if (!string.IsNullOrEmpty(paymentType))
            {
                query = query.Where(p => p.PaymentType.ToLower() == paymentType.ToLower());
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.PredictedPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.PredictedPrice <= maxPrice.Value);
            }

            query = sortOrder switch
            {
                "price_asc" => query.OrderBy(p => p.PredictedPrice),
                "price_desc" => query.OrderByDescending(p => p.PredictedPrice),
                _ => query.OrderBy(p => p.PredictedPrice) //sortare default
            };

            query = sortTime switch
            {
                "time_asc" => query.OrderBy(p => p.CreatedAt),
                "time_desc" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderBy(p => p.CreatedAt) //sortare default
            };

            ViewBag.CurrentPaymentType = paymentType;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.CurrentSortTime = sortTime;

            var result = await query.ToListAsync();

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard(DateTime? fromDate, DateTime? toDate)
        {

            var query = _context.PredictionHistories.AsQueryable();

            if ( fromDate.HasValue)
            {                 
                query = query.Where(p => p.CreatedAt.Date >= fromDate.Value.Date);
            }

            if ( toDate.HasValue)
            {                 
                query = query.Where(p => p.CreatedAt.Date <= toDate.Value.Date);
            }


            // 1. Numărul total de predicții
            var totalPredictions = await query.CountAsync();

            // 2. Preț mediu per tip de plată + număr de predicții per tip
            var paymentTypeStats = await query.
                GroupBy(p => p.PaymentType)
                .Select(g => new PaymentTypeStat
                {
                    PaymentType = g.Key,
                    AveragePrice = g.Average(x => x.PredictedPrice),
                    Count = g.Count()
                })
                .ToListAsync();

            // 3. Distribuția prețurilor pe intervale (buckets)         
            // Definim intervalele: 0-10, 10-20, 20-30, 30-50, >50 (exemplu)
            var allPredictions = await query
                .Select(p => p.PredictedPrice)
                .ToListAsync();

            var buckets = new List<PriceBucketStat>
            {
                new PriceBucketStat { Label = "0 - 10" },
                new PriceBucketStat { Label = "10 - 20" },
                new PriceBucketStat { Label = "20 - 30" },
                new PriceBucketStat { Label = "30 - 50" },
                new PriceBucketStat { Label = "> 50" }
            };

            foreach (var price in allPredictions)
            {
                if (price < 10)
                    buckets[0].Count++;
                else if (price < 20)
                    buckets[1].Count++;
                else if (price < 30)
                    buckets[2].Count++;
                else if (price < 50)
                    buckets[3].Count++;
                else
                    buckets[4].Count++;
            }

            // 4. Construim ViewModel-ul
            var vm = new DashboardViewModel
            {
                TotalPredictions = totalPredictions,
                PaymentTypeStats = paymentTypeStats,
                PriceBuckets = buckets
            };

            return View(vm);
        }
    }
}
