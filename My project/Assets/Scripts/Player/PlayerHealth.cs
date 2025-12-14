using UnityEngine;
using System.Linq;
using UnityEngine.Events;

public class PlayerHealth : HealthManager
{
    public float health = 100f;
    private CharacterController characterController;
    private CameraController cameraController;
    bool landed;
    LayerMask allLayers = ~0;
    [SerializeField] private float damageHeightTreshold;

    [Range(0, 10)]
    [SerializeField] private float fallDamageMultiplier;
    bool playOnce = true;
    float highestDiff = 0f;
    [SerializeField] private float cameraTiltTreshold;
    private Vector3 groundPoint;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip[] hitDamageClip;
    [SerializeField] private AudioClip[] fallDamageClip;
    
    [SerializeField] private GameObject ragdollObj;
    [SerializeField] private GameObject cameraObj;
    [SerializeField] private GameObject handsObj;
    GameObject ragdollDuplicate;
    public static UnityAction OnPlayerDead;


    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        cameraController = Camera.main.GetComponent<CameraController>();
        groundPoint = transform.position;
    }
    public override void TakeDamage(Vector3 location, Vector3 direction, float damage, Collider bodyPart = null, GameObject origin = null)
    {
        PlayerUI playerUIScript = GetComponent<PlayerUI>();
        int armor = playerUIScript.armor;

        if (armor > 100)
        {
            float armorMultiplier = armor / 100f * 0.5f;

            float damageAbsorbed = damage * armorMultiplier;

            armor -= Mathf.RoundToInt(damageAbsorbed * 0.5f);

            float finalDamage = damage - damageAbsorbed;
            health -= finalDamage;

        }
        else
        {
            health -= damage;
        }
        
        armor = Mathf.Max(0, armor);

        sfxSource.PlayOneShot(hitDamageClip[Random.Range(0, hitDamageClip.Length)]);

        if (!DamageIndicatorSystem.CheckObjectInSight(origin.transform)) DamageIndicatorSystem.CreateIndicator(origin.transform);

        if (health < 0)
        {
            ScoreboardAgent agent = GetComponent<ScoreboardAgent>();
            agent.CallbackScoreboard(0, 1, 0, 0);
            ScoreboardAgent enemyAgent = origin.GetComponent<ScoreboardAgent>();
            if (enemyAgent == null) enemyAgent = origin.GetComponentInParent<ScoreboardAgent>();
            enemyAgent.CallbackScoreboard(enemyAgent.id, 0, 1, 500);
            dead = true;
            health = 0;
            HandlePlayerDeath();
        }

    }


    private void Update()
    {
        CheckForFallDamage();

        if (health < 0 && !dead)
        {
            dead = true;
            health = 0;
            HandlePlayerDeath();
        }
        else if (health>0)
        {
            dead = false;
        }
    }

    private void CheckForFallDamage()
    {
        if (!characterController.isGrounded)
        {
            landed = false;

            Vector3 origin = transform.position - new Vector3(0f, 0.8f, 0f);

            float dropHeight = Mathf.Abs(groundPoint.y - origin.y);
            highestDiff = (dropHeight > highestDiff) ? dropHeight : highestDiff;
        }
        else if (characterController.isGrounded && !landed)
        {
            landed = true;

            if (highestDiff > damageHeightTreshold)
            {
                health -= Mathf.Round(highestDiff * fallDamageMultiplier);
                sfxSource.PlayOneShot(fallDamageClip[Random.Range(0, fallDamageClip.Length)]);
                cameraController.TiltCamera(10f, 10f);
            }
            else if (highestDiff > cameraTiltTreshold) cameraController.TiltCamera(5f, 5f);

            highestDiff = 0f;
        }

        if (health < 0)
        {
            ScoreboardAgent agent = GetComponent<ScoreboardAgent>();
            agent.CallbackScoreboard(0, 1, 0, 0);
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.normal.y > 0.94f)
        {
            groundPoint = hit.point;
        }
    }


    private void HandlePlayerDeath()
    {
        GameController.gameRunning = false;
        ragdollDuplicate = Instantiate(ragdollObj, ragdollObj.transform.position,ragdollObj.transform.rotation);

        ragdollDuplicate.SetActive(true);
        handsObj.SetActive(false);

        Transform head = ragdollDuplicate.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name.ToLower() == "head");        
        
        cameraObj.transform.SetParent(head);

        cameraObj.transform.localPosition = new Vector3(-0.071f,-0.175f,0.006f);
        cameraObj.transform.localRotation = new Quaternion(0.5057f, -0.494f, 0.505f, 0.494f);

        CameraController.canRun = false;
        GameController.DisplayMessage("You died... Round over", 3);
        Invoke(nameof(DeleteDuplicate), 6f);
        Invoke(nameof(InvokePlayerDeath), 4f);

    }

    private void DeleteDuplicate()
    {
        Destroy(ragdollDuplicate);
    }

    private void InvokePlayerDeath()
    {
        OnPlayerDead?.Invoke();
    }

}
