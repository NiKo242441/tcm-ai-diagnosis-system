using AutoMapper;
using TcmAiDiagnosis.Dtos;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Web.AutoMapper
{
    public class TreatmentMappingProfile : Profile
    {
        public TreatmentMappingProfile()
        {
            CreateMap<Treatment, TreatmentDto>()
                .ForMember(dest => dest.CreatorName, opt => opt.Ignore())
                .ForMember(dest => dest.PatientInfo, opt => opt.Ignore())
                .ForMember(dest => dest.SyndromeInfo, opt => opt.Ignore());

            CreateMap<Prescription, PrescriptionDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.PrescriptionItems));
            CreateMap<PrescriptionItem, PrescriptionItemDto>();

            CreateMap<Acupuncture, AcupunctureDto>();
            CreateMap<Moxibustion, MoxibustionDto>();
            CreateMap<Cupping, CuppingDto>();

            CreateMap<DietaryTherapy, DietaryTherapyDto>()
                .ForMember(dest => dest.Ingredients, opt => opt.MapFrom(src => src.DietaryTherapyIngredients));
            CreateMap<DietaryTherapyIngredient, DietaryTherapyIngredientDto>();

            CreateMap<LifestyleAdvice, LifestyleAdviceDto>();

            CreateMap<DietaryAdvice, DietaryAdviceDto>()
                .ForMember(dest => dest.RecommendedFoods, opt => opt.MapFrom(src => src.RecommendedFoods))
                .ForMember(dest => dest.AvoidedFoods, opt => opt.MapFrom(src => src.AvoidedFoods));
            CreateMap<RecommendedFood, RecommendedFoodDto>();
            CreateMap<AvoidedFood, AvoidedFoodDto>();

            CreateMap<FollowUpAdvice, FollowUpAdviceDto>()
                .ForMember(dest => dest.MonitoringIndicators, opt => opt.MapFrom(src => src.MonitoringIndicators));
            CreateMap<MonitoringIndicator, MonitoringIndicatorDto>();

            CreateMap<HerbalWarning, HerbalWarningDto>()
                .ForMember(dest => dest.AffectedMedications, opt => opt.MapFrom(src => src.AffectedMedications));
            CreateMap<AffectedMedication, AffectedMedicationDto>();
            CreateMap<DietaryWarning, DietaryWarningDto>();
        }
    }
}
