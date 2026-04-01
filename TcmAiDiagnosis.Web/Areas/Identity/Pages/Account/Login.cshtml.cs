using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TcmAiDiagnosis.Domain;

namespace TcmAiDiagnosis.Web.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly UserDomain _userDomain;

        public LoginModel(UserDomain userDomain)
        {
            _userDomain = userDomain;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public string UserName { get; set; }
            public string Password { get; set; }
            public bool RememberMe { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _userDomain.LoginAsync(Input.UserName, Input.Password);
            if (result.Succeeded)
            {
                return RedirectToPage("/Index");
            }
            ModelState.AddModelError(string.Empty, "登录失败：用户名或密码错误");
            return Page();
        }
    }
}