using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public float Hp;
    public float Damage;
    public float AtackSpeed;
    public float AttackRange = 2;
    public float MovementSpeed = 1;
    public float SuperAttackCooldown = 2;
    public float SuperAttackDamage = 2;

    //private float lastAttackTime = 0;
    private bool isDead = false;
    public Animator AnimatorController;

    private Enemie targetEnemie;
    private bool isSuperAttackAvailable;
    private float attackingTimer;
    private float superAttackCooldownTimer;
    private void Awake()
    {
        GlobalEventManager.OnEnemieDie.AddListener(AddHealth);
        GlobalEventManager.OnAttack.AddListener(TryAttack);
        GlobalEventManager.OnSuperAttack.AddListener(SuperAttack);
    }
    private void SuperAttack()
    {
        if (superAttackCooldownTimer > 0 || attackingTimer > 0) return;

        AnimatorController.SetTrigger("SuperAttack");
        superAttackCooldownTimer = SuperAttackCooldown;
        isSuperAttackAvailable = false;
        StartCoroutine(CooldownSuperAttack());
        GlobalEventManager.SuperAttackCooldown(SuperAttackCooldown);
        Attack(SuperAttackDamage);
    }
    private void TryAttack()
    {
        //if (Time.time - lastAttackTime > AtackSpeed)
        if (attackingTimer > 0) return;

        //lastAttackTime = Time.time;
        AnimatorController.SetTrigger("Attack");
        Attack(Damage);

    }
    private void Attack(float damage)
    {
        targetEnemie = null; // обнуляем таргет
        attackingTimer = AtackSpeed; // фиксируем факт атаки и выставляем таймер
        StartCoroutine(Attacking()); // заряжаем таймер на уменьшение

        var enemies = SceneManager.Instance.Enemies;
        Enemie closestEnemie = null;

        for (int i = 0; i < enemies.Count; i++)
        {
            var enemie = enemies[i];
            if (enemie == null)
            {
                continue;
            }

            if (closestEnemie == null)
            {
                closestEnemie = enemie;
                continue;
            }

            var distance = Vector3.Distance(transform.position, enemie.transform.position);
            var closestDistance = Vector3.Distance(transform.position, closestEnemie.transform.position);

            if (distance < closestDistance)
            {
                closestEnemie = enemie;
            }

        }

        if (closestEnemie != null)
        {
            var distance = Vector3.Distance(transform.position, closestEnemie.transform.position);
            if (distance <= AttackRange)
            {
                closestEnemie.Hp -= damage;

                targetEnemie = closestEnemie;
            }
        }
    }
    private void Update()
    {
        if (isDead)
        {
            return;
        }

        if (Hp <= 0)
        {
            Die();
            return;
        }

        Move();//вызвали метод перемещения
        SuperAttackAvailability();
    }

    private void Die()
    {
        GlobalEventManager.PlayerDied();

        isDead = true;
        AnimatorController.SetTrigger("Die");

        SceneManager.Instance.GameOver();
    }
    //метод перемещения персонажа:
    private void Move() 
    {
        if (attackingTimer > 0) return; // убираем возможность перемещения при атаке
        
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        var targetLookPosition = new Vector3(horizontal, 0, vertical); // вектор направления движения

        if (targetLookPosition != Vector3.zero)
        {
            transform.transform.rotation = Quaternion.LookRotation(targetLookPosition);
            transform.position += MovementSpeed * Time.deltaTime * targetLookPosition.normalized;
            AnimatorController.SetFloat("Speed", MovementSpeed);
        }
        else
            AnimatorController.SetFloat("Speed", 0);
    }
    private IEnumerator Attacking()
    {
        while(attackingTimer > 0)
        {
            attackingTimer -= Time.deltaTime;
            if (targetEnemie != null) transform.transform.rotation = Quaternion.LookRotation(targetEnemie.transform.position - transform.position);
            
            yield return new WaitForEndOfFrame();
        }
    }
    private void SuperAttackAvailability()
    {
        if (superAttackCooldownTimer > 0) return; // optimization

        var enemies = SceneManager.Instance.Enemies;
        Enemie closestEnemie = null;

        for (int i = 0; i < enemies.Count; i++)
        {
            var enemie = enemies[i];
            if (enemie == null)
            {
                continue;
            }

            if (closestEnemie == null)
            {
                closestEnemie = enemie;
                continue;
            }

            var distance = Vector3.Distance(transform.position, enemie.transform.position);
            var closestDistance = Vector3.Distance(transform.position, closestEnemie.transform.position);

            if (distance < closestDistance)
            {
                closestEnemie = enemie;
            }
        }
        if (closestEnemie == null) return;

        var distanceToPlayer = Vector3.Distance(transform.position, closestEnemie.transform.position);

        if (distanceToPlayer <= AttackRange)
        {
            if (!isSuperAttackAvailable)
            {
                isSuperAttackAvailable = true;
                GlobalEventManager.SuperAttackAvailable(true);
            }
        }
        else if (isSuperAttackAvailable)
        {
            isSuperAttackAvailable = false;
            GlobalEventManager.SuperAttackAvailable(false);
        }

    }
    IEnumerator CooldownSuperAttack()
    {
        while(true)
        {
            superAttackCooldownTimer -= Time.deltaTime;

            if(superAttackCooldownTimer <= 0) break; 

            yield return new WaitForEndOfFrame();
        }
    }
    private void AddHealth()
    {
        Hp += 1;
    }
}
