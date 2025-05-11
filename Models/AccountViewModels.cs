using System.ComponentModel.DataAnnotations;

namespace VazirlikWeb.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email manzil kiritilishi shart")]
        [EmailAddress(ErrorMessage = "Email manzil formati noto'g'ri")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Parol kiritilishi shart")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Meni eslab qol")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ism kiritilishi shart")]
        [Display(Name = "Ism")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Familiya kiritilishi shart")]
        [Display(Name = "Familiya")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email manzil kiritilishi shart")]
        [EmailAddress(ErrorMessage = "Email manzil formati noto'g'ri")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Parol kiritilishi shart")]
        [StringLength(100, ErrorMessage = "Parol kamida {2} belgidan iborat bo'lishi kerak", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Parol")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Parolni tasdiqlash")]
        [Compare("Password", ErrorMessage = "Parollar mos kelmadi")]
        public string ConfirmPassword { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Ism kiritilishi shart")]
        [Display(Name = "Ism")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Familiya kiritilishi shart")]
        [Display(Name = "Familiya")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email manzil kiritilishi shart")]
        [EmailAddress(ErrorMessage = "Email manzil formati noto'g'ri")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Rollar")]
        public List<string> SelectedRoles { get; set; } = new List<string>();
        public List<string> AvailableRoles { get; set; } = new List<string>();
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Joriy parol kiritilishi shart")]
        [DataType(DataType.Password)]
        [Display(Name = "Joriy parol")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Yangi parol kiritilishi shart")]
        [StringLength(100, ErrorMessage = "Parol kamida {2} belgidan iborat bo'lishi kerak", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Yangi parol")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yangi parolni tasdiqlash")]
        [Compare("NewPassword", ErrorMessage = "Parollar mos kelmadi")]
        public string ConfirmPassword { get; set; }
    }
}