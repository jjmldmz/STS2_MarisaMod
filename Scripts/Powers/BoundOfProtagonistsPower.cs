using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace marisamod.Scripts.Powers
{
    public class BoundOfProtagonistsPower : AbstractMarisaPower
    {
        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;

        // public override decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        // {
        //     if (target != Owner)
        //     {
        //         return amount;
        //     }
        //     return amount * 0.5m;
        // }
        //
        // public override async Task AfterModifyingHpLostAfterOsty()
        // {
        //     await PowerCmd.Decrement(this);
        // }

        private bool _shouldTrigger;

        private bool ShouldTrigger
        {
            get => _shouldTrigger;
            set
            {
                AssertMutable();
                _shouldTrigger = value;
            }
        }

        public override Task BeforeTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side)
        {
            if (side != Owner.Side)
            {
                return Task.CompletedTask;
            }

            if (base.Owner.Block > 0)
            {
                return Task.CompletedTask;
            }

            ShouldTrigger = true;
            return Task.CompletedTask;
        }

        public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
        {
            if (ShouldTrigger)
            {
                ShouldTrigger = false;
                Flash();
                //await CreatureCmd.GainBlock(base.Owner, base.DynamicVars.Block, null);
                await PowerCmd.Apply<FlightPower>(choiceContext, Owner, 1, Owner, null);
            }
        }
    }
}