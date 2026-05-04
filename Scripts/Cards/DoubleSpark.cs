using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using marisamod.Scenes.Vfx.SparkProjectile;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using marisamod.Scripts.Cards.Colorless;
using marisamod.Scripts.PatchesNModels;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace marisamod.Scripts.Cards
{
    public class DoubleSpark : AbstractMarisaCard,ISparkCard
    {
        public DoubleSpark() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
        {
        }

        //public override string PortraitPath => $"res://img/cards/DoubleSpark_p.png";

        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new DamageVar(7m, ValueProp.Move)
        ];
        
        protected override HashSet<CardTag> CanonicalTags => [MarisaCardTags.Spark];

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Spark>(IsUpgraded)];
        
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target);
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
                //.WithHitFx("vfx/vfx_attack_slash")
                .BeforeDamage(async delegate
                {
                    NCreature? player = NCombatRoom.Instance?.GetCreatureNode(base.Owner.Creature);
                    NCreature? enemy = NCombatRoom.Instance?.GetCreatureNode(cardPlay.Target);
                    if (player != null && enemy != null)
                    {
                        if (_vfx != null && _vfx.GetParent() == NCombatRoom.Instance?.CombatVfxContainer)
                        {
                            _vfx.StartChasing(enemy.VfxSpawnPosition);
                        }
                        else
                        {
                            _vfx = VfxSparkProjectile.Create(this,enemy);
                            _vfx.NoIdle = true;
                            _vfx.StartDamping();
                            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(_vfx);
                            await Cmd.Wait(VfxSparkProjectile.DampingDuration/4f);
                            _vfx.StartChasing(enemy.VfxSpawnPosition);
                        }
                        //TODO:按伤害重设尺寸或添加特效
                        await Cmd.Wait(VfxSparkProjectile.ChaseDuration);
                    }
                })
                .Execute(choiceContext);
            if (CombatState != null)
            {
                var cards = await Spark.CreateInHand(Owner, 1, CombatState);
                if (IsUpgraded)
                {
                    var cardModels = cards as CardModel[] ?? cards.ToArray();
                    if (cardModels.Length != 0)
                    {
                        foreach (var card in cardModels)
                        {
                            CardCmd.Upgrade(card);
                        }
                    }
                }
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(3m);
        }
        
        
        #region ISparkCard
        private VfxSparkProjectile _vfx;
        public Vector4 SparkColor => new(0.15f,1.0f,0.85f,1.0f);
        public void OnDrag()
        {
            if (_vfx != null && _vfx.GetParent() == NCombatRoom.Instance?.CombatVfxContainer)
            {
                _vfx.StartDamping();
            }
            else
            {
                _vfx = VfxSparkProjectile.Create(this);
                NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(_vfx);
            }
        }

        public void OnCancelPlay()
        {
            _vfx.StartWandering();
        }
        #endregion
    }
}