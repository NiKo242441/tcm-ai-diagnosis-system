using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TcmAiDiagnosis.Entities;
using TcmAiDiagnosis.EFContext;
using System.Security.Claims;

namespace TcmAiDiagnosis.Domain
{
    public class UserDomain
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly TcmAiDiagnosisContext _context;

        public UserDomain(UserManager<User> userManager, SignInManager<User> signInManager, TcmAiDiagnosisContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        /// <summary>
        /// 添加患者（符合Identity规范的角色分配方式）
        /// </summary>
        public async Task<User> AddPatientAsync(User user, UserDetail userDetail, string password)
        {
            // 患者TenantId必须为null
            user.TenantId = null;
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;
            return await AddUserWithRoleAsync(user, password, "Patient", userDetail);
        }

        /// <summary>
        /// 添加医生（角色为Doctor）
        /// </summary>
        public async Task<User> AddDoctorAsync(User user, string password)
        {
            return await AddUserWithRoleAsync(user, password, "Doctor");
        }

        /// <summary>
        /// 添加管理员（角色为Manager）
        /// </summary>
        public async Task<User> AddManagerAsync(User user, string password)
        {
            return await AddUserWithRoleAsync(user, password, "Manager");
        }

        private async Task<User> AddUserWithRoleAsync(User user, string password, string roleName, UserDetail userDetail = null)
        {
            // 基础验证
            if (string.IsNullOrEmpty(roleName))
                throw new ArgumentException("角色名称不能为空");

            // 使用Identity创建用户
            var addUser = new User()
            {
                DateOfBirth = user.DateOfBirth,
                Email = user.Email,
                FullName = user.FullName,
                Gender = user.Gender,
                PhoneNumber = user.PhoneNumber,
                TenantId = user.TenantId,
                UserName = user.UserName,
                //SecurityStamp = Guid.NewGuid().ToString()
            };

            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
                throw new InvalidOperationException("用户创建失败: " + string.Join(", ", createResult.Errors.Select(e => e.Description)));

            // 查询目标角色
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
                throw new ArgumentException($"系统中未找到名称为'{roleName}'的角色");

            // 分配角色
            var roleResult = await _userManager.AddToRoleAsync(user, role.Name);
            if (!roleResult.Succeeded)
                throw new InvalidOperationException($"{roleName}角色分配失败: " + string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            if (userDetail != null)
            {
                // 保存用户详细信息
                userDetail.UserId = user.Id;
                userDetail.CreatedAt = DateTime.Now;
                userDetail.UpdatedAt = DateTime.Now;
                await _context.UserDetails.AddAsync(userDetail);
            }
            await _context.SaveChangesAsync();
            return user;
        }

        // 用户登录（含租户状态检查）
        public async Task<SignInResult> LoginAsync(string userName, string password)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return SignInResult.Failed;

            // 检查租户状态（仅非患者用户需要检查）
            if (user.TenantId.HasValue)
            {
                var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.TenantId == user.TenantId);
                if (tenant != null)
                {
                    // 检查过期时间并更新状态
                    if (tenant.ExpirationTime.HasValue && DateTime.Now > tenant.ExpirationTime)
                    {
                        tenant.Status = 0; // 标记为欠费
                        await _context.SaveChangesAsync();
                    }
                    if (tenant.Status != 1) // 租户状态非正常
                        return SignInResult.Failed;
                }
            }

            return await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: false);
        }

        // 修改密码
        public async Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        }

        // 用户登出
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        /// <summary>
        /// 根据ClaimsPrincipal获取当前用户
        /// </summary>
        public async Task<User> GetUserAsync(ClaimsPrincipal principal)
        {
            // 检查principal是否为空或者未认证
            if (principal == null || !principal.Identity.IsAuthenticated)
            {
                return null;
            }
            
            return await _userManager.GetUserAsync(principal);
        }

        // 禁用用户
        public async Task<IdentityResult> DisableUserAsync(User user)
        {
            user.IsDisabled = 1;
            return await _userManager.UpdateAsync(user);
        }

        // 根据用户名查询用户
        public async Task<User> GetUserByUserNameAsync(string userName)
        {
            return await _userManager.FindByNameAsync(userName);
        }

        // 根据租户ID获取用户列表
        public async Task<List<User>> GetUsersByTenantIdAsync(int tenantId)
        {
            return await _userManager.Users
                .Where(u => u.TenantId == tenantId)
                .ToListAsync();
        }

        // 通过角色查询用户
        public async Task<List<User>> GetUsersByRoleAsync(string roleName)
        {
            return await (
                from u in _context.Users
                join ur in _context.UserRoles on u.Id equals ur.UserId
                join r in _context.Roles on ur.RoleId equals r.Id
                where r.Name == roleName
                orderby u.UserName
                select u
            ).ToListAsync();
        }
    }
}