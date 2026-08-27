namespace NotificationForwarder.Application.Models;

public enum NotificationProcessingOutcome
{
    Informational = 0,  
    Forwarded = 1,
    InvalidLevel = 2,
    RateLimited = 3,
}