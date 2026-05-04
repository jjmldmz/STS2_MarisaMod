using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace marisamod.Scenes.Vfx.SparkProjectile;

public partial class VfxSparkProjectile : Node2D
{
    public const string ScenePath = "res://Scenes/Vfx/SparkProjectile/VfxSparkProjectile.tscn";

    #region 公开属性
    public Vector2 Velocity = Vector2.Zero;
    public NCreature? PlayerOwner;//发射它的玩家
    public Vector2? Target;//目标
    public float VfxTime => NoIdle ? _chaseDuration : _chaseDuration + _dampingDuration;
    public bool NoIdle = false;//减速结束后是否直接射向目标
    public NCard? TargetCard = null;
    #endregion
    
    #region AnimationParament
    private float _startSpeed = 50f;
    private float _dampingAcceleration = 0.9f;
    private float _dampingDuration = 0.15f;
    private float _chaseDuration = 0.15f;
    private float _chasingSpeedMixMin = 0.4f;
    private float _chasingDirMixMin = 0.4f;
    const float FadingDuration = 1f;
    #endregion
    public void SetAnimationParament(float dampingDuration = 0.15f,float chaseDuration = 0.15f,float chasingSpeedMixMin = 0.4f,float chasingDirMixMin = 0.4f)
    {
        _dampingDuration =  dampingDuration;
        _chaseDuration = chaseDuration;
        _chasingSpeedMixMin = chasingSpeedMixMin;
        _chasingDirMixMin = chasingDirMixMin;
    }
    #region ChildNode and Material
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
        
    #region State
    private enum ProjectileState
    {
        Damping,    // 减速阶段：发射后逐渐减速
        Idle,       // 待机阶段：接近静止状态，等待触发
        Chasing,    // 追踪阶段：向目标移动
        FadingOut   // 淡出阶段：生命周期结束
    }
    private ProjectileState _state = ProjectileState.Damping;
    #endregion

    #region State Time Counter
    private float _dampingTimeLeft;
    private float _chaseTimeLeft;
    private float _fadeTimeLeft;
    #endregion
    
    #region State update
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
        _dampingTimeLeft = _dampingDuration;
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

    public void StartChasing(Vector2? target = null)
    {
        Log.Info($"start chasing");
        if (_state == ProjectileState.FadingOut) ResetShader();
        //if (_state == ProjectileState.Idle)Trail.CleanTrail();
        Trail.CleanTrail();
        if (target != null)
            Target = target;
        _state = ProjectileState.Chasing;
        _chaseTimeLeft =  _chaseDuration;
        Particles.Emitting = true;
        Trail.LifeTimeOverwrite = -1;
    }
    private void UpdateChasing(double delta)
    {
        if (Target == null)
        {
            StartFading();
            return;
        }

        Vector2 target = (Vector2)Target;
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
        float progress = 1f - Mathf.Min(1f, _chaseTimeLeft / _chaseDuration);
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

    private void UpdateCardTarget()
    {
        if (TargetCard == null)
            return;
        Vector2? local =
            NCombatRoom.Instance?.CombatVfxContainer.GetGlobalTransformWithCanvas().AffineInverse() *
            TargetCard.GetGlobalTransformWithCanvas().Origin;
        if (local != null)
            Target = local.Value;
    }
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
        UpdateCardTarget();
        UpdateMovement(delta);
    }


    public void SetColor(Vector4 color)
    {
        SlugShader.SetShaderParameter("spark_color", color);
        TrailShader.SetShaderParameter("spark_color", color);
    }

    public void ApplySize(float scale)//初始scale不是1
    {
        Slug.Scale *= scale;
        Trail.Scale *= scale;
    }
    public void ApplySizeFromDamage(int damage)//初始scale不是1
    {
        float damageCut = Mathf.Clamp(damage,2f, 128f);
        float scale = Mathf.Log(damageCut) / Mathf.Log(8);
        ApplySize(scale);
    }

    public void VelocityInit(float dir = 0)
    {
        Vector2 containerSize = NCombatRoom.Instance?.CombatVfxContainer.Size ?? new Vector2(1,640);
        float baseScale = containerSize.Y;
        float dampingDistance = 0.2f * baseScale;
        
        float speed = 2.0f* dampingDistance / _dampingDuration;
        _startSpeed = speed;
        _dampingAcceleration = _startSpeed/_dampingDuration;
        
        dir += -0.4f * Mathf.Pi + 0.8f * Mathf.Pi * GD.Randf();
        
        Velocity = speed * Vector2.FromAngle(dir);
        Velocity *= (float)GD.RandRange(0.95f,1.05f);
    }
    
    public static VfxSparkProjectile Create()
    {
        VfxSparkProjectile instance = GD.Load<PackedScene>(ScenePath)
            .Instantiate<VfxSparkProjectile>();
        return instance;
    }
    
    public static VfxSparkProjectile Create(NCreature player, Vector4 color)
    {
        VfxSparkProjectile vfx = Create();
        vfx.SetColor(color);
        vfx.Position = player.VfxSpawnPosition;
        vfx.PlayerOwner= player;
        vfx.VelocityInit();
        vfx.StartDamping();
        return vfx;
    }
    public static VfxSparkProjectile Create(CardModel card, Vector4 color)
    {
        NCreature player = card.Owner.Creature.GetCreatureNode();
        //if (player == null ) 
        VfxSparkProjectile vfx = Create(player, color);
        vfx.ApplySizeFromDamage(card.DynamicVars.Damage.IntValue);
        return vfx;
    }
    public static VfxSparkProjectile Create(CardModel card,Vector4 color,NCreature target)
    {
        VfxSparkProjectile vfx = Create(card,color);
        vfx.Target = target.VfxSpawnPosition;
        vfx.NoIdle = true;
        return vfx;
    }
    
}