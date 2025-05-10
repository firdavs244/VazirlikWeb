using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VazirlikWeb.Models
{
    public class News
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Sarlavha kiritilishi shart.")]
        public string Title { get; set; } = string.Empty; // Default qiymatni qo‘shish

        [Required(ErrorMessage = "Kontent bo‘sh bo‘lmasligi kerak.")]
        public string Content { get; set; } = string.Empty; // Default qiymatni qo‘shish

        [Column(TypeName = "timestamp with time zone")]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        public string? ImageUrl { get; set; }
    }
}
