using BaseLib.Abstracts;
using BaseLib.Utils;
using marisamod.Scripts.PatchesNModels;

namespace marisamod.Scripts.Potions;

[Pool(typeof(MarisaPotionPool))]
public abstract class AbstractMarisaPotion : CustomPotionModel
{
    public override string? CustomPackedImagePath => "res://icon.svg";
    public override string? CustomPackedOutlinePath => "res://icon.svg";
}
