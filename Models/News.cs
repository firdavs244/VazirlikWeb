using System;
using System.ComponentModel.DataAnnotations;

namespace VazirlikWeb.Models
{
    public class News
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public string? ImageUrl { get; set; } // agar rasm bo‘lsa
    }
}
