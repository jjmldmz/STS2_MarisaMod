using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace marisamod.Scripts.Powers;
public class SwipedCardPower : AbstractMarisaPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    private readonly Dictionary<Player,List<CardModel>>  _stolenCardsDic = new();
    private List<CardModel>? MyStolenCards => _stolenCardsDic!.GetValueOrDefault(Owner.Player);  
    public override Task AfterCombatEnd(CombatRoom room)
    {
        var player = Owner.Player;
        if (player == null) return Task.CompletedTask;
        if (MyStolenCards == null) return Task.CompletedTask;
        if (MyStolenCards.Count == 0 ) return Task.CompletedTask;

        IRunState runState = CombatState.RunState;
        foreach (var card in MyStolenCards)
        {
            if (card.DeckVersion == null) continue;
            runState.AddCard(card.DeckVersion, player);
            SpecialCardReward specialCardReward = new SpecialCardReward(card.DeckVersion, player);
            specialCardReward.SetCustomDescriptionEncounterSource(ModelDb.Encounter<ThievingHopperWeak>().Id);
            ((CombatRoom)runState.CurrentRoom!).AddExtraReward(player, specialCardReward);
        }
        MyStolenCards.Clear();
        return Task.CompletedTask;
    }

    public async Task Steal(CardModel card) 
    {
        var player = card.Owner.Creature.Player;
        if (player == null) return;
        _stolenCardsDic.TryGetValue(player, out List<CardModel>? stolenCards);
        if (stolenCards == null)
        {
            stolenCards = new List<CardModel>();
            _stolenCardsDic.Add(player, stolenCards);
        }
        stolenCards.Add(card);
        if (card.DeckVersion != null)
        {
            await CardPileCmd.RemoveFromDeck(card.DeckVersion, showPreview: false);
        }
    }
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        MyStolenCards == null ? Array.Empty<IHoverTip>() : MyStolenCards.Select(c => HoverTipFactory.FromCard(c)).ToArray();
}