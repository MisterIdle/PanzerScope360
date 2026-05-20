using Godot;
using System;

public partial class GameManager : Node
{
    public static GameManager instance { get; private set; }

    [Signal] public delegate void OnPlayerTouchInputEventHandler(int pPlayerId, bool pIsPressed);
    public event Action<GameStats> StateChangedEvent;

    [Export] public VBoxContainer touchContainer;
    [Export] public int playerNum = 2;

    public GameStats currentState = GameStats.TitleScreen;

    public override void _Ready()
    {
        if (instance == null)
            instance = this;
        else
        {
            QueueFree();
            return;
        }

        CallDeferred(MethodName.InitTouch);
    }

    public void ChangeState(GameStats pState)
    {
        if (currentState == pState) 
            return;

        currentState = pState;

        StateChangedEvent?.Invoke(currentState);
    }

    private void InitTouch()
    {
        for (int i = 0; i < playerNum; i++)
        {
            Touch lTouch = new Touch();

            lTouch.playerId = i;
            lTouch.Name = $"Touch Area {i}";

            lTouch.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            lTouch.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            lTouch.MouseFilter = Control.MouseFilterEnum.Stop;

            touchContainer.AddChild(lTouch);
            lTouch.TouchInput += OnAreaTouched;
        }
    }

    private void OnAreaTouched(int pId, bool isPressed)
    {
        EmitSignal(SignalName.OnPlayerTouchInput, pId, isPressed);
    }

    protected override void Dispose(bool pDisposing)
    {
        if (instance == this)
            instance = null;

        base.Dispose(pDisposing);
    }
}