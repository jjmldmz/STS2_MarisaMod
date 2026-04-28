using marisamod.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace marisamod.Scripts.Enchantments;

public class StarlitEnchantment : AbstractMarisaEnchantment
{
    public decimal AmplifyCost = 0;

    public override bool CanEnchant(CardModel card)
    {
        return card.Enchantment == null && !card.Keywords.Contains(CardKeyword.Unplayable);
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card == Card)
        {
            AmplifyCost = 0;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card == Card && cardPlay.Resources.EnergySpent > 0)
        {
            var amt = AmplifyCost + cardPlay.Resources.EnergySpent;
            await PowerCmd.Apply<StarlitPower>(context, Card.Owner.Creature, amt, Card.Owner.Creature, Card);
        }
    }
}