using UnityEngine;
using System;

public class AstroInfo : IPanel
{
    public GameObject Root {get; private set;}
    public RectTransform RectTransform {get; private set;}
    public readonly DropdownComponent Type;
    public readonly TextComponent GravitationalForce;

    public AstroInfo(AstroInfoDependencies dependencies)
    {
        Root = dependencies.Root;
        RectTransform = Root.GetComponent<RectTransform>();
        Type = new DropdownComponent(dependencies.AstroType);
        GravitationalForce = new TextComponent(dependencies.GravitationalForce);
    }
    public void Toggle(bool active)
    {
        Root.SetActive(active);
    }
    public void Update(IEditable editable)
    {
        Root.transform.position = GetDesiredPosition(editable);
        Type.UpdateValue((int)editable.Data.type);
        GravitationalForce.SetValue(editable.Data.orbit.gravity.ToString("F2"));
        GravitationalForce.SetActive(editable.Data.type != AstroType.Sun);
    }
    private Vector3 GetDesiredPosition(IEditable editable)
    {
        Vector3 screenPosition = UnityEngine.Camera.main.WorldToScreenPoint(editable.transform.position);
        RectTransform.pivot = GetDesiredPivot(screenPosition);
        return screenPosition;
    }
    private Vector2 GetDesiredPivot(Vector3 screenPosition)
    {
        int x = screenPosition.x > Screen.width / 2 ? 1 : 0;
        int y = screenPosition.y > Screen.height / 2 ? 1 : 0;
        return new Vector2(x, y);
    }
}
[Serializable]
public class AstroInfoDependencies : PanelDependencies 
{
    public Transform AstroType;
    public Transform GravitationalForce;
}