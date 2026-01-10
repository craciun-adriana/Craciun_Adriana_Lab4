using Microsoft.AspNetCore.Mvc;

namespace Craciun_Adriana_Lab4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictionAPIController : Controller
    {
        private readonly Data.AppDbContext _context;

        public PredictionAPIController(Data.AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpDelete("{idPrediction}")]
        public IActionResult DeletePrediction(int idPrediction)
        {
            var prediction = _context.PredictionHistories.Find(idPrediction);
            if (prediction == null)
            {
                return NotFound("Prediction not found.");
            }

            _context.PredictionHistories.Remove(prediction);
            _context.SaveChanges();

            return Ok("Prediction deleted.");
        }
    }
}
