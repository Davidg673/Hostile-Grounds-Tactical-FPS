using EnemyAI;
using UnityEditor;
using UnityEngine;

public class Knife : MonoBehaviour
{
    public Transform handTransform;
    private WeaponHandler weaponHandlerScript;

    private void Start()
    {
        weaponHandlerScript=GetComponentInParent<WeaponHandler>();
        weaponHandlerScript.OnWeaponUseStarted+= PlayAnimation;
    }
    public void CheckHit()
    {
        Collider[] objectsHit = Physics.OverlapBox(handTransform.position,new Vector3(5.0f,5.0f,5.0f));

        if (objectsHit.Length > 1)
        {
            Vector3 point = objectsHit[0].ClosestPoint(transform.position);
            objectsHit[0].SendMessageUpwards("HitCallback", new HealthManager.DamageInfo(point, transform.position, 10, objectsHit[0],gameObject), SendMessageOptions.DontRequireReceiver);

        }
    }

    private void PlayAnimation()
    {
        GetComponent<Animator>().SetTrigger(0);
    }

}
