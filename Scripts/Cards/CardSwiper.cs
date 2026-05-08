using Godot;
using marisamod.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace marisamod.Scripts.Cards;

public class CardSwiper : AbstractMarisaCard
{
    public override string PortraitPath => $"res://marisamod/images/cards/marisamod-test_marisa_card.png";

    public CardSwiper() : base(1, CardType.Skill, CardRarity.Rare, TargetType.AllAllies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var enumerable = CombatState!.GetTeammatesOf(Owner.Creature)
            .Where(c => c is { IsAlive: true, IsPlayer: true } && c != Owner.Creature);
        List<CardModel> stealedCard = [];
        foreach (var creature in enumerable)
        {
            CardModel? cardToSteal = GetCard(creature.Player);
            if (cardToSteal == null) continue;
            await CardPileCmd.RemoveFromCombat(cardToSteal);

            CardModel copy = GetClone(cardToSteal);
            stealedCard.Add(copy);
            
            if (creature.HasPower<SwipedCardPower>())
            {
                SwipedCardPower pow = creature.GetPower<SwipedCardPower>();
                await pow.Steal(cardToSteal);
            }
            else
            {
                SwipedCardPower pow = (SwipedCardPower)ModelDb.Power<SwipedCardPower>().ToMutable();
                await pow.Steal(cardToSteal);
                await PowerCmd.Apply(new ThrowingPlayerChoiceContext(), pow, creature, 1M, Owner.Creature, this);
            }


        }
        await Cmd.Wait(0.2f);
        foreach (CardModel card in stealedCard)
        {
            //await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
            CardPileCmd.Add(card, PileType.Hand);
        }
        CardCmd.Preview(stealedCard);
    }

    private CardModel? GetCard(Player target)
    {
        List<CardModel> list = (from c in CardPile.GetCards(target, PileType.Draw, PileType.Discard)
            where c.DeckVersion != null
            select c).ToList();
        if (list.Count == 0) return null;
        return list.TakeRandom(1, Owner.RunState.Rng.CombatCardSelection).FirstOrDefault();
    }

    private CardModel GetClone(CardModel card)
    {
        CardModel copy = CombatState.CreateCard(ModelDb.GetById<CardModel>(card.Id),Owner);
        if (card.Enchantment != null && copy.Enchantment == null)
        {
            var zaEnchantment = ModelDb.GetById<EnchantmentModel>(card.Enchantment.Id).ToMutable();
            CardCmd.Enchant(zaEnchantment, copy, card.Enchantment.Amount);
        }
        copy.DynamicVars.Clone(card);
        return copy;
    }
}