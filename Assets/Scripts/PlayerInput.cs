using System;
using DG.Tweening;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
	public static int PlayerHelperID = -1;

	public CryptoInt3 Health = 100;

	public Team PlayerTeam;

	public CryptoFloat PlayerSpeed = 0.18f;

	public bool isAwake;

	public bool Dead = true;

	public bool NoDamage;

	public bool Move = true;

	public bool Zombie;

	public bool DamageSpeed;

	public CryptoInt DamageForce = 0;

	public bool MoveIce;

	public CryptoInt3 MaxHealth = 100;

	public bool Climb;

	public bool Water;

	[Header("UFPS")]
	public vp_FPController FPController;

	public vp_FPCamera FPCamera;

	[Header("Player")]
	public Transform PlayerTransform;

	public Camera PlayerCamera;

	public Camera PlayerWeaponCamera;

	public PlayerWeapons PlayerWeapon;

	public ControllerManager Controller;

	public AudioClip[] PlayerFoosteps;

	[Disabled]
	public Vector2 MoveAxis;

	[Disabled]
	public Vector2 LookAxis;

	[Disabled]
	public float RotateCamera;

	[Header("Fall Damage")]
	public bool FallDamage;

	public CryptoFloat FallDamageThreshold = 10f;

	private bool FallingDamage;

	private float StartFallDamage;

	[Header("AFK")]
	public bool AfkEnabled;

	public float AfkDuration = 40f;

	private float AfkTimer = -1f;

	[Header("Bunny Hop")]
	public bool BunnyHopEnabled;

	public CryptoFloat BunnyHopSpeed = 0.4f;

	public CryptoFloat BunnyHopLerp = 5f;

	public CryptoFloat BunnyHopDefaultLerp = 0.5f;

	public CryptoFloat BunnyHopDefaultSpeed = 0.18f;

	private bool BunnyHopActive;

	private bool BunnyHopAutoJump;

	[Header("Surf")]
	public bool SurfEnabled;

	public CryptoFloat SurfAcceleration = 2f;

	public CryptoFloat SurfMaxSpeed = 120f;

	private CryptoFloat SurfSpeed;

	private bool Surf;

	private bool isStopSurf;

	[Header("Others")]
	public AudioSource m_AudioSource;

	public nTimer Timer;

	private bool isJump;

	private int MoveTimerID;

	private bool shift;

	private byte GroundedDetect;

	private bool isJet;

	public static PlayerInput instance;

	public bool Grounded
	{
		get
		{
			if (Water || Surf)
			{
				return false;
			}
			return mCharacterController.isGrounded;
		}
	}

	public CharacterController mCharacterController
	{
		get
		{
			return FPController.mCharacterController;
		}
	}

	private void Start()
	{
		instance = this;
		isAwake = true;
		Controller = transform.root.GetComponent<ControllerManager>();
		SetHealth(Health);
		EventManager.AddListener("UpdateSettings", UpdateSettings);
		UpdateSettings();
		Timer.In(nValue.int3, true, CheckController);
		Timer.In(nValue.int1, true, CheckCamera);
		Timer.In(nValue.int2, true, UpdateValue);
		BunnyAutoJump[] jumpTriggers = new BunnyAutoJump[100000];
		jumpTriggers = FindObjectsOfType<BunnyAutoJump>();
		for (int i = 0; i < jumpTriggers.Length; i++)
		{
			jumpTriggers[i].gameObject.layer = 2;
		}
		if (PhotonNetwork.room.GetSceneName() != "Jet")
		{
			for (int i = 0; i < FindObjectsOfType<DeadTrigger>().Length; i++)
			{
				FindObjectsOfType<DeadTrigger>()[i].gameObject.layer = 2;
			}
			isJet = false;
		}
		else
		{
			isJet = true;
		}
		for (int i = 0; i < FindObjectsOfType<KingHillTrigger>().Length; i++)
		{
			FindObjectsOfType<KingHillTrigger>()[i].gameObject.layer = 2;
		}
		for (int i = 0; i < FindObjectsOfType<TrapTrigger>().Length; i++)
		{
			FindObjectsOfType<TrapTrigger>()[i].gameObject.layer = 2;
		}
		for (int i = 0; i < FindObjectsOfType<ClimbSystem>().Length; i++)
		{
			FindObjectsOfType<ClimbSystem>()[i].gameObject.layer = 2;
		}
		for (int i = 0; i < FindObjectsOfType<WaterSystem>().Length; i++)
		{
			FindObjectsOfType<WaterSystem>()[i].gameObject.layer = 2;
		}
		if (PhotonNetwork.room.GetSceneName() == "Football")
		{
			Physics.IgnoreCollision(GameObject.Find("Ball").GetComponent<MeshCollider>(), instance.mCharacterController);
		}
	}

	[ContextMenu("UpdateValue")]
	private void UpdateValue()
	{
		Health.UpdateValue();
		PlayerSpeed.UpdateValue();
		DamageForce.UpdateValue();
		MaxHealth.UpdateValue();
		FallDamageThreshold.UpdateValue();
		BunnyHopSpeed.UpdateValue();
		BunnyHopLerp.UpdateValue();
		BunnyHopDefaultLerp.UpdateValue();
		BunnyHopDefaultSpeed.UpdateValue();
		SurfAcceleration.UpdateValue();
		SurfMaxSpeed.UpdateValue();
		SurfSpeed.UpdateValue();
	}

	private void OnEnable()
	{
		PlayerHelperID = -1;
		LODObject.Target = PlayerCamera.transform;
		vp_FPCamera fPCamera = FPCamera;
		fPCamera.BobStepCallback = (vp_FPCamera.BobStepDelegate)Delegate.Combine(fPCamera.BobStepCallback, new vp_FPCamera.BobStepDelegate(PlayFoosteps));
		InputManager.GetButtonDownEvent += GetButtonDown;
		InputManager.GetButtonUpEvent += GetButtonUp;
		InputManager.GetAxisEvent += GetAxis;
		if (GameManager.isStartDamage())
		{
			StartNoDamage();
		}
		if (Climb)
		{
			SetClimb(false);
		}
		if (Water)
		{
			SetWater(false);
		}
		if (MoveIce)
		{
			SetMoveIce(false);
		}
		Dead = false;
		nConsole.AddCommand("realtime_light", nValueType.Bool, OffLight);
		nConsole.AddCommand("shift_controller", nValueType.Bool, OnShiftController);
		TimerManager.In(0.05f, delegate
		{
			InputManager.instance.isCursor = false;
		});
	}

	private void OnDisable()
	{
		vp_FPCamera fPCamera = FPCamera;
		fPCamera.BobStepCallback = (vp_FPCamera.BobStepDelegate)Delegate.Remove(fPCamera.BobStepCallback, new vp_FPCamera.BobStepDelegate(PlayFoosteps));
		InputManager.GetButtonDownEvent -= GetButtonDown;
		InputManager.GetButtonUpEvent -= GetButtonUp;
		InputManager.GetAxisEvent -= GetAxis;
		MoveAxis = Vector2.zero;
		LookAxis = Vector2.zero;
		BunnyHopAutoJump = false;
		SurfSpeed = nValue.float0;
		Dead = true;
		isJump = false;
		nConsole.Remove("realtime_light");
		nConsole.Remove("shift_controller");
	}

	private void OffLight(string command, object value)
	{
		if (command != "realtime_light")
		{
			return;
		}
		GameObject gameObject = GameObject.Find("Directional light realtime");
		if ((bool)value)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(true);
			}
			GameSettings.instance.PlayerAtlasBlue.spriteMaterial.shader = Shader.Find("Mobile/VertexLit (Only Directional Lights)");
			GameSettings.instance.PlayerAtlasRed.spriteMaterial.shader = Shader.Find("Mobile/VertexLit (Only Directional Lights)");
		}
		else
		{
			if (gameObject != null)
			{
				gameObject.SetActive(false);
			}
			GameSettings.instance.PlayerAtlasBlue.spriteMaterial.shader = Shader.Find("MADFINGER/Diffuse/Simple");
			GameSettings.instance.PlayerAtlasRed.spriteMaterial.shader = Shader.Find("MADFINGER/Diffuse/Simple");
		}
		gameObject = null;
	}

	private void GetButtonDown(string name)
	{
		if (name == "Jump")
		{
			isJump = true;
		}
	}

	private void GetButtonUp(string name)
	{
		if (name == "Jump")
		{
			isJump = false;
		}
	}

	private void GetAxis(string name, float value)
	{
		switch (name)
		{
		case "Horizontal":
			MoveAxis.x = value;
			break;
		case "Vertical":
			MoveAxis.y = value;
			break;
		case "Mouse X":
			LookAxis.x = value;
			break;
		case "Mouse Y":
			LookAxis.y = value;
			break;
		}
	}

    private void FixedUpdate()
    {
		if (mCharacterController.isGrounded != FPController.Grounded)
		{
			MoveAxis = Vector2.zero;
			LookAxis = Vector2.zero;
		}
		UpdateMove();
		UpdateJump();
		UpdateBunnyHop();
		UpdateSurf();
		UpdateFallDamage();
		UpdateVelocity();
		UpdateAFK();
		if (isJet)
		{
			SurfTriggerUpdate();
		}
	}

    private void Update()
    {
		UpdateLook();
	}

    private void SurfTriggerUpdate()
	{
		RaycastHit hit;
		if (Physics.Raycast(gameObject.transform.position, gameObject.transform.position + new Vector3(0, -1, 0), out hit, 1f))
		{
			Debug.DrawLine(transform.position, hit.collider.gameObject.transform.position, Color.red);
			if (hit.collider.gameObject.name == "Floor" && !Dead)
			{
				DamageInfo damageInfo = DamageInfo.Get(nValue.int1000, Vector3.zero, Team.None, nValue.int0, nValue.int0, -(int)nValue.int1, false);
				Damage(damageInfo);
			}
		}
	}

	private void UpdateMove()
	{
		if (Move)
		{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
			float g = 0;
			float h = 0;
			if (Input.GetKey(KeyCode.W))
			{
				g = 1;
			}
			if (Input.GetKey(KeyCode.S))
			{
				g = -1;
			}
			if (Input.GetKey(KeyCode.A))
			{
				h = -1;
			}
			if (Input.GetKey(KeyCode.D))
			{
				h = 1;
			}
			MoveAxis = new Vector2(h, g).normalized;
#endif
			if (Input.GetKey(KeyCode.LeftShift))
			{
				MoveAxis /= 1.75f;
			}
			if (PlayerWeapon.isScope && PlayerWeapon.GetSelectedWeaponData().Scope2)
			{
				MoveAxis /= nValue.int2;
			}
			FPController.OnValue_InputMoveVector = MoveAxis;
		}
	}

	private void UpdateLook()
	{
		if (PlayerWeapon.isScope)
		{
			LookAxis *= ((!PlayerWeapon.GetSelectedWeaponData().Scope) ? PlayerWeapon.GetSelectedWeaponData().Scope2Sensitivity : PlayerWeapon.GetSelectedWeaponData().ScopeSensitivity);
		}
		FPCamera.UpdateLook(LookAxis);
		RotateCamera = (0f - FPCamera.Pitch) / nValue.float60;
	}

	private void UpdateJump()
	{
		if (BunnyHopAutoJump)
		{
			if (Grounded)
			{
				if (FPController.CanStartJump())
				{
					FPController.OnStartJump();
				}
			}
			else
			{
				FPController.OnStopJump();
			}
		}
		else if (isJump && !Climb && !Water)
		{
			if (FPController.CanStartJump())
			{
				FPController.OnStartJump();
			}
		}
		else
		{
			FPController.OnStopJump();
		}
	}

	private void UpdateBunnyHop()
	{
		if (!BunnyHopEnabled)
		{
			return;
		}
		if (!Grounded && mCharacterController.velocity.sqrMagnitude > nValue.int20)
		{
			if (!BunnyHopActive)
			{
				BunnyHopActive = true;
				DOTween.Kill("BunnyHop");
				DOTween.To(() => FPController.MotorAcceleration, delegate(float x)
				{
					FPController.MotorAcceleration = x;
				}, BunnyHopSpeed, BunnyHopLerp).SetId("BunnyHop");
			}
		}
		else if (BunnyHopActive)
		{
			BunnyHopActive = false;
			DOTween.Kill("BunnyHop");
			DOTween.To(() => FPController.MotorAcceleration, delegate(float x)
			{
				FPController.MotorAcceleration = x;
			}, BunnyHopDefaultSpeed, BunnyHopDefaultLerp).SetId("BunnyHop");
		}
	}

	private void UpdateSurf()
	{
		if (!SurfEnabled)
		{
			return;
		}
		if (isStopSurf)
		{
			Surf = false;
			SurfSpeed = nValue.float0;
			FPController.Stop();
			isStopSurf = false;
			return;
		}
		if (FPController.GroundAngle > nValue.int30 && mCharacterController.isGrounded)
		{
			if (!Surf)
			{
				SurfSpeed += (SurfAcceleration + mCharacterController.velocity.magnitude * SurfAcceleration);
			}
			else
			{
				SurfSpeed += SurfAcceleration;
			}
			Surf = true;
		}
		else if (FPController.GroundAngle < nValue.int30 && mCharacterController.isGrounded)
		{
			Surf = false;
			SurfSpeed = nValue.float0;
		}
		else if (SurfSpeed > nValue.float0)
		{
			Surf = false;
			SurfSpeed -= SurfAcceleration / nValue.int3;
		}
		SurfSpeed = Mathf.Clamp(SurfSpeed, nValue.float0, SurfMaxSpeed);
		if (SurfSpeed > nValue.float0)
		{
			FPController.AddForce(FPCamera.Forward * (SurfSpeed * nValue.float00001 + MoveAxis.y / nValue.int100));
		}
	}

	private void UpdateFallDamage()
	{
		if (!FallDamage)
		{
			return;
		}
		if (Grounded)
		{
			if (FallingDamage)
			{
				FallingDamage = false;
				if (PlayerTransform.position.y < StartFallDamage - FallDamageThreshold)
				{
					int damage = (int)(StartFallDamage - PlayerTransform.position.y);
					DamageInfo damageInfo = DamageInfo.Get(damage, Vector3.zero, Team.None, nValue.int0, nValue.int0, -nValue.int1, false);
					Damage(damageInfo);
				}
			}
		}
		else if (!FallingDamage)
		{
			FallingDamage = true;
			StartFallDamage = PlayerTransform.position.y;
		}
	}

	private void UpdateVelocity()
	{
		if (mCharacterController.velocity.y < -nValue.int100)
		{
			DamageInfo damageInfo = DamageInfo.Get(nValue.int1000, Vector3.zero, Team.None, nValue.int0, nValue.int0, -nValue.int1, false);
			Damage(damageInfo);
		}
	}

	public void SetBunnyHopAutoJump(bool active)
	{
		BunnyHopAutoJump = active;
	}

	public void Damage(DamageInfo damageInfo)
	{
		if (Dead || NoDamage)
		{
			damageInfo.Dispose();
			return;
		}
		SetHealth(Health - damageInfo.Damage);
		if (damageInfo.AttackPosition != Vector3.zero)
		{
			UIDamage.Damage(damageInfo.AttackPosition, FPCamera.Transform);
			if (DamageForce > nValue.int0)
			{
				float num = nValue.int1 - Vector3.Distance(PlayerTransform.position, damageInfo.AttackPosition) / nValue.int100;
				Vector3 force = (damageInfo.AttackPosition - PlayerTransform.position).normalized * num * -DamageForce / nValue.int100;
				force.y = nValue.float0;
				FPController.AddForce(force);
			}
		}
		if (Health <= nValue.int0)
		{
			GameManager.OnDeadPlayer(damageInfo);
			PlayerWeapon.DeactiveScope();
		}
		else
		{
			PlayerHelperID = damageInfo.PlayerID;
			if (DamageSpeed)
			{
				FPController.MotorAcceleration = nValue.float013;
				if (DOTween.IsTweening("DamageSpeed"))
				{
					DOTween.Kill("DamageSpeed");
				}
				DOTween.To(() => FPController.MotorAcceleration, delegate(float x)
				{
					FPController.MotorAcceleration = x;
				}, nValue.float019, nValue.float15).SetId("DamageSpeed");
			}
			FPCamera.AddRollForce(UnityEngine.Random.Range(-nValue.int2, nValue.int2));
		}
		damageInfo.Dispose();
	}

	private void PlayFoosteps()
	{
		if (!Water && !Climb && Grounded)
		{
			UpdateFoosteps();
		}
	}

	public void UpdateFoosteps()
	{
		if (Settings.Sound)
		{
			AudioClip clip = PlayerFoosteps[UnityEngine.Random.Range(nValue.int0, PlayerFoosteps.Length)];
			m_AudioSource.pitch = UnityEngine.Random.Range(1f, 1.5f);
			m_AudioSource.clip = clip;
			m_AudioSource.Play();
			clip = null;
		}
	}

	public void StartNoDamage()
	{
		NoDamage = true;
		try
		{
			float startDamageTime = GameManager.GetStartDamageTime();
			if (startDamageTime != -nValue.int1)
			{
				Timer.In(GameManager.GetStartDamageTime(), delegate
				{
					NoDamage = false;
				});
			}
		}
		catch
		{
			NoDamage = false;
		}
	}

	private void UpdateSettings()
	{
		Timer.In(nValue.float01, delegate
		{
			float num = Settings.Sensitivity * 16f;
			FPCamera.MouseSensitivity = new Vector2(num, num);
			PlayerWeaponCamera.enabled = Settings.ShowWeapon;
		});
	}

	public void SetHealth(int health)
	{
		Health = health;
		Health = Mathf.Clamp(Health, nValue.int0, MaxHealth);
		UIGameManager.SetHealthLabel(Health);
		Controller.SetHealth((byte)health);
	}

	public void SetMove(bool move)
	{
		Move = move;
		if (!move)
		{
			MoveAxis = Vector2.zero;
			FPController.OnValue_InputMoveVector = MoveAxis;
		}
	}

	public void SetMove(bool move, float duration)
	{
		Move = move;
		if (Timer.isActive(MoveTimerID))
		{
			Timer.Cancel(MoveTimerID);
		}
		MoveTimerID = Timer.In(duration, delegate
		{
			Move = !move;
		});
	}

	public void SetMoveIce(bool active)
	{
		MoveIce = active;
		if (active)
		{
			DOTween.To(() => FPController.MotorDamping, delegate(float x)
			{
				FPController.MotorDamping = x;
			}, nValue.float002, nValue.float02);
		}
		else
		{
			DOTween.To(() => FPController.MotorDamping, delegate(float x)
			{
				FPController.MotorDamping = x;
			}, nValue.float017, nValue.float02);
		}
	}

	public void UpdatePlayerSpeed(float speed)
	{
		PlayerSpeed = speed;
		FPController.MotorAcceleration = (float)PlayerSpeed;
	}

	public void SetPlayerSpeed(float mass)
	{
		FPController.MotorAcceleration = PlayerSpeed - mass;
	}

	public void SetClimb(bool active)
	{
		Climb = active;
		if (Climb)
		{
			FPController.Stop();
			FPController.PhysicsGravityModifier = nValue.int0;
			FPController.MotorFreeFly = true;
		}
		else
		{
			FPController.PhysicsGravityModifier = nValue.float02;
			FPController.MotorFreeFly = false;
		}
	}

	public void SetWater(bool active)
	{
		SetWater(active, false);
	}

	public void SetWater(bool active, bool freeGravity)
	{
		Water = active;
		if (Water)
		{
			if (freeGravity)
			{
				FPController.Stop();
			}
			FPController.PhysicsGravityModifier = nValue.float001;
			FPController.MotorFreeFly = true;
		}
		else
		{
			FPController.PhysicsGravityModifier = nValue.float02;
			FPController.MotorFreeFly = false;
		}
	}

	public void StopSurf()
	{
		if (Surf)
		{
			isStopSurf = true;
		}
	}

	private void CheckController()
	{
		if (Dead)
		{
			return;
		}
		if (mCharacterController.slopeLimit != nValue.float45)
		{
			CheckManager.Detected("Controller Error 1");
		}
		if (mCharacterController.stepOffset != nValue.float03)
		{
			CheckManager.Detected("Controller Error 2");
		}
		if (mCharacterController.center.x != nValue.int0 && mCharacterController.center.y != nValue.float0745 && mCharacterController.center.z != nValue.int0)
		{
			CheckManager.Detected("Controller Error 3");
		}
		if (mCharacterController.radius != nValue.float0375)
		{
			CheckManager.Detected("Controller Error 4");
		}
		if (mCharacterController.height != nValue.float149)
		{
			CheckManager.Detected("Controller Error 5");
		}
		if (mCharacterController.isGrounded)
		{
			if (Physics.CheckSphere(PlayerTransform.position, mCharacterController.radius, -1749041173))
			{
				GroundedDetect = (byte)Mathf.Max(GroundedDetect--, nValue.int0);
			}
			else
			{
				GroundedDetect += (byte)nValue.int3;
			}
		}
		if (GroundedDetect >= nValue.int9)
		{
			CheckManager.Detected("Controller Error 6");
		}
	}

	private void CheckCamera()
	{
		if (Mathf.Abs(FPCamera.Transform.localPosition.x) > nValue.int2 || Mathf.Abs(FPCamera.Transform.localPosition.y) > nValue.int2 || Mathf.Abs(FPCamera.Transform.localPosition.z) > nValue.int2)
		{
			GroundedDetect++;
			if (GroundedDetect >= nValue.int3)
			{
				CheckManager.Detected("Camera Error");
			}
		}
		if (PlayerCamera.nearClipPlane != nValue.float001)
		{
			CheckManager.Detected("Camera Error");
		}
	}

	private void UpdateAFK()
	{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		return;
#endif
		if (!AfkEnabled)
		{
			return;
		}
		if (Input.touchCount != 0)
		{
			if (Input.GetTouch(0).phase == TouchPhase.Began)
			{
				StopAFK();
			}
			if (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled)
			{
				StartAFK();
			}
		}
		if (AfkTimer != -1f && GameManager.GetRoundState() == RoundState.PlayRound)
		{
			AfkTimer -= Time.deltaTime;
			if (AfkTimer < 0f)
			{
				AFKDetection();
			}
		}
	}

	public void StartAFK()
	{
		if (AfkEnabled)
		{
			AfkTimer = AfkDuration;
		}
	}

	public void StopAFK()
	{
		AfkTimer = -1f;
	}

	private void AFKDetection()
	{
		GameManager.SetLeaveRoomText("AFK");
		PhotonNetwork.LeaveRoom();
	}

	private void OnShiftController(string command, object value)
	{
		shift = (bool)value;
	}
}
