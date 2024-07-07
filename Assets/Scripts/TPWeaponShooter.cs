using UnityEngine;

public class TPWeaponShooter : TimerBehaviour
{
	[Header("Weapon")]
	public int WeaponID;

	public int WeaponSkin;

	public byte[] WeaponStickers;

	public MeshAtlas[] WeaponAtlas;

	public bool isMuzzle = true;

	public Transform Muzzle;

	[Header("Transform")]
	public Vector3 DefaultPosition;

	public Quaternion DefaultRotation;

	public Vector3 ContainerPosition;

	public Quaternion ContainerRotation;

	[Header("FireStat")]
	public bool FireStat;

	public GameObject FireStatModel;

	public MeshAtlas[] FireStatCounters;

	public int FireStatValue = -1;

	[Header("Stickers")]
	public MeshAtlas[] Stickers;

	[Header("Spark Effect")]
	public bool SparkEffect;

	public ParticleSystem SparkEffectParticle;

	public Transform SparkEffectTransform;

	private float SparkEffectDistance;

	[Header("Others")]
	public bool CanSound = true;

	public nTimer Timer;

	private PlayerSkin PlayerSkin;

	private WeaponSound FireSound;

	private WeaponSound ReloadSound;

	private bool firstPersonSpectator;

	[Header("Two Handed Weapon")]
	public bool TwoHandedWeapon;

	public GameObject TwoWeapon;

	public Transform TwoMuzzle;

	public Vector3 TwoWeaponPosition;

	public Vector3 TwoWeaponRotation;

	private bool isFireTwoWeapon;

	private Transform cachedTransform;

	private GameObject cachedGameObject;

	public void Init(int weaponID, int weaponSkin, PlayerSkin playerSkin, bool container)
	{
		cachedGameObject = gameObject;
		cachedTransform = transform;
		cachedTransform.localPosition = ((!container) ? DefaultPosition : ContainerPosition);
		cachedTransform.localRotation = ((!container) ? DefaultRotation : ContainerRotation);
		PlayerSkin = playerSkin;
		WeaponID = weaponID;
		WeaponSkin = weaponSkin;
		FireSound = WeaponManager.GetWeaponData(weaponID).FireSound;
		ReloadSound = WeaponManager.GetWeaponData(weaponID).ReloadSound;
		UpdateSkin();
		SetTwoWeapon();
	}

	public void Active()
	{
		cachedGameObject.SetActive(true);
		if (TwoHandedWeapon)
		{
			TwoWeapon.SetActive(true);
		}
	}

	public void Deactive()
	{
		cachedGameObject.SetActive(false);
		if (TwoHandedWeapon)
		{
			TwoWeapon.SetActive(false);
		}
	}

	public void Fire(bool isVisible, DecalInfo decalInfo)
	{
		if (isVisible && isMuzzle)
		{
			if (TwoHandedWeapon && isFireTwoWeapon)
			{
				TwoMuzzle.gameObject.SetActive(true);
				Timer.In(0.05f, delegate
				{
					TwoMuzzle.gameObject.SetActive(false);
				});
			}
			else
			{
				Muzzle.gameObject.SetActive(true);
				Timer.In(0.05f, delegate
				{
					Muzzle.gameObject.SetActive(false);
				});
				if (SparkEffect)
				{
					for (int i = 0; i < decalInfo.Points.Count; i++)
					{
						SparkEffectDistance = Vector3.Distance(decalInfo.Points[i], Muzzle.position);
						if (SparkEffectDistance > 10f && PlayerInput.instance != null && Vector3.Distance(PlayerInput.instance.PlayerTransform.position, Muzzle.position) > 10f)
						{
							SparkEffectTransform.LookAt(decalInfo.Points[i]);
							SparkEffectParticle.startLifetime = SparkEffectDistance / 200f;
							SparkEffectParticle.Emit(1);
						}
					}
				}
			}
			if (TwoHandedWeapon)
			{
				isFireTwoWeapon = !isFireTwoWeapon;
			}
		}
		if (CanSound)
		{
			PlayerSkin.Sounds.Play(FireSound);
		}
	}

	public void Reload()
	{
		if (CanSound)
		{
			PlayerSkin.Sounds.Play(ReloadSound, 0.2f);
		}
	}

	public void UpdateSkin()
	{
		string text = WeaponID + "-" + WeaponSkin;
		for (int i = 0; i < WeaponAtlas.Length; i++)
		{
			if (WeaponAtlas[i].spriteName != text)
			{
				MeshAtlas meshAtlas = WeaponAtlas[i];
				meshAtlas.spriteName = text;
			}
		}
	}

	public void SetFireStat(int firestat)
	{
		if (firestat >= -1)
		{
			FireStat = true;
			FireStatModel.SetActive(true);
			UpdateFireStat(firestat);
		}
	}

	public void UpdateFireStat1()
	{
		if (FireStat)
		{
			UpdateFireStat(FireStatValue + 1);
		}
	}

	public void UpdateFireStat(int counter)
	{
		if (FireStat && FireStatValue != counter)
		{
			FireStatValue = counter;
			string text = counter.ToString("D6");
			for (int i = 0; i < FireStatCounters.Length; i++)
			{
				FireStatCounters[i].spriteName = "f" + text[i];
			}
		}
	}

	public void SetParent(Transform p1, Transform p2)
	{
		if (cachedTransform.parent != p1)
		{
			cachedTransform.SetParent(p1);
			cachedTransform.localPosition = DefaultPosition;
			cachedTransform.localRotation = DefaultRotation;
		}
		if (TwoHandedWeapon && TwoWeapon.transform.parent != p2)
		{
			TwoWeapon.transform.SetParent(p2);
			TwoWeapon.transform.localPosition = TwoWeaponPosition;
			TwoWeapon.transform.localEulerAngles = TwoWeaponRotation;
		}
	}

	public void SetContainer(bool active)
	{
		cachedTransform.localPosition = ((!active) ? DefaultPosition : ContainerPosition);
		cachedTransform.localRotation = ((!active) ? DefaultRotation : ContainerRotation);
	}

	public void SetTwoWeapon()
	{
		if (TwoHandedWeapon)
		{
			TwoWeapon.transform.SetParent(PlayerSkin.PlayerTwoWeaponRoot);
			TwoWeapon.transform.localPosition = TwoWeaponPosition;
			TwoWeapon.transform.localEulerAngles = TwoWeaponRotation;
		}
	}

	public void SetDissolveEffect(bool active, Material mat)
	{
		if (active)
		{
			if (FireStat)
			{
				FireStatModel.SetActive(false);
			}
			for (int i = 0; i < WeaponAtlas.Length; i++)
			{
				WeaponAtlas[i].meshRenderer.material = mat;
			}
		}
		else
		{
			if (FireStat)
			{
				FireStatModel.SetActive(true);
			}
			for (int j = 0; j < WeaponAtlas.Length; j++)
			{
				WeaponAtlas[j].meshRenderer.material = WeaponAtlas[j].atlas.spriteMaterial;
			}
		}
	}

	public void SetStickers(byte[] stickers)
	{
		if (stickers == null || stickers.Length == 0)
		{
			for (int i = 0; i < Stickers.Length; i++)
			{
				Stickers[i].cachedGameObject.SetActive(false);
			}
			return;
		}
		if (CheckStickers(WeaponStickers, stickers))
		{
			for (int j = 0; j < Stickers.Length; j++)
			{
				Stickers[j].cachedGameObject.SetActive(false);
			}
		}
		int num = 0;
		for (int k = 0; k < stickers.Length / 2; k++)
		{
			Stickers[stickers[num] - 1].cachedGameObject.SetActive(true);
			Stickers[stickers[num] - 1].spriteName = stickers[num + 1].ToString();
			num += 2;
		}
		WeaponStickers = stickers;
	}

	private bool CheckStickers(byte[] a1, byte[] a2)
	{
		if (a1 == null || a2 == null)
		{
			return false;
		}
		if (a1.Length != a2.Length)
		{
			return false;
		}
		for (int i = 0; i < a1.Length; i++)
		{
			if (a1[i] != a2[i])
			{
				return false;
			}
		}
		return true;
	}

	public int[] GetStickers()
	{
		int[] array = new int[Stickers.Length];
		for (int i = 0; i < array.Length; i++)
		{
			if (Stickers[i].cachedGameObject.activeSelf)
			{
				try
				{
					array[i] = int.Parse(Stickers[i].spriteName);
				}
				catch
				{
					array[i] = -1;
				}
			}
			else
			{
				array[i] = -1;
			}
		}
		return array;
	}

	public Transform GetCachedTransform()
	{
		if (cachedTransform == null)
		{
			cachedTransform = transform;
		}
		return cachedTransform;
	}

	public void SetFirstSpectator(bool active)
	{
		if (firstPersonSpectator == active)
		{
			return;
		}
		firstPersonSpectator = active;
		for (int i = 0; i < WeaponAtlas.Length; i++)
		{
			WeaponAtlas[i].meshRenderer.enabled = !active;
		}
		Muzzle.GetComponent<Renderer>().enabled = !active;
		if (FireStat)
		{
			if (FireStatModel != null)
			{
				FireStatModel.GetComponent<Renderer>().enabled = !active;
			}
			for (int j = 0; j < FireStatCounters.Length; j++)
			{
				FireStatCounters[j].meshRenderer.enabled = !active;
			}
		}
		for (int k = 0; k < Stickers.Length; k++)
		{
			Stickers[k].meshRenderer.enabled = !active;
		}
	}
}
