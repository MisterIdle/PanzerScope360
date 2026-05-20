using Godot;
using System;

public partial class Obus : Movable
{
    [Export] public float speed = 500.0f;
    [Export] public GpuParticles2D particles;

    public int playerId;
    public event Action<Obus> OnObusDestroyed;

    private float currentLifeTime = 0f;
    private float maxLifeTime = 3.0f;

    public override void _Ready()
    {
        base._Ready();
        InitializeMargin();
    }

    public void Fire(int pPlayer, Vector2 pStartPosition, float pStartRotationDeg, float pSpeed, float pLife)
    {
        string lMonitoring = "monitoring";
        string lMonitorable = "monitorable";

        playerId = pPlayer;
        speed = pSpeed;
        currentSpeed = speed;
        maxLifeTime = pLife;
        currentLifeTime = 0f;

        particles.Emitting = true;

        ApplyOwnerSkin();

        GlobalPosition = pStartPosition;
        GlobalRotationDegrees = pStartRotationDeg;
        InitializeDirection();

        Visible = true;
        SetProcess(true);
        SetPhysicsProcess(true);
        SetDeferred(lMonitoring, true);
        SetDeferred(lMonitorable, true);

        engineStart = true;
    }

    private void ApplyOwnerSkin()
    {
        TankSkin lSkin = SkinManager.instance.GetTankSkin(playerId);
        sprite.Texture = lSkin.obusTexture;
    }

    protected override void Update(float pDelta)
    {
        if (!engineStart)
            return;

        currentLifeTime += pDelta;
        if (currentLifeTime >= maxLifeTime)
        {
            DeactivateObus();
            return;
        }

        HandleMovementPhase(pDelta);
    }

    protected override void OnHit(Area2D pArea)
    {
        bool lHasHit = false;

        if (pArea is Tank lHitedTank && lHitedTank.playerId != playerId)
            lHasHit = true;

        if (pArea is Obus lObus && lObus.playerId != playerId)
        {
            AudioManager.instance.PlaySound(AudioManager.SoundType.Bump, false);
            lHasHit = true;
        }

        if (lHasHit)
        {
            Explosion.CreateExplosion(explosionScene, Position, InGameManager.instance);
            DeactivateObus();
        }
    }

    private void HandleMovementPhase(float pDelta)
    {
        ApplyMovement(pDelta);
        CheckScreenBoundaries();
    }

    private void DeactivateObus()
    {
        string lMonitoring = "monitoring";
        string lMonitorable = "monitorable";

        engineStart = false;

        Explosion.CreateExplosion(explosionScene, Position, InGameManager.instance);
        AudioManager.instance.PlaySound(AudioManager.SoundType.Explosion, false);

        particles.Emitting = false;

        Visible = false;
        SetProcess(false);
        SetPhysicsProcess(false);

        SetDeferred(lMonitoring, false);
        SetDeferred(lMonitorable, false);

        OnObusDestroyed?.Invoke(this);
    }

    public void ForceDeactivate()
    {
        if (engineStart)
            DeactivateObus();
    }
}