using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dating.API.Dtos
{
    //Dto  კლასებს ვქმნით შუამავლად რომელსაც მიიღებს პარამეტრად კლიენტის მხრიდან ეიპიაი
    public class UserForRegisterDto
    {
        [Required]
        public string Username { get; set; }
        [Required]
        [StringLength(8, MinimumLength = 4, ErrorMessage ="You must specify password 4 and 8 characters")]
        public string Password { get; set; }
    }
}
