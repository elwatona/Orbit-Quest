using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrbPanel
{
    readonly Transform _panelTransform;
    private TextMeshProUGUI _speedText;
    private Image _impulseBar;
    private Image _inertiaStabilizer;

    public OrbPanel(Transform panel)
    {
        _panelTransform = panel;
        CacheReferences(_panelTransform);
    }
    void CacheReferences(Transform panelTransform)
    {
        var speedNode = panelTransform?.transform?.Find("Speed/Value");
        _speedText = speedNode.GetComponent<TextMeshProUGUI>();

        var impulseNode = panelTransform?.transform?.Find("Impulse/Fill");
        _impulseBar = impulseNode.GetComponent<Image>();

        var inertiaNode = panelTransform?.transform?.Find("Inertia/Value");
        _inertiaStabilizer = inertiaNode.GetComponent<Image>();
    }
    public void UpdateSpeedText(float speed)
    {
        _speedText.text = speed.ToString("F1");
    }
    public void UpdateImpulseBar(float impulse)
    {
        Color status = impulse == 1f ? Color.white : Color.gray;
        _impulseBar.color = status;
        _impulseBar.fillAmount = impulse;
    }
    public void UpdateInertiaStabilizerText(bool inertiaStabilizer)
    {
        Color status = inertiaStabilizer ? Color.green : Color.yellow;
        _inertiaStabilizer.color = status;
    }
}