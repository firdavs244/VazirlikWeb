using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VazirlikWeb.Models
{
    public class Assignment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Topshiriq nomi kiritilishi shart")]
        [StringLength(200, ErrorMessage = "Topshiriq nomi 200 belgidan oshmasligi kerak")]
        [Display(Name = "Topshiriq nomi")]
        public string Title { get; set; }

        [Display(Name = "Topshiriq tavsifi")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Boshlanish sanasi kiritilishi shart")]
        [Display(Name = "Boshlanish sanasi")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Tugash sanasi kiritilishi shart")]
        [Display(Name = "Tugash sanasi")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Display(Name = "Bajarilgan sana")]
        [DataType(DataType.Date)]
        public DateTime? CompletedDate { get; set; }

        [Display(Name = "Holati")]
        public AssignmentStatus Status { get; set; } = AssignmentStatus.New;

        [Display(Name = "Ustuvorlik")]
        public AssignmentPriority Priority { get; set; } = AssignmentPriority.Medium;

        // Topshiriqni yaratgan foydalanuvchi (Administrator yoki Editor)
        [Display(Name = "Yaratuvchi")]
        public string CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual ApplicationUser CreatedBy { get; set; }

        // Topshiriq tayinlangan foydalanuvchi
        [Required(ErrorMessage = "Bajaruvchi ko'rsatilishi shart")]
        [Display(Name = "Bajaruvchi")]
        public string AssignedToId { get; set; }

        [ForeignKey("AssignedToId")]
        public virtual ApplicationUser AssignedTo { get; set; }

        [Display(Name = "Yaratilgan sana")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Yangilangan sana")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Ensure DateTime values are always in UTC when getting/setting
        // Bu modelning getter va setterlarini to'g'rilash uchun
        private DateTime EnsureUtc(DateTime date)
        {
            if (date.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(date, DateTimeKind.Utc);
            if (date.Kind == DateTimeKind.Local)
                return date.ToUniversalTime();
            return date;
        }
    }

    public enum AssignmentStatus
    {
        [Display(Name = "Yangi")]
        New = 0,

        [Display(Name = "Bajarilmoqda")]
        InProgress = 1,

        [Display(Name = "Tekshirilmoqda")]
        UnderReview = 2,

        [Display(Name = "Bajarilgan")]
        Completed = 3,

        [Display(Name = "Kechiktirilgan")]
        Delayed = 4,

        [Display(Name = "Bekor qilingan")]
        Cancelled = 5
    }

    public enum AssignmentPriority
    {
        [Display(Name = "Past")]
        Low = 0,

        [Display(Name = "O'rta")]
        Medium = 1,

        [Display(Name = "Yuqori")]
        High = 2,

        [Display(Name = "Juda yuqori")]
        Urgent = 3
    }
}