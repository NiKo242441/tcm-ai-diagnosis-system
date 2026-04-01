using Microsoft.EntityFrameworkCore;
using TcmAiDiagnosis.EFContext;
using TcmAiDiagnosis.Entities;
using TcmAiDiagnosis.Domain.Paged;
using System.Linq.Expressions;

namespace TcmAiDiagnosis.Domain
{
    /// <summary>
    /// 中药材领域服务类，处理药材相关业务逻辑
    /// </summary>
    public class HerbDomain
    {
        private readonly TcmAiDiagnosisContext _context;

        public HerbDomain(TcmAiDiagnosisContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 添加新药材
        /// </summary>
        public async Task<Herb> AddHerbAsync(Herb herb)
        {
            // 基础验证
            if (string.IsNullOrWhiteSpace(herb.Name))
                throw new ArgumentException("药材名称不能为空");

            // 检查是否已存在同名药材
            var exists = await _context.Herbs
                .AnyAsync(h => h.Name == herb.Name);
            if (exists)
                throw new InvalidOperationException($"药材名称「{herb.Name}」已存在");

            // 设置默认值
            herb.CreatedAt = DateTime.UtcNow;
            herb.UpdatedAt = DateTime.UtcNow;
            herb.IsDeleted = false; // 默认未删除

            _context.Herbs.Add(herb);
            await _context.SaveChangesAsync();

            return herb;
        }

        /// <summary>
        /// 更新药材信息
        /// </summary>
        public async Task UpdateHerbAsync(Herb updatedHerb)
        {
            var existingHerb = await _context.Herbs
                .FirstOrDefaultAsync(h => h.Id == updatedHerb.Id)
                ?? throw new KeyNotFoundException("未找到指定药材");

            // 检查名称是否重复（排除当前药材）
            if (existingHerb.Name != updatedHerb.Name && await _context.Herbs
                .AnyAsync(h => h.Name == updatedHerb.Name && h.Id != updatedHerb.Id))
            {
                throw new InvalidOperationException($"药材名称「{updatedHerb.Name}」已存在");
            }

            // 更新可编辑字段 - 修正属性映射
            existingHerb.Name = updatedHerb.Name;
            existingHerb.Properties = updatedHerb.Properties;
            existingHerb.Meridians = updatedHerb.Meridians;
            existingHerb.Efficacy = updatedHerb.Efficacy;
            existingHerb.Category = updatedHerb.Category;
            existingHerb.DosageRange = updatedHerb.DosageRange; // 修正：DosageRange
            existingHerb.Precautions = updatedHerb.Precautions;
            existingHerb.ProcessingMethods = updatedHerb.ProcessingMethods; // 修正：ProcessingMethods
            existingHerb.ToxicityLevel = updatedHerb.ToxicityLevel;
            existingHerb.CommonUnit = updatedHerb.CommonUnit;
            existingHerb.Contraindications = updatedHerb.Contraindications;
            existingHerb.Aliases = updatedHerb.Aliases;
            existingHerb.IsCommonlyUsed = updatedHerb.IsCommonlyUsed;
            existingHerb.RequiresSpecialHandling = updatedHerb.RequiresSpecialHandling;
            existingHerb.SpecialUsageInstructions = updatedHerb.SpecialUsageInstructions;
            existingHerb.IsAiOriginated = updatedHerb.IsAiOriginated;
            existingHerb.Status = updatedHerb.Status;
            existingHerb.IsDeleted = updatedHerb.IsDeleted;
            existingHerb.UpdatedAt = DateTime.UtcNow;
            existingHerb.UpdatedByUserId = updatedHerb.UpdatedByUserId;

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 根据ID获取药材详情（包含配伍禁忌信息）
        /// </summary>
        public async Task<Herb?> GetHerbByIdAsync(int herbId, bool includeContraindications = false)
        {
            var query = _context.Herbs.AsQueryable();

            if (includeContraindications)
            {
                query = query
                    .Include(h => h.HerbContraindications)
                    .ThenInclude(hc => hc.ConflictHerb);
            }

            return await query.FirstOrDefaultAsync(h => h.Id == herbId && !h.IsDeleted); // 修正：Id 和 IsDeleted
        }

        /// <summary>
        /// 分页查询药材列表
        /// </summary>
        public async Task<PagedResult<Herb>> GetPagedHerbsAsync(PagedRequest request, string? category = null, int? toxicityLevel = null) // 改回 int?
        {
            var query = _context.Herbs.Where(h => !h.IsDeleted); // 只查询未删除的记录

            // 分类筛选
            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(h => h.Category == category);

            // 毒性筛选 - 使用整数值比较
            if (toxicityLevel.HasValue)
            {
                // 将整数值转换为对应的毒性等级字符串
                var toxicityString = toxicityLevel.Value switch
                {
                    0 => "无毒",
                    1 => "小毒",
                    2 => "有毒",
                    3 => "大毒",
                    _ => null
                };

                if (!string.IsNullOrEmpty(toxicityString))
                {
                    query = query.Where(h => h.ToxicityLevel == toxicityString);
                }
            }

            // 搜索筛选
            if (!string.IsNullOrWhiteSpace(request.SearchKeyword))
            {
                var keyword = request.SearchKeyword.Trim();
                query = query.Where(h =>
                    h.Name.Contains(keyword) ||
                    h.Properties.Contains(keyword) ||
                    h.Meridians.Contains(keyword) ||
                    h.Efficacy.Contains(keyword) ||
                    h.Category.Contains(keyword) ||
                    h.Aliases.Contains(keyword));
            }

            // 排序
            query = query.OrderBy(h => h.Name);

            return await query.ToPagedResultAsync(request.Page, request.PageSize);
        }

        /// <summary>
        /// 获取药材分类列表
        /// </summary>
        public async Task<List<string>> GetHerbCategoriesAsync()
        {
            return await _context.Herbs
                .Where(h => !h.IsDeleted && !string.IsNullOrWhiteSpace(h.Category))
                .Select(h => h.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        /// <summary>
        /// 批量导入药材（跳过重复项）
        /// </summary>
        public async Task<(int successCount, int skipCount)> BatchImportHerbsAsync(List<Herb> herbs)
        {
            if (herbs == null || !herbs.Any())
                return (0, 0);

            var existingNames = await _context.Herbs
                .Where(h => !h.IsDeleted)
                .Select(h => h.Name) // 修正：Name
                .ToListAsync();

            var validHerbs = new List<Herb>();
            int skipCount = 0;

            foreach (var herb in herbs)
            {
                if (string.IsNullOrWhiteSpace(herb.Name) || existingNames.Contains(herb.Name)) // 修正：Name
                {
                    skipCount++;
                    continue;
                }

                herb.CreatedAt = DateTime.UtcNow;
                herb.UpdatedAt = DateTime.UtcNow;
                herb.IsDeleted = false;

                validHerbs.Add(herb);
                existingNames.Add(herb.Name); // 避免同批次内重复
            }

            if (validHerbs.Any())
            {
                await _context.Herbs.AddRangeAsync(validHerbs);
                await _context.SaveChangesAsync();
            }

            return (validHerbs.Count, skipCount);
        }

        /// <summary>
        /// 获取有毒药材列表
        /// </summary>
        public async Task<List<Herb>> GetToxicHerbsAsync()
        {
            return await _context.Herbs
                .Where(h => !h.IsDeleted && h.ToxicityLevel != "无毒" && !string.IsNullOrEmpty(h.ToxicityLevel)) // 修正：使用字符串比较
                .OrderByDescending(h => h.ToxicityLevel) // 按毒性等级排序
                .ThenBy(h => h.Name) // 修正：Name
                .ToListAsync();
        }

        /// <summary>
        /// 检查药材是否存在配伍禁忌
        /// </summary>
        public async Task<bool> HasContraindicationsAsync(int herbId)
        {
            return await _context.HerbContraindications
                .AnyAsync(c => c.PrimaryHerbId == herbId || c.ConflictHerbId == herbId);
        }

        /// <summary>
        /// 获取药材的配伍禁忌列表
        /// </summary>
        public async Task<List<Herb>> GetContraindicatedHerbsAsync(int herbId)
        {
            var contraindications = await _context.HerbContraindications
                .Where(c => c.PrimaryHerbId == herbId || c.ConflictHerbId == herbId)
                .Include(c => c.PrimaryHerb)
                .Include(c => c.ConflictHerb)
                .ToListAsync();

            var result = contraindications
                .Select(c => c.PrimaryHerbId == herbId ? c.ConflictHerb : c.PrimaryHerb)
                .Where(h => h != null && !h.IsDeleted)
                .ToList();

            return result;
        }

        /// <summary>
        /// 检查一组药材是否存在配伍禁忌
        /// </summary>
        /// <param name="herbIds">用户选择的药材ID列表</param>
        /// <returns>禁忌药材对列表</returns>
        public async Task<List<(Herb HerbA, Herb HerbB)>> CheckHerbConflictsAsync(List<int> herbIds)
        {
            var conflicts = new List<(Herb, Herb)>();

            if (herbIds == null || herbIds.Count < 2)
                return conflicts; // 少于两个药材就没有禁忌

            // 查询所有相关禁忌
            var contraindications = await _context.HerbContraindications
                .Include(c => c.PrimaryHerb)
                .Include(c => c.ConflictHerb)
                .Where(c => herbIds.Contains(c.PrimaryHerbId) && herbIds.Contains(c.ConflictHerbId))
                .Where(c => !c.PrimaryHerb.IsDeleted && !c.ConflictHerb.IsDeleted) // 只检查未删除的药材
                .ToListAsync();

            foreach (var c in contraindications)
            {
                conflicts.Add((c.PrimaryHerb, c.ConflictHerb));
            }

            return conflicts;
        }

        /// <summary>
        /// 软删除药材
        /// </summary>
        public async Task SoftDeleteHerbAsync(int herbId, int? deletedByUserId = null)
        {
            var herb = await _context.Herbs
                .FirstOrDefaultAsync(h => h.Id == herbId && !h.IsDeleted)
                ?? throw new KeyNotFoundException("未找到指定药材");

            herb.IsDeleted = true;
            herb.UpdatedAt = DateTime.UtcNow;
            herb.UpdatedByUserId = deletedByUserId;

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 恢复已删除的药材
        /// </summary>
        public async Task RestoreHerbAsync(int herbId, int? restoredByUserId = null)
        {
            var herb = await _context.Herbs
                .FirstOrDefaultAsync(h => h.Id == herbId && h.IsDeleted)
                ?? throw new KeyNotFoundException("未找到已删除的药材");

            herb.IsDeleted = false;
            herb.UpdatedAt = DateTime.UtcNow;
            herb.UpdatedByUserId = restoredByUserId;

            await _context.SaveChangesAsync();
        }
    }
}