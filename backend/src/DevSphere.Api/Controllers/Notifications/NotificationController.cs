using DevSphere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DevSphere.Api.Controllers.Notifications;

[ApiController]
[Route("api/notifications")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _service;


    public NotificationController(
        INotificationService service)
    {
        _service = service;
    }



    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = User.FindFirst(
            ClaimTypes.NameIdentifier
        )?.Value;


        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }


        var result = await _service
            .GetMyNotificationsAsync(userId);


        return Ok(result);
    }



    [HttpPut("{id}/read")]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(
        Guid id)
    {
        await _service.MarkAsReadAsync(id);

        return Ok(new
        {
            message = "Notification marked as read."
        });
    }
}