
using Godot;
using marisamod.Scripts.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace marisamod.Scenes.Vfx.SparkProjectile;

public interface ISparkCard
{
    public Vector4 SparkColor { get; }
    public void OnDrag();
    public void OnCancelPlay();
}