using System.Collections;
using EnemyAI;
using UnityEngine;
using UnityEngine.UI;

public class GrenadeEffect : MonoBehaviour
{
    [SerializeField] public Throwable.Type type;
    [SerializeField] private GameObject fireEffect;
    [SerializeField] private GameObject smokeEffect;
    [SerializeField] private GameObject HEEffect;
    [SerializeField] private AudioSource sfxEffect;
    [SerializeField] private AudioClip flashClip;
    [SerializeField] private AudioClip HEClip;
    [SerializeField] private AudioClip DecoyClip;

    [SerializeField] private float radiusHE;
    [SerializeField] private float radiusFlash;

    [SerializeField] private float decoyTime;
    [SerializeField] private float flashTime;


    private Coroutine flashRoutine;
    private int CTMask;
    private int TMask;


    [SerializeField] private float grenadeBaseDamage;
    [SerializeField] GameObject flashContainer;


    [SerializeField] private float timerToExplode;
    void Start()
    {
        CTMask = 1<< LayerMask.NameToLayer("CT");
        TMask = 1 << LayerMask.NameToLayer("T");

        StartCoroutine(Countdown());
    }

    private void PlayEffect()
    {
        switch (type)
        {
            case Throwable.Type.Fire: FireEffect(); break;
            case Throwable.Type.Smoke: SmokeEffect(); break;
            case Throwable.Type.Flash: CheckForTargetsFlash(radiusFlash); break;
            case Throwable.Type.HE: CheckForTargetsHE(radiusHE); break;
            case Throwable.Type.Decoy: PlayDecoySound(); break;

        }
    }

    private void FireEffect()
    {
        GameObject effectInstace=Instantiate(fireEffect,transform.position,Quaternion.LookRotation(Vector3.up));
        effectInstace.SetActive(true);

        Destroy(gameObject);
    }
    private void SmokeEffect()
    {
        GameObject effectInstace = Instantiate(smokeEffect, transform.position,Quaternion.identity);
        effectInstace.SetActive(true);
        Destroy(gameObject);
    }

    private void CheckForTargetsHE(float radius)
    {
        Collider[] hitsCT = Physics.OverlapSphere(transform.position, radius, CTMask);
        Collider[] hitsT = Physics.OverlapSphere(transform.position, radius, TMask);

        foreach (Collider hit in hitsCT)
        {
            PlayerHealth playerHealth = hit.gameObject.GetComponent<PlayerHealth>();
            EnemyHealth enemyHealth = hit.gameObject.GetComponent<EnemyHealth>();

            Vector3 hitPoint = hit.ClosestPoint(transform.position);
            Vector3 direction = hitPoint - transform.position;
            float damage = grenadeBaseDamage * (1 / Vector3.Magnitude(direction));
            if (playerHealth != null) playerHealth.TakeDamage(hitPoint, direction, Mathf.Round(damage),null,gameObject);
            if (enemyHealth != null) enemyHealth.TakeDamage(hitPoint, direction, damage, hit);
        }

        foreach (Collider hit in hitsT)
        {
            PlayerHealth playerHealth = hit.gameObject.GetComponent<PlayerHealth>();
            EnemyHealth enemyHealth = hit.gameObject.GetComponent<EnemyHealth>();

            Vector3 hitPoint = hit.ClosestPoint(transform.position);
            Vector3 direction = hitPoint - transform.position;
            float damage = grenadeBaseDamage * (1 / Vector3.Magnitude(direction));
            if (playerHealth != null) playerHealth.TakeDamage(hitPoint, direction, Mathf.Round(damage),null,gameObject);
            if (enemyHealth != null) enemyHealth.TakeDamage(hitPoint, direction, damage, hit);
        }

        GameObject effectInstance = Instantiate(HEEffect, transform.position, Quaternion.identity);
        effectInstance.SetActive(true);
    }

    private void CheckForTargetsFlash(float radius)
    {
        Collider[] hitsCT = Physics.OverlapSphere(transform.position, radius, CTMask);
        Collider[] hitsT = Physics.OverlapSphere(transform.position, radius, TMask);

        foreach (Collider hit in hitsCT)
        {
            if (hit.gameObject.GetComponent<PlayerHealth>() != null)
            {
                if (flashRoutine != null)
                {
                    StopCoroutine(flashRoutine);
                    flashRoutine = null;
                }
                flashRoutine=StartCoroutine(ApplyFlash(hit));
            }
        }

        foreach (Collider hit in hitsT)
        {
               if (flashRoutine != null)
                {
                    StopCoroutine(flashRoutine);
                    flashRoutine = null;
                }
                flashRoutine=StartCoroutine(ApplyFlash(hit));
        }
    }


    private IEnumerator PlayDecoySound()
    {
        sfxEffect.PlayOneShot(DecoyClip);
        yield return new WaitForSeconds(decoyTime);
        Destroy(gameObject);
    }

    private IEnumerator Countdown()
    {
        yield return new WaitForSeconds(timerToExplode);
        PlayEffect();
        GetComponent<Renderer>().enabled = false;

    }

    private IEnumerator ApplyFlash(Collider hit)
    {
        Vector3 hitPoint = hit.ClosestPoint(transform.position);
        Vector3 direction = hitPoint - transform.position;
        float distance = Vector3.Magnitude(direction);

        flashContainer.SetActive(true);
        Image flashImage = flashContainer.GetComponent<Image>();
        Color alpha = flashImage.color;

        float alphaCap = Mathf.Clamp(radiusFlash - distance,0.1f,radiusFlash) / radiusFlash *1.7f;
        
        alpha.a = 0f;
        flashImage.color = alpha;
        Debug.Log(alphaCap);
        while (alpha.a < alphaCap)
        {
            alpha.a += Time.deltaTime * 10f;
            alpha.a = Mathf.Clamp(alpha.a, 0f, alphaCap);
            flashImage.color = alpha;
            yield return null;
        }   

        alpha.a = alphaCap;
        flashImage.color = alpha;

        yield return new WaitForSeconds(flashTime);

        while (alpha.a > 0.1f)
        {
            alpha.a -= Time.deltaTime * 2f;
            flashImage.color = alpha;

            yield return null;
        }

        alpha.a = 0f;
        flashImage.color = alpha;
        flashContainer.SetActive(false);

        Destroy(gameObject);
    }


}
