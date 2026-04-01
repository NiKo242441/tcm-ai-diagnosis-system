using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using TcmAiDiagnosis.EFContext;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Web.Pages.Appointments
{
    public class EditModel : PageModel
    {
        private readonly TcmAiDiagnosisContext _context;

        public EditModel(TcmAiDiagnosisContext context)
        {
            _context = context;
        }


        [BindProperty]
        public Appointment Appointment { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
                return NotFound();

            Appointment = appointment;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == Appointment.AppointmentId);

            if (appointment == null)
                return NotFound();

            appointment.Status = Appointment.Status;

            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
