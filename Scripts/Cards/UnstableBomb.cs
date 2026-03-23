// using MegaCrit.Sts2.Core.Commands;
// using MegaCrit.Sts2.Core.Entities.Cards;
// using MegaCrit.Sts2.Core.Extensions;
// using MegaCrit.Sts2.Core.GameActions.Multiplayer;
// using MegaCrit.Sts2.Core.Localization.DynamicVars;
// using MegaCrit.Sts2.Core.Models;
// using MegaCrit.Sts2.Core.ValueProps;
//
// namespace marisamod.Scripts.Cards;
//
// public class UnstableBomb : AbstractMarisaCard
// {
//     public UnstableBomb() : base(1, CardType.Attack, CardRarity.Common, TargetType.RandomEnemy)
//     {
//     }
//
//     private static readonly int[] RandomPool = [0, 1, 2, 3];
//
//     protected override IEnumerable<DynamicVar> CanonicalVars =>
//     [
//         new CalculationBaseVar(2),
//         new ExtraDamageVar(1),
//         new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) 
//             => RandomPool.TakeRandom(1, card.Owner.RunState.Rng.CombatCardSelection).FirstOrDefault()),
//         new DynamicVar("Upper",5),
//         new RepeatVar(3)
//     ];
//
//     protected override void OnUpgrade()
//     {
//         DynamicVars.Repeat.UpgradeValueBy(1);
//     }
//
//     public override Task AfterModifyingDamageAmount(CardModel? cardSource)
//     {
//         if (cardSource == this)
//         {
//             DynamicVars["Upper"].BaseValue = DynamicVars.CalculationBase.BaseValue + 3;
//         }
//
//         return Task.CompletedTask;
//     }
//
//     protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
//     {
//         for (var i = 0; i < DynamicVars.Repeat.IntValue; i++)
//         {
//             await DamageCmd.Attack(DynamicVars.CalculatedDamage.Calculate(cardPlay.Target)).FromCard(this)
//                 .TargetingRandomOpponents(CombatState)
//                 .WithHitFx("vfx/vfx_attack_slash")
//                 .Execute(choiceContext);
//         }
//     }
// }