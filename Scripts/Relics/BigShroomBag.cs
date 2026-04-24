using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;

namespace marisamod.Scripts.Relics;

public class BigShroomBag : AbstractMarisaRelic
{
    public override RelicRarity Rarity => RelicRarity.Event;
    

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card is SporeMind && cardPlay.Card.Owner == Owner)
        {
            await CardPileCmd.Draw(context, DynamicVars["Draw"].BaseValue, Owner);
            await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
            await CreatureCmd.Heal(Owner.Creature,DynamicVars.Heal.IntValue);
        }
    }
}