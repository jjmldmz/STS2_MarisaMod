using BaseLib.Abstracts;
using marisamod.Scripts.Characters;
using marisamod.Scripts.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;

namespace marisamod.Scripts.Events;

public class HungryForMushroomsMarisa : CustomEventModel
{
    public override bool IsAllowed(IRunState runState)
    {
        return false;
    }

    public override LocString InitialDescription => L10NLookup("HUNGRY_FOR_MUSHROOMS..pages.INITIAL.description");

    private async Task BigMushroom()
    {
        await RelicCmd.Obtain<BigMushroom>(Owner!);
        SetEventFinished(L10NLookup("HUNGRY_FOR_MUSHROOMS.pages.BIG_MUSHROOM.description"));
    }

    private async Task FragrantMushroom()
    {
        await RelicCmd.Obtain<FragrantMushroom>(Owner!);
        SetEventFinished(L10NLookup("HUNGRY_FOR_MUSHROOMS.pages.FRAGRANT_MUSHROOM.description"));
    }

    private async Task PackThemAll()
    {
        await RelicCmd.Obtain<ShroomBag>(Owner!);
        SetEventFinished(L10NLookup("MARISAMOD-HUNGRY_FOR_MUSHROOMS_MARISA.pages.PACK_THEM_ALL.description"));
    }
    
    private async Task BigShroomBag()
    {
        var relic = Owner!.Relics.FirstOrDefault(x => x is ShroomBag);
        await RelicCmd.Remove(relic!);
        await RelicCmd.Obtain<BigShroomBag>(Owner);

        SetEventFinished(L10NLookup("MARISAMOD-HUNGRY_FOR_MUSHROOMS_MARISA.pages.PACK_THEM_ALL.description"));
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        EventOption option;
        if (Owner!.Character is not MarisaCharacter)
        {
            option = new EventOption(this, null, "MARISAMOD-HUNGRY_FOR_MUSHROOMS_MARISA.options.LOCKED");
        }
        else if (Owner.Relics.Any(x => x is ShroomBag))
        {
            option = RelicOption<BigShroomBag>(BigShroomBag);
        }
        else
        {
            option = RelicOption<ShroomBag>(PackThemAll);
        }

        return
        [
            RelicOption<BigMushroom>(BigMushroom),
            RelicOption<FragrantMushroom>(FragrantMushroom).ThatDoesDamage(15m),
            option
        ];
    }


}