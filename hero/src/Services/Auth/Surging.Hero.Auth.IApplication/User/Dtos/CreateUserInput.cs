﻿using Surging.Hero.Common;
using System.ComponentModel.DataAnnotations;

namespace Surging.Hero.Auth.IApplication.User.Dtos
{
    public class CreateUserInput : UserDtoBase
    {
        [Required(ErrorMessage = "用户名不允许为空")]
        [RegularExpression(RegExpConstants.UserName, ErrorMessage = "用户名不允许为空")]
        public string UserName { get; set; }

        [RegularExpression(RegExpConstants.Password, ErrorMessage = "密码格式不正确")]
        public string Password { get; set; }
    }
}
