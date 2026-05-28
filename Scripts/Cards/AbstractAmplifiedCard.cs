using marisamod.Scripts.Enchantments;
using marisamod.Scripts.PatchesNModels;
using marisamod.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace marisamod.Scripts.Cards
{
    public abstract class AbstractAmplifiedCard(
        int baseCost,
        int kickerCost,
        CardType type,
        CardRarity rarity,
        TargetType target) : AbstractMarisaCard(baseCost, type, rarity, target)
    {
        public int KickerCost { get; } = kickerCost;

        //public bool IsAmplified { get; protected set; }
        public bool AmplifiedInPreview;

        public bool AmplifiedInPlay;

        //private bool _costModifiedForAmplify;

        //public bool CostModifiedForAmplify => _costModifiedForAmplify;

        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new EnergyVar(KickerCost)
        ];

        public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [
            MarisaCardKeyWords.Amplify
        ];

        protected override bool ShouldGlowGoldInternal => AmplifiedInPreview; //IsAmplified;

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            AmplifiedInPlay = false;
            if (Owner.Creature.HasPower<OneTimeOffPower>())
            {
                return;
            }

            if (cardPlay.IsAutoPlay || Owner.Creature.HasPower<MillisecondPulsarsPower>() ||
                Owner.Creature.HasPower<PulseMagicPower>())
            {
                AmplifiedInPlay = true;
            }
            else if (Owner.PlayerCombatState?.Energy >= KickerCost)
            {
                AmplifiedInPlay = true;
                if (KickerCost > 0)
                {
                    if (Enchantment is StarlitEnchantment enchantment)
                    {
                        enchantment.AmplifyCost = DynamicVars.Energy.IntValue;
                    }

                    await PlayerCmd.LoseEnergy(DynamicVars.Energy.BaseValue, Owner);
                }
            }
        }

        // public override IEnumerable<CardKeyword> CanonicalKeywords => [
        //     MarisaCardKeyWords.Amplify
        // ];

        public virtual void ValidateAmplify()
        {
            if (RunState == null)
                return;
            if (Owner.PlayerCombatState == null) return;
            switch (Pile)
            {
                //if (Owner.PlayerCombatState.Hand.Cards.Contains(this))
                case { Type: PileType.Hand } when Owner.Creature.HasPower<OneTimeOffPower>():
                    //SetAmplifyState(false, false);
                    AmplifiedInPreview = false;
                    break;
                case { Type: PileType.Hand } when Owner.Creature.HasPower<MillisecondPulsarsPower>() ||
                                                  Owner.Creature.HasPower<PulseMagicPower>():
                    //SetAmplifyState(true, true);
                    AmplifiedInPreview = true;
                    break;
                case { Type: PileType.Hand } when Owner.PlayerCombatState.Energy < EnergyCost.GetWithModifiers(CostModifiers.All):
                    //SetAmplifyState(false, false);
                    AmplifiedInPreview = false;
                    break;
                case { Type: PileType.Hand }:
                {
                    if (Owner.PlayerCombatState.Energy >=
                        EnergyCost.GetWithModifiers(CostModifiers.All) + KickerCost)
                    {
                        //SetAmplifyState(true, false);
                        AmplifiedInPreview = true;
                    }

                    break;
                }
                case { Type: PileType.Discard or PileType.Draw or PileType.Exhaust }:
                    //SetAmplifyState(false, false);
                    AmplifiedInPreview = false;
                    break;
            }
        }

        // private void SetAmplifyState(bool isAmplified, bool costFree)
        // {
        //     AmplifiedInPreview = isAmplified;
        //     //IsAmplified = isAmplified;
        //     // if (isAmplified && !costFree && !_costModifiedForAmplify)
        //     // {
        //     //     EnergyCost.AddThisCombat(KickerCost);
        //     //     _costModifiedForAmplify = true;
        //     // }
        //     //
        //     // if (!isAmplified && _costModifiedForAmplify || costFree && _costModifiedForAmplify)
        //     // {
        //     //     EnergyCost.AddThisCombat(-KickerCost);
        //     //     _costModifiedForAmplify = false;
        //     // }
        // }

        public override Task AfterCardEnteredCombat(CardModel card)
        {
            if (card == this)
                ValidateAmplify();
            return Task.CompletedTask;
        }

        public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
        {
            ValidateAmplify();
            return Task.CompletedTask;
        }

        public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
        {
            if (card == this)
                ValidateAmplify();
            return Task.CompletedTask;
        }

        // public override Task AfterEnergyReset(Player player)
        // {
        //     ValidateAmplify();
        //     return Task.CompletedTask;
        // }
        //
        // public override Task AfterPotionUsed(PotionModel potion, Creature? target)
        // {
        //     ValidateAmplify();
        //     return Task.CompletedTask;
        // }

        // public override Task BeforeCardAutoPlayed(CardModel card, Creature? target, AutoPlayType type)
        // {
        //     if (card == this)
        //     {
        //         SetAmplifyState(true, true);
        //     }
        //
        //     return Task.CompletedTask;
        // }
    }
}