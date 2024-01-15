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
        GlobalEventManager.OnSuperAttack.AddListener(SuperAttack); // подписываемся на ивент супер атаки
    }
    private void SuperAttack() // метод супер атаки
    {
        if (superAttackCooldownTimer > 0 || attackingTimer > 0) return;

        AnimatorController.SetTrigger("SuperAttack"); // вызываем анимацию супер атаки
        superAttackCooldownTimer = SuperAttackCooldown; // выставляем кд
        isSuperAttackAvailable = false; // убираем возможность супер атаки
        StartCoroutine(CooldownSuperAttack()); // включаем кд супер атаки
        GlobalEventManager.SuperAttackCooldown(SuperAttackCooldown); // ивент на начало супер атаки
        Attack(SuperAttackDamage); // атакуем с удвоенным уроном
    }
    private void TryAttack()
    {
        if (attackingTimer > 0) return;

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
    
    private void Move() // метод перемещения персонажа
    {
        if (attackingTimer > 0) return; // убираем возможность перемещения при атаке
        
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        var targetLookPosition = new Vector3(horizontal, 0, vertical); // вектор направления движения

        if (targetLookPosition != Vector3.zero) // если мы куда-то хотим двигаться
        {
            transform.rotation = Quaternion.LookRotation(targetLookPosition); // поворачиваем персонажа на targetLookPosition
            transform.position += MovementSpeed * Time.deltaTime * targetLookPosition.normalized; // перемещаем персонажа в сторону нормализованного targetLookPosition
            AnimatorController.SetFloat("Speed", MovementSpeed); // анимируем (требует оптимизации? bool)
        }
        else
            AnimatorController.SetFloat("Speed", 0); // выключаем анимацию (требует оптимизации? bool)
    }
    private IEnumerator Attacking() // корутина таймера атаки
    {
        while(attackingTimer > 0) // если кулдаун атаки не прошёл
        {
            attackingTimer -= Time.deltaTime; // убираем время кулдауна
            if (targetEnemie != null) transform.transform.rotation = Quaternion.LookRotation(targetEnemie.transform.position - transform.position); // если существует таргет энеми то смотрим на него
            
            yield return new WaitForEndOfFrame();
        }
    }
    private void SuperAttackAvailability() // возможность атаковать
    {
        if (superAttackCooldownTimer > 0) return; // лёгкая оптимизация

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

            var distance = Vector3.Distance(transform.position, enemie.transform.position); // расстояние до врага
            var closestDistance = Vector3.Distance(transform.position, closestEnemie.transform.position); // расстояние до ближайшего (зафиксированного) врага

            if (distance < closestDistance) // если дистанция меньше чем последняя зафиксированная, то выставляем ближайшего врага
            {
                closestEnemie = enemie;
            }
        }
        if (closestEnemie == null) return;

        var distanceToPlayer = Vector3.Distance(transform.position, closestEnemie.transform.position); // дистанция от игрока до врага

        if (distanceToPlayer <= AttackRange) // если дистанция <= радиус атаки
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
    IEnumerator CooldownSuperAttack() // корутина на кд супер атаки
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
