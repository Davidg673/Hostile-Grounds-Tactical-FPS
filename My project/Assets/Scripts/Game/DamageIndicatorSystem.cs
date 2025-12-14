using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DamageIndicatorSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DamageIndicator indicatorPrefab = null;
    [SerializeField] private RectTransform holder = null;
    [SerializeField] private Transform player = null;

    private Dictionary<Transform, DamageIndicator> Indicators = new Dictionary<Transform, DamageIndicator>();

    #region Delegates
    public static UnityAction<Transform> CreateIndicator = delegate { };
    public static Func<Transform, bool> CheckObjectInSight = null;
    #endregion


    private void OnEnable()
    {
        CreateIndicator += Create;
        CheckObjectInSight += Insight;
    }

    private void OnDisable()
    {
        CreateIndicator -= Create;
        CheckObjectInSight -= Insight;

        CreateIndicator = delegate {};
        CheckObjectInSight= null;
    }
    
    void Create(Transform target)
    {
        if (Indicators.ContainsKey(target))
        {
            Indicators[target].Restart();
            return;
        }

        DamageIndicator newIndicator = Instantiate(indicatorPrefab, holder);
        newIndicator.Register(target, player, new UnityAction(() => { Indicators.Remove(target); }));

        Indicators.Add(target, newIndicator);

    }

    bool Insight(Transform target)
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        return screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
    }
}
