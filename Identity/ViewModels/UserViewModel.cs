using System.ComponentModel.DataAnnotations;

namespace Identity.ViewModels
{
    public class UserViewModel
    {
        [Required(ErrorMessage = "Kullacını adı gerekli")]
        [Display(Name = "Kullanıcı Adı")]
        public string UserName { get; set; }
        [Display(Name = "Telefon Numarası")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Email Gerekli")]
        [EmailAddress(ErrorMessage = "Email adresi yanlış")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Şifre belirleyiniz")]
        [Display(Name = "Şifre")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}