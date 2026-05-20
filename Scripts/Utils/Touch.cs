using Godot;

public partial class Touch : Control
{
    [Signal] public delegate void TouchInputEventHandler(int pId, bool pIsPressed);

    public int playerId;
    private bool lastState = false;

    public override void _GuiInput(InputEvent pEvent)
    {
        bool lIsMouse = pEvent is InputEventMouseButton lMouse && lMouse.ButtonIndex == MouseButton.Left;
        bool lIsTouch = pEvent is InputEventScreenTouch;

        if (lIsMouse || lIsTouch)
        {
            bool lPress = pEvent.IsPressed();

            if (lPress == lastState)
                return;

            lastState = lPress;

            EmitSignal(SignalName.TouchInput, playerId, lPress);
            AcceptEvent();
        }
    }
}
