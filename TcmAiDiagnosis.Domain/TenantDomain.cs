using Microsoft.EntityFrameworkCore;
using TcmAiDiagnosis.Entities;
using TcmAiDiagnosis.EFContext;

namespace TcmAiDiagnosis.Domain
{
    public class TenantDomain
    {
        private readonly TcmAiDiagnosisContext _context;

        public TenantDomain(TcmAiDiagnosisContext context)
        {
            _context = context;
        }

        // 创建租户（接受Tenant实体参数，保持数据库默认字段）
        public async Task<Tenant> CreateTenantAsync(Tenant tenant)
        {
            if (string.IsNullOrWhiteSpace(tenant.TenantName))
                throw new ArgumentException("租户名称不能为空");
            if (string.IsNullOrWhiteSpace(tenant.ContactPhone))
                throw new ArgumentException("联系电话不能为空");
            // 强制设置数据库管理字段（不覆盖用户传入值）
            tenant.Status = 1; // 默认正常状态
            tenant.CreatedAt = DateTime.Now;
            tenant.UpdatedAt = DateTime.Now;
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();
            return tenant;
        }

        // 修改租户信息（接受Tenant实体参数，仅更新指定字段）
        public async Task UpdateTenantAsync(Tenant updatedTenant)
        {
            var existingTenant = await _context.Tenants.FindAsync(updatedTenant.TenantId);
            if (existingTenant != null)
            {
                // 仅更新可修改字段，保持原有状态、过期时间等不变
                existingTenant.TenantName = updatedTenant.TenantName;
                existingTenant.ContactPerson = updatedTenant.ContactPerson;
                existingTenant.ContactPhone = updatedTenant.ContactPhone;
                existingTenant.Address = updatedTenant.Address;
                existingTenant.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        // 启用/禁用租户（状态：1正常，-1禁用）
        public async Task ToggleTenantStatusAsync(int tenantId, bool isActive)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant != null)
            {
                tenant.Status = isActive ? 1 : -1;
                await _context.SaveChangesAsync();
            }
        }

        // 租户续约（更新续费记录并调整租户状态及过期时间）
        public async Task RenewTenantAsync(Renewal renewal)
        {
            // 添加续费记录
            _context.Renewals.Add(renewal);

            // 更新租户状态和过期时间
            var tenant = await _context.Tenants
                .Include(t => t.Renewals)
                .FirstOrDefaultAsync(t => t.TenantId == renewal.TenantId);

            if (tenant != null)
            {
                var latestRenewal = tenant.Renewals.OrderByDescending(r => r.ValidTo).FirstOrDefault();
                tenant.ExpirationTime = latestRenewal?.ValidTo; // 更新过期时间
                tenant.Status = DateTime.Now <= tenant.ExpirationTime ? 1 : 0; // 根据过期时间更新状态
                await _context.SaveChangesAsync();
            }
        }
        // 根据租户名称查询租户
        public async Task<Tenant> GetTenantByNameAsync(string tenantName)
        {
            return await _context.Tenants
                .FirstOrDefaultAsync(t => t.TenantName == tenantName);
        }

        // 获取所有租户
        public async Task<List<Tenant>> GetAllTenantsAsync()
        {
            return await _context.Tenants.ToListAsync();
        }
    }
}