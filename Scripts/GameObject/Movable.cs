using Godot;

public partial class Movable : Area2D
{
    [Export] protected Sprite2D sprite;
    [Export] protected PackedScene explosionScene;

    public bool engineStart = false;
    protected float currentSpeed;
    protected Vector2 moveDirection;
    protected float bounceMargin = 0f;

    public override void _Ready()
    {
        AreaEntered += OnHit;
    }

    public override void _Process(double pDelta)
    {
        Update((float)pDelta);
    }

    protected virtual void Update(float pDelta) { }

    protected void InitializeMargin()
    {
        Vector2 lSize = sprite.Texture.GetSize() * sprite.Scale;
        bounceMargin = Mathf.Max(lSize.X, lSize.Y);
    }

    protected void InitializeDirection()
    {
        moveDirection = Vector2.Right.Rotated(Mathf.DegToRad(RotationDegrees));
    }

    protected void ApplyMovement(float pDelta)
    {
        float lScaledSpeed = currentSpeed * GlobalScale.X;
        GlobalPosition += moveDirection * lScaledSpeed * pDelta;
    }

    protected void CheckScreenBoundaries()
    {
        Rect2 lBounds = GetViewportRect();
        HandleXBound(lBounds);
        HandleYBound(lBounds);
    }

    private void HandleXBound(Rect2 pBounds)
    {
        if (GlobalPosition.X < pBounds.Position.X + bounceMargin || GlobalPosition.X > pBounds.End.X - bounceMargin)
        {
            moveDirection.X *= -1;
            RotationDegrees = Mathf.RadToDeg(moveDirection.Angle());
            GlobalPosition = new Vector2(Mathf.Clamp(GlobalPosition.X, pBounds.Position.X + bounceMargin, pBounds.End.X - bounceMargin), GlobalPosition.Y);
            
            OnBounce();
        }
    }

    private void HandleYBound(Rect2 pBounds)
    {
        if (GlobalPosition.Y < pBounds.Position.Y + bounceMargin || GlobalPosition.Y > pBounds.End.Y - bounceMargin)
        {
            moveDirection.Y *= -1;
            RotationDegrees = Mathf.RadToDeg(moveDirection.Angle());
            GlobalPosition = new Vector2(GlobalPosition.X, Mathf.Clamp(GlobalPosition.Y, pBounds.Position.Y + bounceMargin, pBounds.End.Y - bounceMargin));
            OnBounce();
        }
    }

    protected virtual void OnBounce() { }
    protected virtual void OnHit(Area2D pArea) { }
}