using UnityEngine;

public class DropWeapon : MonoBehaviour
{
	public int ID;

	[SelectedWeapon]
	public int weaponID;

	public GameObject Weapon;

	public MeshAtlas[] WeaponAtlas;

	public MeshAtlas[] Stickers;

	public GameObject FireStat;

	public MeshAtlas[] FireStatCounters;

	public bool DestroyDrop;

	public float DestroyTime = -1f;

	public bool CustomData;

	public WeaponCustomData Data = new WeaponCustomData();

	private bool isEnterTrigger;

	private int TimerID;

	private int EventID;

	private void OnEnable()
	{
		EventID = EventManager.AddListener<DamageInfo>("DeadPlayer", OnDeadPlayer);
		if (DestroyTime > 0f)
		{
			TimerID = TimerManager.In(DestroyTime, DestroyWeapon);
		}
	}

	private void OnDisable()
	{
		Deactive();
		EventManager.ClearEvent(EventID);
		TimerManager.Cancel(TimerID);
	}

	private void GetButtonDown(string name)
	{
		if (isEnterTrigger && name == "Use" && !PlayerInput.instance.PlayerWeapon.Wielded)
		{
			DropWeaponManager.PickupWeapon(ID);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			PlayerInput component = other.GetComponent<PlayerInput>();
			if (component != null)
			{
				isEnterTrigger = true;
				InputManager.GetButtonDownEvent += GetButtonDown;
				UIControllerList.Use.cachedGameObject.SetActive(true);
				UIControllerList.UseText.text = Localization.Get("Pick up") + " " + WeaponManager.GetWeaponName(weaponID);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (isEnterTrigger && other.CompareTag("Player"))
		{
			PlayerInput component = other.GetComponent<PlayerInput>();
			if (component != null)
			{
				Deactive();
			}
		}
	}

	private void OnDeadPlayer(DamageInfo info)
	{
		Deactive();
	}

	private void Deactive()
	{
		InputManager.GetButtonDownEvent -= GetButtonDown;
		if (isEnterTrigger)
		{
			UIControllerList.Use.cachedGameObject.SetActive(false);
			UIControllerList.UseText.text = string.Empty;
		}
		isEnterTrigger = false;
	}

	public void OnDropWeapon()
	{
		WeaponData weaponData = WeaponManager.GetWeaponData(weaponID);
		PlayerInput.instance.PlayerWeapon.DropWeapon(weaponData.Type);
		PlayerInput.instance.PlayerWeapon.UpdateWeapon(weaponData.ID, false, (!CustomData) ? null : Data);
		Deactive();
		if (DestroyDrop)
		{
			DestroyWeapon();
		}
	}

	public void DestroyWeapon()
	{
		PoolManager.Despawn(weaponID + "-Drop", Weapon);
	}

	public void UpdateWeapon()
	{
		if (CustomData)
		{
			string spriteName = weaponID + "-" + Data.Skin;
			for (int i = 0; i < WeaponAtlas.Length; i++)
			{
				WeaponAtlas[i].spriteName = spriteName;
			}
			if (Data.FireStat)
			{
				FireStat.SetActive(true);
				string text = Data.FireStatCounter.ToString("D6");
				for (int j = 0; j < text.Length; j++)
				{
					FireStatCounters[j].spriteName = "f" + text[j];
				}
			}
			else
			{
				FireStat.SetActive(false);
			}
		}
		else
		{
			string spriteName2 = weaponID + "-0";
			for (int k = 0; k < WeaponAtlas.Length; k++)
			{
				WeaponAtlas[k].spriteName = spriteName2;
			}
		}
	}
}
