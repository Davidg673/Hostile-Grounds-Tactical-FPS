using System.Collections;
using UnityEngine;

public class ThrowableBase : MonoBehaviour
{
    [SerializeField] protected float explosionDelay = 3f;
    private WaitForSeconds waitForExplosion => new WaitForSeconds(explosionDelay);
    public GameObject droppedGrenadePrefab;
    public GameObject parentGrenade;
    public int cost;

    public IEnumerator DestroyInstance(float aliveTime)
    {
        yield return new WaitForSeconds(aliveTime);

        Destroy(gameObject);
    }


}
