using Godot;

public partial class BaseMenu : Control
{
    [Export] public GameStats menuState = GameStats.TitleScreen;

    public override void _Ready()
    {
        if (GameManager.instance == null)
            return;

        GameManager.instance.OnPlayerTouchInput += OnPlayerTouched;
        GameManager.instance.StateChangedEvent += OnStateChanged;
        OnStateChanged(GameManager.instance.currentState);
    }

    public virtual void OnPlayerTouched(int pPlayerId, bool isPressed) { }

    public virtual void OnStateChanged(GameStats pState) { }
}