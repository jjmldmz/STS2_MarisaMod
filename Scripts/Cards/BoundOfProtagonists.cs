using marisamod.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace marisamod.Scripts.Cards;

public class BoundOfProtagonists : AbstractMarisaCard//AbstractAmplifiedCard //AbstractMarisaCard
{
    public BoundOfProtagonists() : base( 2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    //public override string PortraitPath => "res://marisamod/images/cards/marisamod-test_marisa_card.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([new DynamicVar("Power", 1)]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<FlightPower>()
    ];

    protected override void OnUpgrade()
    {
        //DynamicVars["Power"].UpgradeValueBy(1);
        //DynamicVars.Energy.UpgradeValueBy(-1);
        EnergyCost.UpgradeBy(-1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await base.OnPlay(choiceContext, cardPlay);
        await PowerCmd.Apply<BoundOfProtagonistsPower>(choiceContext, Owner.Creature, DynamicVars["Power"].IntValue, Owner.Creature, this);
        // if (AmplifiedInPlay)
        // {
        //     await PowerCmd.Apply<FlightPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
        // }
    }
}