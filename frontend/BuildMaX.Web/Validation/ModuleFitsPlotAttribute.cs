using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

// (własny validator: cross-field)
namespace BuildMaX.Web.Validation
{
    public class ModuleFitsPlotAttribute : ValidationAttribute
    {
        private readonly string _plotAreaProperty;

        public ModuleFitsPlotAttribute(string plotAreaProperty)
        {
            _plotAreaProperty = plotAreaProperty;
            ErrorMessage = "Powierzchnia modułu nie może przekraczać 60% powierzchni działki.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var moduleArea = value as decimal?;
            var plotProp = validationContext.ObjectType.GetProperty(_plotAreaProperty);

            if (plotProp == null)
                return new ValidationResult($"Nie znaleziono pola {_plotAreaProperty}");

            var plotArea = plotProp.GetValue(validationContext.ObjectInstance) as decimal?;

            if (moduleArea.HasValue && plotArea.HasValue && moduleArea > plotArea * 0.6m)
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }
    }
}
