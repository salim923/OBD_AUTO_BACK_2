using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using OBD.Infrastructure.Persistence;
using Stripe;
using Stripe.Checkout;

namespace OBD.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PremiumController : ControllerBase
    {
        private readonly ObdDbContext _context;
        private readonly IConfiguration _configuration;

        public PremiumController(ObdDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        [HttpPost]
        [Route("upgrade")]
        public async Task<IActionResult> UpgradeToPremium()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = 3099   , 
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Premium Upgrade",
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = $"{_configuration["AppSettings:BaseUrl"]}/premium-success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{_configuration["AppSettings:BaseUrl"]}/dashboard",
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            return Ok(new { sessionId = session.Id });
        }

        [HttpPost]
        [Route("upgrade/success")]

        public async Task<IActionResult> UpgradeSuccess([FromQuery] string session_id)
        {
            if (string.IsNullOrEmpty(session_id))
            {
                return BadRequest("session_id is required");
            }

            var service = new SessionService();
            var session = await service.GetAsync(session_id);
            Console.WriteLine($"Session ID: {session_id}, Payment Status: {session.PaymentStatus}");

            if (session.PaymentStatus != "paid")
            {
                return BadRequest("Payment not completed");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }

            user.IsPremium = true;
            user.PremiumExpiration = DateTime.UtcNow.AddYears(1); 

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User upgraded to premium successfully." });
        }


        [HttpGet]
        [Route("status")]
        public async Task<IActionResult> GetPremiumStatus()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                isPremium = user.IsPremium,
                premiumExpiration = user.PremiumExpiration
            });
        }

        [HttpPost]
        [Route("cancel")]
        public async Task<IActionResult> CancelUpgrade()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }

            user.IsPremium = false;
            user.PremiumExpiration = null;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Premium upgrade canceled successfully." });
        }
    }
}
