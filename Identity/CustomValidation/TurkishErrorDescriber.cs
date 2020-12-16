using Microsoft.AspNetCore.Identity;

namespace Identity.CustomValidation
{
    public class TurkishErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError InvalidUserName(string userName)
        {
            return new IdentityError
            {
                Code = "InvalidUserName",
                Description = $"{userName} kullanıcı adı geçersizdir."
            };
        }

        public override IdentityError DuplicateEmail(string email)
        {
            return new IdentityError
            {
                Code = "DuplicateEmail",
                Description = $"{email} email adresi ile daha önce kayıt olunmuş."
            };
        }

        public override IdentityError PasswordTooShort(int length)
        {
            return new IdentityError
            {
                Code = "PasswordTooShort",
                Description = $"Şifreniz en az {length} karakter olmalıdır."
            };
        }

        public override IdentityError DuplicateUserName(string userName)
        {
            return new IdentityError
            {
                Code = "DuplicateEmail",
                Description = $"{userName} kullanıcı adı daha önce alınmış."
            };
        }
    }
}