using System;

public class LongRuleValidator : RuleValidator<long>
{
    public override bool ValidateRule(object value, object parameterValue, Operand operation)
    {
        var val = (long)value;
        var param = (long)parameterValue;
            
        return operation switch
        {
            Operand.Greater => val > param,
            Operand.Lesser => val < param,
            Operand.Equal => val == param,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}