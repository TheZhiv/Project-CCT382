using UnityEngine;
using UnityEngine.UI;

public class ToggleObject : MonoBehaviour
{
    [Header("Shield")]
    public GameObject targetObject;
    public GameObject inactiveShieldObject;

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip shieldActivateSound;
    public AudioClip shieldDeactivateSound;

    [Header("Energy")]
    public float maxEnergy = 100f;
    public float energyDrainRate = 20f;
    public float energyRechargeRate = 10f;
    public Slider energyBar;

    private float currentEnergy;
    private bool shieldActive = false;

    void Start()
    {
        currentEnergy = maxEnergy;

        // Shield starts OFF
        targetObject.SetActive(false);

        // Opposite object starts ON
        inactiveShieldObject.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleShield();
        }

        if (shieldActive)
        {
            currentEnergy -= energyDrainRate * Time.deltaTime;

            if (currentEnergy <= 0f)
            {
                currentEnergy = 0f;
                DisableShield();
            }
        }
        else
        {
            currentEnergy += energyRechargeRate * Time.deltaTime;
            currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        }

        energyBar.value = currentEnergy / maxEnergy;
    }

    void ToggleShield()
    {
        if (shieldActive)
        {
            DisableShield();
        }
        else if (currentEnergy > 0f)
        {
            EnableShield();
        }
    }

    void EnableShield()
    {
        shieldActive = true;

        targetObject.SetActive(true);
        inactiveShieldObject.SetActive(false);

        audioSource.PlayOneShot(shieldActivateSound);
    }

    void DisableShield()
    {
        shieldActive = false;

        targetObject.SetActive(false);
        inactiveShieldObject.SetActive(true);

        audioSource.PlayOneShot(shieldDeactivateSound);
    }
}