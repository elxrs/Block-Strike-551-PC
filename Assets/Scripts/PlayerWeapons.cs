using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerWeapons : MonoBehaviour
{
	[Serializable]
	public class PlayerWeaponData
	{
		public bool Enabled;

		public bool CustomData;

		public CryptoInt ID;

		public bool CanFire;

		public CryptoString Name;

		public CryptoInt FaceDamage;

		public CryptoInt BodyDamage;

		public CryptoInt HandDamage;

		public CryptoInt LegDamage;

		public CryptoFloat FireRate;

		public CryptoFloat Accuracy;

		public CryptoFloat FireAccuracy;

		public CryptoInt FireBullets;

		public CryptoFloat ReloadTime;

		public CryptoFloat Distance;

		public CryptoFloat Mass;

		public bool Scope;

		public CryptoInt ScopeSize;

		public CryptoFloat ScopeSensitivity;

		public CryptoFloat ScopeRecoil;

		public CryptoFloat ScopeAccuracy;

		public bool Scope2;

		public CryptoInt Scope2Size;

		public CryptoFloat Scope2Sensitivity;

		public WeaponSound FireSound;

		public WeaponSound ReloadSound;

		public CryptoInt Ammo;

		public CryptoInt AmmoTotal;

		public CryptoInt AmmoMax;

		public CryptoInt Skin;

		public CryptoInt FireStat;

		public CryptoInt[] Stickers;

		public float LastFire;

		public FPWeaponShooter Script;
	}

	public WeaponType SelectedWeapon;

	public bool CanFire = true;

	public bool isDebug;

	[Disabled]
	public bool isFire;

	[Disabled]
	public bool isScope;

	[Disabled]
	public bool isReload;

	private int ReloadTimeID;

	[Disabled]
	public bool Wielded;

	private int WieldedTimeID;

	public bool isDropWeapon;

	private float DropWeaponTime = -1f;

	public bool InfiniteAmmo;

	private bool isDryFire;

	private CryptoVector2 FireAccuracy;

	private Ray FireRay;

	private RaycastHit FireRaycastHit;

	private string FireRayTag;

	[Header("Weapons Data")]
	public PlayerWeaponData KnifeData = new PlayerWeaponData();

	public PlayerWeaponData PistolData = new PlayerWeaponData();

	public PlayerWeaponData RifleData = new PlayerWeaponData();

	private bool isUpdateWeaponData;

	[Header("RigidBody")]
	public bool PushRigidbody;

	public float PushRigidbodyForce = 1000f;

	[Header("Sounds")]
	public PlayerSounds Sounds;

	[Header("Others")]
	public LayerMask FireLayers;

	public PlayerInput m_PlayerInput;

	public Camera PlayerCamera;

	public Camera WeaponCamera;

	public nTimer Timer;

	private Dictionary<string, GameObject> WeaponObjects = new Dictionary<string, GameObject>();

	private void Start()
	{
		EventManager.AddListener<DamageInfo>("KillPlayer", KillPlayer);
		Timer.In(nValue.int2, true, UpdateValue);
	}

	public byte[] SerializeWeapons()
	{
		byte[] array = new byte[6]
		{
			(byte)(int)GetSelectedWeaponData().ID,
			(byte)(int)GetSelectedWeaponData().Skin,
			0,
			0,
			0,
			0
		};
		byte b = 2;
		if (SelectedWeapon != WeaponType.Rifle)
		{
			array[b] = (byte)(int)RifleData.ID;
			array[b + 1] = (byte)(int)RifleData.Skin;
			b += 2;
		}
		if (SelectedWeapon != WeaponType.Pistol)
		{
			array[b] = (byte)(int)PistolData.ID;
			array[b + 1] = (byte)(int)PistolData.Skin;
			b += 2;
		}
		if (SelectedWeapon != WeaponType.Knife)
		{
			array[b] = (byte)(int)KnifeData.ID;
			array[b + 1] = (byte)(int)KnifeData.Skin;
		}
		for (int i = 0; i < array.Length; i++)
		{
			print(array[i]);
		}
		return array;
	}

	[ContextMenu("UpdateValue")]
	private void UpdateValue()
	{
		FireAccuracy.UpdateValue();
		UpdateValueWeaponData(GetSelectedWeaponData());
	}

	private void UpdateValueWeaponData(PlayerWeaponData data)
	{
		data.ID.UpdateValue();
		data.FaceDamage.UpdateValue();
		data.BodyDamage.UpdateValue();
		data.HandDamage.UpdateValue();
		data.LegDamage.UpdateValue();
		data.FireRate.UpdateValue();
		data.Accuracy.UpdateValue();
		data.FireAccuracy.UpdateValue();
		data.FireBullets.UpdateValue();
		data.ReloadTime.UpdateValue();
		data.Distance.UpdateValue();
		data.Mass.UpdateValue();
		data.ScopeSize.UpdateValue();
		data.ScopeSensitivity.UpdateValue();
		data.ScopeRecoil.UpdateValue();
		data.ScopeAccuracy.UpdateValue();
		data.Scope2Size.UpdateValue();
		data.Scope2Sensitivity.UpdateValue();
		data.Ammo.UpdateValue();
		data.AmmoTotal.UpdateValue();
		data.AmmoMax.UpdateValue();
		data.Skin.UpdateValue();
		data.FireStat.UpdateValue();
	}

	private void OnEnable()
	{
		UICrosshair.SetActiveCrosshair(true);
		InputManager.GetButtonDownEvent += GetButtonDown;
		InputManager.GetButtonUpEvent += GetButtonUp;
	}

	private void OnDisable()
	{
		UICrosshair.SetActiveCrosshair(false);
		InputManager.GetButtonDownEvent -= GetButtonDown;
		InputManager.GetButtonUpEvent -= GetButtonUp;
		isReload = false;
		isFire = false;
		Timer.Cancel(ReloadTimeID);
		Timer.Cancel(WieldedTimeID);
	}

	private void GetButtonDown(string name)
	{
		switch (name)
		{
		case "Fire":
			isFire = true;
			break;
		case "Aim":
			ScopeWeapon(true);
			break;
		case "Reload":
			ReloadWeapon();
			break;
		case "Pause":
		case "Statistics":
			DeactiveScope();
			break;
		case "SelectWeapon":
			if (isDropWeapon)
			{
				DropWeaponTime = 0f;
			}
			else
			{
				UpdateSelectWeapon();
			}
			break;
		}
	}

	private void GetButtonUp(string name)
	{
		switch (name)
		{
		case "Fire":
			isFire = false;
			isDryFire = false;
			break;
		case "SelectWeapon":
			if (isDropWeapon && DropWeaponTime >= 0f)
			{
				UpdateSelectWeapon();
				DropWeaponTime = -1f;
			}
			break;
		}
	}

	private void Update()
	{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		if (Input.GetKey(KeyCode.Alpha1) && GameObject.Find("Display") != null)
		{
			SetWeapon(WeaponType.Rifle, true);
		}
		if (Input.GetKey(KeyCode.Alpha2) && GameObject.Find("Display") != null)
		{
			SetWeapon(WeaponType.Pistol, true);
		}
		if (Input.GetKey(KeyCode.Alpha3) && GameObject.Find("Display") != null)
		{
			SetWeapon(WeaponType.Knife, true);
		}
#endif
		if (isDropWeapon && DropWeaponTime >= 0f)
		{
			if (DropWeaponTime >= 0.5f)
			{
				DropWeapon(true);
				DropWeaponTime = -1f;
			}
			DropWeaponTime += Time.deltaTime;
		}
	}

	private void FixedUpdate()
    {
		if (isFire)
		{
			FireWeapon();
		}
	}

	private void UpdateSelectWeapon()
	{
		if (SelectedWeapon == WeaponType.Knife)
		{
			if (RifleData.Enabled)
			{
				SetWeapon(WeaponType.Rifle);
			}
			else if (PistolData.Enabled)
			{
				SetWeapon(WeaponType.Pistol);
			}
		}
		else if (SelectedWeapon == WeaponType.Pistol)
		{
			if (KnifeData.Enabled)
			{
				SetWeapon(WeaponType.Knife);
			}
			else if (RifleData.Enabled)
			{
				SetWeapon(WeaponType.Rifle);
			}
		}
		else if (SelectedWeapon == WeaponType.Rifle)
		{
			if (PistolData.Enabled)
			{
				SetWeapon(WeaponType.Pistol);
			}
			else if (KnifeData.Enabled)
			{
				SetWeapon(WeaponType.Knife);
			}
		}
	}

	public PlayerWeaponData GetSelectedWeaponData()
	{
		return GetWeaponData(SelectedWeapon);
	}

	public PlayerWeaponData GetWeaponData(WeaponType weapon)
	{
		switch (weapon)
		{
		case WeaponType.Knife:
			return KnifeData;
		case WeaponType.Pistol:
			return PistolData;
		case WeaponType.Rifle:
			return RifleData;
		default:
			return null;
		}
	}

	public PlayerWeaponData GetWeaponData(int weaponID)
	{
		if (KnifeData.ID == weaponID)
		{
			return KnifeData;
		}
		if (PistolData.ID == weaponID)
		{
			return PistolData;
		}
		if (RifleData.ID == weaponID)
		{
			return RifleData;
		}
		return null;
	}

	public void UpdateWeaponAll()
	{
		UpdateWeaponAll(WeaponManager.DefaultWeaponType);
	}

	public void UpdateWeaponAll(WeaponType selectWeapon)
	{
		UpdateWeaponAll(selectWeapon, null, null, null);
	}

	public void UpdateWeaponAll(WeaponType selectWeapon, WeaponCustomData dataKnife, WeaponCustomData dataPistol, WeaponCustomData dataRifle)
	{
		isUpdateWeaponData = true;
		DeactiveScope();
		DeactiveAll();
		UpdateWeaponData(WeaponType.Knife, dataKnife);
		UpdateWeaponData(WeaponType.Pistol, dataPistol);
		UpdateWeaponData(WeaponType.Rifle, dataRifle);
		Timer.In(nValue.float005, delegate
		{
			SetWeapon(selectWeapon, false);
			isUpdateWeaponData = false;
		});
		UpdateShowButtons();
	}

	public void UpdateWeaponAll(WeaponType selectWeapon, int knife, int pistol, int rifle, WeaponCustomData dataKnife, WeaponCustomData dataPistol, WeaponCustomData dataRifle)
	{
		isUpdateWeaponData = true;
		DeactiveScope();
		DeactiveAll();
		UpdateWeaponData(knife, dataKnife);
		UpdateWeaponData(pistol, dataPistol);
		UpdateWeaponData(rifle, dataRifle);
		Timer.In(nValue.float005, delegate
		{
			SetWeapon(selectWeapon, false);
			isUpdateWeaponData = false;
		});
		UpdateShowButtons();
	}

	public void UpdateWeapon(WeaponType type, bool isSelectWeapon)
	{
		UpdateWeapon(type, isSelectWeapon, null);
	}

	public void UpdateWeapon(WeaponType type, bool isSelectWeapon, WeaponCustomData customData)
	{
		int weaponID = WeaponManager.GetSelectWeapon(type);
		Timer.In(nValue.float005, delegate
		{
			isUpdateWeaponData = true;
			DeactiveAll();
			WeaponManager.SetSelectWeapon(type, weaponID);
			UpdateWeaponData(type, customData);
			Timer.In(nValue.float005, delegate
			{
				if (isSelectWeapon)
				{
					SetWeapon(type, false);
				}
				else
				{
					SetWeapon(SelectedWeapon, false);
				}
				isUpdateWeaponData = false;
			});
			UpdateShowButtons();
		});
	}

	public void UpdateWeapon(int weaponID, bool isSelectWeapon)
	{
		UpdateWeapon(weaponID, isSelectWeapon, null);
	}

	public void UpdateWeapon(int weaponID, bool isSelectWeapon, WeaponCustomData customData)
	{
		Timer.In(nValue.float005, delegate
		{
			isUpdateWeaponData = true;
			DeactiveAll();
			UpdateWeaponData(weaponID, customData);
			Timer.In(nValue.float005, delegate
			{
				if (isSelectWeapon)
				{
					SetWeapon(WeaponManager.GetWeaponData(weaponID).Type, false);
				}
				else
				{
					SetWeapon(SelectedWeapon, false);
				}
				isUpdateWeaponData = false;
			});
			UpdateShowButtons();
		});
	}

	private void UpdateShowButtons()
	{
		int num = nValue.int0;
		if (KnifeData.Enabled)
		{
			num++;
		}
		if (PistolData.Enabled)
		{
			num++;
		}
		if (RifleData.Enabled)
		{
			num++;
		}
		if (num >= nValue.int2)
		{
			UIControllerList.SelectWeapon.cachedGameObject.SetActive(true);
		}
		else
		{
			UIControllerList.SelectWeapon.cachedGameObject.SetActive(false);
		}
	}

	private void UpdateWeaponData(WeaponType weaponType)
	{
		UpdateWeaponData(weaponType, null);
	}

	private void UpdateWeaponData(int weaponID)
	{
		UpdateWeaponData(weaponID, null);
	}

	private void UpdateWeaponData(WeaponType weaponType, WeaponCustomData customData)
	{
		WeaponData data = null;
		switch (weaponType)
		{
		case WeaponType.Knife:
			if (WeaponManager.HasSelectWeapon(WeaponType.Knife))
			{
				data = WeaponManager.GetSelectWeaponData(WeaponType.Knife);
			}
			break;
		case WeaponType.Pistol:
			if (WeaponManager.HasSelectWeapon(WeaponType.Pistol))
			{
				data = WeaponManager.GetSelectWeaponData(WeaponType.Pistol);
			}
			break;
		case WeaponType.Rifle:
			if (WeaponManager.HasSelectWeapon(WeaponType.Rifle))
			{
				data = WeaponManager.GetSelectWeaponData(WeaponType.Rifle);
			}
			break;
		}
		UpdateWeaponData(weaponType, data, true, customData);
	}

	private void UpdateWeaponData(int weaponID, WeaponCustomData customData)
	{
		WeaponData weaponData = WeaponManager.GetWeaponData(weaponID);
		UpdateWeaponData(weaponData.Type, weaponData, true, customData);
	}

	private void UpdateWeaponData(WeaponType weaponType, WeaponData data, bool isUpgrade, WeaponCustomData customData)
	{
		PlayerWeaponData playerWeaponData = new PlayerWeaponData();
		if (data != null && !WeaponObjects.ContainsKey(data.Name) && data.FpsPrefab != null)
		{
			GameObject fpsPrefab = data.FpsPrefab;
			fpsPrefab = Utils.AddChild(fpsPrefab, m_PlayerInput.FPCamera.transform);
			WeaponObjects.Add(data.Name, fpsPrefab);
			fpsPrefab.SetActive(true);
		}
		if (data != null)
		{
			playerWeaponData.Enabled = true;
			playerWeaponData.CustomData = false;
			playerWeaponData.ID = (int)data.ID;
			playerWeaponData.CanFire = data.CanFire;
			playerWeaponData.Name = (string)data.Name;
			playerWeaponData.FaceDamage = (int)data.FaceDamage;
			playerWeaponData.BodyDamage = (int)data.BodyDamage;
			playerWeaponData.HandDamage = (int)data.HandDamage;
			playerWeaponData.LegDamage = (int)data.LegDamage;
			playerWeaponData.FireRate = (float)data.FireRate;
			playerWeaponData.Accuracy = (float)data.Accuracy;
			playerWeaponData.FireAccuracy = (float)data.FireAccuracy;
			playerWeaponData.Ammo = (int)data.Ammo;
			playerWeaponData.AmmoTotal = (int)data.Ammo;
			playerWeaponData.AmmoMax = (int)data.MaxAmmo;
			playerWeaponData.Mass = (float)data.Mass;
			playerWeaponData.FireBullets = (int)data.FireBullets;
			playerWeaponData.ReloadTime = (float)data.ReloadTime;
			playerWeaponData.Distance = (float)data.Distance;
			playerWeaponData.Scope = data.Scope;
			playerWeaponData.ScopeSize = (int)data.ScopeSize;
			playerWeaponData.ScopeSensitivity = (float)data.ScopeSensitivity;
			playerWeaponData.ScopeRecoil = (float)data.ScopeRecoil;
			playerWeaponData.ScopeAccuracy = (float)data.ScopeAccuracy;
			playerWeaponData.Scope2 = data.Scope2;
			playerWeaponData.Scope2Size = (int)data.Scope2Size;
			playerWeaponData.Scope2Sensitivity = (float)data.Scope2Sensitivity;
			playerWeaponData.FireSound = data.FireSound;
			playerWeaponData.ReloadSound = data.ReloadSound;
			playerWeaponData.Skin = SaveLoadManager.GetWeaponSkinSelected(playerWeaponData.ID);
			playerWeaponData.FireStat = SaveLoadManager.GetFireStatCounter(playerWeaponData.ID, playerWeaponData.Skin);
			playerWeaponData.Stickers = Utils.ArrayIntToArrayCryptoInt(AccountManager.GetWeaponStickers(playerWeaponData.ID, playerWeaponData.Skin).ToArray());
			playerWeaponData.LastFire = 0f;
			playerWeaponData.Script = WeaponObjects[data.Name].GetComponent<FPWeaponShooter>();
			if (customData != null)
			{
				playerWeaponData.CustomData = customData.CustomData;
				if (customData.FaceDamage != -nValue.int1)
				{
					playerWeaponData.FaceDamage = (int)customData.FaceDamage;
				}
				if (customData.BodyDamage != -nValue.int1)
				{
					playerWeaponData.BodyDamage = (int)customData.BodyDamage;
				}
				if (customData.HandDamage != -nValue.int1)
				{
					playerWeaponData.HandDamage = (int)customData.HandDamage;
				}
				if (customData.LegDamage != -nValue.int1)
				{
					playerWeaponData.LegDamage = (int)customData.LegDamage;
				}
				if (customData.Ammo != -nValue.int1)
				{
					playerWeaponData.Ammo = (int)customData.Ammo;
				}
				if (customData.AmmoMax != -nValue.int1)
				{
					playerWeaponData.AmmoMax = (int)customData.AmmoMax;
				}
				if (customData.AmmoTotal != -nValue.int1)
				{
					playerWeaponData.AmmoTotal = (int)customData.AmmoTotal;
				}
				if (customData.Mass != 0f - nValue.float1)
				{
					playerWeaponData.Mass = customData.Mass;
				}
				if (customData.Skin != -nValue.int1)
				{
					playerWeaponData.Skin = (int)customData.Skin;
				}
				if (customData.FireStat && customData.FireStatCounter >= 0)
				{
					playerWeaponData.FireStat = (int)customData.FireStatCounter;
				}
				if (customData.Stickers != null && customData.Stickers.Length > 0)
				{
					playerWeaponData.Stickers = customData.Stickers;
				}
			}
			playerWeaponData.Script.UpdateWeaponData(playerWeaponData);
		}
		else
		{
			playerWeaponData.Enabled = false;
		}
		switch (weaponType)
		{
		case WeaponType.Knife:
			KnifeData = playerWeaponData;
			break;
		case WeaponType.Pistol:
			PistolData = playerWeaponData;
			break;
		case WeaponType.Rifle:
			RifleData = playerWeaponData;
			break;
		}
	}

	public void SetWeapon(WeaponType weapon)
	{
		SetWeapon(weapon, true);
	}

	public void SetWeapon(WeaponType weapon, bool checkSelectedWeapon)
	{
		if ((!checkSelectedWeapon || SelectedWeapon != weapon) && GetWeaponData(weapon).Enabled)
		{
			DeactiveScope();
			DeactiveAll();
			SelectedWeapon = weapon;
			PlayerWeaponData selectedWeaponData = GetSelectedWeaponData();
			UICrosshair.SetAccuracy(selectedWeaponData.Accuracy);
			UIGameManager.SetAmmoLabel(selectedWeaponData.Ammo, selectedWeaponData.AmmoMax, InfiniteAmmo);
			UIControllerList.Aim.cachedGameObject.SetActive(selectedWeaponData.Scope || selectedWeaponData.Scope2);
			m_PlayerInput.SetPlayerSpeed(selectedWeaponData.Mass);
			Timer.Cancel(ReloadTimeID);
			isReload = false;
			Sounds.Stop();
			Timer.Cancel(WieldedTimeID);
			Wielded = true;
			WieldedTimeID = Timer.In(nValue.float05, delegate
			{
				Wielded = false;
			});
			switch (weapon)
			{
			case WeaponType.Knife:
				KnifeData.Script.Active();
				m_PlayerInput.Controller.SetWeapon(KnifeData);
				break;
			case WeaponType.Pistol:
				PistolData.Script.Active();
				m_PlayerInput.Controller.SetWeapon(PistolData);
				break;
			case WeaponType.Rifle:
				RifleData.Script.Active();
				m_PlayerInput.Controller.SetWeapon(RifleData);
				break;
			}
		}
	}

	private void DeactiveAll()
	{
		if (KnifeData.Script != null)
		{
			KnifeData.Script.Deactive();
		}
		if (PistolData.Script != null)
		{
			PistolData.Script.Deactive();
		}
		if (RifleData.Script != null)
		{
			RifleData.Script.Deactive();
		}
	}

	public void FireWeapon()
	{
		if (CanFire && !isReload && GameManager.GetRoundState() != RoundState.EndRound)
		{
			switch (SelectedWeapon)
			{
			case WeaponType.Knife:
				Fire(KnifeData);
				break;
			case WeaponType.Pistol:
				Fire(PistolData);
				break;
			case WeaponType.Rifle:
				Fire(RifleData);
				break;
			}
		}
	}

	private void Fire(PlayerWeaponData weapon)
	{
		if (Wielded || weapon.LastFire > Time.time || isUpdateWeaponData || !weapon.CanFire)
		{
			return;
		}
		if ((bool)weapon.Script.Knife.Enabled)
		{
			weapon.LastFire = Time.time + weapon.FireRate;
			Sounds.Play(weapon.FireSound);
			weapon.Script.Fire();
			Timer.In(weapon.Script.Knife.Delay, delegate
			{
				FireData(weapon);
			});
		}
		else if (weapon.Ammo > 0)
		{
			--weapon.Ammo;
			weapon.LastFire = Time.time + weapon.FireRate;
			Sounds.Play(weapon.FireSound);
			weapon.Script.Fire();
			FireData(weapon);
			if (isScope && weapon.Scope && weapon.ScopeRecoil != nValue.int0)
			{
				m_PlayerInput.FPCamera.Pitch -= weapon.ScopeRecoil;
				float num = weapon.ScopeRecoil / nValue.int2;
				m_PlayerInput.FPCamera.Yaw += UnityEngine.Random.Range(0f - num, num);
			}
		}
		else if (weapon.AmmoMax == 0)
		{
			DryFire(weapon);
		}
		else
		{
			Reload(weapon);
		}
	}

	private void FireData(PlayerWeaponData weapon)
	{
		UIGameManager.SetAmmoLabel(weapon.Ammo, weapon.AmmoMax, InfiniteAmmo);
		DecalInfo decalInfo = DecalInfo.Get();
		if (SelectedWeapon == WeaponType.Knife)
		{
			decalInfo.isKnife = true;
		}
		for (int i = nValue.int0; i < weapon.FireBullets; i++)
		{
			if (isScope && weapon.Scope)
			{
				FireAccuracy = UICrosshair.Fire(weapon.ScopeAccuracy);
			}
			else
			{
				FireAccuracy = UICrosshair.Fire(weapon.FireAccuracy);
			}
			FireAccuracy.x = UnityEngine.Random.Range(0f - FireAccuracy.x, FireAccuracy.x);
			FireAccuracy.y = UnityEngine.Random.Range(0f - FireAccuracy.y, FireAccuracy.y);
			FireRay = PlayerCamera.ViewportPointToRay(new Vector3(nValue.float05 + FireAccuracy.x, nValue.float05 + FireAccuracy.y, nValue.int0));
			if (nRaycast.RaycastFire(FireRay, weapon.Distance, "PlayerMain", "PlayerMain2", "Player", FireLayers))
			{
				if (CheckPlayerRaycast(nRaycast.hitPoint, weapon.Distance))
				{
					if (decalInfo.BloodDecal == nValue.int200)
					{
						decalInfo.BloodDecal = (byte)decalInfo.Points.Count;
						decalInfo.Points.Add(nRaycast.hitPoint);
						decalInfo.Normals.Add(Vector3.zero);
					}
					DamageInfo value = DamageInfo.Get(weapon.BodyDamage, m_PlayerInput.PlayerTransform.position, m_PlayerInput.PlayerTeam, weapon.ID, weapon.Skin, PhotonNetwork.player.ID, false);
					nRaycast.hitTransform.SendMessage("Damage", value, SendMessageOptions.DontRequireReceiver);
					if (!isScope)
					{
						SparkEffectManager.Fire(nRaycast.hitPoint, nRaycast.hitDistance);
					}
				}
			}
			else
			{
				if (!Physics.Raycast(FireRay, out FireRaycastHit, weapon.Distance, FireLayers) || !CheckPlayerRaycast(FireRaycastHit.point, weapon.Distance))
				{
					continue;
				}
				FireRayTag = FireRaycastHit.transform.tag;
				if (!(FireRayTag != "IgnoreDecal") || !(FireRayTag != "PlayerSkin"))
				{
					continue;
				}
				if (FireRayTag == "RigidbodyObject")
				{
					if (PushRigidbody)
					{
						FireRaycastHit.transform.GetComponent<RigidbodyObject>().Force(PlayerCamera.transform.forward * PushRigidbodyForce);
					}
					continue;
				}
				if (FireRayTag == "DamageObject")
				{
					DamageInfo value2 = DamageInfo.Get(weapon.BodyDamage, m_PlayerInput.PlayerTransform.position, m_PlayerInput.PlayerTeam, weapon.ID, weapon.Skin, PhotonNetwork.player.ID, false);
					FireRaycastHit.transform.SendMessage("Damage", value2, SendMessageOptions.DontRequireReceiver);
					continue;
				}
				if (FireRayTag == "PaintObject")
				{
					EventManager.Dispatch("Paint", FireRaycastHit);
					FireRaycastHit.transform.SendMessage("OnPaint", FireRaycastHit, SendMessageOptions.DontRequireReceiver);
					continue;
				}
				if (FireRayTag == "CreativeObject")
				{
					EventManager.Dispatch("Creative", FireRaycastHit);
					continue;
				}
				decalInfo.Points.Add(FireRaycastHit.point);
				decalInfo.Normals.Add(FireRaycastHit.normal);
				if (!isScope)
				{
					SparkEffectManager.Fire(FireRaycastHit.point, FireRaycastHit.distance);
				}
			}
		}
		m_PlayerInput.Controller.FireWeapon(decalInfo);
	}

	private bool CheckPlayerRaycast(Vector3 point, float distance)
	{
		if (nRaycast.RaycastCheck(new Ray(point, -FireRay.direction), m_PlayerInput.FPCamera.Transform, distance, "PlayerCamera", FireLayers))
		{
			return true;
		}
		Debug.DrawRay(point, -FireRay.direction, Color.red, 5f);
		return false;
	}

	private void DryFire(PlayerWeaponData weapon)
	{
		if (!isDryFire)
		{
			isDryFire = true;
			weapon.Script.DryFire();
			Sounds.Play(WeaponSound.AmmoEmpty);
		}
	}

	public void ReloadWeapon()
	{
		if (isReload)
		{
			return;
		}
		switch (SelectedWeapon)
		{
		case WeaponType.Knife:
			if (KnifeData.Script != null)
			{
				KnifeData.Script.StartInspectWeapon();
			}
			break;
		case WeaponType.Pistol:
			Reload(PistolData);
			break;
		case WeaponType.Rifle:
			Reload(RifleData);
			break;
		}
	}

	private void Reload(PlayerWeaponData weapon)
	{
		if (!weapon.Enabled || Wielded)
		{
			return;
		}
		if (isScope)
		{
			DeactiveScope();
		}
		if (weapon.Ammo == weapon.AmmoTotal || weapon.AmmoMax == nValue.int0)
		{
			weapon.Script.StartInspectWeapon();
			return;
		}
		isReload = true;
		Sounds.Play(weapon.ReloadSound);
		weapon.Script.Reload(weapon.ReloadTime);
		m_PlayerInput.Controller.ReloadWeapon();
		ReloadTimeID = Timer.In(weapon.ReloadTime + nValue.float05, delegate
		{
			isReload = false;
			if (InfiniteAmmo)
			{
				weapon.Ammo = weapon.AmmoTotal;
			}
			else if (weapon.AmmoMax > weapon.AmmoTotal)
			{
				PlayerWeaponData playerWeaponData = weapon;
				playerWeaponData.AmmoMax = playerWeaponData.AmmoMax - (weapon.AmmoTotal - weapon.Ammo);
				weapon.Ammo = weapon.AmmoTotal;
			}
			else
			{
				int num = weapon.Ammo;
				PlayerWeaponData playerWeaponData2 = weapon;
				playerWeaponData2.Ammo = playerWeaponData2.Ammo + weapon.AmmoMax;
				weapon.Ammo = Mathf.Min(weapon.AmmoTotal, weapon.Ammo);
				PlayerWeaponData playerWeaponData3 = weapon;
				playerWeaponData3.AmmoMax = playerWeaponData3.AmmoMax - (weapon.Ammo - num);
				weapon.AmmoMax = Mathf.Max(nValue.int0, weapon.AmmoMax);
			}
			UIGameManager.SetAmmoLabel(weapon.Ammo, weapon.AmmoMax, InfiniteAmmo);
		});
	}

	private void ScopeWeapon(bool check)
	{
		if ((check && isReload) || (!GetSelectedWeaponData().Scope && !GetSelectedWeaponData().Scope2 && check))
		{
			return;
		}
		isScope = !isScope;
		LODObject.isScope = isScope;
		DOTween.Kill("Scope");
		DOTween.Kill("Scope2");
		if (isScope)
		{
			if (GetSelectedWeaponData().Scope)
			{
				PlayerCamera.DOFieldOfView(GetSelectedWeaponData().ScopeSize, nValue.float02).id = "Scope";
				WeaponCamera.fieldOfView = nValue.int1;
				UICrosshair.SetActiveScope(true);
			}
			else if (GetSelectedWeaponData().Scope2)
			{
				PlayerCamera.DOFieldOfView(GetSelectedWeaponData().Scope2Size, nValue.float02).id = "Scope";
				WeaponCamera.DOFieldOfView(GetSelectedWeaponData().Scope2Size, nValue.float02).id = "Scope2";
			}
		}
		else
		{
			DOTween.Kill("Scope");
			DOTween.Kill("Scope2");
			PlayerCamera.DOFieldOfView(nValue.float60, nValue.float02).id = "Scope";
			if (GetSelectedWeaponData().Scope2)
			{
				WeaponCamera.DOFieldOfView(nValue.float60, nValue.float02).id = "Scope2";
			}
			else
			{
				WeaponCamera.fieldOfView = nValue.float60;
			}
			UICrosshair.SetActiveScope(false);
		}
		GetSelectedWeaponData().Script.ScopeRifle();
	}

	public void DeactiveScope()
	{
		if (isScope)
		{
			ScopeWeapon(false);
		}
		if (m_PlayerInput.Dead)
		{
			UICrosshair.SetActiveCrosshair(false);
		}
	}

	private void KillPlayer(DamageInfo damageInfo)
	{
		if (!LevelManager.CustomMap)
		{
			PlayerWeaponData playerWeaponData = null;
			if (PistolData.Enabled && PistolData.ID == damageInfo.WeaponID && PistolData.Script.FireStat.Enabled)
			{
				playerWeaponData = PistolData;
			}
			else if (RifleData.Enabled && RifleData.ID == damageInfo.WeaponID && RifleData.Script.FireStat.Enabled)
			{
				playerWeaponData = RifleData;
			}
			if (playerWeaponData != null && !playerWeaponData.CustomData)
			{
				int num = SaveLoadManager.GetFireStatCounter(damageInfo.WeaponID, damageInfo.WeaponSkinID) + nValue.int1;	
				playerWeaponData.Script.UpdateFireStat(num);
				PlayerRoundManager.SetFireStat1(damageInfo.WeaponID, damageInfo.WeaponSkinID);
				SaveLoadManager.SetFireStatCounter(damageInfo.WeaponID, damageInfo.WeaponSkinID, num);
				m_PlayerInput.Controller.UpdateFireStatValue(playerWeaponData.ID);
			}
		}
	}

	public void DropWeapon()
	{
		DropWeapon(SelectedWeapon, false);
	}

	public void DropWeapon(bool updateWeapon)
	{
		DropWeapon(SelectedWeapon, updateWeapon);
	}

	public void DropWeapon(WeaponType type)
	{
		DropWeapon(type, false);
	}

	public void DropWeapon(WeaponType type, bool updateWeapon)
	{
		if (type == WeaponType.Knife)
		{
			return;
		}
		PlayerWeaponData weaponData = GetWeaponData(type);
		if (weaponData.Enabled)
		{
			DropWeaponManager.CreateWeapon(false, (byte)(int)weaponData.ID, (byte)(int)weaponData.Skin, true, m_PlayerInput.PlayerTransform.position, m_PlayerInput.PlayerTransform.eulerAngles, StickersToByteArray(weaponData.Stickers), weaponData.FireStat, weaponData.Ammo, weaponData.AmmoMax);
			if (updateWeapon)
			{
				weaponData.Enabled = false;
				UpdateSelectWeapon();
			}
		}
	}

	private byte[] StickersToByteArray(CryptoInt[] stickers)
	{
		byte[] array = new byte[stickers.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (byte)(int)stickers[i];
		}
		return array;
	}
}
