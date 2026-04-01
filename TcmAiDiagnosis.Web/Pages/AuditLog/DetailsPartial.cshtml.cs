using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TcmAiDiagnosis.EFContext;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Web.Pages.AuditLog
{
    public class DetailsPartialModel : PageModel
    {
        private readonly TcmAiDiagnosisContext _context;

        public DetailsPartialModel(TcmAiDiagnosisContext context)
        {
            _context = context;
        }

        public TcmAiDiagnosis.Entities.AuditLog? AuditLog { get; set; }

        public async Task OnGetAsync(long id)
        {
            AuditLog = await _context.AuditLogs
                .Include(a => a.User)
                .Include(a => a.Reviewer)  // 包含审核人信息
                .Include(a => a.Tenant)    // 包含租户信息
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.LogId == id);
        }
    }
}