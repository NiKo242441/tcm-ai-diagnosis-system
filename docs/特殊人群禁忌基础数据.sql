-- 特殊人群禁忌基础数据
-- 用于中医药材对特殊人群（老人、儿童、孕妇、哺乳期妇女）的禁忌管理

-- 插入特殊人群禁忌数据
INSERT INTO HerbSpecialPopulationContraindications 
(HerbId, SpecialPopulationType, AgeRange, SeverityLevel, ContraindicationReason, AdverseReactions, AlternativeSuggestions, Evidence, SpecialNotes, CreatedAt, UpdatedAt) 
VALUES

-- 孕妇禁忌药材
(1, 'Pregnant', NULL, 'Critical', '活血化瘀，易导致流产', '子宫收缩、出血、流产', '可用当归身、白芍等温和补血药', '《神农本草经》记载为有毒药材', '孕期全程禁用'),
(2, 'Pregnant', NULL, 'Severe', '破血逐瘀，孕妇慎用', '腹痛、出血、早产', '可用丹参、红花等较温和的活血药', '现代药理研究证实有子宫兴奋作用', '孕早期绝对禁用'),
(3, 'Pregnant', NULL, 'Critical', '峻下逐水，损伤胎气', '腹泻、脱水、流产', '便秘可用火麻仁、郁李仁等润肠药', '《本草纲目》记载孕妇禁用', '整个孕期禁用'),

-- 哺乳期禁忌药材
(4, 'Lactating', NULL, 'Moderate', '苦寒伤胃，影响乳汁分泌', '乳汁减少、婴儿腹泻', '可用党参、黄芪等温补药材', '临床观察发现影响泌乳', '哺乳期慎用，必要时减量'),
(5, 'Lactating', NULL, 'Severe', '辛热燥烈，通过乳汁影响婴儿', '婴儿烦躁、便秘、上火', '可用麦冬、玉竹等滋阴药', '现代研究证实可通过乳汁传递', '哺乳期避免使用'),

-- 儿童禁忌药材
(6, 'Children', '0-12', 'Critical', '毒性较强，儿童肝肾功能未成熟', '肝肾损伤、中毒', '可用金银花、连翘等清热解毒药', '多项研究证实儿童易中毒', '12岁以下儿童禁用'),
(7, 'Children', '0-6', 'Severe', '苦寒伤脾，影响儿童脾胃发育', '腹泻、食欲不振、发育迟缓', '可用太子参、白术等健脾药', '中医理论认为儿童脾常不足', '6岁以下慎用'),
(8, 'Children', '0-3', 'Moderate', '辛散耗气，婴幼儿慎用', '出汗过多、精神萎靡', '感冒可用紫苏叶、生姜等温和发散药', '婴幼儿汗腺发育不完全', '3岁以下减量使用'),

-- 老年人禁忌药材
(9, 'Elderly', '65+', 'Moderate', '峻下伤正，老年人体虚慎用', '腹泻、脱水、电解质紊乱', '便秘可用肉苁蓉、何首乌等润肠药', '老年人肠胃功能减退', '老年人慎用，必要时减量'),
(10, 'Elderly', '70+', 'Severe', '辛热升散，老年人阴虚慎用', '血压升高、心悸、失眠', '可用天麻、钩藤等平肝药', '老年人多阴虚阳亢', '70岁以上避免使用'),
(11, 'Elderly', '65+', 'Moderate', '苦寒伤阳，老年人阳气不足慎用', '腹泻、畏寒、乏力', '可用干姜、附子等温阳药配伍', '老年人阳气渐衰', '老年人慎用，配伍温药'),

-- 多个特殊人群共同禁忌
(12, 'Pregnant', NULL, 'Critical', '有毒，孕妇禁用', '中毒、流产', '根据具体症状选择安全药材', '古今医家均认为有毒', '孕妇绝对禁用'),
(12, 'Children', '0-12', 'Critical', '有毒，儿童禁用', '中毒、肝肾损伤', '根据具体症状选择安全药材', '儿童中毒案例较多', '儿童绝对禁用'),
(12, 'Lactating', NULL, 'Severe', '有毒，哺乳期禁用', '通过乳汁影响婴儿', '根据具体症状选择安全药材', '可通过乳汁传递毒性', '哺乳期禁用'),

(13, 'Pregnant', NULL, 'Severe', '活血破血，孕妇慎用', '出血、流产风险', '可用当归、川芎等温和活血药', '传统认为破血药孕妇慎用', '孕期慎用'),
(13, 'Children', '0-6', 'Moderate', '药性较强，幼儿慎用', '可能引起不适', '可用丹参等较温和的药材', '幼儿用药需谨慎', '6岁以下慎用'),

-- 年龄段细分的禁忌
(14, 'Children', '0-1', 'Critical', '新生儿肝肾功能不成熟', '代谢障碍、中毒', '新生儿疾病需专业医师指导', '新生儿药物代谢能力差', '1岁以下禁用'),
(14, 'Children', '1-3', 'Severe', '婴幼儿慎用', '不良反应风险高', '可在医师指导下减量使用', '婴幼儿用药需特别谨慎', '3岁以下慎用'),
(14, 'Children', '3-12', 'Moderate', '儿童减量使用', '按体重调整剂量', '可在医师指导下使用', '儿童用药剂量需调整', '需要减量使用'),

(15, 'Elderly', '65-75', 'Moderate', '老年人肝肾功能下降', '药物蓄积风险', '定期监测肝肾功能', '老年人药物代谢能力下降', '需要减量并监测'),
(15, 'Elderly', '75+', 'Severe', '高龄老人慎用', '不良反应风险显著增加', '建议选择更安全的替代药物', '高龄老人多器官功能衰退', '高龄老人避免使用');

-- 添加索引以提高查询性能
CREATE INDEX IF NOT EXISTS idx_herb_special_contraindication_herb_id ON HerbSpecialPopulationContraindications(HerbId);
CREATE INDEX IF NOT EXISTS idx_herb_special_contraindication_population_type ON HerbSpecialPopulationContraindications(SpecialPopulationType);
CREATE INDEX IF NOT EXISTS idx_herb_special_contraindication_severity ON HerbSpecialPopulationContraindications(SeverityLevel);

-- 添加注释
COMMENT ON TABLE HerbSpecialPopulationContraindications IS '中药材特殊人群禁忌表';
COMMENT ON COLUMN HerbSpecialPopulationContraindications.HerbId IS '药材ID，关联Herbs表';
COMMENT ON COLUMN HerbSpecialPopulationContraindications.SpecialPopulationType IS '特殊人群类型：Elderly(老年人)、Children(儿童)、Pregnant(孕妇)、Lactating(哺乳期妇女)';
COMMENT ON COLUMN HerbSpecialPopulationContraindications.AgeRange IS '适用年龄范围，如：0-12、65+、18-65等';
COMMENT ON COLUMN HerbSpecialPopulationContraindications.SeverityLevel IS '严重程度：Mild(轻度)、Moderate(中度)、Severe(重度)、Critical(严重)';
COMMENT ON COLUMN HerbSpecialPopulationContraindications.ContraindicationReason IS '禁忌原因';
COMMENT ON COLUMN HerbSpecialPopulationContraindications.AdverseReactions IS '可能的不良反应';
COMMENT ON COLUMN HerbSpecialPopulationContraindications.AlternativeSuggestions IS '替代药物建议';
COMMENT ON COLUMN HerbSpecialPopulationContraindications.Evidence IS '禁忌依据（文献、临床经验等）';
COMMENT ON COLUMN HerbSpecialPopulationContraindications.SpecialNotes IS '特殊说明';