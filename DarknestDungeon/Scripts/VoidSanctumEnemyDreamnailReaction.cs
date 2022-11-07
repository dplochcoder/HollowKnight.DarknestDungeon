using DarknestDungeon.IC;
using ItemChanger;
using SFCore.Utils;

namespace DarknestDungeon.Scripts
{
    public class VoidSanctumEnemyDreamnailReaction : EnemyDreamnailReaction
    {
        private enum ConvoType
        {
            DEFAULT,
            VOID_FLAME,
            VOID_HEART,
            VOID_HEART_AND_FLAME,
        }

        private VoidFlameModule Vfm;

        private ConvoType ComputeType()
        {
            bool hasVoidHeart = PlayerData.instance.GetBool(nameof(PlayerData.gotShadeCharm));
            bool hasFlame = Vfm.HeroHasVoidFlame;

            return hasVoidHeart ? (hasFlame ? ConvoType.VOID_HEART_AND_FLAME : ConvoType.VOID_HEART)
                : (hasFlame ? ConvoType.VOID_FLAME : ConvoType.DEFAULT);
        }

        public string convo;
        public string flameConvo;
        public string heartConvo;
        public string flameHeartConvo;

        private void Awake()
        {
            Vfm = ItemChangerMod.Modules.Get<VoidFlameModule>();
            ResetConvoFields();

            VoidFlameModule.OnVoidFlameStateChange += OnVoidFlameStateChange;
            // TODO: Track Void heart
        }

        private void OnDestroy()
        {
            VoidFlameModule.OnVoidFlameStateChange -= OnVoidFlameStateChange;
            // TODO: Track Void heart
        }

        private void OnVoidFlameStateChange(bool hasFlame) => ResetConvoFields();

        private const string GENERIC = "GENERIC";

        private string GetConvoForType(ConvoType type) => type switch
        {
            ConvoType.DEFAULT => convo ?? GENERIC,
            ConvoType.VOID_FLAME => flameConvo ?? convo ?? GENERIC,
            ConvoType.VOID_HEART => heartConvo ?? convo ?? GENERIC,
            ConvoType.VOID_HEART_AND_FLAME => flameHeartConvo ?? heartConvo ?? flameConvo ?? convo ?? GENERIC,
            _ => GENERIC,
        };

        private void ResetConvoFields()
        {
            var prompts = ItemChangerMod.Modules.Get<PromptsModule>();

            string key = GetConvoForType(ComputeType());
            if (prompts.Data.DreamnailPrompts.TryGetValue(key, out var values))
            {
                this.SetAttr<EnemyDreamnailReaction, string>("convoTitle", key);
                this.SetAttr<EnemyDreamnailReaction, int>("convoAmount", values.Count);
            }
            else base.Reset();
        }
    }
}
