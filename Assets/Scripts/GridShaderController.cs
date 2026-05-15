using UnityEngine;
public class GridShaderController
{
    readonly Renderer _renderer;
    readonly MaterialPropertyBlock _propertyBlock;
    private Vector2 _currentSize;
    private float _currentThickness;
    private float _currentOpacity;
    
    private Vector2 _baseSize;
    private float _baseThickness;
    private float _baseOpacity;

    public Vector2 DesiredSize => _currentSize != _baseSize ? _currentSize : _baseSize;
    public float DesiredThickness => _currentThickness != _baseThickness ? _currentThickness : _baseThickness;
    public float DesiredOpacity => _currentOpacity != _baseOpacity ? _currentOpacity : _baseOpacity;
#region ShaderIDs
    readonly int _size = Shader.PropertyToID("_Size");
    readonly int _thickness = Shader.PropertyToID("_Thickness");
    readonly int _opacity = Shader.PropertyToID("_Opacity");
#endregion

    public GridShaderController(Renderer renderer)
    {
        _renderer = renderer;

        _baseSize = _renderer.material.GetVector(_size);
        _baseThickness = _renderer.material.GetFloat(_thickness);
        _baseOpacity = _renderer.material.GetFloat(_opacity);

        _currentSize = _baseSize;
        _currentThickness = _baseThickness;
        _currentOpacity = _baseOpacity;

        _propertyBlock = new MaterialPropertyBlock();
    }
    private void Apply()
    {
        _renderer.GetPropertyBlock(_propertyBlock);

        _propertyBlock.SetVector(_size, DesiredSize);
        _propertyBlock.SetFloat(_thickness, DesiredThickness); 
        _propertyBlock.SetFloat(_opacity, DesiredOpacity);

        _renderer.SetPropertyBlock(_propertyBlock);
    }
    public void UpdateOpacity(float value)
    {
        _currentOpacity = Mathf.Clamp01(value);
        Apply();
    }
}