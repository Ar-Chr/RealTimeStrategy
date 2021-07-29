using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HealthDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image healthImage;
    [SerializeField] private GameObject healthParentObject;
    [SerializeField] private Health health;
    [SerializeField] private bool showOnTakeDamage;
    [SerializeField] private float onTakeDamageShowTime;

    private float onTakeDamageTimeShown;

    private bool showingBecauseOfPointer;
    private bool showingBecauseOfDamage;

    private void Awake()
    {
        health.ClientOnHealthChanged += HandleHealthChanged;
    }

    private void Update()
    {
        if (showingBecauseOfDamage)
        {
            if (onTakeDamageTimeShown < onTakeDamageShowTime)
            {
                onTakeDamageTimeShown += Time.deltaTime;
            }
            else
            {
                showingBecauseOfDamage = false;
                if (!showingBecauseOfPointer)
                    healthParentObject.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        health.ClientOnHealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        healthImage.fillAmount = (float)currentHealth / maxHealth;

        if (showOnTakeDamage)
        {
            showingBecauseOfDamage = true;
            healthParentObject.SetActive(true);
            onTakeDamageTimeShown = 0;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        showingBecauseOfPointer = true;
        healthParentObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        showingBecauseOfPointer = false;

        if (!showingBecauseOfDamage)
            healthParentObject.SetActive(false);
    }
}
