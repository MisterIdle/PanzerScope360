using Godot;

public partial class FloatingText : Control
{
    [Export] private Label label;

    [ExportGroup("Distances")]
    [Export] private float startOffset = 1500f;
    [Export] private float driftOffset = 100f;

    [ExportGroup("Timing")]
    [Export] private float enterDuration = 0.5f;
    [Export] private float driftDuration = 1.0f;
    [Export] private float exitDuration = 0.5f;
    [Export] private float fadeInRatio = 0.5f;

    [ExportGroup("Visuals")]
    [Export] private Color fullOpacity = Colors.White;
    [Export] private Color zeroOpacity = new Color(1, 1, 1, 0);
    [Export] private LabelSettings labelSettingsTemplate;

    [ExportGroup("State")]
    [Export] private bool isTop;

    private Tween currentTween;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        label.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Center;
        
        label.LabelSettings = (LabelSettings)labelSettingsTemplate.Duplicate();

        Resized += UpdatePivot;
    }

    public void Setup(string pText, Color pColor, int pSize, bool pIsTop, float pEnterDur, float pDriftDur, float pExitDur)
    {
        isTop = pIsTop;
        enterDuration = pEnterDur;
        driftDuration = pDriftDur;
        exitDuration = pExitDur;

        label.Text = pText;

        if (label.LabelSettings != null)
        {
            label.LabelSettings.FontColor = pColor;
            label.LabelSettings.FontSize = pSize;
        }

        UpdatePivot();
        Animate();
    }

    private void UpdatePivot()
    {
        label.PivotOffset = Size / 2;
        label.Rotation = isTop ? Mathf.Pi : 0;
    }

    private void Animate()
    {
        currentTween?.Kill();

        float lDirection = isTop ? -1f : 1f;

        Vector2 lStartPos = new Vector2(-startOffset * lDirection, 0);
        Vector2 lCenterPos = Vector2.Zero;
        Vector2 lDriftPos = new Vector2(driftOffset * lDirection, 0);
        Vector2 lEndPos = new Vector2(startOffset * lDirection, 0);

        label.Position = lStartPos;
        label.Modulate = zeroOpacity;

        currentTween = CreateTween().SetProcessMode(Tween.TweenProcessMode.Idle);

        currentTween.TweenProperty(label, TweenPaths.position, lCenterPos, enterDuration)
              .SetTrans(Tween.TransitionType.Back)
              .SetEase(Tween.EaseType.Out);

        currentTween.Parallel().TweenProperty(label, TweenPaths.modulate, fullOpacity, enterDuration * fadeInRatio);

        currentTween.TweenProperty(label, TweenPaths.position, lDriftPos, driftDuration)
              .SetTrans(Tween.TransitionType.Linear)
              .SetEase(Tween.EaseType.InOut);

        currentTween.TweenProperty(label, TweenPaths.position, lEndPos, exitDuration)
              .SetTrans(Tween.TransitionType.Back)
              .SetEase(Tween.EaseType.In);

        currentTween.Parallel().TweenProperty(label, TweenPaths.modulate, zeroOpacity, exitDuration);

        currentTween.TweenCallback(Callable.From(FinishAnimation));
    }

    private void FinishAnimation()
    {
        TextManager.instance.ReturnToPool(this);
    }
}