using Godot;
using Godot.Collections;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace marisamod.Scripts.PatchesNModels;

public partial class MehParticlesContainer : NParticlesContainer
{

	public override void _Ready()
	{
		base._Ready();
		Traverse.Create(this).Field("_particles").SetValue(new Array<GpuParticles2D>());
	}

}
