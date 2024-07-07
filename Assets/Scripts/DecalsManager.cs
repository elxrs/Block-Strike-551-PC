using UnityEngine;

public class DecalsManager : MonoBehaviour
{
	public float FrontOfWall = 0.02f;

	public Transform[] BulletHoles;

	private int LastBulletHole;

	public Transform BloodEffect;

	private int BloodEffectTimerID;

	private static DecalsManager instance;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		EventManager.AddListener("UpdateSettings", UpdateSettings);
		UpdateSettings();
	}

	public static void FireWeapon(DecalInfo decalInfo)
	{
		for (int i = 0; i < decalInfo.Points.Count; i++)
		{
			if (decalInfo.BloodDecal == i)
			{
				if (Settings.Blood)
				{
					instance.CreateBloodEffect(decalInfo.Points[i]);
				}
			}
			else if (Settings.BulletHole && !decalInfo.isKnife)
			{
				instance.CreateBulletHole(decalInfo.Points[i], decalInfo.Normals[i]);
			}
		}
		decalInfo.Dispose();
	}

	public void CreateBulletHole(Vector3 point, Vector3 normal)
	{
		if (!(point == Vector3.zero) || !(normal == Vector3.zero))
		{
			if (LastBulletHole > BulletHoles.Length - 1)
			{
				LastBulletHole = 0;
			}
			Transform transform = BulletHoles[LastBulletHole];
			transform.position = point + normal * instance.FrontOfWall;
			transform.rotation = Quaternion.LookRotation(normal);
			Vector3 eulerAngles = transform.eulerAngles;
			transform.eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, Random.value * 360f);
			LastBulletHole++;
		}
	}

	public static void ClearBulletHoles()
	{
		for (int i = 0; i < instance.BulletHoles.Length; i++)
		{
			instance.BulletHoles[i].position = Vector3.right * 5000f;
		}
		instance.LastBulletHole = 0;
	}

	public void CreateBloodEffect(Vector3 pos)
	{
		Transform activeCamera = CameraManager.GetActiveCamera();
		if (!(activeCamera == null))
		{
			if (TimerManager.IsActive(BloodEffectTimerID))
			{
				TimerManager.Cancel(BloodEffectTimerID);
			}
			BloodEffect.position = pos;
			BloodEffect.LookAt(activeCamera.position);
			Vector3 eulerAngles = BloodEffect.eulerAngles;
			BloodEffect.eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, Random.value * 360f);
			BloodEffectTimerID = TimerManager.In(0.05f, delegate
			{
				BloodEffect.position = Vector3.right * 5000f;
			});
		}
	}

	private void UpdateSettings()
	{
		if (!Settings.BulletHole)
		{
			ClearBulletHoles();
		}
	}
}
