using Godot;
using System;
using System.Collections.Generic;

public partial class Tank : Movable
{
    public static event Action<int> OnEnemyHit;
    public static List<Tank> activeTanks = new List<Tank>();

    [ExportGroup("Pool")]
    [Export] public int poolSize = 30;

    [ExportGroup("References")]
    [Export] private PackedScene obusScene;
    [Export] private Marker2D muzzle;
    [Export] private CharacterSkinDisplay characterSkinDisplay;

    [ExportGroup("PC Controls")]
    [Export] public Key playerAKey = Key.Q;
    [Export] public Key playerBKey = Key.M;

    [ExportGroup("Movement")]
    [Export] public float normalSpeed = 200.0f;
    [Export] public float introSpeed = 50.0f;
    [Export] public float stepSpeed = -600.0f;
    [Export] public float dashDeceleration = 5.0f;
    [Export] public float bumpSpeed = 800.0f;

    [ExportGroup("Rotation")]
    [Export] public float rotationDuration = 1.5f;
    [Export] public float rotationDegrees = 360f;
    [Export] public float emotionDuration = 1.5f;
    [Export] public float moveBobSpeed = 0.1f;
    [Export] public float moveBobAmount = 2.0f;
    [Export] public Color chargeMaxColor = new Color(1.0f, 0.2f, 0.2f);

    [ExportGroup("Shoot")]
    [Export] public float shootCooldown = 0.3f;
    [Export] public float minObusSpeed = 200f;
    [Export] public float maxObusSpeed = 1000f;
    [Export] public float minObusLife = 0.2f;
    [Export] public float maxObusLife = 4.0f;

    [ExportGroup("Health")]
    [Export] public int maxHealth = 5;
    [Export] public float invulnerabilityDuration = 0.7f;
    [Export] public float invulnerableRotationSpeed = 860f;

    [ExportGroup("Destruction")]
    [Export] private Texture2D destroyTank;
    [Export] private GpuParticles2D explosion;

    [ExportGroup("Visuals")]
    [Export] public float blinkDelay = 0.1f;
    [Export] public float squashFactor = 0.3f;
    [Export] public float maxShakeIntensity = 3.0f;
    [Export] public Vector2 recoilStretchScale = new Vector2(1.35f, 0.65f);
    [Export] public Vector2 maxRecoilStretchScale = new Vector2(1.9f, 0.25f);
    [Export] public float recoilRecoverTime = 0.7f;

    public int currentHealth;
    public int playerId;

    private int sweepDirection = 1;
    private float rotationTimer = 0f;
    private float invulnerabilityTimer = 0f;
    private float currentShootCooldown = 0f;
    private bool isInputPressed = false;
    private bool isReacting = false;

    private AudioStreamPlayer currentReloadSound;

    private Tween faceTween;
    private Tween blobTween;
    private Tween blinkTween;

    protected Action<float> doAction;

    private ObjectPool obusPool;

    public override void _Ready()
    {
        base._Ready();

        activeTanks.Add(this);
        OnEnemyHit += HandleEnemyHit;
        characterSkinDisplay.targetPlayerId = playerId;

        ApplyPlayerSkin();
        InitializeMargin();

        currentHealth = maxHealth;

        obusPool = new ObjectPool(obusScene, poolSize, InGameManager.instance, InitializeObus);

        InitStopped();
        FullReset();
    }

    private void InitializeObus(Node pNode)
    {
        string lMonitoring = "monitoring";
        string lMonitorable = "monitorable";

        if (pNode is Obus pObus)
        {
            pObus.GlobalPosition = new Vector2(-9999, -9999);
            pObus.OnObusDestroyed += ReturnObusToPool;
            pObus.Visible = false;
            pObus.SetProcess(false);
            pObus.SetPhysicsProcess(false);
            pObus.SetDeferred(lMonitoring, false);
            pObus.SetDeferred(lMonitorable, false);
        }
    }

    private void ReturnObusToPool(Obus pObus)
    {
        obusPool.Release(pObus);
    }

    public override void _ExitTree()
    {
        OnEnemyHit -= HandleEnemyHit;
        activeTanks.Remove(this);
    }

    public override void _Process(double pDelta)
    {
        float lDelta = (float)pDelta;

        HandlePCInput();

        if (currentShootCooldown > 0)
            currentShootCooldown -= lDelta;

        doAction?.Invoke(lDelta);
    }

    private void HandlePCInput()
    {
        Godot.Key lKey = Godot.Key.None;

        if (playerId == 0)
            lKey = playerAKey;
        else if (playerId == 1)
            lKey = playerBKey;

        if (lKey == Godot.Key.None) return;

        bool lIsKeyPressed = Input.IsPhysicalKeyPressed(lKey);

        if (lIsKeyPressed && !isInputPressed)
            SetRotating(true);
        else if (!lIsKeyPressed && isInputPressed)
            SetRotating(false);
    }

    public void InitStopped()
    {
        currentSpeed = 0;
        StopBlob();
        blinkTween?.Kill();
        sprite.Visible = true;
        ResetSpriteVisuals();

        doAction = (pDelta) => { };
    }

    public void InitIntro()
    {
        currentSpeed = introSpeed;
        moveDirection = Vector2.FromAngle(Rotation);

        StartBlob();

        doAction = ApplyMovement;
    }

    public void InitNormal()
    {
        moveDirection = Vector2.FromAngle(Rotation);

        StartBlob();

        blinkTween?.Kill();
        sprite.Visible = true;

        doAction = Normal;
    }

    private void Normal(float pDelta)
    {
        if (isInputPressed && currentShootCooldown <= 0)
        {
            InitCharging();
            return;
        }

        currentSpeed = Mathf.Lerp(currentSpeed, normalSpeed, dashDeceleration * pDelta);

        ApplyMovement(pDelta);
        CheckScreenBoundaries();
    }

    public void InitCharging()
    {
        rotationTimer = 0f;
        StopBlob();

        currentReloadSound = AudioManager.instance.PlaySound(AudioManager.SoundType.Reload, false);

        doAction = Charging;
    }

    private void Charging(float pDelta)
    {
        if (rotationTimer < rotationDuration)
        {
            rotationTimer = Mathf.Min(rotationTimer + pDelta, rotationDuration);
            float lRotationAmount = (rotationDegrees / rotationDuration) * pDelta * sweepDirection;
            RotationDegrees += lRotationAmount;
            moveDirection = Vector2.FromAngle(Rotation);
        }

        ApplyChargeVisuals();
    }

    public void InitInvulnerable()
    {
        invulnerabilityTimer = invulnerabilityDuration;

        StopBlob();
        ResetSpriteVisuals();
        StartBlinking();

        doAction = Invulnerable;
    }

    private void Invulnerable(float pDelta)
    {
        invulnerabilityTimer -= pDelta;
        RotationDegrees += invulnerableRotationSpeed * pDelta;
        currentSpeed = Mathf.Lerp(currentSpeed, normalSpeed, dashDeceleration * pDelta);

        ApplyMovement(pDelta);
        CheckScreenBoundaries();

        if (invulnerabilityTimer <= 0)
            InitNormal();
    }

    public void SetRotating(bool pIsRotating)
    {
        isInputPressed = pIsRotating;

        if (isInputPressed && doAction == Normal && currentShootCooldown <= 0)
            InitCharging();
        else if (!isInputPressed && doAction == Charging)
            ExecuteShoot();
    }

    private void ExecuteShoot()
    {
        currentShootCooldown = shootCooldown;
        float lChargeRatio = rotationTimer / rotationDuration;

        float lObusSpeed = Mathf.Lerp(minObusSpeed, maxObusSpeed, lChargeRatio);
        float lObusLife = Mathf.Lerp(minObusLife, maxObusLife, lChargeRatio);

        if (currentReloadSound != null)
        {
            AudioManager.instance.StopSound(currentReloadSound);
            currentReloadSound = null;
        }

        AudioManager.instance.PlaySound(AudioManager.SoundType.Shoot, false);

        Obus lObus = (Obus)obusPool.Get();
        lObus.Fire(playerId, muzzle.GlobalPosition, muzzle.GlobalRotationDegrees, lObusSpeed, lObusLife);

        ApplyStretchRecoil(lChargeRatio);

        currentSpeed = stepSpeed;
        sweepDirection *= -1;
        InitNormal();
    }

    protected override void OnHit(Area2D pArea)
    {
        if (pArea is Tank lOtherTank)
        {
            AudioManager.instance.PlaySound(AudioManager.SoundType.Bump, false);
            moveDirection = (GlobalPosition - lOtherTank.GlobalPosition).Normalized();
            RotationDegrees = Mathf.RadToDeg(moveDirection.Angle());
        }

        if (pArea is Obus lObus && lObus.playerId != playerId && doAction != Invulnerable)
        {
            moveDirection = (GlobalPosition - lObus.GlobalPosition).Normalized();
            currentSpeed = bumpSpeed;

            Explosion.CreateExplosion(explosionScene, Position, InGameManager.instance);
            ChangeEmotion(CharacterSkinDisplay.FaceState.Sad);

            OnEnemyHit?.Invoke(lObus.playerId);

            currentHealth--;
            InGameManager.instance?.UpdateHealthBar(playerId, currentHealth, maxHealth);

            UpdateAllBaseEmotions();

            if (currentHealth <= 0)
                DestroyTank();
            else
                InitInvulnerable();
        }
    }

    private void DestroyTank()
    {
        InGameManager.instance.OnTankDied(playerId);
        ChangeEmotion(CharacterSkinDisplay.FaceState.Sad);

        sprite.Texture = destroyTank;
        explosion.Emitting = true;

        StartBlinking();
    }

    private void StartBlinking()
    {
        blinkTween?.Kill();
        blinkTween = CreateTween().SetLoops();
        blinkTween.TweenCallback(Callable.From(() => { sprite.Visible = !sprite.Visible; })).SetDelay(blinkDelay);
    }

    private void StartBlob()
    {
        blobTween?.Kill();
        blobTween = CreateTween().SetProcessMode(Tween.TweenProcessMode.Idle).SetLoops();

        blobTween.TweenProperty(sprite, TweenPaths.position, new Vector2(0, -moveBobAmount), moveBobSpeed)
          .SetTrans(Tween.TransitionType.Sine)
          .SetEase(Tween.EaseType.Out);

        blobTween.TweenProperty(sprite, TweenPaths.position, new Vector2(-moveBobAmount, 0), moveBobSpeed)
          .SetTrans(Tween.TransitionType.Sine)
          .SetEase(Tween.EaseType.In);

        blobTween.TweenProperty(sprite, TweenPaths.position, new Vector2(0, moveBobAmount), moveBobSpeed)
          .SetTrans(Tween.TransitionType.Sine)
          .SetEase(Tween.EaseType.In);

        blobTween.TweenProperty(sprite, TweenPaths.position, new Vector2(moveBobAmount, 0), moveBobSpeed)
          .SetTrans(Tween.TransitionType.Sine)
          .SetEase(Tween.EaseType.In);
    }

    private void StopBlob()
    {
        blobTween?.Kill();
        sprite.Position = Vector2.Zero;
    }

    private void ApplyChargeVisuals()
    {
        float lRawRatio = rotationTimer / rotationDuration;
        float lSmoothRatio = Mathf.SmoothStep(0f, 1f, lRawRatio);

        float lSquashFactor = lSmoothRatio * squashFactor;
        sprite.Scale = new Vector2(1.0f + lSquashFactor, 1.0f - lSquashFactor);

        float lShakeIntensity = lSmoothRatio * maxShakeIntensity;
        sprite.Position = new Vector2(
          (float)GD.RandRange(-lShakeIntensity, lShakeIntensity),
          (float)GD.RandRange(-lShakeIntensity, lShakeIntensity)
        );

        sprite.Modulate = Colors.White.Lerp(chargeMaxColor, lSmoothRatio);
    }

    private void ApplyStretchRecoil(float pChargeRatio)
    {
        Vector2 lTargetRecoil = recoilStretchScale.Lerp(maxRecoilStretchScale, pChargeRatio);
        sprite.Scale = lTargetRecoil;

        Tween lTween = CreateTween().SetProcessMode(Tween.TweenProcessMode.Idle).SetParallel();

        lTween.TweenProperty(sprite, TweenPaths.scale, Vector2.One, recoilRecoverTime)
          .SetTrans(Tween.TransitionType.Elastic)
          .SetEase(Tween.EaseType.Out);

        lTween.TweenProperty(sprite, TweenPaths.modulate, Colors.White, recoilRecoverTime)
          .SetTrans(Tween.TransitionType.Elastic)
          .SetEase(Tween.EaseType.Out);
    }

    private void ResetSpriteVisuals()
    {
        sprite.Scale = Vector2.One;
        sprite.Position = Vector2.Zero;
        sprite.Modulate = Colors.White;
    }

    public void ChangeEmotion(CharacterSkinDisplay.FaceState pState)
    {
        isReacting = true;
        characterSkinDisplay.SetFace(pState);
        characterSkinDisplay.PlayReaction();

        faceTween?.Kill();
        faceTween = CreateTween();

        faceTween.TweenInterval(emotionDuration);
        faceTween.TweenCallback(Callable.From(() => {
            isReacting = false;
            characterSkinDisplay.SetFace(GetBaseEmotion());
        }));
    }

    public static void UpdateAllBaseEmotions()
    {
        foreach (var tank in activeTanks)
        {
            tank.UpdateBaseEmotionVisual();
        }
    }

    public void UpdateBaseEmotionVisual()
    {
        if (!isReacting)
        {
            characterSkinDisplay.SetFace(GetBaseEmotion());
        }
    }

    private CharacterSkinDisplay.FaceState GetBaseEmotion()
    {
        if (activeTanks.Count <= 1) return CharacterSkinDisplay.FaceState.Normal;

        bool isHighest = true;
        bool isLowest = true;

        foreach (var tank in activeTanks)
        {
            if (tank == this) continue;
            if (currentHealth < tank.currentHealth) isHighest = false;
            if (currentHealth > tank.currentHealth) isLowest = false;
        }

        if (isHighest && !isLowest) return CharacterSkinDisplay.FaceState.Happy;
        if (isLowest && !isHighest) return CharacterSkinDisplay.FaceState.Sad;

        return CharacterSkinDisplay.FaceState.Normal;
    }

    private void ApplyPlayerSkin()
    {
        sprite.Texture = SkinManager.instance.GetTankSkin(playerId).tankTexture;
    }

    private void HandleEnemyHit(int pShooterId)
    {
        if (pShooterId == playerId)
            ChangeEmotion(CharacterSkinDisplay.FaceState.Happy);
    }

    protected override void OnBounce()
    {
        moveDirection = Vector2.FromAngle(Rotation);
    }

    public void FullReset()
    {
        currentHealth = maxHealth;
        explosion.Emitting = false;

        isInputPressed = false;
        currentShootCooldown = 0f;
        rotationTimer = 0f;

        if (currentReloadSound != null)
        {
            AudioManager.instance.StopSound(currentReloadSound);
            currentReloadSound = null;
        }

        ApplyPlayerSkin();
        InitStopped();

        isReacting = false;
        UpdateAllBaseEmotions();
    }
}