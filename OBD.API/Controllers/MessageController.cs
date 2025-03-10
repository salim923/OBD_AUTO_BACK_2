using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBD.Domain.Entities;
using OBD.Infrastructure.Persistence;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace OBD.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly ObdDbContext _context;
        private readonly IConfiguration _configuration;

        public MessageController(ObdDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private int? GetAdminIdFromToken()
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminIdClaim))
            {
                return null;
            }
            return int.Parse(adminIdClaim);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] MessageInsert messageInsert)
        {
            if (messageInsert == null)
            {
                return BadRequest();
            }

            var adminId = GetAdminIdFromToken();
            if (adminId == null)
            {
                return Unauthorized();
            }

            var exist = _context.Admins.Find(adminId);

            var users = _context.Users.Where(u => messageInsert.RecipientEmails.Contains(u.Email)).ToList();

            if (!users.Any())
            {
                return NotFound("No users found with the provided email addresses.");
            }

            var message = new Message
            {
                Title = messageInsert.Title,
                Type = messageInsert.Type,
                Status = messageInsert.Status,
                DateSent = DateTime.UtcNow,
                Content = messageInsert.Content,
                MessageRecipients = new List<MessageRecipient>()
            };

            foreach (var user in users)
            {
                message.MessageRecipients.Add(new MessageRecipient
                {
                    UserId = user.Id
                });
            }

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            foreach (var user in users)
            {
                await SendEmailAsync(user.Email, message.Title, message.Content);
            }

            return Ok(message);
        }



        [HttpPost("send/all")]
        public async Task<IActionResult> SendToAll([FromBody] MessageInsert messageInsert)
        {
            if (messageInsert == null)
            {
                return BadRequest();
            }

            var adminId = GetAdminIdFromToken();
            if (adminId == null)
            {
                return Unauthorized();
            }

            var exist = _context.Admins.Find(adminId);
            var allUsers = await _context.Users.ToListAsync();
            var message = new Message
            {
                Title = messageInsert.Title,
                Type = messageInsert.Type,
                Status = messageInsert.Status,
                DateSent = DateTime.UtcNow,
                Content = messageInsert.Content,
                MessageRecipients = new List<MessageRecipient>()
            };

            foreach (var user in allUsers)
            {
                message.MessageRecipients.Add(new MessageRecipient
                {
                    UserId = user.Id
                });
            }

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

  
            foreach (var user in allUsers)
            {
                await SendEmailAsync(user.Email, message.Title, message.Content);
            }

            return Ok(message);
        }


        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentMessages()
        {
            var recentMessages = await _context.Messages
                .Include(m => m.MessageRecipients)
                .OrderByDescending(m => m.DateSent)
                .Take(10)
                .ToListAsync();

            return Ok(recentMessages);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _context.Messages
                .Include(m => m.MessageRecipients)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (message == null)
            {
                return NotFound();
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetMessageCount()
        {
            var messageCount = await _context.Messages.CountAsync();
            return Ok(messageCount);
        }

     

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient(_configuration["SmtpSettings:Host"])
            {
                Port = int.Parse(_configuration["SmtpSettings:Port"]),
                Credentials = new NetworkCredential(_configuration["SmtpSettings:Username"], _configuration["SmtpSettings:Password"]),
                EnableSsl = bool.Parse(_configuration["SmtpSettings:EnableSsl"]),
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["SmtpSettings:From"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
