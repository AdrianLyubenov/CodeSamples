public interface IRuleValidator
{
    public bool ValidateRule(object value, object parameterValue, Operand operation);
}