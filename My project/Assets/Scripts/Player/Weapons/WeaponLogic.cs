using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponLogic : MonoBehaviour
{
    [Header("Refferences")]
    private ParticleSystem muzzleParticleSystem;
    private AudioSource sfxSource;
    private Light muzzleFlashLight;
    public Transform visualShootSource;
    private WeaponHandler weaponHandler;
    private Transform raycastShootSource;
    private Animator animator;
    private PlayerMovement playerMovement;
    private Camera mainCam;

    [Header("Settings")]
    [SerializeField] private FireMode fireMode;
    private float deployTime = 0.3f;

    [Tooltip("Rate of fire in rounds per minute/ includes semi auto/ excludes bolt action")]
    [SerializeField] private float fireRate = 600;

    [Tooltip("Shotgun spread")]
    [SerializeField] private float ShotSpread = 0f;

    [Tooltip("Useful for shotguns")]
    [SerializeField] private bool interruptableReload;

    [Tooltip("Set reload when ammo still left. Set from reload animation clip ")]
    [SerializeField] private float reloadTimeEmpty;

    [Tooltip("Set reload when no ammo is left. Set from reload animation clip ")]
    [SerializeField] private float reloadTimeNormal;

    [Tooltip("Time between shots on a bol action weapon. Set from animation clip")]
    [SerializeField] private float manualFireModeInterval = 0.5f;

    [Tooltip("Maximum distance for the raycast hit")]
    [SerializeField] private float maxShootDistance = 100f;
    [SerializeField] private LayerMask hittableLayers = ~0;

    private Vector3 shootTargetPos;

    [SerializeField] private float muzzleFlashSize = 0.5f;
    [SerializeField] private float muzzleFlashLightTime = 0.2f;

    [SerializeField] private int magCapacity = 30;
    [SerializeField] private int maxBulletsAllowed = 120;

    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip emptyShootClip;
    [SerializeField] private AudioClip[] reloadSequenceSFXs;

    private float fireInterval => 60f / fireRate;

    [SerializeField]private int currentBulletsInMag;
    [SerializeField]private int currentBulletsInStock;


    private WaitForSeconds waitForNextShoot;
    private WaitForSeconds waitForReloadEmpty;
    private WaitForSeconds waitForReloadNormal;

    private WaitForSeconds waitForDeploy => new WaitForSeconds(deployTime);

    private Coroutine shootOnceRoutine;
    private Coroutine reloadRoutine;
    private Coroutine emptyShootRoutine;
    private Coroutine muzzleFlashLightRoutine;

    private bool canShoot = false;

    private ParticleSystem.MainModule muzzlePsMainModule;
    private float muzzleFlashLightSpeed => 1 / muzzleFlashLightTime;
    private float muzzleFlashLightIntensity;

    [Header("Recoil")]
    private Vector3 startPos;
    private Quaternion startRot;
    [SerializeField] private float kickbackOffsetAllowed;
    [SerializeField] private float kickbackOffset;
    private Vector3 kickbackLimit;
    [SerializeField] private float recoilRecoveryTime;
    [SerializeField] private float kickbackSmoothing;
    private Vector3 recoilTargetPos;

    private Vector3 recoilVelocityRef;
    private bool weaponShot;

    public enum FireMode
    {
        Manual,
        SemiAuto,
        Auto
    }




    void Awake()
    {
        InitWeapon();
    }

    void Update()
    {
        if (weaponHandler != null)
        {
            if (fireMode == FireMode.Auto)
            {
                if (weaponHandler.weaponUseHold) Use();
            }
        }

        RecoverRecoil();
        PassPlayerSpeedToAnimator(); 
    }



    private float currPlayerSpeed;
    private float refVel;

    private void PassPlayerSpeedToAnimator()
    {
        currPlayerSpeed = Mathf.SmoothDamp(currPlayerSpeed, playerMovement.normalizedSpeed, ref refVel, 0.15f);
        animator.SetFloat("PlayerSpeed", currPlayerSpeed);
    }


    private void RecoverRecoil()
    {
        if (weaponShot)
        {
        transform.localPosition = Vector3.SmoothDamp(
            transform.localPosition,
            recoilTargetPos,
            ref recoilVelocityRef,
            kickbackSmoothing
        );

            recoilTargetPos = Vector3.MoveTowards(
               recoilTargetPos,
               startPos,
               recoilRecoveryTime * Time.deltaTime
        );
        }
            //transform.localPosition = Vector3.SmoothDamp(transform.localPosition, startPos, ref recoilVelocityRef, recoilRecoveryTime);
    }

    private void AddRecoil()
    {
        if (!weaponShot)
        {
            startPos = transform.localPosition;
            startRot = transform.localRotation;
            weaponShot = true;
        }


        
        Vector3 localOffset = transform.localPosition+new Vector3(0f, 0f, -kickbackOffset) - startPos;

        localOffset = new Vector3(
            Mathf.Clamp(localOffset.x, -kickbackLimit.x, kickbackLimit.x),
            Mathf.Clamp(localOffset.y, -kickbackLimit.y, kickbackLimit.y),
            Mathf.Clamp(localOffset.z, -kickbackLimit.z, kickbackLimit.z)
        );

        recoilTargetPos=localOffset+ startPos;
       
    }

    public void InitWeapon()
    {
        if (fireMode == FireMode.Manual)
            waitForNextShoot = new WaitForSeconds(manualFireModeInterval);
        else if (fireMode == FireMode.SemiAuto || fireMode == FireMode.Auto)
            waitForNextShoot = new WaitForSeconds(fireInterval);

        RefreshBulletData();

        waitForReloadEmpty = new WaitForSeconds(reloadTimeEmpty);
        waitForReloadNormal = new WaitForSeconds(reloadTimeNormal);


        muzzleParticleSystem = visualShootSource.GetComponent<ParticleSystem>();
        sfxSource = visualShootSource.GetComponent<AudioSource>();
        if (muzzleParticleSystem != null) muzzlePsMainModule = muzzleParticleSystem.main;
        muzzleFlashLight = visualShootSource.GetComponent<Light>();
        muzzleFlashLightIntensity = muzzleFlashLight.intensity;
        muzzleFlashLight.intensity = 0;

        animator = GetComponent<Animator>();
        playerMovement = GetComponentInParent<PlayerMovement>();
        weaponHandler = GetComponentInParent<WeaponHandler>();
        mainCam = GetComponentInParent<Camera>();

        raycastShootSource = weaponHandler.raycastSource;


        //Recoil
        kickbackLimit = new Vector3(0f, 0f, kickbackOffsetAllowed);

    }




    public void RefreshBulletData()
    {
        currentBulletsInMag = magCapacity;
        currentBulletsInStock = maxBulletsAllowed;
    }
    public void ChangehBulletData(int magBullets, int StockBullets)
    {
        currentBulletsInMag = magBullets;
        currentBulletsInStock = StockBullets;
    }

    private IEnumerator Deploy()
    {
        yield return null;
        yield return waitForDeploy;
        canShoot = true;
    }



    private void OnEnable()
    {
        weaponShot = false;

        canShoot = false;
        shootOnceRoutine = null;
        reloadRoutine = null;


        StartCoroutine(Deploy());

        if (weaponHandler != null && playerMovement != null)
        {
            animator.SetFloat("IsEmpty", currentBulletsInMag == 0 ? 1 : 0);

            weaponHandler.OnWeaponUseStarted += Use;
            weaponHandler.OnWeaponReloadPressed += Reload;
            WeaponHandler.OnWeaponInspectPressed += OnInspectPressed;
        }
    }

    private void OnDisable()
    {
        if (weaponShot)
        {
            transform.localPosition = startPos;
            transform.localRotation = startRot;
        }
        

        StopAllCoroutines();
        shootOnceRoutine = null;

        if (weaponHandler != null && playerMovement != null)
        {
            weaponHandler.OnWeaponUseStarted -= Use;
            weaponHandler.OnWeaponReloadPressed -= Reload;
            WeaponHandler.OnWeaponInspectPressed -= OnInspectPressed;
        }
    }




    private void Use()
    {
        if (currentBulletsInMag > 0)
        {
            if (canShoot) Shoot();
        }
        else
        {
            if (emptyShootClip != null && reloadRoutine == null)
            {
                if (emptyShootRoutine == null)
                {
                    emptyShootRoutine = StartCoroutine(EmptyShoot());
                }
            }
        }

    }


    private void Shoot()
    {
        if (reloadRoutine != null && interruptableReload)
        {
            StopCoroutine(reloadRoutine);
            reloadRoutine = null;
            animator.SetTrigger("Locomotion");
            canShoot = true;

            return;
        }
        else if (shootOnceRoutine == null)
        {
            shootOnceRoutine = StartCoroutine(ShootOnce());
        }
    }

    private IEnumerator ShootOnce()
    {
        currentBulletsInMag--;

       if (currentBulletsInMag == 0)
            {
                animator.SetFloat("IsEmpty", 1);
            }

        if (fireMode == FireMode.SemiAuto || fireMode == FireMode.Auto)
        {
            AddRecoil();
            animator.SetTrigger("Shoot");
        }
              
            else if (fireMode == FireMode.Manual && currentBulletsInMag > 0)
        {
            AddRecoil();

            yield return new WaitForSeconds(0.7f);

            animator.SetTrigger("BoltJerk");
            canShoot = false;       
        }
        canShoot = false;

        //TODO: update HUD here
        PlayShootSFX();
        DoMuzzleFlash(muzzleFlashSize);

        Vector3 dir = GetShootDir();

        //Recoil here
        //
        bool isHit = false;
        RaycastHit hit;

        if (isHit = Physics.Raycast(raycastShootSource.position, dir, out hit, maxShootDistance, hittableLayers, QueryTriggerInteraction.Ignore))
        {
            shootTargetPos = hit.point;
        }
        else
        {
            shootTargetPos = raycastShootSource.position + raycastShootSource.forward * maxShootDistance;
        }

        //TODO: Hit logic here

        if (currentBulletsInMag > 0)
        {
            yield return waitForNextShoot;
            canShoot = true;
        }
        else
        {
            yield return null;
        }

        shootOnceRoutine = null;

    }

    private Vector3 GetShootDir()
    {
        Vector3 dir = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)).direction;;

        //TODO: Recoil/spread logic here

        return dir;
    }

    private Vector3 GetShotSpread()
    {
        Vector3 dir = raycastShootSource.forward * 2;

        dir += Random.onUnitSphere * ShotSpread;

        return dir.normalized;
    }

    private void PlayShootSFX()
    {
        if (shootClip != null)
            sfxSource.PlayOneShot(shootClip);
    }
    private void DoMuzzleFlash(float size)
    {
        if (muzzleParticleSystem == null)
            return;

        muzzlePsMainModule.startSizeMultiplier = size;
        muzzleParticleSystem.Emit(1);

        if (muzzleFlashLightRoutine != null) StopCoroutine(muzzleFlashLightRoutine);
        muzzleFlashLightRoutine = StartCoroutine(MuzzleFlashLightAnim());
    }

    private IEnumerator MuzzleFlashLightAnim()
    {
        float t = 1;

        muzzleFlashLight.intensity = muzzleFlashLightIntensity;

        while (t > 0)
        {
            t -= Time.deltaTime * muzzleFlashLightSpeed;
            muzzleFlashLight.intensity *= t;
            yield return null;
        }

        muzzleFlashLightRoutine = null;

    }
    private IEnumerator EmptyShoot()
    {
        if (emptyShootClip != null)
            sfxSource.PlayOneShot(emptyShootClip);

        yield return new WaitForSeconds(fireInterval * 4);

        emptyShootRoutine = null;

    }

    private void Reload()
    {
        if (currentBulletsInStock > 0 && currentBulletsInMag != magCapacity && shootOnceRoutine == null)
            if (reloadRoutine == null)
            {
                muzzleFlashLight.intensity = 0;
                StopAllCoroutines();

                canShoot = false;
                reloadRoutine = StartCoroutine(ReloadWeapon());
            }
    }

    private void OnInspectPressed()
    {
        if (shootOnceRoutine != null) return;

        animator.SetTrigger("Inspect");
    }

    private IEnumerator ReloadWeapon()
    {
        animator.SetTrigger("Reload");

        //TODO: single clip reload sfx

        if (interruptableReload)
        {
            //wait for at least one round insert
            yield return waitForReloadNormal;
        }
        else
        {
            if (currentBulletsInMag == 0)
                yield return waitForReloadEmpty;
            else
                yield return waitForReloadNormal;

            animator.SetFloat("IsEmpty", 0);

            int emptyBulletSlots = magCapacity - currentBulletsInMag;

            if (currentBulletsInStock >= emptyBulletSlots)
            {
                currentBulletsInStock -= emptyBulletSlots;
                currentBulletsInMag = magCapacity;
            }
            else if (currentBulletsInStock > 0)
            {
                currentBulletsInMag += currentBulletsInStock;
                currentBulletsInStock = 0;
            }

            //TODO: Update HUD

            canShoot = true;    
            reloadRoutine = null;
        }
    }

    //Called from the animation event
    public void OneRoundInsert()
    {
        if (currentBulletsInStock > 0)
        {
            currentBulletsInStock--;
            currentBulletsInMag++;
            //TODO: update HUD
        }
        else
        {
            if (reloadRoutine != null) StopCoroutine(reloadRoutine);
            reloadRoutine = null;

            animator.SetTrigger("Locomotion");

            canShoot = true;

            return;
        }
        if (GetEmptyBulletSlots() == 0)
        {
            if (reloadRoutine != null) StopCoroutine(reloadRoutine);
            reloadRoutine = null;

            animator.SetTrigger("Locomotion");
        }
    }

    public void PlayReloadSFX()
    {
        //TODO: single clip reload sfx here
    }

    //caled from the animation events system
    public void PlaySFX(int id)
    {
        if (reloadSequenceSFXs == null || reloadSequenceSFXs.Length == 0 || reloadSequenceSFXs[id] == null)
            return;

        sfxSource.PlayOneShot(reloadSequenceSFXs[id]);
    }
    private int GetEmptyBulletSlots()
    {
        return magCapacity - currentBulletsInMag;
    }

}
