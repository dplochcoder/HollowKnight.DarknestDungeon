using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public delegate void DeathEvent();

    [SerializeField]
    public int hp;

    [SerializeField]
    public bool hasSpecialDeath;

    [SerializeField]
    public bool deathReset;

    [SerializeField]
    public bool damageOverride;

    public bool isDead;

    public int smallGeoDrops;
    public int mediumGeoDrops;
    public int largeGeoDrops;

    public event DeathEvent OnDeath;
}
