using System.ComponentModel.DataAnnotations;

namespace CloudStorage.Client.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = false)]
public class UpperSymbolAttribute : ValidationAttribute
{

    public override bool IsValid(object? value)
    {
        if (value is null)
        {
            return false;
        }

        return value.ToString().Any(char.IsUpper);
    }
}
