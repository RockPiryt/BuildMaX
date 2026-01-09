using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

// (opcjonalnie: reguły wariantów)
namespace BuildMaX.Web.Validation
{
    public class VariantAvailabilityAttribute : ValidationAttribute
    {
        private readonly string _plotAreaProperty;

        public VariantAvailabilityAttribute(string plotAreaProperty)
        {
            _plotAreaProperty = plotAreaProperty;
            ErrorMessage = "Wybrany wariant nie jest dostępny dla tak małej działki.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            var plotProp = validationContext.ObjectType.GetProperty(_plotAreaProperty);
            var plotArea = plotProp?.GetValue(validationContext.ObjectInstance) as decimal?;

            if (plotArea.HasValue && plotArea < 1000)
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }
    }
}
