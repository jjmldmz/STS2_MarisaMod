using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace marisamod.Scripts.PatchesNModels;

public static class MarisaCardKeyWords
{
    [CustomEnum("Amplify")]
    [KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Amplify;
}

public static class MarisaCardTags
{
    [CustomEnum]
    public static CardTag Spark;
}