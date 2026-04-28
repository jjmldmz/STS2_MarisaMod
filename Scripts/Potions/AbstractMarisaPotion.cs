using BaseLib.Abstracts;
using BaseLib.Utils;
using marisamod.Scripts.PatchesNModels;

namespace marisamod.Scripts.Potions;

[Pool(typeof(MarisaPotionPool))]
public abstract class AbstractMarisaPotion : CustomPotionModel
{
    public override string? CustomPackedImagePath => $"res://marisamod/images/potions/{Id.Entry.ToLowerInvariant()}.png"; //GetImagePath();
    public override string? CustomPackedOutlinePath => $"res://marisamod/images/potions/{Id.Entry.ToLowerInvariant()}_outline.png"; //GetOutlinePath();

    // protected abstract string GetImagePath();
    // protected abstract string GetOutlinePath();

    protected const string GodotIconPath = "res://icon.svg";

    // protected string PotionIconPath => $"res://marisamod/images/potions/{Id.Entry.ToLowerInvariant()}.png";
    // protected string PotionOutlinePath => $"res://marisamod/images/potions/{Id.Entry.ToLowerInvariant()}_outline.png";
}