using System;
using Godot;
using marisamod.Scripts.Cards;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace marisamod.Scenes.Vfx.SparkProjectile;

public partial class VfxSparkProjectile : Node2D
{
    public const string ScenePath = "res://Scenes/Vfx/SparkProjectile/VfxSparkProjectile.tscn";

    #region 公开属性
    public Vector2 Velocity = Vector2.Zero;
    public NCreature? PlayerOwner;//发射它的玩家
    public ISparkCard CardOwner;

    public float TimeLeft //动画总时间
    {
        get
        {
            if (NoIdle)
                return ChaseDuration;
            else
                return DampingDuration + ChaseDuration;
            
        }
    }
    public bool NoIdle = false;//减速结束后是否直接射向目标
    #endregion
    
    #region 动画参数
    //初始速度，用于参考,依据窗口初始化
    private float _startSpeed = 50f;
    //减速阶段参数
    private float _dampingAcceleration = 0.9f;
    public const float DampingDuration = 0.6f;
    //漫游阶段
    private float _orbitPhase = Mathf.Tau;
    private float _orbitSpeed = 1f;
    private float _ellipseRotation = Mathf.Tau;
    private float _ellipseRotationSpeed = 0.2f;
    private float _majorRadius;
    private float _minorRadius;
    //追踪阶段
    public const float ChaseDuration = 0.15f;
    private Vector2? _target;//目标
    //淡出
    private const float FadingDuration = 1f;
    #endregion
    
    #region 状态

    private enum ProjectileState
    {
        Damping,    // 减速阶段：发射后逐渐减速
        Idle,       // 待机阶段：接近静止状态，等待触发
        Wandering,  // 随机漫游阶段：触发被取消，环绕玩家游走
        Chasing,    // 追踪阶段：向目标移动
        FadingOut   // 淡出阶段：生命周期结束
    }
    private ProjectileState _state = ProjectileState.Damping;
    #endregion

    #region 子节点与材质缓存

    private Trail? _trail;
    public Trail Trail => _trail ??= GetNode<Trail>("trail");
    private ShaderMaterial? _trailShader;
    private ShaderMaterial TrailShader => _trailShader ??= (ShaderMaterial)Trail.Material;

    private MeshInstance2D? _slug;
    public MeshInstance2D Slug => _slug ??= GetNode<MeshInstance2D>("slug");
    private ShaderMaterial? _slugShader;
    private ShaderMaterial SlugShader => _slugShader ??= (ShaderMaterial)Slug.Material;
    private GpuParticles2D? _particles;
    public GpuParticles2D Particles => _particles ??= GetNode<GpuParticles2D>("particles");
    private ParticleProcessMaterial? _particleProcess;

    public ParticleProcessMaterial ParticleProcess =>
        _particleProcess ??= (ParticleProcessMaterial)Particles.ProcessMaterial;

    #endregion

    #region 状态计时
    private float _dampingTimeLeft;
    private float _chaseTimeLeft;
    private float _fadeTimeLeft;
    #endregion
    
    #region 状态逻辑
    private void ResetShader()
    {
        SlugShader.SetShaderParameter("visible", 1.0);
        TrailShader.SetShaderParameter("visible",1.0);
    }
    public void StartDamping()
    {
        Log.Info($"start damping");
        if (_state == ProjectileState.FadingOut) ResetShader();
        _state =  ProjectileState.Damping; 
        _dampingTimeLeft = DampingDuration;
    }
    private void UpdateDamping(double delta)
    {
        float deltaF = (float)delta;
        float speed = Velocity.Length();
        // 保留最低速度，确保方向有意义
        float minSpeed = _dampingAcceleration * deltaF * 0.001f;
        if (speed > _dampingAcceleration * deltaF)
        {
            speed -= _dampingAcceleration * deltaF;
        }
        else
        {
            speed = minSpeed;
        }
        Velocity = Velocity.Normalized() * speed;
        
        _dampingTimeLeft -= deltaF;
        Trail.LifeTimeOverwrite = _dampingTimeLeft;
        //切换状态
        if ( (_dampingTimeLeft <= 0 || speed <= minSpeed)  && _state == ProjectileState.Damping)
        {
            Velocity = Velocity.Normalized() * minSpeed;
            if (NoIdle)
                StartChasing();
            else 
                StartIdle();
        }
    }

    public void StartIdle()
    {
        Log.Info($"start idle");
        if (_state == ProjectileState.FadingOut) ResetShader();
        _state = ProjectileState.Idle;
        
    }
    private void UpdateIdle(double delta)
    {
        //TODO:检测关联卡牌当前是否正在被拖拽
        if (Trail.LifeTimeOverwrite > 4*(float)delta)//快速衰减尾迹长度
            Trail.LifeTimeOverwrite -= 2*(float)delta;
        else
            Trail.LifeTimeOverwrite = 0.0001f;

    }
    public void StartWandering()
    {
        Log.Info($"start wandering");
        if (_state == ProjectileState.FadingOut) ResetShader();
        _state = ProjectileState.Wandering;
        Trail.LifeTimeOverwrite = -1;
        Trail.CleanTrail();
    }
    private void UpdateWandering(float delta)
    {
        if (PlayerOwner == null)
            return;

        Vector2 center = PlayerOwner.VfxSpawnPosition;

        _orbitPhase += _orbitSpeed * delta;
        _ellipseRotation += _ellipseRotationSpeed * delta;

        // 椭圆上的局部点
        Vector2 localTarget = new Vector2(
            Mathf.Cos(_orbitPhase) * _majorRadius,
            Mathf.Sin(_orbitPhase) * _minorRadius
        );

        // 整个椭圆旋转
        Vector2 targetPos = center + localTarget.Rotated(_ellipseRotation);

        Vector2 toTarget = targetPos - Position;
        float dist = toTarget.Length();
        
        float desiredSpeed = Mathf.Lerp(
            0.35f * _startSpeed,
            0.85f * _startSpeed,
            Mathf.Clamp(dist / 80f, 0f, 1f)
        );
        if (toTarget.Length() <= desiredSpeed)
        {
            Velocity = toTarget;
        }
        else
        {
            Vector2 desiredVelocity = toTarget.Normalized() * desiredSpeed;
            float steering = 4.0f;
            Velocity = Velocity.Lerp(desiredVelocity, steering * delta);
        }

    }

    public void StartChasing(Vector2? target = null)
    {
        Log.Info($"start chasing");
        if (_state == ProjectileState.FadingOut) ResetShader();
        if (target != null)
            _target = target;
        _state = ProjectileState.Chasing;
        _chaseTimeLeft =  ChaseDuration;
        Trail.CleanTrail();
        Particles.Emitting = true;
        Trail.LifeTimeOverwrite = -1;
    }
    private void UpdateChasing(double delta)
    {
        if (_target == null)
        {
            StartFading();
            return;
        }

        Vector2 target = (Vector2)_target;
        float deltaF = (float)delta;
        _chaseTimeLeft -= deltaF;
        
        if (_chaseTimeLeft <= 0 || deltaF > _chaseTimeLeft + 0.001f)
        {
            // 到达目标
            Position = target;
            //Velocity = Vector2.Zero;
            Particles.Emitting = false;
            StartFading();
            return;
        }
        
        Vector2 diff = target - Position;
        if (diff.IsZeroApprox())
        {
            Particles.Emitting = false;
            StartFading();
            return;
        }
        
        // 计算目标速度（基于剩余时间）
        Vector2 targetVelocity = diff / _chaseTimeLeft;
        float targetSpeed = targetVelocity.Length();
        float oldSpeed = Velocity.Length();
        
        // 速度混合：progress 0→1，时间从 ChaseDuration 到 0
        float progress = 1f - Mathf.Min(1f, _chaseTimeLeft / ChaseDuration);
        float speedMix = Mathf.Clamp(progress + 0.4f, 0f, 1f);
        targetSpeed = Mathf.Lerp(oldSpeed, targetSpeed, speedMix);
        targetVelocity = targetVelocity.Normalized() * targetSpeed;
        
        float dirMix = Mathf.Clamp(progress + 0.4f, 0f, 1f);
        Vector2 oldDirection = Velocity.LengthSquared() > 0.01f ? Velocity.Normalized() : targetVelocity.Normalized();
        Velocity = targetVelocity * dirMix + oldDirection * targetSpeed * (1f - dirMix);
    }

    public void StartFading()
    {
        Log.Info($"start fading");
        _state = ProjectileState.FadingOut;
        _fadeTimeLeft = FadingDuration;
    }
    private void UpdateFading(double delta)
    {
        _fadeTimeLeft -= (float)delta;
        
        float alpha = Mathf.Max(0f, 2.0f * (_fadeTimeLeft - 0.5f));
        SlugShader.SetShaderParameter("visible", alpha);
        TrailShader.SetShaderParameter("visible", alpha);
        SlugShader.SetShaderParameter("projectile_speed", 0f);
        if (_fadeTimeLeft < 0f)
        {
            QueueFree();
        }
            
    }
    #endregion
    
    #region 更新函数
    private void UpdateTrailParticles(Vector2 displacement)
    {
        if (_state != ProjectileState.Chasing)
        {
            Particles.Emitting = false;
            return;
        }
        const float particleDensity = 2f;
        float amount = particleDensity * displacement.Length() + 0.5f;
        Particles.Position = -0.5f * displacement;
        Particles.AmountRatio = Mathf.Min(1f, amount / 100f);
        Particles.Rotation = displacement.Angle();
        ParticleProcess.EmissionBoxExtents = new Vector3(displacement.Length(), 1f, 0f);
    }
    
    private void UpdateMovement(double delta)
    {
        // 更新视觉参数（所有状态共用）
        if (Velocity.LengthSquared() > 0.0001f)
        {
            Slug.Rotation = Velocity.Angle();
            float speedNormalized = Mathf.Clamp(Velocity.Length() / _startSpeed, 0, 1);
            speedNormalized = Mathf.Pow(speedNormalized, 0.4f);
            SlugShader.SetShaderParameter("projectile_speed", speedNormalized);
            //Log.Info($"当前速度{ speedNormalized}");
        }
        Vector2 oldVelocity = Velocity;
        // 根据状态执行不同逻辑
        switch (_state)
        {
            case ProjectileState.Damping:
                UpdateDamping(delta);
                break;
            case ProjectileState.Idle:
                UpdateIdle(delta);
                break;
            case ProjectileState.Chasing:
                UpdateChasing(delta);
                break;
            case ProjectileState.Wandering:
                UpdateWandering((float)delta);
                break;
            case ProjectileState.FadingOut:
                UpdateFading(delta);
                break;
        }
        
        // 更新位置和拖尾粒子（仅在移动状态）
        if (_state != ProjectileState.FadingOut)
        {
            Vector2 displacement = Velocity * (float)delta;
            UpdateTrailParticles(displacement);
            Position += displacement;
            float turnAmount = Mathf.Abs(oldVelocity.AngleTo(Velocity)) / (Mathf.Pi / 36f);
            SlugShader.SetShaderParameter("turn_amount", Mathf.Min(turnAmount, 1f));
        }
    }
    #endregion
    public override void _Process(double delta)
    {
        UpdateMovement(delta);
    }
    
    public void SetColor(Vector4 color)
    {
        SlugShader.SetShaderParameter("spark_color", color);
        TrailShader.SetShaderParameter("spark_color", color);
    }
    public void ApplySize(float scale)
    {
        Slug.Scale *= scale;
        Trail.Scale *= scale;
    }
    public void VelocityInit(Vector2? target = null)
    {
        Vector2 containerSize = NCombatRoom.Instance?.CombatVfxContainer.Size ?? new Vector2(1,640);
        float baseScale = containerSize.Y;
        float dampingDistance = 0.15f * baseScale;//减速距离为0.15个屏幕宽度
        
        float speed = 2.0f* dampingDistance / DampingDuration;
        _startSpeed = speed;
        _dampingAcceleration = _startSpeed/DampingDuration;
        
        //随机化方向
        float dir = 0;
        if (target.HasValue)
        {
            dir = (target.Value - Position).Angle();
        }
        dir += -0.4f * Mathf.Pi + 0.8f * Mathf.Pi * GD.Randf();
        
        Velocity = speed * Vector2.FromAngle(dir);
        
        //设定漫游参数
        _majorRadius = baseScale * 0.2f *(float)GD.RandRange(1f, 1.25f);
        _minorRadius = baseScale * 0.1f*(float)GD.RandRange(0.8f, 1f);
        _orbitPhase *= GD.Randf();
        _ellipseRotation *= GD.Randf();
        _orbitSpeed = (float)GD.RandRange(1.4f, 1.8f);
        _ellipseRotationSpeed = (float)GD.RandRange(0.1f, 0.3f);
        if (GD.Randf() < 0.5f)
            _ellipseRotationSpeed *= -1;
    }
    
    //一共三种情况create，从手牌拖动预生成，直接打出牌，单纯装饰火花。
    
    //单纯生成火花。
    public static VfxSparkProjectile Create()
    {
        VfxSparkProjectile instance = GD.Load<PackedScene>(ScenePath)
            .Instantiate<VfxSparkProjectile>();
        
        return instance;
    }
    //为卡牌生成火花。初始化速度。
    public static VfxSparkProjectile? Create(ISparkCard card)
    {
        if (card is not AbstractMarisaCard marisaCard)
        {
            //TODO:日志
            return null;
        }
        NCreature? player = marisaCard.Owner.Creature.GetCreatureNode();
        if (player == null)
        {
            //TODO:日志
            return null;
        }
        VfxSparkProjectile vfx = Create();
        vfx.SetColor(card.SparkColor);
        vfx.Position = player.VfxSpawnPosition;
        vfx.PlayerOwner= player;
        vfx.CardOwner = card;
        vfx.VelocityInit();
        vfx.StartDamping();
        return vfx;
    }
    //生成后直接射向目标
    public static VfxSparkProjectile? Create(ISparkCard card,NCreature target)
    {
        VfxSparkProjectile? vfx = Create(card);
        if (vfx == null)
        {
            //TODO:
            return null;
        }
        vfx._target = target.VfxSpawnPosition;
        vfx.NoIdle = true;
        return vfx;
    }
}