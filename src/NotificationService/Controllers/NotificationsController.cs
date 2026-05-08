using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Models;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationDbContext _context;
        public NotificationsController(NotificationDbContext context)
        {
            _context = context;
        }

        [HttpPost("email")]
        public async Task<IActionResult> SendEmail([FromBody] Notification notification)
        {
            notification.NotificationId = Guid.NewGuid();
            notification.Type = "Email";
            notification.Status = "Pending";
            notification.CreatedAt = DateTime.UtcNow;
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            // TODO: Integrate with SendGrid or Azure Communication Services
            return Ok(notification);
        }

        [HttpPost("sms")]
        public async Task<IActionResult> SendSms([FromBody] Notification notification)
        {
            notification.NotificationId = Guid.NewGuid();
            notification.Type = "SMS";
            notification.Status = "Pending";
            notification.CreatedAt = DateTime.UtcNow;
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            // TODO: Integrate with Azure Communication Services or Twilio
            return Ok(notification);
        }

        [HttpPost("push")]
        public async Task<IActionResult> SendPush([FromBody] Notification notification)
        {
            notification.NotificationId = Guid.NewGuid();
            notification.Type = "Push";
            notification.Status = "Pending";
            notification.CreatedAt = DateTime.UtcNow;
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            // TODO: Integrate with Azure Notification Hubs or FCM
            return Ok(notification);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Notifications.OrderByDescending(n => n.CreatedAt).ToListAsync());
        }
    }
}
