using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerSkin : TimerBehaviour
{
	public bool isPlayerActive;

	public byte Health;

	public Team PlayerTeam;

	private bool StartDamage;

	public ControllerManager Controller;

	public PlayerAnimator PlayerAnimator;

	public RagdollObject PlayerRagdoll;

	public Transform[] PlayerLimbs;

	public MeshAtlas[] PlayerAtlas;

	public MeshAtlas PlayerHeadAtlas;

	public MeshAtlas[] PlayerBodyAtlas;

	public MeshAtlas[] PlayerLegsAtlas;

	public PlayerSkinIcon PlayerIcon;

	public Transform[] PlayerWeaponContainers;

	public Transform PlayerWeaponRoot;

	public Transform PlayerTwoWeaponRoot;

	public AudioClip[] PlayerFoosteps;

	public AudioSource cachedAudioSource;

	public Transform PlayerTransform;

	public Transform PlayerSpectatePoint;

	public GameObject PlayerRoot;

	public nTimer Timer;

	public PlayerSounds Sounds;

	private bool Sound = true;

	private bool ShowDamage;

	public float PhotonSpeed = 10f;

	public Vector3 PhotonPosition = Vector3.zero;

	public Quaternion PhotonRotation = Quaternion.identity;

	public bool Dead;

	private float Move;

	public float Rotate;

	private Tweener RotateTween;

	private bool Foostep;

	private bool mVisible;

	public int HeadSkin = -1;

	public int BodySkin = -1;

	public int LegsSkin = -1;

	private bool activeRagdoll;

	public TPWeaponShooter SelectWeapon;

	private TPWeaponShooter ContainerWeapon1;

	private TPWeaponShooter ContainerWeapon2;

	private TPWeaponShooter ContainerWeapon3;

	private List<TPWeaponShooter> WeaponsList = new List<TPWeaponShooter>();

	public bool visible
	{
		get
		{
			return mVisible;
		}
	}

	private void Start()
	{
		Controller = PlayerTransform.root.GetComponent<ControllerManager>();
		Timer.In(0.15f, true, CheckPosition);
		Timer.In(0.12f, true, UpdateMove);
		EventManager.AddListener("UpdateSettings", UpdateSettings);
		UpdateSettings();
		RotateTween = DOTween.To(() => Rotate, delegate(float x)
		{
			Rotate = x;
		}, 0f, 0.15f).SetAutoKill(false).SetUpdate(UpdateType.Late)
			.OnUpdate(UpdateRotate)
			.SetEase(Ease.Linear);
	}

	private void UpdateSettings()
	{
		Sound = Settings.Sound;
		ShowDamage = Settings.ShowDamage;
	}

	public void OnEnableRoot()
	{
		Foostep = false;
		isPlayerActive = true;
		PlayerAnimator.SetDefault();
		RotateTween.Pause();
	}

	public void OnDisableRoot()
	{
		isPlayerActive = false;
		RotateTween.Play();
	}

	private void CheckPosition()
	{
		if (visible && !Dead && (PlayerTransform.localPosition - PhotonPosition).sqrMagnitude > 3f)
		{
			PlayerTransform.localPosition = PhotonPosition;
		}
	}

	private void LateUpdate()
	{
		if (visible)
		{
			if (PlayerTransform.localPosition != PhotonPosition)
			{
				PlayerTransform.localPosition = Vector3Lerp(PlayerTransform.localPosition, PhotonPosition, Time.deltaTime * PhotonSpeed);
			}
			if (PlayerTransform.localRotation != PhotonRotation)
			{
				PlayerTransform.localRotation = Quaternion.Lerp(PlayerTransform.localRotation, PhotonRotation, Time.deltaTime * PhotonSpeed);
			}
		}
		else
		{
			if (PlayerTransform.localPosition != PhotonPosition)
			{
				PlayerTransform.localPosition = PhotonPosition;
			}
			if (PlayerTransform.localRotation != PhotonRotation)
			{
				PlayerTransform.localRotation = PhotonRotation;
			}
		}
	}

	private Vector3 Vector3Lerp(Vector3 from, Vector3 to, float t)
	{
		from.x += (to.x - from.x) * t;
		from.y += (to.y - from.y) * t;
		from.z += (to.z - from.z) * t;
		return from;
	}

	private void UpdateMove()
	{
		if (isPlayerActive)
		{
			if (visible)
			{
				PlayerAnimator.move = Move;
			}
			if (!Foostep && Sound && Mathf.Abs(Move) >= 0.6f)
			{
				UpdateFoosteps();
			}
		}
	}

	private void UpdateRotate()
	{
		PlayerAnimator.rotate = Rotate;
	}

	public void SetMove(float move)
	{
		Move = move;
	}

	public float GetMove()
	{
		return Move;
	}

	public void SetRotate(float rotate)
	{
		if (visible && !Dead)
		{
			if (RotateTween != null)
			{
				RotateTween.ChangeStartValue(Rotate);
				RotateTween.ChangeEndValue(rotate).Restart();
			}
		}
		else
		{
			Rotate = rotate;
			UpdateRotate();
		}
	}

	public void SetGrounded(bool grounded)
	{
		if (isPlayerActive)
		{
			PlayerAnimator.grounded = grounded;
		}
	}

	public void SetPlayerVisible(bool visible)
	{
		mVisible = visible;
		if (!visible)
		{
			PlayerAnimator.move = 0f;
			RotateTween.Pause();
		}
		else
		{
			RotateTween.Play();
		}
	}

	public void SetWeapon(byte[] weapons, int fireStat, byte[] stickers)
	{
		WeaponData weaponData = WeaponManager.GetWeaponData(weapons[0]);
		PlayerAnimator.SetWeapon(weaponData.Animation);
		if (SelectWeapon != null && SelectWeapon.name == weaponData.Name)
		{
			return;
		}
		if (SelectWeapon != null)
		{
			SelectWeapon.Deactive();
		}
		TPWeaponShooter tPWeaponShooter = ContainsWeapon(weaponData.Name);
		if (tPWeaponShooter == null)
		{
			GameObject gameObject = Utils.AddChild(weaponData.TpsPrefab, PlayerWeaponRoot, weaponData.TpsPrefab.transform.position, weaponData.TpsPrefab.transform.rotation);
			SelectWeapon = gameObject.GetComponent<TPWeaponShooter>();
			SelectWeapon.name = weaponData.Name;
			WeaponsList.Add(SelectWeapon);
			SelectWeapon.Init(weaponData.ID, weapons[1], this, false);
			if (fireStat > 0)
			{
				SelectWeapon.SetFireStat(fireStat);
			}
			SelectWeapon.SetStickers(stickers);
		}
		else
		{
			SelectWeapon = tPWeaponShooter;
			SelectWeapon.WeaponSkin = weapons[1];
			SelectWeapon.UpdateSkin();
			if (fireStat > 0)
			{
				SelectWeapon.UpdateFireStat(fireStat);
			}
			SelectWeapon.SetStickers(stickers);
		}
		SelectWeapon.Active();
		Sounds.Stop();
	}

	private void ClearCointainers()
	{
		if (ContainerWeapon1 != null)
		{
			ContainerWeapon1.Deactive();
			ContainerWeapon1 = null;
		}
		if (ContainerWeapon2 != null)
		{
			ContainerWeapon2.Deactive();
			ContainerWeapon2 = null;
		}
		if (ContainerWeapon3 != null)
		{
			ContainerWeapon3.Deactive();
			ContainerWeapon3 = null;
		}
	}

	private void SetContainerWeapon(int weaponID, int skin)
	{
		if (weaponID == 0)
		{
			return;
		}
		WeaponData weaponData = WeaponManager.GetWeaponData(weaponID);
		print(SelectWeapon.WeaponID + " == " + weaponData.ID);
		if (WeaponManager.GetWeaponData(SelectWeapon.WeaponID).Type == weaponData.Type || SelectWeapon.WeaponID == weaponID)
		{
			print("Ignore");
		}
		else
		{
			if ((weaponData.Type == WeaponType.Knife && ContainerWeapon1 != null) || (weaponData.Type == WeaponType.Pistol && ContainerWeapon2 != null) || (weaponData.Type == WeaponType.Rifle && ContainerWeapon3 != null))
			{
				return;
			}
			TPWeaponShooter tPWeaponShooter = ContainsWeapon(weaponData.Name);
			if (tPWeaponShooter == null)
			{
				GameObject gameObject = Utils.AddChild(weaponData.TpsPrefab, PlayerWeaponRoot, weaponData.TpsPrefab.transform.position, weaponData.TpsPrefab.transform.rotation);
				tPWeaponShooter = gameObject.GetComponent<TPWeaponShooter>();
				tPWeaponShooter.name = weaponData.Name;
				WeaponsList.Add(tPWeaponShooter);
				SelectWeapon.Init(weaponData.ID, skin, this, true);
			}
			else
			{
				tPWeaponShooter.WeaponSkin = skin;
				tPWeaponShooter.UpdateSkin();
			}
			switch (weaponData.Type)
			{
			case WeaponType.Knife:
				if (tPWeaponShooter.GetCachedTransform().parent != PlayerWeaponContainers[0])
				{
					tPWeaponShooter.GetCachedTransform().SetParent(PlayerWeaponContainers[0]);
				}
				ContainerWeapon1 = tPWeaponShooter;
				break;
			case WeaponType.Pistol:
				if (tPWeaponShooter.GetCachedTransform().parent != PlayerWeaponContainers[1])
				{
					tPWeaponShooter.GetCachedTransform().SetParent(PlayerWeaponContainers[1]);
				}
				ContainerWeapon2 = tPWeaponShooter;
				break;
			case WeaponType.Rifle:
				if (tPWeaponShooter.GetCachedTransform().parent != PlayerWeaponContainers[2])
				{
					tPWeaponShooter.GetCachedTransform().SetParent(PlayerWeaponContainers[2]);
				}
				ContainerWeapon3 = tPWeaponShooter;
				break;
			}
			tPWeaponShooter.SetContainer(true);
			tPWeaponShooter.Active();
		}
	}

	public void SetWeapon(WeaponData weaponType, int skinID, int fireStat, byte[] stickers)
	{
		PlayerAnimator.SetWeapon(weaponType.Animation);
		if (SelectWeapon != null && SelectWeapon.name == weaponType.Name)
		{
			return;
		}
		if (SelectWeapon != null)
		{
			SelectWeapon.Deactive();
		}
		TPWeaponShooter tPWeaponShooter = ContainsWeapon(weaponType.Name);
		if (tPWeaponShooter == null)
		{
			GameObject gameObject = Utils.AddChild(weaponType.TpsPrefab, PlayerWeaponRoot, weaponType.TpsPrefab.transform.position, weaponType.TpsPrefab.transform.rotation);
			SelectWeapon = gameObject.GetComponent<TPWeaponShooter>();
			SelectWeapon.name = weaponType.Name;
			WeaponsList.Add(SelectWeapon);
			SelectWeapon.Init(weaponType.ID, skinID, this, false);
			if (fireStat > 0)
			{
				SelectWeapon.SetFireStat(fireStat);
			}
			SelectWeapon.SetStickers(stickers);
		}
		else
		{
			SelectWeapon = tPWeaponShooter;
			SelectWeapon.WeaponSkin = skinID;
			SelectWeapon.UpdateSkin();
			if (fireStat > 0)
			{
				SelectWeapon.UpdateFireStat(fireStat);
			}
			SelectWeapon.SetStickers(stickers);
		}
		SelectWeapon.Active();
		Sounds.Stop();
	}

	private TPWeaponShooter ContainsWeapon(string weaponName)
	{
		for (int i = 0; i < WeaponsList.Count; i++)
		{
			if (weaponName == WeaponsList[i].name)
			{
				return WeaponsList[i];
			}
		}
		return null;
	}

	public void Fire(DecalInfo decalInfo)
	{
		if (SelectWeapon != null)
		{
			SelectWeapon.Fire(visible, decalInfo);
		}
		DecalsManager.FireWeapon(decalInfo);
	}

	public void Reload()
	{
		if (SelectWeapon != null)
		{
			PlayerAnimator.reload = true;
			SelectWeapon.Reload();
		}
	}

	public void Damage(DamageInfo damageInfo)
	{
		if (!Dead && (PlayerTeam != damageInfo.AttackerTeam || GameManager.GetFriendDamage()) && !StartDamage)
		{
			if (ShowDamage)
			{
				UIToast.Show(Localization.Get("Damage") + ": " + damageInfo.Damage, 2f);
			}
			UICrosshair.Hit();
			Controller.Damage(damageInfo);
		}
	}

	public void ActiveRagdoll(Vector3 force, bool head)
	{
		if (visible)
		{
			if (force.magnitude < 0.5f)
			{
				force = new Vector3(0f, 0f, Random.Range(-1, 1));
			}
			Sounds.Stop();
			if (!activeRagdoll)
			{
				Timer.In(2f, delegate
				{
					if (Dead)
					{
						if (activeRagdoll)
						{
							DeactiveRagdoll();
						}
					}
					else
					{
						DeactiveRagdoll();
					}
				});
			}
			activeRagdoll = true;
			if (SelectWeapon != null)
			{
				SelectWeapon.SetParent(PlayerRagdoll.playerWeaponRoot, PlayerRagdoll.player2WeaponRoot);
			}
			PlayerRagdoll.Active(force, PlayerLimbs);
			PlayerRoot.SetActive(false);
		}
		else
		{
			if (activeRagdoll)
			{
				DeactiveRagdoll();
			}
			PlayerRoot.SetActive(false);
		}
	}

	public void DeactiveRagdoll()
	{
		if (SelectWeapon != null)
		{
			SelectWeapon.SetParent(PlayerWeaponRoot, PlayerTwoWeaponRoot);
		}
		activeRagdoll = false;
		PlayerRagdoll.Deactive();
	}

	private void UpdateFoosteps()
	{
		Foostep = true;
		cachedAudioSource.pitch = Random.Range(1f, 1.5f);
		cachedAudioSource.clip = PlayerFoosteps[Random.Range(0, PlayerFoosteps.Length)];
		cachedAudioSource.Play();
		Timer.In(0.3f, FinishFoosteps);
	}

	private void FinishFoosteps()
	{
		Foostep = false;
	}

	public void SetPosition(Vector3 pos)
	{
		PhotonPosition = pos;
		PlayerTransform.localPosition = PhotonPosition;
	}

	public void SetRotation(Vector3 rot)
	{
		PhotonRotation = Quaternion.Euler(rot);
		PlayerTransform.localRotation = PhotonRotation;
	}

	public void SetSkin(int head, int body, int legs)
	{
		HeadSkin = head;
		BodySkin = body;
		LegsSkin = legs;
	}

	public void UpdateSkin()
	{
		UIAtlas atlas = ((PlayerTeam != Team.Blue) ? GameSettings.instance.PlayerAtlasRed : GameSettings.instance.PlayerAtlasBlue);
		if (HeadSkin == 99)
		{
			atlas = GameSettings.instance.PlayerAtlasRed;
		}
		Timer.In(0.01f, delegate
		{
			PlayerHeadAtlas.atlas = atlas;
			PlayerHeadAtlas.spriteName = "0-" + HeadSkin;
			for (int i = 0; i < PlayerBodyAtlas.Length; i++)
			{
				PlayerBodyAtlas[i].atlas = atlas;
				PlayerBodyAtlas[i].spriteName = "1-" + BodySkin;
			}
			for (int j = 0; j < PlayerLegsAtlas.Length; j++)
			{
				PlayerLegsAtlas[j].atlas = atlas;
				PlayerLegsAtlas[j].spriteName = "2-" + LegsSkin;
			}
		});
		Timer.In(Random.Range(0.1f, 0.12f), delegate
		{
			PlayerRagdoll.SetSkin(atlas, HeadSkin.ToString(), BodySkin.ToString(), LegsSkin.ToString());
		});
	}

	public void StartDamageTime()
	{
		if (GameManager.isStartDamage())
		{
			StartDamage = true;
			Timer.In(GameManager.GetStartDamageTime(), delegate
			{
				StartDamage = false;
			});
		}
	}
}
