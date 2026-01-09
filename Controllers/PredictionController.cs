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
        public async Task<IActionResult> History()
        {
            var histories = await _context.PredictionHistories.OrderByDescending(h => h.CreatedAt).ToListAsync();
            return View(histories);
        }
    }
}
