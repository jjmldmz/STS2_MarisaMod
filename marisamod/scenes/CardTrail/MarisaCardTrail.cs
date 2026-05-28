using System;
using MegaCrit.Sts2.Core.Nodes.Vfx;
namespace marisamod.Scenes.CardTrail;

public partial class MarisaCardTrail : NCardTrail
{
    /*
    public GpuParticles2D Particles => _particles ??= GetNode<GpuParticles2D>("particles");
    private ParticleProcessMaterial? _particleProcess;
    public ParticleProcessMaterial ParticleProcess =>
        _particleProcess ??= (ParticleProcessMaterial)Particles.ProcessMaterial;
    private void UpdateTrailParticles(Vector2 displacement)
    {
        
        if (_state != ProjectileState.Chasing)
        {
            Particles.Emitting = false;
            return;
        }

        const float particleDensity = 1f;
        float amount = particleDensity * displacement.Length() + 0.5f;
        Particles.Position = -0.5f * displacement;
        Particles.AmountRatio = Mathf.Min(1f, amount / 100f);
        Particles.Rotation = displacement.Angle();
        ParticleProcess.EmissionBoxExtents = new Vector3(displacement.Length(), 1f, 0f);
    }*/
}