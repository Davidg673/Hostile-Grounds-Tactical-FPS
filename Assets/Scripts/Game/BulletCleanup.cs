using System.Collections;
using UnityEngine;

public class BulletCleanup : MonoBehaviour
{
    [SerializeField] private AudioClip[] bulletImpactSFX;
    bool playOnce=true;
    private void Start()
    {
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (playOnce)
        {
            playOnce = false;
            
            int clipIndex = Random.Range(0, 1);
            if (Random.value < 0.4)
            {
                AudioSource.PlayClipAtPoint(bulletImpactSFX[clipIndex], collision.contacts[0].point,0.2f);        
            }

            StartCoroutine(StartFade());

        }
    }

    private IEnumerator StartFade()
    {
        Vector3 finalSize = transform.localScale/5;
        float elapsed = 0f;
        float duration = 2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            transform.localScale = Vector3.Lerp(transform.localScale, finalSize, Time.deltaTime*0.005f);

            yield return null;
        }

        Destroy(gameObject);
    }
}
