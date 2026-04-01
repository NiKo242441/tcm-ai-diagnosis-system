using Microsoft.AspNetCore.Mvc.RazorPages;
using TcmAiDiagnosis.Entities;
using TcmAiDiagnosis.EFContext;

namespace TcmAiDiagnosis.Web.Pages.Appointments
{
    public class StatisticsModel : PageModel
    {
        private readonly TcmAiDiagnosisContext _context;

        public StatisticsModel(TcmAiDiagnosisContext context)
        {
            _context = context;
        }

        public int PendingCount { get; set; }
        public int CompletedCount { get; set; }
        public int CanceledCount { get; set; }
        public int TotalCount { get; set; }

        public List<Appointment> Appointments { get; set; } = new();

        public void OnGet()
        {
            // 1️⃣ 直接从数据库拿数据
            Appointments = _context.Appointments.ToList();

            // 2️⃣ 直接后端统计
            PendingCount = Appointments.Count(a => a.Status == "待就诊");
            CompletedCount = Appointments.Count(a => a.Status == "已完成");
            CanceledCount = Appointments.Count(a => a.Status == "已取消");
            TotalCount = Appointments.Count;
        }

        // 医生统计
        public Dictionary<string, int> GetDoctorStatistics()
        {
            return Appointments
                .Where(a => !string.IsNullOrEmpty(a.DoctorName))
                .GroupBy(a => a.DoctorName)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        // 每日趋势
        public Dictionary<string, int> GetDailyTrend()
        {
            return Appointments
                .GroupBy(a => a.AppointmentTime.Date)
                .OrderBy(g => g.Key)
                .ToDictionary(
                    g => g.Key.ToString("yyyy-MM-dd"),
                    g => g.Count()
                );
        }
    }
}
