using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;

public class EffectCleanUp : MonoBehaviour
{
	[SerializeField] protected static int numEffectsTotal=20;
	protected static Queue<GameObject> effectsQueue = new Queue<GameObject>();
	[SerializeField] bool hasTimedCleanUp;
	[Range(0, 10)]
	public float aliveTime;
	ParticleSystem ps;
	[SerializeField] private GameObject bulletHoleObject;

	void OnEnable()
	{
		if (bulletHoleObject != null)
        {
			Renderer rend = bulletHoleObject.GetComponent<Renderer>();
			Material matInstance = rend.material;
			matInstance.mainTextureScale = new Vector2(0.5f, 0.5f);

			int index = Random.Range(0, 4);
			Vector2 offset = Vector2.zero;

			switch (index)
			{
				case 0: offset = new Vector2(0f, 0.5f); break; // top-left
				case 1: offset = new Vector2(0.5f, 0.5f); break; // top-right
				case 2: offset = new Vector2(0f, 0f); break; // bottom-left
				case 3: offset = new Vector2(0.5f, 0f); break; // bottom-right
			}            
        }

		if (!hasTimedCleanUp)
		{
			if (effectsQueue.Count > numEffectsTotal)
			{
				GameObject effectRemoved = effectsQueue.Dequeue();
				Destroy(effectRemoved);
				effectsQueue.Enqueue(gameObject);

			}
			else
			{
				effectsQueue.Enqueue(gameObject);
			}
		}
		else
		{
			ps = GetComponent<ParticleSystem>();
			StartCoroutine(CheckIfPlaying());
		}

	}

    void OnDisable()
    {
        effectsQueue.Clear();
    }

    IEnumerator CheckIfPlaying()
    {
        yield return new WaitForSeconds(0.1f);
        while (ps.isPlaying && !ps.main.loop)
        {
			yield return null;
        }

		if (aliveTime > 0) yield return new WaitForSeconds(aliveTime);
		else Destroy(gameObject);

    }
	
}
