using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcmAiDiagnosis.Entities.Enums
{
    public enum AuditStatus
    {
        Pending = 0,    // 待审核
        Approved = 1,   // 已通过
        Rejected = 2    // 已驳回
    }
}
