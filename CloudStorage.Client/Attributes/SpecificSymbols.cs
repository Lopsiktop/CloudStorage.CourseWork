using System.ComponentModel.DataAnnotations;

namespace CloudStorage.Client.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = false)]
public class SpecificSymbolsAttribute : ValidationAttribute
{

    public override bool IsValid(object? value)
    {
        if (value is null)
        {
            return false;
        }

        var chars = new string[] { "!", "@", "#", "$", "%", "^", ":", "&", "?", "*", "(", ")", "-", "_", "+", "=", "№", "\"", "<", ">", "~", "`", ".", ",", "[", "]", "{", "}", "\\", "|", "/", ";", "'" };
        return chars.Any(x => value.ToString().Contains(x));
    }
}
