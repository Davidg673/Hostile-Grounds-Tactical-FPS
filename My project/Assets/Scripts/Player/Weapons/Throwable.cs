using System.Collections;
using UnityEngine;

public class Throwable : ThrowableBase
{
    [Header("References")]
    private Animator animator;
    private AudioSource sfxSource;
    public Transform throwSourcePrimary;
    public Transform throwSourceSecondary;
    public GameObject throwablePrefab;
    private GameObject throwableInstance;

    [Header("Rest")]
    private Transform throwSource;
    private WeaponHandler weaponHandler;
    private PlayerMovement playerMovement;

    private bool canThrow;
    private bool pinPulled;
    private float ThrowForce;

    private Coroutine throwRoutine;
    private WaitForSeconds throwRecoverDelay;
    private WaitForSeconds waitForDeploy = new WaitForSeconds(0.3f);

    [SerializeField] private float recoverTime = 1.5f;
    [SerializeField] private float throwForcePrimary = 25f;
    [SerializeField] private float throwForceSecondary = 25f;

    [SerializeField] private AudioClip pinPullClip;
    [SerializeField] private AudioClip throwClip;


    void Start()
    {

    }
    void Update()
    {
        if (weaponHandler != null)
        {
            if (weaponHandler.weaponSecondaryUseHold || weaponHandler.weaponUseHold) PreUse();
        }

        animator.SetFloat("PlayerSpeed", playerMovement.normalizedSpeed);
    }


    void Awake()
    {
        InitThrowable();
    }

    public void InitThrowable()
    {
        animator = GetComponent<Animator>();
        sfxSource = GetComponent<AudioSource>();

        throwRecoverDelay = new WaitForSeconds(recoverTime);

        weaponHandler = GetComponentInParent<WeaponHandler>();
        playerMovement = GetComponentInParent<PlayerMovement>();

    }

    private IEnumerator Deploy()
    {
        yield return null;
        yield return waitForDeploy;
        canThrow = true;
    }

    private void OnEnable()
    {
        canThrow = false;
        throwRoutine = null;

        if (weaponHandler == null) return;
        StartCoroutine(Deploy());

        weaponHandler.OnWeaponUseFinished += Use;
        weaponHandler.OnWeaponUseSecondaryFinished += UseSecondary;
    }

    private void OnDisable()
    {
        pinPulled = false;
        if (weaponHandler == null) return;


        weaponHandler.OnWeaponUseFinished -= Use;
        weaponHandler.OnWeaponUseSecondaryFinished -= UseSecondary;
    }

    private void PreUse()
    {
        if (canThrow && !pinPulled)
        {
            if (animator != null) animator.SetTrigger("PullPin");
            pinPulled = true;
        }
    }

    private void Use()
    {
        if (!pinPulled) return;

        PreThrow(true);
    }

    private void UseSecondary()
    {
        if (!pinPulled) return;

        PreThrow(false);
    }

    private void PreThrow(bool high)
    {
        if (high)
        {
            throwSource = throwSourcePrimary;
            ThrowForce = throwForcePrimary;
            if (animator != null) animator.SetTrigger("ThrowHigh");
        }
        else
        {
            throwSource = throwSourceSecondary;
            ThrowForce = throwForceSecondary;
            if (animator != null) animator.SetTrigger("ThrowLow");
        }
    }

    public void PlayPullPinSFX()
    {
        if (sfxSource == null || pinPullClip == null) return;

        sfxSource.PlayOneShot(pinPullClip);
    }

    public void PlayThrowSFX()
    {
        if (sfxSource == null || throwClip == null) return;

        sfxSource.PlayOneShot(throwClip);
    }

    /// <summary>
    /// Called from the animation event system
    /// </summary>
    public void Throw()
    {
        if (throwRoutine == null && canThrow) throwRoutine = StartCoroutine(Throw(ThrowForce));
    }
    
    private IEnumerator Throw(float force)
    {
        PlayThrowSFX();

        canThrow = false;
        pinPulled = false;

        throwableInstance = Instantiate(throwablePrefab, throwSource.position, throwSource.rotation);
        throwableInstance.SetActive(true);

        throwableInstance.GetComponent<Rigidbody>().AddForce(throwSource.forward * force, ForceMode.Impulse);
        throwableInstance.GetComponent<Rigidbody>().AddTorque(Random.onUnitSphere * force * 0.5f, ForceMode.Impulse);

        yield return null;

        throwableInstance.GetComponent<ThrowableBase>().DestroyInstance(explosionDelay);

        throwRoutine = null;
        canThrow = false;
        gameObject.SetActive(false);
        weaponHandler.RemoveFromThrowables(gameObject);
    }

}
