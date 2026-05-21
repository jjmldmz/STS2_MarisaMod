using Godot;
using marisamod.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace marisamod.Scripts.Enchantments;

public class StarlitEnchantment : AbstractMarisaEnchantment
{
    public int AmplifyCost = 0;

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
        var amt = AmplifyCost + cardPlay.Resources.EnergySpent;
        var amtFin = Mathf.RoundToInt(Mathf.Pow(2, amt));
        if (cardPlay.Card == Card && amtFin > 0)
        {
            await PowerCmd.Apply<StarlitPower>(context, Card.Owner.Creature, amtFin, Card.Owner.Creature, Card);
        }
    }
}