using System.Text;
using Microsoft.AspNetCore.Identity;

namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 用户信息格式化工具类，用于将User和UserDetail对象转换为文本描述
    /// </summary>
    public static class UserInfoFormatter
    {
        /// <summary>
        /// 将用户信息转换为文本描述，每行一项数据
        /// </summary>
        /// <param name="user">用户实体对象</param>
        /// <param name="userDetail">用户详细信息实体对象</param>
        /// <returns>格式化后的文本描述字符串</returns>
        public static string ToTextDescription(User user, UserDetail userDetail = null)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "用户实体对象不能为空");

            var stringBuilder = new StringBuilder();

            // 添加User表基本信息
            stringBuilder.AppendLine("用户ID：" + user.Id);
            stringBuilder.AppendLine("用户名：" + (user.UserName ?? "未设置"));
            stringBuilder.AppendLine("电子邮箱：" + (user.Email ?? "未设置"));
            stringBuilder.AppendLine("手机号码：" + (user.PhoneNumber ?? "未设置"));
            stringBuilder.AppendLine("租户ID：" + (user.TenantId.HasValue ? user.TenantId.ToString() : "未设置"));
            stringBuilder.AppendLine("用户全名：" + (user.FullName ?? "未设置"));
            stringBuilder.AppendLine("用户性别：" + (user.Gender ?? "未知"));
            stringBuilder.AppendLine("出生日期：" + (user.DateOfBirth.HasValue ? user.DateOfBirth.Value.ToString("yyyy年MM月dd日") : "未设置"));
            stringBuilder.AppendLine("账户状态：" + (user.IsDisabled == 0 ? "活跃" : "禁用"));
            stringBuilder.AppendLine("记录创建时间：" + user.CreatedAt.ToString("yyyy年MM月dd日 HH:mm:ss"));
            stringBuilder.AppendLine("记录最后更新时间：" + (user.UpdatedAt.ToString("yyyy年MM月dd日 HH:mm:ss")));

            // 添加UserDetail表详细信息
            if (userDetail != null)
            {
                stringBuilder.AppendLine("家庭住址：" + (userDetail.HomeAddress ?? "未设置"));
                stringBuilder.AppendLine("紧急联系人姓名：" + (userDetail.EmergencyContactName ?? "未设置"));
                stringBuilder.AppendLine("紧急联系人电话：" + (userDetail.EmergencyContactPhone ?? "未设置"));
                stringBuilder.AppendLine("血型：" + (userDetail.BloodType ?? "未设置"));
                stringBuilder.AppendLine("慢性病史：" + (userDetail.PastMedicalHistory ?? "无"));
                stringBuilder.AppendLine("传染病史：" + (userDetail.InfectiousDiseaseHistory ?? "无"));
                stringBuilder.AppendLine("过敏史：" + (userDetail.AllergyHistory ?? "无"));
                stringBuilder.AppendLine("家族病史：" + (userDetail.FamilyMedicalHistory ?? "无"));
                stringBuilder.AppendLine("详细信息创建时间：" + userDetail.CreatedAt.ToString("yyyy年MM月dd日 HH:mm:ss"));
                stringBuilder.AppendLine("详细信息最后更新时间：" + (userDetail.UpdatedAt.ToString("yyyy年MM月dd日 HH:mm:ss")));
            }

            return stringBuilder.ToString();
        }
    }
}