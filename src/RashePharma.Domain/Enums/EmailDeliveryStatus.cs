namespace RashePharma.Domain.Enums.Email;

public enum EmailDeliveryStatus
{
    Unknown = 0,
    Accepted = 1,
    Delivered = 2,
    Opened = 3,
    Clicked = 4,
    Bounced = 5,
    Complained = 6,
    Unsubscribed = 7
}
