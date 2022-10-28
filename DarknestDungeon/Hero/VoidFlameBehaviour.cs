using DarknestDungeon.IC;
using UnityEngine;

namespace DarknestDungeon.Hero
{
    public class VoidFlameBehaviour : MonoBehaviour
    {
        // Must be set before Start().
        public VoidFlameModule Vfm;

        private HeroController hc;

        private void Start()
        {
            hc = gameObject.GetComponent<HeroController>();

            hc.OnTakenDamage += SurrenderVoidFlameDamage;
            On.HeroController.EnterSceneDreamGate += SurrenderVoidFlameDreamgate;

            VoidFlameModule.OnVoidFlameStateChange += ToggleVoidFlame;

            ToggleVoidFlame(Vfm.HeroHasVoidFlame);
        }

        private void OnDestroy()
        {
            hc.OnTakenDamage -= SurrenderVoidFlameDamage;
            On.HeroController.EnterSceneDreamGate -= SurrenderVoidFlameDreamgate;

            VoidFlameModule.OnVoidFlameStateChange -= ToggleVoidFlame;
        }

        private void SurrenderVoidFlame(bool damage)
        {
            if (!Vfm.LoseTemporaryFlame()) return;

            // TODO: UI treatment if not due to damage
        }

        private void SurrenderVoidFlameDamage() => SurrenderVoidFlame(true);

        private void SurrenderVoidFlameDreamgate(On.HeroController.orig_EnterSceneDreamGate orig, HeroController self) {
            orig(self);
            SurrenderVoidFlame(false);
        }

        private void ToggleVoidFlame(bool haveFlame)
        {
            // TODO
        }
    }
}
