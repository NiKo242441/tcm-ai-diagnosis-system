using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TcmAiDiagnosis.Entities;
using TcmAiDiagnosis.Domain.Exceptions;

namespace TcmAiDiagnosis.Domain.Validators
{
    /// <summary>
    /// 治疗方案数据验证器
    /// 提供输入数据验证、API响应验证和实体数据验证功能
    /// </summary>
    public class TreatmentDataValidator
    {
        private readonly ILogger<TreatmentDataValidator> _logger;

        public TreatmentDataValidator(ILogger<TreatmentDataValidator> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 验证证候数据是否适合生成治疗方案
        /// </summary>
        /// <param name="syndrome">证候实体</param>
        /// <param name="visit">就诊实体</param>
        /// <param name="patientUser">患者用户实体</param>
        /// <exception cref="TreatmentDataValidationException">当数据验证失败时抛出</exception>
        public void ValidateInputData(Syndrome syndrome, Visit visit, User patientUser)
        {
            var errors = new List<string>();

            // 验证证候数据
            if (syndrome == null)
            {
                errors.Add("证候数据不能为空");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(syndrome.SyndromeName))
                {
                    errors.Add("证候名称不能为空");
                }

                if (syndrome.Confidence <= 0 || syndrome.Confidence > 100)
                {
                    errors.Add("证候置信度必须在0-100之间");
                }

                if (syndrome.VisitId <= 0)
                {
                    errors.Add("证候必须关联有效的就诊记录");
                }
            }

            // 验证就诊数据
            if (visit == null)
            {
                errors.Add("就诊数据不能为空");
            }
            else
            {
                if (visit.VisitId <= 0)
                {
                    errors.Add("就诊ID无效");
                }

                if (visit.VisitDate == default)
                {
                    errors.Add("就诊日期无效");
                }

                if (visit.Series?.PatientUserId <= 0)
                {
                    errors.Add("就诊必须关联有效的患者");
                }
            }

            // 验证患者数据
            if (patientUser == null)
            {
                errors.Add("患者数据不能为空");
            }
            else
            {
                if (patientUser.Id <= 0)
                {
                    errors.Add("患者ID无效");
                }

                if (string.IsNullOrWhiteSpace(patientUser.UserName))
                {
                    errors.Add("患者用户名不能为空");
                }
            }

            if (errors.Any())
            {
                _logger.LogWarning("治疗方案输入数据验证失败，错误数量: {ErrorCount}", errors.Count);
                throw new TreatmentDataValidationException("输入数据验证失败", errors.ToArray());
            }

            _logger.LogInformation("治疗方案输入数据验证通过");
        }

        /// <summary>
        /// 验证Dify API输入数据
        /// </summary>
        /// <param name="inputs">API输入字典</param>
        /// <exception cref="TreatmentDataValidationException">当输入数据验证失败时抛出</exception>
        public void ValidateApiInputs(Dictionary<string, string> inputs)
        {
            var errors = new List<string>();

            if (inputs == null || !inputs.Any())
            {
                errors.Add("API输入数据不能为空");
            }
            else
            {
                // 验证必需的输入字段
                var requiredFields = new[] { "confirmed_syndrome", "patient_info", "visit_description", "syndrome_detail" };
                
                foreach (var field in requiredFields)
                {
                    if (!inputs.ContainsKey(field) || string.IsNullOrWhiteSpace(inputs[field]))
                    {
                        errors.Add($"必需字段 '{field}' 不能为空");
                    }
                }

                // 验证字段长度
                foreach (var kvp in inputs)
                {
                    if (kvp.Value?.Length > 10000) // 设置合理的最大长度
                    {
                        errors.Add($"字段 '{kvp.Key}' 长度超过限制（最大10000字符）");
                    }
                }
            }

            if (errors.Any())
            {
                _logger.LogWarning("Dify API输入数据验证失败，错误数量: {ErrorCount}", errors.Count);
                throw new TreatmentDataValidationException("API输入数据验证失败", errors.ToArray());
            }

            _logger.LogInformation("Dify API输入数据验证通过");
        }

        /// <summary>
        /// 验证Dify API响应数据
        /// </summary>
        /// <param name="apiResponse">API响应字符串</param>
        /// <returns>解析后的JSON元素</returns>
        /// <exception cref="ApiResponseParseException">当响应格式无效时抛出</exception>
        public JsonElement ValidateAndParseApiResponse(string apiResponse)
        {
            if (string.IsNullOrWhiteSpace(apiResponse))
            {
                throw new ApiResponseParseException("API响应内容为空", apiResponse ?? string.Empty);
            }

            try
            {
                var jsonDocument = JsonDocument.Parse(apiResponse);
                var rootElement = jsonDocument.RootElement;

                // 验证基本结构
                if (!rootElement.TryGetProperty("data", out var dataElement))
                {
                    throw new ApiResponseParseException("API响应缺少 'data' 字段", apiResponse);
                }

                if (!dataElement.TryGetProperty("outputs", out var outputsElement))
                {
                    throw new ApiResponseParseException("API响应缺少 'data.outputs' 字段", apiResponse);
                }

                _logger.LogInformation("API响应数据验证通过，响应长度: {Length} 字符", apiResponse.Length);
                return rootElement;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "API响应JSON解析失败");
                throw new ApiResponseParseException("API响应JSON格式无效", apiResponse, ex);
            }
        }

        /// <summary>
        /// 验证治疗方案实体数据
        /// </summary>
        /// <param name="treatment">治疗方案实体</param>
        /// <exception cref="TreatmentDataValidationException">当实体数据验证失败时抛出</exception>
        public void ValidateTreatmentEntity(Treatment treatment)
        {
            var errors = new List<string>();

            if (treatment == null)
            {
                errors.Add("治疗方案实体不能为空");
            }
            else
            {
                if (treatment.SyndromeId <= 0)
                {
                    errors.Add("证候ID无效");
                }

                if (treatment.VisitId <= 0)
                {
                    errors.Add("就诊ID无效");
                }

                if (treatment.PatientId <= 0)
                {
                    errors.Add("患者ID无效");
                }

                if (string.IsNullOrWhiteSpace(treatment.Version))
                {
                    errors.Add("版本号不能为空");
                }

                if (treatment.CreatedByUserId <= 0)
                {
                    errors.Add("创建用户ID无效");
                }

                if (treatment.TenantId <= 0)
                {
                    errors.Add("租户ID无效");
                }

                // 验证治疗方案内容
                ValidateTreatmentContent(treatment, errors);
            }

            if (errors.Any())
            {
                _logger.LogWarning("治疗方案实体数据验证失败，错误数量: {ErrorCount}", errors.Count);
                throw new TreatmentDataValidationException("治疗方案实体数据验证失败", errors.ToArray());
            }

            _logger.LogInformation("治疗方案实体数据验证通过");
        }

        /// <summary>
        /// 验证治疗方案内容
        /// </summary>
        private void ValidateTreatmentContent(Treatment treatment, List<string> errors)
        {
            // 验证至少有一种治疗方式
            var hasContent = false;

            if (treatment.Prescriptions?.Any() == true)
            {
                hasContent = true;
                ValidatePrescriptions(treatment.Prescriptions, errors);
            }

            if (treatment.Acupunctures?.Any() == true)
            {
                hasContent = true;
                ValidateAcupunctures(treatment.Acupunctures, errors);
            }

            if (treatment.Moxibustions?.Any() == true)
            {
                hasContent = true;
            }

            if (treatment.Cuppings?.Any() == true)
            {
                hasContent = true;
            }

            if (treatment.DietaryTherapies?.Any() == true)
            {
                hasContent = true;
                ValidateDietaryTherapies(treatment.DietaryTherapies, errors);
            }

            if (treatment.LifestyleAdvices?.Any() == true)
            {
                hasContent = true;
            }

            if (treatment.DietaryAdvices?.Any() == true)
            {
                hasContent = true;
            }

            if (treatment.FollowUpAdvices?.Any() == true)
            {
                hasContent = true;
            }

            if (!hasContent)
            {
                errors.Add("治疗方案必须包含至少一种治疗方式或建议");
            }
        }

        /// <summary>
        /// 验证中药处方
        /// </summary>
        private void ValidatePrescriptions(ICollection<Prescription> prescriptions, List<string> errors)
        {
            foreach (var prescription in prescriptions)
            {
                if (string.IsNullOrWhiteSpace(prescription.Name))
                {
                    errors.Add("处方名称不能为空");
                }

                if (prescription.PrescriptionItems?.Any() != true)
                {
                    errors.Add($"处方 '{prescription.Name}' 缺少药材明细");
                    continue;
                }

                foreach (var item in prescription.PrescriptionItems)
                {
                    if (string.IsNullOrWhiteSpace(item.Name))
                    {
                        errors.Add("药材名称不能为空");
                    }

                    if (string.IsNullOrWhiteSpace(item.Dosage))
                    {
                        errors.Add($"药材 '{item.Name}' 的用量不能为空");
                    }
                }
            }
        }

        /// <summary>
        /// 验证针灸治疗
        /// </summary>
        private void ValidateAcupunctures(ICollection<Acupuncture> acupunctures, List<string> errors)
        {
            foreach (var acupuncture in acupunctures)
            {
                if (string.IsNullOrWhiteSpace(acupuncture.PointName))
                {
                    errors.Add("针灸穴位名称不能为空");
                }

                if (string.IsNullOrWhiteSpace(acupuncture.Location))
                {
                    errors.Add($"穴位 '{acupuncture.PointName}' 的定位不能为空");
                }
            }
        }

        /// <summary>
        /// 验证食疗方案
        /// </summary>
        private void ValidateDietaryTherapies(ICollection<DietaryTherapy> therapies, List<string> errors)
        {
            foreach (var therapy in therapies)
            {
                if (string.IsNullOrWhiteSpace(therapy.Name))
                {
                    errors.Add("食疗方案名称不能为空");
                }

                if (therapy.DietaryTherapyIngredients?.Any() != true)
                {
                    errors.Add($"食疗方案 '{therapy.Name}' 必须包含至少一种食材");
                }
            }
        }
    }
}