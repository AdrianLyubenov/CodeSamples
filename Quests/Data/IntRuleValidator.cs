using System;

public class IntRuleValidator : RuleValidator<int>
{
    public override bool ValidateRule(object value, object parameterValue, Operand operation)
    {
        var val = (int)value;
        var param = (int)parameterValue;
            
        return operation switch
        {
            Operand.Greater => val > param,
            Operand.Lesser => val < param,
            Operand.Equal => val == param,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}