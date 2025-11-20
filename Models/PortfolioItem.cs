using System.ComponentModel.DataAnnotations;

namespace KlodTattooWeb.Models;

public class PortfolioItem
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty; // Es: "Rosa Realistica"

    public string Description { get; set; } = string.Empty;

    [Required]
    public string ImageUrl { get; set; } = string.Empty; // Percorso dell'immagine

    public string Style { get; set; } = "Blackwork"; // Es: Traditional, Realistico

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}