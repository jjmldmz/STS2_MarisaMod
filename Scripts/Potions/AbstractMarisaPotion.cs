using BaseLib.Abstracts;
using BaseLib.Utils;
using marisamod.Scripts.PatchesNModels;

namespace marisamod.Scripts.Potions;

[Pool(typeof(MarisaPotionPool))]
public abstract class AbstractMarisaPotion : CustomPotionModel
{
    public override string? CustomPackedImagePath => GetImagePath();
    public override string? CustomPackedOutlinePath => GetOutlinePath();
    
    protected abstract string GetImagePath();
    protected abstract string GetOutlinePath();

    protected const string GodotIconPath = "res://icon.svg";

    protected string PotionIconPath => $"res://marisamod/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected string PotionOutlinePath => $"res://marisamod/images/cards/{Id.Entry.ToLowerInvariant()}_outline.png";
}
