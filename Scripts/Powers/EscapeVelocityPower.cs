using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace marisamod.Scripts.Powers;

public class EscapeVelocityPower : AbstractMarisaPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeTurnEndEarly(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side)
            return;
        var draw = Mathf.Min(Owner.Player!.PlayerCombatState!.Hand.Cards.Count, Amount);

        if (draw > 0)
        {
            await PowerCmd.Apply<DrawCardsNextTurnPower>(choiceContext, Owner, draw, Owner, null);
        }
    }
}