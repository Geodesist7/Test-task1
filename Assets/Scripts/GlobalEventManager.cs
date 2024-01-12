using UnityEngine;
using UnityEngine.Events;

public class GlobalEventManager
{
    public static UnityEvent OnEnemieDie = new UnityEvent();
    public static UnityEvent OnPlayerDie = new UnityEvent();
    public static UnityEvent OnAttack = new UnityEvent();
    public static UnityEvent OnSuperAttack = new UnityEvent();
    public static UnityEvent<float> OnSuperAttackCooldown = new UnityEvent<float>();
    public static UnityEvent<bool> OnSuperAttackAvailable = new UnityEvent<bool>();

    public static void EnemieDied()
    {
        OnEnemieDie.Invoke();
    }
    public static void PlayerDied()
    {
        OnPlayerDie.Invoke();
    }
    public static void SuperAttack()
    {
        OnSuperAttack.Invoke();
    }
    public static void Attack()
    {
        OnAttack.Invoke();
    }
    public static void SuperAttackCooldown(float cooldown)
    {
        OnSuperAttackCooldown.Invoke(cooldown);
    }
    public static void SuperAttackAvailable(bool isAvailable)
    {
        OnSuperAttackAvailable.Invoke(isAvailable);
    }
}