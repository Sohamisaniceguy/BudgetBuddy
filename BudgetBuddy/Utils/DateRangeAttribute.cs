using System.ComponentModel.DataAnnotations;

namespace BudgetBuddy.Utils
{
    public class DateRangeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var startDate = (DateTime)value;
            var endDateProperty = validationContext.ObjectType.GetProperty("EndDate");

            if (endDateProperty == null)
            {
                return new ValidationResult("Invalid property name for end date.");
            }

            var endDate = (DateTime)endDateProperty.GetValue(validationContext.ObjectInstance);

            if (startDate >= endDate)
            {
                return new ValidationResult("Start date must be earlier than the end date.");
            }

            return ValidationResult.Success;
        }
    }
}
