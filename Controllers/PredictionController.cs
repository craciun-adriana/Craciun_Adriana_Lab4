using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using static Craciun_Adriana_Lab4.PricePredictionModel;
using System.IO;

namespace Craciun_Adriana_Lab4.Controllers
{
    public class PredictionController : Controller
    {
        [HttpGet]
        public IActionResult Price()
        {
            return View(new ModelInput());
        }

        [HttpPost]
        public IActionResult Price(ModelInput input)
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
            return View(input);
        }
    }
}
