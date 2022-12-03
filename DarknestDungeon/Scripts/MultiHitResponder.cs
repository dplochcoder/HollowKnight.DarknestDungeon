using UnityEngine;

namespace DarknestDungeon.Scripts
{
    // TC doesn't use GetComponents<>, so if we have multiple IHitResponders, we need to have one
    // of these components before all the others to proxy the correct behaviour.
    public class MultiHitResponder : MonoBehaviour, IHitResponder
    {
        public void Hit(HitInstance damageInstance)
        {
            foreach (var responder in gameObject.GetComponents<IHitResponder>())
            {
                DarknestDungeon.Log($"Found IHitResponder: {responder.GetType().Name}");
                if (responder != this)
                {
                    DarknestDungeon.Log("Activating IHitResponder");
                    responder.Hit(damageInstance);
                }
            }
        }
    }
}
