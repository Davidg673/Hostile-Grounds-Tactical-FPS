using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// Base code provided by rukass kirk evicius  from youtube tutorial https://www.youtube.com/watch?v=BC3AKOQUx04 . Own implementation and editing added.
/// </summary> 
public class DamageIndicator : MonoBehaviour
{
    private const float maxTimer = 3f;
    private float timer = maxTimer;

    private CanvasGroup canvasGroup = null;
    protected CanvasGroup CanvasGroup
    {
        get
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup.AddComponent<CanvasGroup>();
            }
            return canvasGroup;
        }
    }


    private RectTransform rect = null;
    protected RectTransform Rect
    {
        get
        {
            if (rect == null)
            {
                rect=GetComponent<RectTransform>();
                if (rect == null) rect = gameObject.AddComponent<RectTransform>();
            }
            return rect;
        }
    }

    public Transform Target { get; protected set; } = null;
    private Transform player = null;


    private IEnumerator countdownRoutine = null;
    private UnityAction unRegister = null;

    private Quaternion tRot = Quaternion.identity;
    private Vector3 tPos = Vector3.zero;



    public void Register(Transform target, Transform player, UnityAction unRegister)
    {
        Target = target;
        this.player = player;
        this.unRegister = unRegister;

        StartCoroutine(RotateToTarget());
        StartTimer();
    }

    public void Restart()
    {
        timer = maxTimer;

        StartTimer();
    }

    private void StartTimer()
    {
        if (countdownRoutine != null) StopCoroutine(countdownRoutine);
        countdownRoutine = Countdown();
        StartCoroutine(countdownRoutine);
    }

    private IEnumerator RotateToTarget()
    {
        while (enabled)
        {
            if (Target)
            {
                tPos = Target.position;
                tRot = Target.rotation;
            }

            Vector3 direction = player.position - tPos;

            tRot = Quaternion.LookRotation(direction);
            tRot.z = -tRot.y;
            tRot.x = 0f;
            tRot.y = 0f;


            Vector3 northDirection = new Vector3(0f, 0f, player.eulerAngles.y);
            Rect.localRotation = tRot * Quaternion.Euler(northDirection);

            yield return null;
        }
    }

    private IEnumerator Countdown()
    {
        while (CanvasGroup.alpha < 1f)
        {
            CanvasGroup.alpha += 10 * Time.deltaTime;
            yield return null;
        }

        while (timer > 0)
        {
            timer--;
            yield return new WaitForSeconds(1f);
        }

        while (CanvasGroup.alpha > 0f)
        {
            CanvasGroup.alpha -= 5 * Time.deltaTime;
            yield return null;
        }

        unRegister();
        Destroy(gameObject);
    }

}
