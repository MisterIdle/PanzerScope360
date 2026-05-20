using Godot;

public partial class UIManager : BaseMenu
{
    [ExportGroup("Zoom")]
    [Export] private TextureRect frame;
    [Export] private Vector2 baseScale = new Vector2(0.8f, 0.8f);
    [Export] private Vector2 zoomScale = new Vector2(2f, 2f);
    [Export] private float zoomDuration = 0.5f;

    [ExportGroup("Container")]
    [Export] private Control handContainer;

    private bool isStarting = false;
    private Tween zoomTween;

    public override void _Ready()
    {
        base._Ready();

        frame.PivotOffset = frame.Size / 2;
        frame.Scale = baseScale;
    }

    public override void OnStateChanged(GameStats pState)
    {
        base.OnStateChanged(pState);

        AudioManager.instance.StopMusic();

        if (pState == GameStats.Waiting || pState == GameStats.TitleScreen)
        {
            isStarting = false;
            handContainer.Modulate = new Color(1, 1, 1, 1);

            zoomTween?.Kill();
            zoomTween = CreateTween().SetProcessMode(Tween.TweenProcessMode.Idle);

            zoomTween.TweenProperty(frame, TweenPaths.scale, baseScale, zoomDuration)
                     .SetTrans(Tween.TransitionType.Quint)
                     .SetEase(Tween.EaseType.Out);
        }
    }

    public override void OnPlayerTouched(int pPlayerId, bool isPressed)
    {
        if (!isPressed || isStarting || GameManager.instance.currentState != GameStats.Waiting)
            return;

        if (zoomTween?.IsRunning() ?? false)
            return;

        isStarting = true;
        handContainer.Modulate = new Color(1, 1, 1, 0);

        ZoomAndCountdown();
    }

    private void ZoomAndCountdown()
    {
        zoomTween?.Kill();
        zoomTween = CreateTween().SetProcessMode(Tween.TweenProcessMode.Idle);

        zoomTween.TweenProperty(frame, TweenPaths.scale, zoomScale, zoomDuration)
                 .SetTrans(Tween.TransitionType.Quint)
                 .SetEase(Tween.EaseType.Out);

        MenuManager.instance.OpenMenu(GameStats.InGame);
    }
}