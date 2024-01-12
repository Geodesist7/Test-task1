using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager: MonoBehaviour
{
    public Button attackButton, superAttackButton;
    public Text superAttackButtonText;
    private float superAttackCooldownTimer;
    private void Awake()
    {
        GlobalEventManager.OnPlayerDie.AddListener(PlayerDied);
        GlobalEventManager.OnSuperAttackCooldown.AddListener(SuperAttackCooldown);
        GlobalEventManager.OnSuperAttackAvailable.AddListener(SuperAttackAvailability);
    }
    private void Start()
    {
        superAttackButton.interactable = false;
    }
    public void SuperAttack()
    {
        superAttackButton.interactable = false;
        GlobalEventManager.SuperAttack();
    }
    public void TryAttack()
    {
        GlobalEventManager.Attack();
    }

    private void PlayerDied()
    {
        attackButton.gameObject.SetActive(false);
        superAttackButton.gameObject.SetActive(false);
    }
    private void SuperAttackCooldown(float cooldown)
    {
        superAttackButton.interactable = false;
        superAttackCooldownTimer = cooldown;
        StartCoroutine(CooldownSuperAttack());
    }
    private void SuperAttackAvailability(bool isAvailable)
    {
        superAttackButton.interactable = isAvailable;
    }
    IEnumerator CooldownSuperAttack()
    {
        while (true)
        {
            superAttackCooldownTimer -= Time.deltaTime;

            superAttackButtonText.text = $"Cooldown {Math.Round(superAttackCooldownTimer, 2)}s";

            if (superAttackCooldownTimer <= 0)
            {
                superAttackButtonText.text = "Super attack";
                break;
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
