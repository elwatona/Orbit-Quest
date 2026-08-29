using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameMenu : IPanel
{
    public const int GraphicsTabIndex = 0;
    public const int EditorTabIndex = 1;
    public const int ControlsTabIndex = 2;
    public const int AudioTabIndex = 3;

    const int TabGraphics = GraphicsTabIndex;
    const int TabEditor = EditorTabIndex;
    const int TabControls = ControlsTabIndex;
    const int TabAudio = AudioTabIndex;

    public GameObject Root { get; private set; }

    readonly GameSettings _gameSettings;
    readonly Action _onResume;
    readonly Action<bool> _onControlsVisible;
    readonly List<BoundSlider> _sliders = new List<BoundSlider>();
    readonly List<BoundToggle> _toggles = new List<BoundToggle>();
    readonly List<BoundRange> _ranges = new List<BoundRange>();
    readonly Image[] _tabImages = new Image[4];
    readonly GameObject[] _pages = new GameObject[4];
    RectTransform _panelRect;
    bool _bound;

    public InGameMenu(
        InGameMenuDependencies dependencies,
        Transform parent,
        GameSettings gameSettings,
        Action onResume,
        Action<bool> onControlsVisible)
    {
        _gameSettings = gameSettings;
        _onResume = onResume;
        _onControlsVisible = onControlsVisible;

        if (dependencies != null && dependencies.Root != null)
            Root = dependencies.Root;
        else
            Root = BuildRoot(parent);

        BuildContent(Root.transform, dependencies);
        Root.SetActive(false);
    }

    public void Toggle(bool active)
    {
        if (active)
        {
            Bind();
            Root.SetActive(true);
            Root.transform.SetAsLastSibling();
            ShowTab(TabGraphics);
            return;
        }

        Unbind();
        Root.SetActive(false);
        _onControlsVisible?.Invoke(false);
    }

    public void OpenOnTab(int tabIndex)
    {
        if (!Root.activeSelf)
        {
            Bind();
            Root.SetActive(true);
            Root.transform.SetAsLastSibling();
        }

        ShowTab(tabIndex);
    }

    public void AttachControls(Controls controls)
    {
        if (controls == null || controls.Root == null || _pages[TabControls] == null)
            return;

        Transform page = _pages[TabControls].transform;
        RectTransform rect = controls.Root.transform as RectTransform;
        if (rect == null)
            return;

        rect.SetParent(page, false);
        Stretch(rect);

        LayoutElement layout = controls.Root.GetComponent<LayoutElement>();
        if (layout == null)
            layout = controls.Root.AddComponent<LayoutElement>();
        layout.minHeight = 0f;
        layout.preferredHeight = 0f;
        layout.flexibleWidth = 1f;
        layout.flexibleHeight = 1f;
        layout.ignoreLayout = true;

        ContentSizeFitter fitter = controls.Root.GetComponent<ContentSizeFitter>();
        if (fitter != null)
            fitter.enabled = false;

        ConfigureEmbeddedControls(controls.Root);

        DraggeableUI drag = controls.Root.GetComponent<DraggeableUI>();
        if (drag != null)
            drag.enabled = false;

        controls.Toggle(false);
    }

    void Bind()
    {
        if (_bound || _gameSettings == null)
            return;

        _gameSettings.Initialize();
        foreach (BoundSlider bound in _sliders)
        {
            bound.Slider.UpdateValueRange(bound.Parameter.Min, bound.Parameter.Max);
            bound.Slider.UpdateValue(bound.Parameter.Value);
            bound.Slider.OnValueChanged += bound.Parameter.SetValue;
        }
        foreach (BoundToggle bound in _toggles)
        {
            bound.Toggle.UpdateValue(bound.Parameter.Value);
            bound.Toggle.OnValueChanged += bound.Parameter.SetValue;
        }
        foreach (BoundRange bound in _ranges)
        {
            bound.Field.UpdateValue(new Vector2(bound.Parameter.Min, bound.Parameter.Max));
            bound.Field.OnValueChanged += bound.HandleValueChanged;
        }
        _bound = true;
    }

    void Unbind()
    {
        if (!_bound)
            return;

        foreach (BoundSlider bound in _sliders)
            bound.Slider.OnValueChanged -= bound.Parameter.SetValue;
        foreach (BoundToggle bound in _toggles)
            bound.Toggle.OnValueChanged -= bound.Parameter.SetValue;
        foreach (BoundRange bound in _ranges)
            bound.Field.OnValueChanged -= bound.HandleValueChanged;
        _bound = false;
    }

    GameObject BuildRoot(Transform parent)
    {
        var root = new GameObject("InGameMenu", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        Stretch(rect);
        Image dimmer = root.GetComponent<Image>();
        dimmer.color = new Color(0f, 0f, 0f, 0.45f);
        dimmer.raycastTarget = true;
        return root;
    }

    void BuildContent(Transform root, InGameMenuDependencies dependencies)
    {
        GameObject titlePrefab = dependencies != null ? dependencies.TitlePrefab : null;
        TMP_FontAsset font = ResolveFont(titlePrefab);

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VerticalLayoutGroup));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.SetParent(root, false);
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(440f, 0f);

        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(0.32941177f, 0.32941177f, 0.32941177f, 0.92f);
        panelImage.raycastTarget = true;

        VerticalLayoutGroup layout = panel.GetComponent<VerticalLayoutGroup>();
        _panelRect = panelRect;
        layout.padding = new RectOffset(16, 16, 16, 16);
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        AddTitle(panel.transform, titlePrefab, "PAUSE");
        BuildTabBar(panel.transform, font);
        BuildPages(panel.transform, dependencies);
        AddButton(panel.transform, font, "Resume", () => _onResume?.Invoke(), 36f);
        ShowTab(TabGraphics);
    }

    void BuildTabBar(Transform parent, TMP_FontAsset font)
    {
        var tabBar = new GameObject("TabBar", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        tabBar.transform.SetParent(parent, false);
        EnsureLayoutHeight(tabBar, 36f);

        HorizontalLayoutGroup row = tabBar.GetComponent<HorizontalLayoutGroup>();
        row.spacing = 6f;
        row.childAlignment = TextAnchor.MiddleCenter;
        row.childControlWidth = true;
        row.childControlHeight = true;
        row.childForceExpandWidth = true;
        row.childForceExpandHeight = true;

        string[] labels = { "Graphics", "Editor", "Controls", "Audio" };
        for (int i = 0; i < labels.Length; i++)
        {
            int tabIndex = i;
            GameObject buttonGo = AddButton(tabBar.transform, font, labels[i], () => ShowTab(tabIndex), 36f);
            _tabImages[i] = buttonGo.GetComponent<Image>();
        }
    }

    void BuildPages(Transform parent, InGameMenuDependencies dependencies)
    {
        GameObject sliderPrefab = dependencies != null ? dependencies.SliderPrefab : null;
        GameObject togglePrefab = dependencies != null ? dependencies.TogglePrefab : null;
        GameObject titlePrefab = dependencies != null ? dependencies.TitlePrefab : null;
        GameObject vector2Prefab = dependencies != null ? dependencies.Vector2Prefab : null;

        var pages = new GameObject("Pages", typeof(RectTransform), typeof(LayoutElement), typeof(RectMask2D));
        pages.transform.SetParent(parent, false);
        LayoutElement pagesLayout = pages.GetComponent<LayoutElement>();
        pagesLayout.minHeight = 80f;
        pagesLayout.preferredHeight = 320f;
        pagesLayout.flexibleWidth = 1f;
        pagesLayout.flexibleHeight = 1f;

        _pages[TabGraphics] = CreatePage(pages.transform, "GraphicsPage");
        _pages[TabEditor] = CreateScrollPage(pages.transform, "EditorPage");
        _pages[TabControls] = CreatePage(pages.transform, "ControlsPage");
        _pages[TabAudio] = CreatePage(pages.transform, "AudioPage");

        if (_gameSettings == null)
            return;

        _gameSettings.Initialize();
        foreach (FloatParameter parameter in _gameSettings.Graphics.Parameters)
            AddSlider(_pages[TabGraphics].transform, sliderPrefab, parameter);
        AddToggle(_pages[TabGraphics].transform, togglePrefab, _gameSettings.Graphics.SkyboxEnabled);

        Transform editorContent = _pages[TabEditor].transform.Find("Viewport/Content");
        if (editorContent == null)
            editorContent = _pages[TabEditor].transform;

        TMP_FontAsset font = ResolveFont(titlePrefab);
        AddEditorHint(editorContent, font);
        AddTitle(editorContent, titlePrefab, "Astros");
        foreach (RangeParameter parameter in _gameSettings.Edition.AstroParameters)
            AddRange(editorContent, vector2Prefab, parameter, font);

        AddTitle(editorContent, titlePrefab, "Player");
        foreach (RangeParameter parameter in _gameSettings.Edition.PlayerParameters)
            AddRange(editorContent, vector2Prefab, parameter, font);

        foreach (FloatParameter parameter in _gameSettings.Audio.Parameters)
            AddSlider(_pages[TabAudio].transform, sliderPrefab, parameter);
    }

    GameObject CreatePage(Transform parent, string name)
    {
        var page = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup));
        RectTransform rect = page.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        Stretch(rect);

        VerticalLayoutGroup layout = page.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(4, 4, 4, 4);
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        return page;
    }

    GameObject CreateScrollPage(Transform parent, string name)
    {
        var page = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        RectTransform pageRect = page.GetComponent<RectTransform>();
        pageRect.SetParent(parent, false);
        Stretch(pageRect);

        Image pageImage = page.GetComponent<Image>();
        pageImage.color = new Color(0f, 0f, 0f, 0.01f);
        pageImage.raycastTarget = true;

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.SetParent(page.transform, false);
        Stretch(viewportRect);
        Image viewportImage = viewport.GetComponent<Image>();
        viewportImage.color = Color.clear;
        viewportImage.raycastTarget = true;

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.SetParent(viewport.transform, false);
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(4, 4, 4, 4);
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = page.GetComponent<ScrollRect>();
        scroll.viewport = viewportRect;
        scroll.content = contentRect;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        return page;
    }

    public void ShowTab(int tabIndex)
    {
        for (int i = 0; i < _pages.Length; i++)
        {
            if (_pages[i] != null)
                _pages[i].SetActive(i == tabIndex);
            if (_tabImages[i] != null)
                _tabImages[i].color = i == tabIndex
                    ? new Color(0.35f, 0.35f, 0.35f, 1f)
                    : new Color(0.18f, 0.18f, 0.18f, 1f);
        }
        _onControlsVisible?.Invoke(tabIndex == TabControls);
        Canvas.ForceUpdateCanvases();
        ClampPanelToCanvas();
    }

    void ClampPanelToCanvas()
    {
        if (_panelRect == null || Root == null)
            return;

        RectTransform rootRect = Root.GetComponent<RectTransform>();
        float available = rootRect.rect.height;
        const float edge = 16f;
        float maxH = Mathf.Max(available - edge * 2f, 120f);
        float preferred = LayoutUtility.GetPreferredHeight(_panelRect);
        if (preferred < 1f)
            preferred = _panelRect.rect.height;
        float height = Mathf.Min(preferred, maxH);
        _panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    void AddTitle(Transform parent, GameObject titlePrefab, string text)
    {
        if (titlePrefab != null)
        {
            GameObject instance = UnityEngine.Object.Instantiate(titlePrefab, parent);
            var header = new TitleHeader(instance.transform);
            header.SetTitle(text);
            EnsureLayoutHeight(instance, 28f);
            return;
        }

        CreateLabel(parent, text, 18f, FontStyles.Bold);
    }

    void AddSlider(Transform parent, GameObject sliderPrefab, FloatParameter parameter)
    {
        if (sliderPrefab == null || parameter == null)
            return;

        GameObject instance = UnityEngine.Object.Instantiate(sliderPrefab, parent);
        instance.name = parameter.Id;
        ConfigureSliderLayout(instance);
        EnsureLayoutHeight(instance, 48f);

        Transform label = instance.transform.Find("Text");
        if (label != null && label.TryGetComponent(out TextMeshProUGUI tmp))
        {
            tmp.text = parameter.DisplayName;
            tmp.enableAutoSizing = false;
            tmp.fontSize = 14f;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
        }

        var slider = new SliderComponent(instance.transform);
        _sliders.Add(new BoundSlider(slider, parameter));
    }

    void AddToggle(Transform parent, GameObject togglePrefab, BoolParameter parameter)
    {
        if (togglePrefab == null || parameter == null)
            return;

        GameObject instance = UnityEngine.Object.Instantiate(togglePrefab, parent);
        instance.name = parameter.Id;
        ConfigureToggleLayout(instance);
        EnsureLayoutHeight(instance, 40f);

        Transform label = instance.transform.Find("Text");
        if (label != null && label.TryGetComponent(out TextMeshProUGUI tmp))
        {
            tmp.text = parameter.DisplayName;
            tmp.enableAutoSizing = false;
            tmp.fontSize = 14f;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
        }

        var toggle = new ToggleComponent(instance.transform);
        _toggles.Add(new BoundToggle(toggle, parameter));
    }

    void AddEditorHint(Transform parent, TMP_FontAsset font)
    {
        var hintGo = new GameObject("EditorHint", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
        hintGo.transform.SetParent(parent, false);

        LayoutElement layout = hintGo.GetComponent<LayoutElement>();
        layout.minHeight = 56f;
        layout.preferredHeight = 56f;
        layout.flexibleWidth = 1f;

        TextMeshProUGUI tmp = hintGo.GetComponent<TextMeshProUGUI>();
        tmp.text = "These fields set the min and max ranges of Edition sliders, not the current property values.";
        tmp.fontSize = 12f;
        tmp.enableWordWrapping = true;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        tmp.raycastTarget = false;
        if (font != null)
            tmp.font = font;
    }

    void AddRange(Transform parent, GameObject vector2Prefab, RangeParameter parameter, TMP_FontAsset font)
    {
        if (vector2Prefab == null || parameter == null)
            return;

        var row = new GameObject(parameter.Id + "Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(parent, false);
        EnsureLayoutHeight(row, 48f);

        HorizontalLayoutGroup rowLayout = row.GetComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 6f;
        rowLayout.childAlignment = TextAnchor.MiddleLeft;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = true;

        GameObject instance = UnityEngine.Object.Instantiate(vector2Prefab, row.transform);
        instance.name = parameter.Id;
        ConfigureVector2Layout(instance);
        LayoutElement fieldLayout = instance.GetComponent<LayoutElement>();
        if (fieldLayout == null)
            fieldLayout = instance.AddComponent<LayoutElement>();
        fieldLayout.minHeight = 48f;
        fieldLayout.preferredHeight = 48f;
        fieldLayout.flexibleWidth = 1f;
        fieldLayout.flexibleHeight = 0f;

        GridLayoutGroup rootGrid = instance.GetComponent<GridLayoutGroup>();
        if (rootGrid != null)
            rootGrid.cellSize = new Vector2(148f, 40f);

        Transform variable = instance.transform.Find("Variable");
        GridLayoutGroup variableGrid = variable != null ? variable.GetComponent<GridLayoutGroup>() : null;
        if (variableGrid != null)
            variableGrid.cellSize = new Vector2(68f, 28f);

        Transform label = instance.transform.Find("Text");
        Transform title = instance.transform.Find("Title");
        Transform nameLabel = title != null ? title : label;
        if (nameLabel != null && nameLabel.TryGetComponent(out TextMeshProUGUI tmp))
        {
            tmp.text = parameter.DisplayName;
            tmp.enableAutoSizing = false;
            tmp.fontSize = 14f;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
        }

        var field = new Vector2Component(instance.transform);
        _ranges.Add(new BoundRange(field, parameter));

        GameObject resetButton = AddButton(row.transform, font, "Reset", () =>
        {
            parameter.ResetToDefaults();
            field.UpdateValue(new Vector2(parameter.Min, parameter.Max));
        }, 48f);
        LayoutElement resetLayout = resetButton.GetComponent<LayoutElement>();
        resetLayout.minWidth = 64f;
        resetLayout.preferredWidth = 64f;
        resetLayout.flexibleWidth = 0f;
    }

    static void ConfigureToggleLayout(GameObject instance)
    {
        GridLayoutGroup rootGrid = instance.GetComponent<GridLayoutGroup>();
        if (rootGrid == null)
            return;

        rootGrid.cellSize = new Vector2(196f, 32f);
        rootGrid.spacing = new Vector2(8f, 0f);
        rootGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        rootGrid.constraintCount = 2;
        rootGrid.childAlignment = TextAnchor.MiddleLeft;
    }

    static void ConfigureVector2Layout(GameObject instance)
    {
        GridLayoutGroup rootGrid = instance.GetComponent<GridLayoutGroup>();
        if (rootGrid != null)
        {
            rootGrid.cellSize = new Vector2(196f, 40f);
            rootGrid.spacing = new Vector2(8f, 0f);
            rootGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            rootGrid.constraintCount = 2;
            rootGrid.childAlignment = TextAnchor.MiddleLeft;
        }

        Transform variable = instance.transform.Find("Variable");
        GridLayoutGroup variableGrid = variable != null ? variable.GetComponent<GridLayoutGroup>() : null;
        if (variableGrid != null)
        {
            variableGrid.cellSize = new Vector2(90f, 28f);
            variableGrid.spacing = new Vector2(8f, 0f);
            variableGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            variableGrid.constraintCount = 2;
            variableGrid.childAlignment = TextAnchor.MiddleCenter;
        }
    }

    static void ConfigureSliderLayout(GameObject instance)
    {
        GridLayoutGroup rootGrid = instance.GetComponent<GridLayoutGroup>();
        if (rootGrid != null)
        {
            rootGrid.cellSize = new Vector2(196f, 40f);
            rootGrid.spacing = new Vector2(8f, 0f);
            rootGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            rootGrid.constraintCount = 2;
            rootGrid.childAlignment = TextAnchor.MiddleLeft;
        }

        Transform variable = instance.transform.Find("Variable");
        GridLayoutGroup variableGrid = variable != null ? variable.GetComponent<GridLayoutGroup>() : null;
        if (variableGrid != null)
        {
            variableGrid.cellSize = new Vector2(90f, 28f);
            variableGrid.spacing = new Vector2(8f, 0f);
            variableGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            variableGrid.constraintCount = 2;
            variableGrid.childAlignment = TextAnchor.MiddleCenter;
        }
    }

    GameObject AddButton(Transform parent, TMP_FontAsset font, string label, Action onClick, float height)
    {
        var buttonGo = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonGo.transform.SetParent(parent, false);
        EnsureLayoutHeight(buttonGo, height);

        Image image = buttonGo.GetComponent<Image>();
        image.color = new Color(0.18f, 0.18f, 0.18f, 1f);

        Button button = buttonGo.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => onClick?.Invoke());

        CreateLabel(buttonGo.transform, label, 14f, FontStyles.Normal, font, fillParent: true);
        return buttonGo;
    }

    static void CreateLabel(Transform parent, string text, float fontSize, FontStyles style, TMP_FontAsset font = null, bool fillParent = false)
    {
        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(parent, false);
        RectTransform rect = labelGo.GetComponent<RectTransform>();
        if (fillParent)
            Stretch(rect);
        else
            EnsureLayoutHeight(labelGo, fontSize + 10f);

        TextMeshProUGUI tmp = labelGo.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        if (font != null)
            tmp.font = font;
        tmp.raycastTarget = false;
    }

    static TMP_FontAsset ResolveFont(GameObject titlePrefab)
    {
        if (titlePrefab == null)
            return null;
        return titlePrefab.GetComponent<TextMeshProUGUI>()?.font;
    }

    static void ConfigureEmbeddedControls(GameObject root)
    {
        VerticalLayoutGroup vlg = root.GetComponent<VerticalLayoutGroup>();
        if (vlg != null)
        {
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.UpperCenter;
        }

        ScrollRect scroll = root.GetComponentInChildren<ScrollRect>(true);
        Transform scrollBranch = null;
        if (scroll != null)
        {
            Transform current = scroll.transform;
            while (current.parent != null && current.parent != root.transform)
                current = current.parent;
            scrollBranch = current.parent == root.transform ? current : scroll.transform;

            LayoutElement scrollLayout = scrollBranch.GetComponent<LayoutElement>();
            if (scrollLayout == null)
                scrollLayout = scrollBranch.gameObject.AddComponent<LayoutElement>();
            scrollLayout.minHeight = 40f;
            scrollLayout.preferredHeight = 80f;
            scrollLayout.flexibleHeight = 1f;
            scrollLayout.flexibleWidth = 1f;
        }

        foreach (Transform child in root.transform)
        {
            if (child == scrollBranch)
                continue;

            LayoutElement childLayout = child.GetComponent<LayoutElement>();
            if (childLayout == null)
                childLayout = child.gameObject.AddComponent<LayoutElement>();
            childLayout.flexibleHeight = 0f;
            if (childLayout.minHeight < 1f)
                childLayout.minHeight = 28f;
            if (childLayout.preferredHeight < 1f)
                childLayout.preferredHeight = 28f;
        }
    }

    static void EnsureLayoutHeight(GameObject go, float height)
    {
        LayoutElement layout = go.GetComponent<LayoutElement>();
        if (layout == null)
            layout = go.AddComponent<LayoutElement>();
        layout.minHeight = height;
        layout.preferredHeight = height;
        layout.flexibleWidth = 1f;
        layout.flexibleHeight = 0f;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    readonly struct BoundSlider
    {
        public BoundSlider(SliderComponent slider, FloatParameter parameter)
        {
            Slider = slider;
            Parameter = parameter;
        }

        public SliderComponent Slider { get; }
        public FloatParameter Parameter { get; }
    }

    readonly struct BoundToggle
    {
        public BoundToggle(ToggleComponent toggle, BoolParameter parameter)
        {
            Toggle = toggle;
            Parameter = parameter;
        }

        public ToggleComponent Toggle { get; }
        public BoolParameter Parameter { get; }
    }

    sealed class BoundRange
    {
        public BoundRange(Vector2Component field, RangeParameter parameter)
        {
            Field = field;
            Parameter = parameter;
        }

        public Vector2Component Field { get; }
        public RangeParameter Parameter { get; }

        public void HandleValueChanged(Vector2 value)
        {
            Parameter.SetRange(value.x, value.y);
            Field.UpdateValue(new Vector2(Parameter.Min, Parameter.Max));
        }
    }
}

[Serializable]
public class InGameMenuDependencies : PanelDependencies
{
    public GameObject SliderPrefab;
    public GameObject TitlePrefab;
    public GameObject TogglePrefab;
    public GameObject Vector2Prefab;
}
