using System.ComponentModel.DataAnnotations;

namespace KlodTattooWeb.Models;

public class BookingRequest
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Il nome è obbligatorio")]
    public string ClientName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string BodyPart { get; set; } = string.Empty; // Es: Avambraccio

    [Required]
    public string IdeaDescription { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime PreferredDate { get; set; }

    public bool IsConfirmed { get; set; } = false;
}