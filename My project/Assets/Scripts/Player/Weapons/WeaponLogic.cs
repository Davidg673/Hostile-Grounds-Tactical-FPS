using System.Collections;
using Unity.Cinemachine;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Events;

public class WeaponLogic : MonoBehaviour
{
    [Header("Refferences")]
    public Transform visualShootSource;
    private WeaponHandler weaponHandler;
    private Transform raycastShootSource;
    private Animator animator;
    private PlayerMovement playerMovement;
    private Camera mainCam;
    private CameraRecoil cameraScript;
    private DynamicCrosshair crosshairScript;
    private GameObject crosshairObject;

    [Header("Settings")]
    [SerializeField] private FireMode fireMode;
    [SerializeField] private float deployTime;

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

    [SerializeField] private float verticalRecoilAllowed;
    [SerializeField] private float verticalRecoil;

    [SerializeField] private float horizontalRecoilAllowed;
    [SerializeField] private float horizontalRecoil;


    [Tooltip("How smooth the weapon comes back from recoil")]
    [SerializeField] private float kickbackRecoveryTime;

    [Tooltip("How smooth the recoil happens")]
    [SerializeField] private float kickbackSmoothing;

    [Tooltip("How fast vertical Recoil recovers")]
    [SerializeField] private float verticalRecoilRecoveryTime;

    [Tooltip("How fast vertical Recoil happens")]
    [SerializeField] private float verticalRecoilSmoothing;

    private Vector3 recoilTargetPos;
    private Quaternion recoilTargetRot;
    private Vector3 recoilVelocityRef;
    private bool weaponShot;
    [SerializeField] float crosshairSpread;

    [Tooltip("How far the bullet moves up")]
    [SerializeField] float verticalBulletRecoil;

    [Tooltip("How far the bullet moves sideways")]
    [SerializeField] float horizontalBulletSpread;
    private float currentVerticalBulletRecoil;
    private float currentHorizontalBulletSpread;

    [Tooltip("Bullet recoil boundary")]
    [SerializeField] float verticalBulletMax;

    [Tooltip("Bullet sway boundary")]
    [SerializeField] float horizontalBulletMax;


    [Tooltip("Bullet recoil/sway return speed")]
    [SerializeField] float directionReturnSpeed;

    [Tooltip("how much to reduce/increase the effect of recoil by")]
    [Range(0, 5)]
    [SerializeField] private float recoilControl;

    [Tooltip("how much to reduce/increase the effect of sway by")]
    [Range(0, 5)]

    [SerializeField] private float spreadControl;
    [Range(0, 5)]
    [SerializeField] private float cameraRecoilMult;

    [Range(0, 5)]
    [SerializeField] private float cameraSpreadMult;
    private float spreadControlStatic;
    private float recoilControlStatic;


    private float currentRecoilControl; //used to change value runtime
    private float currentSpreadControl;
    

    Vector3 direction;
    Vector3 originalDirection;
    

    [Header("SFX/VFX")]
    private ParticleSystem muzzleParticleSystem;
    private AudioSource sfxSource;
    private Light muzzleFlashLight;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip emptyShootClip;
    [SerializeField] private AudioClip[] reloadSequenceSFXs;
    [SerializeField] private GameObject[] bulletHoleArr;




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


        currentRecoilControl = recoilControl;
        currentSpreadControl = spreadControl; //neccessary for bullet recoil

        RecoverRecoil();
        PassPlayerSpeedToAnimator(); 
    }



    private float currPlayerSpeed;
    private float refVel;

    private void PassPlayerSpeedToAnimator()
    {
        currPlayerSpeed = Mathf.SmoothDamp(currPlayerSpeed, playerMovement.normalizedSpeed, ref refVel, 0.15f);
        if (playerMovement.normalizedSpeed > 0.1f)
        {
            recoilControl = recoilControlStatic * 1.5f;
            spreadControl = spreadControlStatic * 1.5f;
        }
        else
        {
            recoilControl = recoilControlStatic;
            spreadControl = spreadControlStatic;
        }

        animator.SetFloat("PlayerSpeed", currPlayerSpeed);
    }

    private void RecoverRecoil()
    {
        if (weaponShot)
        {
            //add kickback recoil
            transform.localPosition = Vector3.SmoothDamp(
                transform.localPosition,
                recoilTargetPos,
                ref recoilVelocityRef,
                kickbackSmoothing
            );
            
            //add horizontal and vertical recoil
            transform.localRotation = Quaternion.Slerp(transform.localRotation, recoilTargetRot, verticalRecoilSmoothing);

            //return kickback recoil
            recoilTargetPos = Vector3.MoveTowards(
                recoilTargetPos,
                startPos,
                kickbackRecoveryTime * Time.deltaTime
            );
            //return horizontal and vertical recoil
            recoilTargetRot = Quaternion.RotateTowards(recoilTargetRot, startRot, verticalRecoilRecoveryTime * Time.deltaTime);

            //return bullet recoil
            originalDirection = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)).direction;

            currentHorizontalBulletSpread = Mathf.MoveTowards(currentHorizontalBulletSpread, 0f, directionReturnSpeed * Time.deltaTime);
            currentVerticalBulletRecoil = Mathf.MoveTowards(currentVerticalBulletRecoil, 0f, directionReturnSpeed * Time.deltaTime);
            
            direction = originalDirection + new Vector3(currentHorizontalBulletSpread,currentVerticalBulletRecoil,0f);

        }
    }

    private void AddRecoil()
    {
        if (!weaponShot)
        {
            startPos = transform.localPosition;
            startRot = transform.localRotation;
            weaponShot = true;
        }


        //Offset with rotation
        Vector3 localOffset = transform.localPosition + new Vector3(0f, 0f, -kickbackOffset) - startPos;

        //clamp offset
        localOffset = new Vector3(
            Mathf.Clamp(localOffset.x, -kickbackOffsetAllowed, kickbackOffsetAllowed),
            Mathf.Clamp(localOffset.y, -verticalRecoilAllowed, verticalRecoilAllowed),
            Mathf.Clamp(localOffset.z, -kickbackOffsetAllowed, kickbackOffsetAllowed)
        );
       
        recoilTargetPos = localOffset + startPos;

        //Get offset with recoil
        Quaternion recoilRotation = Quaternion.Euler(-verticalRecoil, horizontalRecoil, 0f);
        Quaternion recoilOffset = Quaternion.Inverse(startRot) * transform.localRotation *recoilRotation;

        Vector3 euler=recoilOffset.eulerAngles;

        //Convert to 180 degrees for easy clamping
        euler.x = (euler.x > 180f) ? euler.x - 360f : euler.x;
        euler.y = (euler.y > 180f) ? euler.y - 360f : euler.y;
        euler.z = (euler.z > 180f) ? euler.z - 360f : euler.z;

        //actual clamping
        euler.x = Mathf.Clamp(euler.x, -verticalRecoilAllowed, verticalRecoilAllowed);
        euler.y = Mathf.Clamp(euler.y, -horizontalRecoilAllowed, horizontalRecoilAllowed);
        euler.z = Mathf.Clamp(euler.z, 0f, 0f);

        recoilOffset = Quaternion.Euler(euler);
        recoilTargetRot = startRot * recoilOffset;

       
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
        crosshairObject = GameObject.Find("Crosshair");
        crosshairScript = crosshairObject.GetComponent<DynamicCrosshair>();
        cameraScript = GetComponentInParent<CameraRecoil>();

        raycastShootSource = weaponHandler.raycastSource;
        originalDirection = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)).direction;
        direction = originalDirection;
        spreadControlStatic = spreadControl;
        recoilControlStatic = recoilControl;

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

        if (fireMode == FireMode.Manual) crosshairObject.SetActive(false);
        else crosshairObject.SetActive(true);

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
            direction = originalDirection;
        }

        currentHorizontalBulletSpread = 0f;
        currentVerticalBulletRecoil = 0f;
        

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

        Vector3 dir = default;

        if (currentBulletsInMag == 0)
        {
            animator.SetFloat("IsEmpty", 1);
        }

        if (fireMode == FireMode.SemiAuto || fireMode == FireMode.Auto)
        {
            animator.SetTrigger("Shoot");
            dir = GetShootDir();
            crosshairScript.SetSpread(crosshairSpread);

        }

        else if (fireMode == FireMode.Manual && currentBulletsInMag > 0)
        {

            animator.SetTrigger("BoltJerk");
            canShoot = false;

            dir = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)).direction;

        }

        cameraScript.RecoilFire(verticalRecoil * cameraRecoilMult,horizontalRecoil *cameraRecoilMult,0f);
        DoMuzzleFlash(muzzleFlashSize);
        PlayShootSFX();
        AddRecoil();
      
        canShoot = false;
        bool isHit = false;
        RaycastHit hit;

        if (isHit = Physics.Raycast(raycastShootSource.position, dir, out hit, maxShootDistance, hittableLayers, QueryTriggerInteraction.Ignore))
        {
            shootTargetPos = hit.point;
            HandleHit(hit,shootTargetPos);

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
    
    private void HandleHit(RaycastHit hit,Vector3 hitPoint)
    {
        LayerMask objectLayer = hit.collider.gameObject.layer;

        if (objectLayer == LayerMask.NameToLayer("Player"))
        {
            //Handle hit effect
        }
        else
        {   
            foreach (GameObject bulletHole in bulletHoleArr)
            {
                if (bulletHole.gameObject.layer == objectLayer)
                {
                    GameObject tempObj = Instantiate(bulletHole, shootTargetPos, Quaternion.LookRotation(hit.normal));
                    tempObj.SetActive(true);

                }
            }
        }
    }

 
    private Vector3 GetShootDir()
    {

        if (float.IsNaN(currentHorizontalBulletSpread) || float.IsNaN(currentVerticalBulletRecoil))
        {
            currentHorizontalBulletSpread = 0f;
            currentVerticalBulletRecoil = 0f;
        }


        float randomSpread = Random.Range(-horizontalBulletSpread, horizontalBulletSpread);

        Quaternion spreadRotation = Quaternion.Euler(-currentVerticalBulletRecoil, currentHorizontalBulletSpread, 0f);
        Quaternion finalRotation = mainCam.transform.rotation * spreadRotation;


        float verticalFactor = Mathf.Pow(currentVerticalBulletRecoil / verticalBulletMax, 2f);
        float horizontalFactor = Mathf.Pow(Mathf.Abs(currentHorizontalBulletSpread) / horizontalBulletMax, 2f);

        currentVerticalBulletRecoil += verticalBulletRecoil * (1f + verticalFactor) * currentRecoilControl;
        currentHorizontalBulletSpread += randomSpread * (1f + horizontalFactor) * currentSpreadControl;

        currentHorizontalBulletSpread = Mathf.Clamp(currentHorizontalBulletSpread, -horizontalBulletMax, horizontalBulletMax);
        currentVerticalBulletRecoil = Mathf.Clamp(currentVerticalBulletRecoil, 0f, verticalBulletMax);

        return finalRotation * Vector3.forward;
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
