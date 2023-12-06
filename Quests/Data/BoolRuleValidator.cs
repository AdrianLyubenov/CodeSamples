public class BoolRuleValidator : RuleValidator<bool>
{
    public override bool ValidateRule(object value, object parameterValue, Operand operation)
    {
        return (bool)value == (bool)parameterValue;
    }
}