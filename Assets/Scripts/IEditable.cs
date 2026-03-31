public interface IEditable
{
    string DisplayName { get; }
    void Selected();
    void Deselected();
    void Deactivate();
}
