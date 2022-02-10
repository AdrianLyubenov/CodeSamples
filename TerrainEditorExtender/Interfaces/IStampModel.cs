namespace Megalith
{
    public interface IStampModel
    {
        bool selected { get; set; }
        bool hidden  { get; set; }

        ModelBase Clone(bool keepId = false);
        void Apply(ModelBase clone);
    }
}
