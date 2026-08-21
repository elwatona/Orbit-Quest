using UnityEngine;
using System;

public class AstroInfo : IPanel
{
    const float ScreenMargin = 8f;
    const float BehindCenterEpsilon = 1f;

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
        Type.UpdateValue((int)editable.Data.type);
        GravitationalForce.SetValue(editable.Data.orbit.gravity.ToString("F2"));
        GravitationalForce.SetActive(editable.Data.type != AstroType.Sun);
        Follow(editable);
    }
    public void Follow(IEditable editable)
    {
        Root.transform.position = GetDesiredPosition(editable);
    }
    private Vector3 GetDesiredPosition(IEditable editable)
    {
        UnityEngine.Camera camera = UnityEngine.Camera.main;
        if (camera == null)
            return Root.transform.position;

        Vector3 screenPosition = camera.WorldToScreenPoint(editable.transform.position);
        if (screenPosition.z <= 0f)
            screenPosition = ReflectAroundScreenCenter(screenPosition);

        screenPosition = PushFromCenterIfNeeded(screenPosition);
        RectTransform.pivot = GetDesiredPivot(screenPosition);
        return ClampToScreen(screenPosition);
    }
    private static Vector3 ReflectAroundScreenCenter(Vector3 screenPosition)
    {
        screenPosition.x = Screen.width - screenPosition.x;
        screenPosition.y = Screen.height - screenPosition.y;
        return screenPosition;
    }
    private static Vector3 PushFromCenterIfNeeded(Vector3 screenPosition)
    {
        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f;
        Vector2 offset = new Vector2(screenPosition.x - centerX, screenPosition.y - centerY);
        if (offset.sqrMagnitude > BehindCenterEpsilon * BehindCenterEpsilon)
            return screenPosition;

        screenPosition.x = centerX;
        screenPosition.y = 0f;
        return screenPosition;
    }
    private Vector2 GetDesiredPivot(Vector3 screenPosition)
    {
        int x = screenPosition.x > Screen.width / 2 ? 1 : 0;
        int y = screenPosition.y > Screen.height / 2 ? 1 : 0;
        return new Vector2(x, y);
    }
    private Vector3 ClampToScreen(Vector3 screenPosition)
    {
        Vector2 size = RectTransform.rect.size;
        Vector3 scale = RectTransform.lossyScale;
        float width = size.x * scale.x;
        float height = size.y * scale.y;
        Vector2 pivot = RectTransform.pivot;

        float minX = ScreenMargin + width * pivot.x;
        float maxX = Screen.width - ScreenMargin - width * (1f - pivot.x);
        float minY = ScreenMargin + height * pivot.y;
        float maxY = Screen.height - ScreenMargin - height * (1f - pivot.y);

        screenPosition.x = minX > maxX ? Screen.width * 0.5f : Mathf.Clamp(screenPosition.x, minX, maxX);
        screenPosition.y = minY > maxY ? Screen.height * 0.5f : Mathf.Clamp(screenPosition.y, minY, maxY);
        return screenPosition;
    }
}
[Serializable]
public class AstroInfoDependencies : PanelDependencies 
{
    public Transform AstroType;
    public Transform GravitationalForce;
}
