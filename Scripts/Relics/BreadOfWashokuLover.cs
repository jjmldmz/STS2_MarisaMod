using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace marisamod.Scripts.Relics;

[Pool(typeof(EventRelicPool))]
public class BreadOfWashokuLover : AbstractMarisaRelic
{
    public override RelicRarity Rarity => RelicRarity.Event;

    private bool _isActivating;

    private int _triggers;

    public override string FlashSfx => "event:/sfx/ui/relic_activate_draw";

    public override bool ShowCounter => true;

    public override int DisplayAmount
    {
        get
        {
            if (!IsActivating)
            {
                return Triggers % DynamicVars.Cards.IntValue;
            }
            return DynamicVars.Cards.IntValue;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(13)];
    private bool IsActivating
    {
        get => _isActivating;
        set
        {
            AssertMutable();
            _isActivating = value;
            UpdateDisplay();
        }
    }

    [SavedProperty]
    public int Triggers
    {
        get => _triggers;
        set
        {
            AssertMutable();
            _triggers = value;
            if (_triggers>=DynamicVars.Cards.IntValue){}
            UpdateDisplay();
        }
    }
    
    private void UpdateDisplay()
    {
        if (IsActivating)
        {
            Status = RelicStatus.Normal;
        }
        else
        {
            var intValue = DynamicVars.Cards.IntValue;
            Status = Triggers == intValue - 1 ? RelicStatus.Active : RelicStatus.Normal;
        }
        InvokeDisplayAmountChanged();
    }
}