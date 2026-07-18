public interface ILimitable
{
    Limits Limits { get; }
    void SetLimits(Limits limits);
}