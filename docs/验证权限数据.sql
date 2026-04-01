-- 权限管理系统数据验证脚本
-- 使用数据库
USE `tcm-ai-diagnosis`;

-- ========================================
-- 1. 检查权限表数据
-- ========================================
SELECT '=== 权限表统计 ===' as '';
SELECT COUNT(*) as '权限总数' FROM permissions;

SELECT '=== 按分类统计权限 ===' as '';
SELECT 
    Category as '权限分类', 
    COUNT(*) as '权限数量'
FROM permissions 
GROUP BY Category 
ORDER BY COUNT(*) DESC;

SELECT '=== 按模块统计权限 ===' as '';
SELECT 
    Module as '权限模块', 
    COUNT(*) as '权限数量'
FROM permissions 
GROUP BY Module 
ORDER BY COUNT(*) DESC;

-- ========================================
-- 2. 检查角色权限关联
-- ========================================
SELECT '=== 角色权限关联统计 ===' as '';
SELECT COUNT(*) as '角色权限关联总数' FROM role_permissions;

SELECT '=== 各角色的权限数量 ===' as '';
SELECT 
    r.Name as '角色代码',
    r.ShowName as '角色名称',
    COUNT(rp.PermissionId) as '权限数量'
FROM AspNetRoles r
LEFT JOIN role_permissions rp ON r.Id = rp.RoleId
GROUP BY r.Id, r.Name, r.ShowName
ORDER BY COUNT(rp.PermissionId) DESC;

-- ========================================
-- 3. 查看Manager角色的权限详情
-- ========================================
SELECT '=== Manager（租户管理员）角色权限 ===' as '';
SELECT 
    p.Category as '权限分类',
    p.PermissionName as '权限名称',
    p.PermissionCode as '权限代码'
FROM role_permissions rp
JOIN permissions p ON rp.PermissionId = p.PermissionId
JOIN AspNetRoles r ON rp.RoleId = r.Id
WHERE r.Name = 'Manager'
ORDER BY p.Category, p.SortOrder;

-- ========================================
-- 4. 查看Doctor角色的权限详情
-- ========================================
SELECT '=== Doctor（医生）角色权限 ===' as '';
SELECT 
    p.Category as '权限分类',
    p.PermissionName as '权限名称',
    p.PermissionCode as '权限代码'
FROM role_permissions rp
JOIN permissions p ON rp.PermissionId = p.PermissionId
JOIN AspNetRoles r ON rp.RoleId = r.Id
WHERE r.Name = 'Doctor'
ORDER BY p.Category, p.SortOrder;

-- ========================================
-- 5. 查看Patient角色的权限详情
-- ========================================
SELECT '=== Patient（患者）角色权限 ===' as '';
SELECT 
    p.Category as '权限分类',
    p.PermissionName as '权限名称',
    p.PermissionCode as '权限代码'
FROM role_permissions rp
JOIN permissions p ON rp.PermissionId = p.PermissionId
JOIN AspNetRoles r ON rp.RoleId = r.Id
WHERE r.Name = 'Patient'
ORDER BY p.Category, p.SortOrder;

-- ========================================
-- 6. 查看Pharmacist角色的权限详情
-- ========================================
SELECT '=== Pharmacist（药剂师）角色权限 ===' as '';
SELECT 
    p.Category as '权限分类',
    p.PermissionName as '权限名称',
    p.PermissionCode as '权限代码'
FROM role_permissions rp
JOIN permissions p ON rp.PermissionId = p.PermissionId
JOIN AspNetRoles r ON rp.RoleId = r.Id
WHERE r.Name = 'Pharmacist'
ORDER BY p.Category, p.SortOrder;

-- ========================================
-- 7. 检查权限变更日志表
-- ========================================
SELECT '=== 权限变更日志统计 ===' as '';
SELECT COUNT(*) as '日志记录数' FROM permission_change_logs;

-- ========================================
-- 8. 检查临时权限表
-- ========================================
SELECT '=== 临时权限统计 ===' as '';
SELECT COUNT(*) as '临时权限记录数' FROM temporary_permissions;

-- ========================================
-- 9. 查看所有权限列表（前20条）
-- ========================================
SELECT '=== 权限列表（前20条）===' as '';
SELECT 
    PermissionId as 'ID',
    PermissionCode as '权限代码',
    PermissionName as '权限名称',
    Category as '分类',
    Module as '模块',
    IsSystem as '系统权限',
    IsActive as '是否启用'
FROM permissions 
ORDER BY Category, Module, SortOrder
LIMIT 20;

-- ========================================
-- 10. 如果数据为空，执行清理脚本
-- ========================================
-- 如果上述查询显示权限数量为0，可以执行以下脚本清空表，然后重启应用
-- DELETE FROM role_permissions;
-- DELETE FROM permissions;
-- 然后重启应用程序，数据初始化会自动运行
