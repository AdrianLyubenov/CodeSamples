public class StringRuleValidator : RuleValidator<string>
{
    public override bool ValidateRule(object value, object parameterValue, Operand operation)
    {
        var val = (string)value;
        var param = (string)parameterValue;

        return val == param;
    }
}