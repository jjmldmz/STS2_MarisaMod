using BaseLib.Abstracts;
using Godot;

namespace marisamod.Scripts.PatchesNModels;

public class MarisaRelicPool : CustomRelicPoolModel
{
    // 卡池的能量图标。加载路径为“res://images/atlases/ui_atlas.sprites/card/energy_{EnergyColorName}.tres”。
    //public override string EnergyColorName => "marisa";

    public override string BigEnergyIconPath => "res://marisamod/images/ui/cardOrb.png";

    public override string TextEnergyIconPath => "res://marisamod/images/ui/energyOrb-lighter.png";

    public override Color LabOutlineColor => new(0f, 0.1f, 0.5f);
}
