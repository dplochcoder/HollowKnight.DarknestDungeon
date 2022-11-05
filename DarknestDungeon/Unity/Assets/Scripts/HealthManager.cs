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

    public event DeathEvent OnDeath;
}
