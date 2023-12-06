public abstract class RuleValidator<T> : IRuleValidator
{
    public abstract bool ValidateRule(object value, object parameterValue, Operand operation);
}