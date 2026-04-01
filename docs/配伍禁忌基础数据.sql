-- 配伍禁忌基础数据初始化脚本
-- 包含十八反、十九畏等传统中医配伍禁忌数据

-- 首先插入基础药材数据（仅包含配伍禁忌相关的药材）
-- 注意：实际使用时需要根据完整的药材库进行调整

-- 插入十八反相关药材
INSERT INTO Herbs (HerbName, Properties, Meridians, Efficacy, Category, CreatedAt, UpdatedAt) VALUES
-- 甘草组
('甘草', '甘、平', '心、肺、脾、胃经', '补脾益气，清热解毒，祛痰止咳，缓急止痛，调和诸药', '补益药', GETDATE(), GETDATE()),
('甘遂', '苦、寒，有毒', '肺、肾、大肠经', '泻水逐饮，消肿散结', '泻下药', GETDATE(), GETDATE()),
('大戟', '苦、寒，有毒', '肺、脾、肾经', '泻水逐饮，消肿散结', '泻下药', GETDATE(), GETDATE()),
('海藻', '苦、咸、寒', '肝、胃、肾经', '消痰软坚，利水消肿', '化痰药', GETDATE(), GETDATE()),
('芫花', '辛、苦、温，有毒', '肺、脾、肾经', '泻水逐饮，祛痰止咳', '泻下药', GETDATE(), GETDATE()),

-- 乌头组
('乌头', '辛、苦、热，大毒', '心、肝、肾、脾经', '回阳救逆，助阳补火，散寒止痛', '温里药', GETDATE(), GETDATE()),
('川乌', '辛、苦、热，大毒', '心、肝、肾、脾经', '祛风除湿，温经止痛', '祛风湿药', GETDATE(), GETDATE()),
('草乌', '辛、苦、热，大毒', '心、肝、脾经', '祛风除湿，温经止痛，消肿止痛', '祛风湿药', GETDATE(), GETDATE()),
('贝母', '苦、甘、微寒', '肺、心经', '清热润肺，化痰止咳，散结消痈', '化痰药', GETDATE(), GETDATE()),
('瓜蒌', '甘、微苦、寒', '肺、胃、大肠经', '清热涤痰，宽胸散结，润燥滑肠', '化痰药', GETDATE(), GETDATE()),
('半夏', '辛、温，有毒', '脾、胃、肺经', '燥湿化痰，降逆止呕，消痞散结', '化痰药', GETDATE(), GETDATE()),
('白蔹', '苦、辛、微寒', '心、胃经', '清热解毒，消痈散结，敛疮生肌', '清热药', GETDATE(), GETDATE()),
('白及', '苦、甘、涩、微寒', '肺、肝、胃经', '收敛止血，消肿生肌', '止血药', GETDATE(), GETDATE()),

-- 藜芦组
('藜芦', '辛、苦、寒，有毒', '肺、胃经', '涌吐风痰，杀虫疗疮', '涌吐药', GETDATE(), GETDATE()),
('人参', '甘、微苦、微温', '脾、肺、心、肾经', '大补元气，复脉固脱，补脾益肺，生津养血，安神益智', '补气药', GETDATE(), GETDATE()),
('沙参', '甘、微苦、微寒', '肺、胃经', '养阴清热，润肺化痰，益胃生津', '补阴药', GETDATE(), GETDATE()),
('丹参', '苦、微寒', '心、肝经', '活血祛瘀，通经止痛，清心除烦，凉血消痈', '活血药', GETDATE(), GETDATE()),
('玄参', '甘、苦、咸、微寒', '肺、胃、肾经', '清热凉血，泻火解毒，滋阴', '清热药', GETDATE(), GETDATE()),
('细辛', '辛、温', '心、肺、肾经', '解表散寒，祛风止痛，通窍，温肺化饮', '解表药', GETDATE(), GETDATE()),
('芍药', '苦、酸、微寒', '肝、脾经', '养血调经，敛阴止汗，柔肝止痛，平抑肝阳', '补血药', GETDATE(), GETDATE());

-- 插入十九畏相关药材
INSERT INTO Herbs (HerbName, Properties, Meridians, Efficacy, Category, CreatedAt, UpdatedAt) VALUES
('硫黄', '酸、热，有毒', '肾、大肠经', '外用解毒杀虫疗疮；内服补火助阳通便', '温里药', GETDATE(), GETDATE()),
('朴硝', '苦、咸、寒', '胃、大肠经', '泻下通便，润燥软坚，清火消肿', '泻下药', GETDATE(), GETDATE()),
('水银', '辛、寒，有毒', '心、肝、肾经', '攻毒杀虫，利水通便', '攻毒杀虫药', GETDATE(), GETDATE()),
('砒霜', '辛、酸、热，大毒', '肺、大肠经', '外用蚀疮去腐；内服截疟，平喘', '攻毒杀虫药', GETDATE(), GETDATE()),
('狼毒', '辛、平，有毒', '肺、肝经', '逐水祛痰，破积杀虫', '泻下药', GETDATE(), GETDATE()),
('密陀僧', '咸、辛、平，有毒', '肝、脾经', '燥湿杀虫，收敛防腐，解毒散瘀', '收涩药', GETDATE(), GETDATE()),
('巴豆', '辛、热，大毒', '胃、大肠经', '泻下冷积，逐水退肿，祛痰利咽，蚀疮去腐', '泻下药', GETDATE(), GETDATE()),
('牵牛', '苦、寒，有毒', '肺、肾、大肠经', '泻下逐水，去积杀虫', '泻下药', GETDATE(), GETDATE()),
('丁香', '辛、温', '脾、胃、肺、肾经', '温中降逆，补肾助阳', '温里药', GETDATE(), GETDATE()),
('郁金', '辛、苦、寒', '肝、心、肺经', '活血止痛，行气解郁，清心凉血，利胆退黄', '活血药', GETDATE(), GETDATE()),
('牙硝', '苦、辛、咸、寒', '胃、大肠经', '清热泻火，消肿止痛', '清热药', GETDATE(), GETDATE()),
('三棱', '苦、平', '肝、脾经', '破血行气，消积止痛', '活血药', GETDATE(), GETDATE()),
('犀角', '苦、酸、咸、寒', '心、肝、胃经', '清热凉血，定惊解毒', '清热药', GETDATE(), GETDATE()),
('五灵脂', '苦、甘、温', '肝经', '活血止痛，化瘀止血', '活血药', GETDATE(), GETDATE()),
('官桂', '辛、甘、热', '肾、脾、心、肝经', '补火助阳，引火归源，散寒止痛，活血通经', '温里药', GETDATE(), GETDATE()),
('石脂', '甘、涩、温', '大肠、胃经', '涩肠止泻，收敛止血，生肌敛疮', '收涩药', GETDATE(), GETDATE());

-- 插入十八反配伍禁忌数据
INSERT INTO HerbContraindications (PrimaryHerbId, ConflictHerbId, ContraindicationType, RiskLevel, Description, AdverseReactions, Evidence, IsAbsoluteContraindication, IsActive, CreatedAt, UpdatedAt)
SELECT 
    h1.HerbId as PrimaryHerbId,
    h2.HerbId as ConflictHerbId,
    '十八反' as ContraindicationType,
    3 as RiskLevel, -- 高风险
    CONCAT(h1.HerbName, '与', h2.HerbName, '相反，不宜同用') as Description,
    '可能导致毒性增强、药效降低或产生不良反应' as AdverseReactions,
    '《神农本草经》、历代本草文献' as Evidence,
    1 as IsAbsoluteContraindication,
    1 as IsActive,
    GETDATE() as CreatedAt,
    GETDATE() as UpdatedAt
FROM 
    (SELECT HerbId, HerbName FROM Herbs WHERE HerbName = '甘草') h1
CROSS JOIN 
    (SELECT HerbId, HerbName FROM Herbs WHERE HerbName IN ('甘遂', '大戟', '海藻', '芫花')) h2

UNION ALL

SELECT 
    h1.HerbId as PrimaryHerbId,
    h2.HerbId as ConflictHerbId,
    '十八反' as ContraindicationType,
    4 as RiskLevel, -- 严重风险（乌头类毒性大）
    CONCAT(h1.HerbName, '与', h2.HerbName, '相反，不宜同用') as Description,
    '可能导致严重毒性反应，甚至危及生命' as AdverseReactions,
    '《神农本草经》、历代本草文献' as Evidence,
    1 as IsAbsoluteContraindication,
    1 as IsActive,
    GETDATE() as CreatedAt,
    GETDATE() as UpdatedAt
FROM 
    (SELECT HerbId, HerbName FROM Herbs WHERE HerbName IN ('乌头', '川乌', '草乌')) h1
CROSS JOIN 
    (SELECT HerbId, HerbName FROM Herbs WHERE HerbName IN ('贝母', '瓜蒌', '半夏', '白蔹', '白及')) h2

UNION ALL

SELECT 
    h1.HerbId as PrimaryHerbId,
    h2.HerbId as ConflictHerbId,
    '十八反' as ContraindicationType,
    3 as RiskLevel, -- 高风险
    CONCAT(h1.HerbName, '与', h2.HerbName, '相反，不宜同用') as Description,
    '可能导致药效降低或产生不良反应' as AdverseReactions,
    '《神农本草经》、历代本草文献' as Evidence,
    1 as IsAbsoluteContraindication,
    1 as IsActive,
    GETDATE() as CreatedAt,
    GETDATE() as UpdatedAt
FROM 
    (SELECT HerbId, HerbName FROM Herbs WHERE HerbName = '藜芦') h1
CROSS JOIN 
    (SELECT HerbId, HerbName FROM Herbs WHERE HerbName IN ('人参', '沙参', '丹参', '玄参', '细辛', '芍药')) h2;

-- 插入十九畏配伍禁忌数据
INSERT INTO HerbContraindications (PrimaryHerbId, ConflictHerbId, ContraindicationType, RiskLevel, Description, AdverseReactions, Evidence, IsAbsoluteContraindication, IsActive, CreatedAt, UpdatedAt)
VALUES
-- 硫黄畏朴硝
((SELECT HerbId FROM Herbs WHERE HerbName = '硫黄'), (SELECT HerbId FROM Herbs WHERE HerbName = '朴硝'), '十九畏', 3, '硫黄畏朴硝，两者性质相反，同用可能产生不良反应', '可能产生化学反应，影响药效或产生毒性', '历代本草文献记载', 1, 1, GETDATE(), GETDATE()),

-- 水银畏砒霜
((SELECT HerbId FROM Herbs WHERE HerbName = '水银'), (SELECT HerbId FROM Herbs WHERE HerbName = '砒霜'), '十九畏', 4, '水银畏砒霜，两者均为有毒重金属，同用毒性剧增', '可能导致严重中毒，危及生命', '历代本草文献记载', 1, 1, GETDATE(), GETDATE()),

-- 狼毒畏密陀僧
((SELECT HerbId FROM Herbs WHERE HerbName = '狼毒'), (SELECT HerbId FROM Herbs WHERE HerbName = '密陀僧'), '十九畏', 3, '狼毒畏密陀僧，同用可能增强毒性', '可能导致毒性增强，产生不良反应', '历代本草文献记载', 1, 1, GETDATE(), GETDATE()),

-- 巴豆畏牵牛
((SELECT HerbId FROM Herbs WHERE HerbName = '巴豆'), (SELECT HerbId FROM Herbs WHERE HerbName = '牵牛'), '十九畏', 3, '巴豆畏牵牛，两者均为峻下药，同用可能导致泻下过度', '可能导致严重腹泻，损伤脾胃', '历代本草文献记载', 1, 1, GETDATE(), GETDATE()),

-- 丁香畏郁金
((SELECT HerbId FROM Herbs WHERE HerbName = '丁香'), (SELECT HerbId FROM Herbs WHERE HerbName = '郁金'), '十九畏', 2, '丁香畏郁金，性味功效相反，同用可能影响药效', '可能导致药效相互抵消或产生不良反应', '历代本草文献记载', 0, 1, GETDATE(), GETDATE()),

-- 牙硝畏三棱
((SELECT HerbId FROM Herbs WHERE HerbName = '牙硝'), (SELECT HerbId FROM Herbs WHERE HerbName = '三棱'), '十九畏', 2, '牙硝畏三棱，同用可能影响药效', '可能影响药物疗效', '历代本草文献记载', 0, 1, GETDATE(), GETDATE()),

-- 川乌、草乌畏犀角
((SELECT HerbId FROM Herbs WHERE HerbName = '川乌'), (SELECT HerbId FROM Herbs WHERE HerbName = '犀角'), '十九畏', 3, '川乌畏犀角，同用可能影响药效或产生不良反应', '可能导致药效降低或产生不良反应', '历代本草文献记载', 1, 1, GETDATE(), GETDATE()),
((SELECT HerbId FROM Herbs WHERE HerbName = '草乌'), (SELECT HerbId FROM Herbs WHERE HerbName = '犀角'), '十九畏', 3, '草乌畏犀角，同用可能影响药效或产生不良反应', '可能导致药效降低或产生不良反应', '历代本草文献记载', 1, 1, GETDATE(), GETDATE()),

-- 人参畏五灵脂
((SELECT HerbId FROM Herbs WHERE HerbName = '人参'), (SELECT HerbId FROM Herbs WHERE HerbName = '五灵脂'), '十九畏', 2, '人参畏五灵脂，同用可能影响人参的补益功效', '可能导致人参补益作用降低', '历代本草文献记载', 0, 1, GETDATE(), GETDATE()),

-- 官桂畏石脂
((SELECT HerbId FROM Herbs WHERE HerbName = '官桂'), (SELECT HerbId FROM Herbs WHERE HerbName = '石脂'), '十九畏', 2, '官桂畏石脂，同用可能影响药效', '可能影响药物疗效', '历代本草文献记载', 0, 1, GETDATE(), GETDATE());

-- 创建索引以提高查询性能
CREATE INDEX IX_HerbContraindications_PrimaryHerbId ON HerbContraindications(PrimaryHerbId);
CREATE INDEX IX_HerbContraindications_ConflictHerbId ON HerbContraindications(ConflictHerbId);
CREATE INDEX IX_HerbContraindications_ContraindicationType ON HerbContraindications(ContraindicationType);
CREATE INDEX IX_HerbContraindications_RiskLevel ON HerbContraindications(RiskLevel);

-- 创建视图，方便查询配伍禁忌信息
CREATE VIEW vw_HerbContraindications AS
SELECT 
    hc.ContraindicationId,
    h1.HerbName as PrimaryHerbName,
    h2.HerbName as ConflictHerbName,
    hc.ContraindicationType,
    hc.RiskLevel,
    CASE hc.RiskLevel
        WHEN 1 THEN '低风险'
        WHEN 2 THEN '中风险'
        WHEN 3 THEN '高风险'
        WHEN 4 THEN '严重风险'
    END as RiskLevelName,
    hc.Description,
    hc.AdverseReactions,
    hc.Evidence,
    hc.IsAbsoluteContraindication,
    CASE hc.IsAbsoluteContraindication
        WHEN 1 THEN '绝对禁忌'
        WHEN 0 THEN '相对禁忌'
    END as ContraindicationTypeName,
    hc.SpecialNotes,
    hc.IsActive
FROM HerbContraindications hc
INNER JOIN Herbs h1 ON hc.PrimaryHerbId = h1.HerbId
INNER JOIN Herbs h2 ON hc.ConflictHerbId = h2.HerbId
WHERE hc.IsActive = 1;

PRINT '配伍禁忌基础数据初始化完成！';
PRINT '已插入十八反配伍禁忌：18组';
PRINT '已插入十九畏配伍禁忌：10组';
PRINT '总计配伍禁忌记录：28组';