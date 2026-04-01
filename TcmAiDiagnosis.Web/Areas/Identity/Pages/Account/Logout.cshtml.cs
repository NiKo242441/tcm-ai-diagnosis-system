using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TcmAiDiagnosis.Domain;

namespace TcmAiDiagnosis.Web.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly UserDomain _userDomain;

        public LogoutModel(UserDomain userDomain)
        {
            _userDomain = userDomain;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _userDomain.LogoutAsync();
            return RedirectToPage("/Account/Login");
        }
    }
}