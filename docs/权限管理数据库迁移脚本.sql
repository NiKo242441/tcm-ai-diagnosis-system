-- ============================================
-- 权限管理系统 - 数据库迁移脚本
-- 创建日期: 2026-01-22
-- 说明: 创建权限管理相关表结构
-- ============================================

-- 1. 创建权限表 (permissions)
CREATE TABLE IF NOT EXISTS `permissions` (
    `permission_id` INT NOT NULL AUTO_INCREMENT COMMENT '权限ID',
    `permission_code` VARCHAR(100) NOT NULL COMMENT '权限代码（唯一标识）',
    `permission_name` VARCHAR(100) NOT NULL COMMENT '权限名称',
    `description` VARCHAR(500) NULL COMMENT '权限描述',
    `category` VARCHAR(50) NOT NULL COMMENT '权限分类',
    `module` VARCHAR(50) NOT NULL COMMENT '权限模块',
    `is_system` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否为系统权限',
    `is_active` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否启用',
    `sort_order` INT NOT NULL DEFAULT 0 COMMENT '排序号',
    `created_at` DATETIME(6) NOT NULL COMMENT '创建时间',
    `updated_at` DATETIME(6) NOT NULL COMMENT '更新时间',
    PRIMARY KEY (`permission_id`),
    UNIQUE INDEX `IX_Permission_Code` (`permission_code`),
    INDEX `IX_Permission_Category` (`category`),
    INDEX `IX_Permission_Module` (`module`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='权限表';

-- 2. 创建角色权限关联表 (role_permissions)
CREATE TABLE IF NOT EXISTS `role_permissions` (
    `role_id` INT NOT NULL COMMENT '角色ID',
    `permission_id` INT NOT NULL COMMENT '权限ID',
    `is_granted` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否允许',
    `granted_at` DATETIME(6) NOT NULL COMMENT '分配时间',
    `granted_by` INT NULL COMMENT '分配人ID',
    `remarks` VARCHAR(500) NULL COMMENT '备注',
    PRIMARY KEY (`role_id`, `permission_id`),
    INDEX `IX_RolePermission_RoleId` (`role_id`),
    INDEX `IX_RolePermission_PermissionId` (`permission_id`),
    CONSTRAINT `FK_RolePermission_Role` FOREIGN KEY (`role_id`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_RolePermission_Permission` FOREIGN KEY (`permission_id`) REFERENCES `permissions` (`permission_id`) ON DELETE CASCADE,
    CONSTRAINT `FK_RolePermission_GrantedBy` FOREIGN KEY (`granted_by`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='角色权限关联表';

-- 3. 创建权限变更日志表 (permission_change_logs)
CREATE TABLE IF NOT EXISTS `permission_change_logs` (
    `log_id` BIGINT NOT NULL AUTO_INCREMENT COMMENT '日志ID',
    `change_type` VARCHAR(20) NOT NULL COMMENT '变更类型（Grant-授予, Revoke-撤销, Modify-修改）',
    `target_type` VARCHAR(20) NOT NULL COMMENT '目标类型（Role-角色, User-用户）',
    `target_id` INT NOT NULL COMMENT '目标ID',
    `target_name` VARCHAR(100) NOT NULL COMMENT '目标名称',
    `permission_id` INT NOT NULL COMMENT '权限ID',
    `permission_code` VARCHAR(100) NOT NULL COMMENT '权限代码',
    `permission_name` VARCHAR(100) NOT NULL COMMENT '权限名称',
    `before_state` TEXT NULL COMMENT '变更前状态（JSON格式）',
    `after_state` TEXT NULL COMMENT '变更后状态（JSON格式）',
    `change_reason` VARCHAR(500) NOT NULL COMMENT '变更原因',
    `applicant_id` INT NULL COMMENT '申请人ID',
    `applicant_name` VARCHAR(100) NULL COMMENT '申请人姓名',
    `approver_id` INT NULL COMMENT '审批人ID',
    `approver_name` VARCHAR(100) NULL COMMENT '审批人姓名',
    `approval_status` VARCHAR(20) NOT NULL DEFAULT 'Approved' COMMENT '审批状态',
    `approval_comments` VARCHAR(500) NULL COMMENT '审批意见',
    `approved_at` DATETIME(6) NULL COMMENT '审批时间',
    `operated_by` INT NOT NULL COMMENT '操作人ID',
    `operated_by_name` VARCHAR(100) NOT NULL COMMENT '操作人姓名',
    `operated_at` DATETIME(6) NOT NULL COMMENT '操作时间',
    `client_ip` VARCHAR(45) NOT NULL COMMENT '客户端IP地址',
    `user_agent` VARCHAR(500) NULL COMMENT '用户代理',
    `tenant_id` INT NULL COMMENT '租户ID',
    `is_cross_tenant` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '是否跨租户操作',
    `valid_from` DATETIME(6) NULL COMMENT '有效期开始时间',
    `valid_to` DATETIME(6) NULL COMMENT '有效期结束时间',
    `remarks` VARCHAR(1000) NULL COMMENT '备注',
    PRIMARY KEY (`log_id`),
    INDEX `IX_PermissionChangeLog_ChangeType` (`change_type`),
    INDEX `IX_PermissionChangeLog_Target` (`target_type`, `target_id`),
    INDEX `IX_PermissionChangeLog_PermissionId` (`permission_id`),
    INDEX `IX_PermissionChangeLog_OperatedAt` (`operated_at`),
    INDEX `IX_PermissionChangeLog_TenantId` (`tenant_id`),
    CONSTRAINT `FK_PermissionChangeLog_Permission` FOREIGN KEY (`permission_id`) REFERENCES `permissions` (`permission_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_PermissionChangeLog_Applicant` FOREIGN KEY (`applicant_id`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_PermissionChangeLog_Approver` FOREIGN KEY (`approver_id`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_PermissionChangeLog_Operator` FOREIGN KEY (`operated_by`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_PermissionChangeLog_Tenant` FOREIGN KEY (`tenant_id`) REFERENCES `tenants` (`tenant_id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='权限变更日志表';

-- 4. 创建临时权限表 (temporary_permissions)
CREATE TABLE IF NOT EXISTS `temporary_permissions` (
    `temp_permission_id` BIGINT NOT NULL AUTO_INCREMENT COMMENT '临时权限ID',
    `scenario_type` VARCHAR(50) NOT NULL COMMENT '场景类型（Consultation-会诊, Referral-转诊, Maintenance-系统维护）',
    `user_id` INT NOT NULL COMMENT '用户ID（被授权用户）',
    `permission_id` INT NOT NULL COMMENT '权限ID',
    `resource_type` VARCHAR(50) NOT NULL COMMENT '资源类型',
    `resource_id` INT NOT NULL COMMENT '资源ID',
    `source_tenant_id` INT NOT NULL COMMENT '授权租户ID',
    `target_tenant_id` INT NULL COMMENT '目标租户ID',
    `grant_reason` VARCHAR(500) NOT NULL COMMENT '授权原因',
    `granted_by` INT NOT NULL COMMENT '授权人ID',
    `granted_at` DATETIME(6) NOT NULL COMMENT '授权时间',
    `valid_from` DATETIME(6) NOT NULL COMMENT '有效期开始时间',
    `valid_to` DATETIME(6) NOT NULL COMMENT '有效期结束时间',
    `requires_patient_consent` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '是否需要患者授权',
    `patient_consent_status` VARCHAR(20) NULL COMMENT '患者授权状态',
    `patient_consent_at` DATETIME(6) NULL COMMENT '患者授权时间',
    `requires_admin_approval` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '是否需要租户管理员批准',
    `admin_approval_status` VARCHAR(20) NULL COMMENT '管理员批准状态',
    `approved_by` INT NULL COMMENT '管理员批准人ID',
    `approved_at` DATETIME(6) NULL COMMENT '管理员批准时间',
    `approval_comments` VARCHAR(500) NULL COMMENT '批准意见',
    `status` VARCHAR(20) NOT NULL DEFAULT 'Active' COMMENT '权限状态',
    `revoked_by` INT NULL COMMENT '撤销人ID',
    `revoked_at` DATETIME(6) NULL COMMENT '撤销时间',
    `revoke_reason` VARCHAR(500) NULL COMMENT '撤销原因',
    `auto_revoke` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否自动回收',
    `access_limit` INT NOT NULL DEFAULT 0 COMMENT '访问次数限制',
    `access_count` INT NOT NULL DEFAULT 0 COMMENT '已访问次数',
    `last_accessed_at` DATETIME(6) NULL COMMENT '最后访问时间',
    `remarks` VARCHAR(1000) NULL COMMENT '备注',
    PRIMARY KEY (`temp_permission_id`),
    INDEX `IX_TemporaryPermission_ScenarioType` (`scenario_type`),
    INDEX `IX_TemporaryPermission_UserId` (`user_id`),
    INDEX `IX_TemporaryPermission_Resource` (`resource_type`, `resource_id`),
    INDEX `IX_TemporaryPermission_Status` (`status`),
    INDEX `IX_TemporaryPermission_ValidPeriod` (`valid_from`, `valid_to`),
    INDEX `IX_TemporaryPermission_SourceTenantId` (`source_tenant_id`),
    CONSTRAINT `FK_TemporaryPermission_User` FOREIGN KEY (`user_id`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_TemporaryPermission_Permission` FOREIGN KEY (`permission_id`) REFERENCES `permissions` (`permission_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_TemporaryPermission_GrantedBy` FOREIGN KEY (`granted_by`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_TemporaryPermission_ApprovedBy` FOREIGN KEY (`approved_by`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_TemporaryPermission_RevokedBy` FOREIGN KEY (`revoked_by`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_TemporaryPermission_SourceTenant` FOREIGN KEY (`source_tenant_id`) REFERENCES `tenants` (`tenant_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_TemporaryPermission_TargetTenant` FOREIGN KEY (`target_tenant_id`) REFERENCES `tenants` (`tenant_id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='临时权限表';

-- ============================================
-- 迁移完成
-- ============================================
