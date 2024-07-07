using UnityEngine;

public class SparkEffectManager : MonoBehaviour
{
	public ParticleSystem Spark;

	private Transform mTransform;

	private bool Active;

	private static SparkEffectManager instance;

	private void Start()
	{
		instance = this;
		mTransform = transform;
	}

	public static void ClearParent()
	{
		instance.mTransform.SetParent(null);
		instance.Active = false;
	}

	public static void SetParent(Transform parent, Vector3 pos)
	{
		if (parent == null)
		{
			instance.mTransform.SetParent(null);
			instance.Active = false;
			return;
		}
		instance.mTransform.SetParent(parent);
		instance.mTransform.localPosition = pos;
		instance.mTransform.localEulerAngles = new Vector3(0f, -4f, 0f);
		instance.Active = true;
	}

	public static void Fire(Vector3 point, float distance)
	{
		if (Settings.ProjectileEffect && distance > 2.5f && instance.Active && Random.value > 0.2f)
		{
			instance.mTransform.LookAt(point);
			instance.Spark.Emit(1);
		}
	}
}
