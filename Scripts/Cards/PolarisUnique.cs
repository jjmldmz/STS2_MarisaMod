using marisamod.Scripts.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace marisamod.Scripts.Cards
{
    public class PolarisUnique : AbstractMarisaCard
    {
        public PolarisUnique() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
        {
        }

        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new EnergyVar(2)
        ];

        protected override void OnUpgrade()
        {
            DynamicVars.Energy.UpgradeValueBy(1);
        }
        
        protected override bool IsPlayable =>
            !CombatManager.Instance.History.CardPlaysFinished.Any(e => e.HappenedThisTurn(CombatState) && e.CardPlay.Card is PolarisUnique && e.CardPlay.Card.Owner == Owner);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var card = (await CardSelectCmd.FromHand(choiceContext,
                Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, 0, 1),
                card => card.EnergyCost.Canonical != -1,
                this)).FirstOrDefault();
            card?.EnergyCost.AddThisCombat(1);
            await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
            //EnergyCost.AddThisCombat(1);
            // await PowerCmd.Apply<PolarisUniquePower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
        }
    }
}