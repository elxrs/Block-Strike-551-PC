using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
	public enum CameraType
	{
		None,
		Dead,
		Static,
		Spectate,
		FirstPerson
	}

	[Serializable]
	public class SpectateClass
	{
		public float distance;

		public float distanceMin = 0.5f;

		public float distanceMax = 3f;

		public float speedRotation = 2.5f;

		public int selectPoint;

		public Transform point;

		public Vector2 value;

		public Quaternion rotation;

		public Vector3 position;

		public Vector3 negDistance;

		public Ray ray = default(Ray);

		public RaycastHit raycastHit;

		public List<PlayerSkin> players = new List<PlayerSkin>();
	}

	[Serializable]
	public class FirstPersonClass
	{
		public Camera weaponCamera;

		public Vector3 headPos;

		public ControllerManager selectPlayer;

		public int selectPlayerID;

		public int selectPlayerIndex;

		public Dictionary<int, FPWeaponShooter> weaponList = new Dictionary<int, FPWeaponShooter>();

		public FPWeaponShooter selectWeapon;

		public Vector3 position;
	}

	public CameraType SelectCameraType;

	public Camera cachedCamera;

	private Rigidbody cachedRigidbody;

	private Transform cachedTransform;

	public SpectateClass spectate;

	public FirstPersonClass firstPerson;

	public static bool teamCamera;

	private int selectedPlayer = -1;

	private static CameraManager instance;

	public static event Action<int> SelectPlayerEvent;

	private void Awake()
	{
		instance = this;
		cachedRigidbody = cachedCamera.GetComponent<Rigidbody>();
		cachedTransform = cachedCamera.transform;
	}

	private void OnEnable()
	{
		InputManager.GetButtonDownEvent += GetButtonDown;
		InputManager.GetAxisEvent += GetAxis;
	}

	private void OnDisable()
	{
		InputManager.GetButtonDownEvent -= GetButtonDown;
		InputManager.GetAxisEvent -= GetAxis;
	}

	private void GetButtonDown(string name)
	{
		if (name == "Fire")
		{
			switch (SelectCameraType)
			{
			case CameraType.Spectate:
				UpdateSpectateSelectPlayers();
				break;
			case CameraType.FirstPerson:
				UpdateFirstSelectPlayers();
				break;
			}
		}
		else if (name == "Jump" && UISpectator.GetActive())
		{
			switch (SelectCameraType)
			{
			case CameraType.Spectate:
				ActiveFirstPersonCamera(PhotonPlayer.Find(selectedPlayer));
				break;
			case CameraType.FirstPerson:
				ActiveSpectateCamera(PhotonPlayer.Find(selectedPlayer));
				break;
			}
		}
	}

	private void GetAxis(string name, float value)
	{
		switch (name)
		{
		case "Mouse X":
		{
			CameraType selectCameraType = SelectCameraType;
			if (selectCameraType == CameraType.Spectate)
			{
				spectate.value.x += value * spectate.speedRotation * spectate.distance;
			}
			break;
		}
		case "Mouse Y":
		{
			CameraType selectCameraType = SelectCameraType;
			if (selectCameraType == CameraType.Spectate)
			{
				spectate.value.y -= value * spectate.speedRotation;
			}
			break;
		}
		}
	}

	public static void SelectPlayer(PhotonPlayer player)
	{
		switch (instance.SelectCameraType)
		{
		case CameraType.Spectate:
			instance.UpdateSpectateSelectPlayers(player);
			break;
		case CameraType.FirstPerson:
			instance.UpdateFirstSelectPlayers(player);
			break;
		}
	}

	public static void ActiveDeadCamera(Vector3 position, Vector3 rotation, Vector3 force)
	{
		DeactiveAll();
		instance.SelectCameraType = CameraType.Dead;
		instance.cachedTransform.gameObject.SetActive(true);
		instance.cachedCamera.GetComponent<Collider>().isTrigger = false;
		instance.cachedRigidbody.isKinematic = false;
		instance.cachedTransform.position = position;
		instance.cachedTransform.eulerAngles = rotation;
		instance.cachedRigidbody.velocity = Vector3.zero;
		instance.cachedRigidbody.AddForce(force);
		instance.cachedRigidbody.AddRelativeForce(force);
		LODObject.Target = instance.cachedTransform;
		SkyboxManager.GetCameraParent().localEulerAngles = Vector3.zero;
	}

	public static void DeactiveDeadCamera()
	{
		if (instance.SelectCameraType == CameraType.Dead)
		{
			instance.SelectCameraType = CameraType.None;
			instance.cachedRigidbody.isKinematic = true;
			instance.cachedCamera.GetComponent<Collider>().isTrigger = true;
			instance.cachedTransform.gameObject.SetActive(false);
		}
	}

	public static void ActiveSpectateCamera()
	{
		ActiveSpectateCamera(null);
	}

	public static void ActiveSpectateCamera(PhotonPlayer player)
	{
		DeactiveAll();
		instance.SelectCameraType = CameraType.Spectate;
		instance.cachedTransform.gameObject.SetActive(true);
		GameObject camera = GameObject.Find("RoundManager/OthersManager/Camera");
		camera.GetComponent<BoxCollider>().enabled = false;
		instance.UpdateSpectateSelectPlayers(player);
		LODObject.Target = instance.cachedTransform;
		SkyboxManager.GetCameraParent().localEulerAngles = Vector3.zero;
	}

	public static void DeactiveSpectateCamera()
	{
		if (instance.SelectCameraType == CameraType.Spectate)
		{
			instance.SelectCameraType = CameraType.None;
			instance.cachedTransform.gameObject.SetActive(false);
			GameObject camera = GameObject.Find("RoundManager/OthersManager/Camera");
			camera.GetComponent<BoxCollider>().enabled = true;
		}
	}

	public static void ActiveStaticCamera()
	{
		DeactiveAll();
		instance.SelectCameraType = CameraType.Static;
		instance.cachedTransform.gameObject.SetActive(true);
		Transform transform = GameObject.FindGameObjectWithTag("StaticPoint").transform;
		instance.cachedTransform.position = transform.position;
		instance.cachedTransform.rotation = transform.rotation;
		LODObject.Target = instance.cachedTransform;
		SkyboxManager.GetCameraParent().localEulerAngles = Vector3.zero;
		transform = null;
	}

	public static void DeactiveStaticCamera()
	{
		if (instance.SelectCameraType == CameraType.Static)
		{
			instance.SelectCameraType = CameraType.None;
			instance.cachedTransform.gameObject.SetActive(false);
		}
	}

	public static void ActiveFirstPersonCamera()
	{
		ActiveFirstPersonCamera(null);
	}

	public static void ActiveFirstPersonCamera(PhotonPlayer player)
	{
		DeactiveAll();
		instance.SelectCameraType = CameraType.FirstPerson;
		instance.cachedTransform.gameObject.SetActive(true);
		if (instance.firstPerson.weaponCamera == null)
		{
			GameObject gameObject = new GameObject("WeaponCamera");
			gameObject.transform.SetParent(instance.cachedTransform);
			Camera camera = gameObject.AddComponent<Camera>();
			camera.transform.localPosition = Vector3.zero;
			camera.transform.localEulerAngles = Vector3.zero;
			camera.clearFlags = CameraClearFlags.Depth;
			camera.cullingMask = nValue.int1 << 31;
			camera.depth = nValue.int1;
			camera.farClipPlane = nValue.int100;
			camera.nearClipPlane = nValue.float001;
			camera.fieldOfView = nValue.int60;
			instance.firstPerson.weaponCamera = camera;
			gameObject = null;
		}
		else
		{
			instance.firstPerson.weaponCamera.gameObject.SetActive(true);
		}
		instance.UpdateFirstSelectPlayers(player);
		LODObject.Target = instance.cachedTransform;
		SkyboxManager.GetCameraParent().localEulerAngles = Vector3.zero;
		UICrosshair.SetActiveCrosshair(true);
		ControllerManager.SetWeaponEvent += instance.SetWeaponEventFirst;
		ControllerManager.SetDeadEvent += instance.SetDeadEventFirst;
		ControllerManager.SetFireEvent += instance.SetFireEventFirst;
		ControllerManager.SetReloadEvent += instance.SetReloadEventFirst;
	}

	public static void DeactiveFirstPersonCamera()
	{
		if (instance.SelectCameraType != CameraType.FirstPerson)
		{
			return;
		}
		instance.SelectCameraType = CameraType.None;
		instance.cachedTransform.gameObject.SetActive(false);
		if (instance.firstPerson.weaponCamera != null)
		{
			instance.firstPerson.weaponCamera.gameObject.SetActive(false);
		}
		foreach (KeyValuePair<int, FPWeaponShooter> weapon in instance.firstPerson.weaponList)
		{
			weapon.Value.Deactive();
		}
		if (instance.firstPerson.selectPlayer != null)
		{
			instance.firstPerson.selectPlayer.PlayerSkin.PlayerAnimator.rootPos = Vector3.zero;
		}
		UICrosshair.SetActiveCrosshair(false);
		ControllerManager.SetWeaponEvent -= instance.SetWeaponEventFirst;
		ControllerManager.SetDeadEvent -= instance.SetDeadEventFirst;
		ControllerManager.SetFireEvent -= instance.SetFireEventFirst;
		ControllerManager.SetReloadEvent -= instance.SetReloadEventFirst;
	}

	public static void DeactiveAll()
	{
		DeactiveDeadCamera();
		DeactiveSpectateCamera();
		DeactiveStaticCamera();
		DeactiveFirstPersonCamera();
	}

	private void LateUpdate()
	{
		switch (SelectCameraType)
		{
		case CameraType.Spectate:
			UpdateSpectate();
			break;
		case CameraType.Dead:
			UpdateDead();
			break;
		case CameraType.FirstPerson:
			UpdateFirst();
			break;
		case CameraType.Static:
			break;
		}
	}

	private void UpdateFirst()
	{
		if (firstPerson.selectPlayer == null)
		{
			ActiveSpectateCamera();
		}
		cachedTransform.localPosition = Vector3.Lerp(cachedTransform.localPosition, firstPerson.selectPlayer.PlayerSkin.PhotonPosition + firstPerson.headPos, Time.deltaTime * 10f);
		cachedTransform.localRotation = Quaternion.Lerp(cachedTransform.localRotation, firstPerson.selectPlayer.PlayerSkin.PhotonRotation * Quaternion.Euler(firstPerson.selectPlayer.PlayerSkin.Rotate * -60f, 0f, 0f), Time.deltaTime * 10f);
		if (firstPerson.selectWeapon != null)
		{
			firstPerson.selectWeapon.FPWeapon.SpectatorVelocity = firstPerson.selectPlayer.PlayerSkin.GetMove() * 30f;
		}
		SkyboxManager.GetCamera().rotation = cachedTransform.rotation;
	}

	private void UpdateFirstSelectPlayers()
	{
		UpdateFirstSelectPlayers(null);
	}

	private void UpdateFirstSelectPlayers(PhotonPlayer player)
	{
		if (player != null)
		{
			for (int i = 0; i < ControllerManager.ControllerList.Count; i++)
			{
				if (ControllerManager.ControllerList[i].photonView.ownerId == player.ID)
				{
					if (ControllerManager.ControllerList[i].PlayerSkin != null && ControllerManager.ControllerList[i].PlayerSkin.isPlayerActive)
					{
						if (firstPerson.selectPlayer != null)
						{
							firstPerson.selectPlayer.PlayerSkin.PlayerAnimator.rootPos = Vector3.zero;
						}
						firstPerson.selectPlayer = ControllerManager.ControllerList[i];
						firstPerson.selectPlayerID = firstPerson.selectPlayer.photonView.ownerId;
						firstPerson.selectPlayer.PlayerSkin.PlayerAnimator.rootPos = Vector3.back * 2f;
						cachedTransform.localPosition = firstPerson.selectPlayer.PlayerSkin.PhotonPosition + firstPerson.headPos;
						cachedTransform.localRotation = firstPerson.selectPlayer.PlayerSkin.PhotonRotation * Quaternion.Euler(firstPerson.selectPlayer.PlayerSkin.Rotate * -60f, 0f, 0f);
						UpdateWeaponFirst();
						if (SelectPlayerEvent != null)
						{
							selectedPlayer = firstPerson.selectPlayerID;
							SelectPlayerEvent(selectedPlayer);
						}
						return;
					}
					break;
				}
			}
		}
		List<ControllerManager> list = new List<ControllerManager>();
		for (int j = 0; j < ControllerManager.ControllerList.Count; j++)
		{
			if (teamCamera)
			{
				if (!ControllerManager.ControllerList[j].photonView.owner.GetDead() && ControllerManager.ControllerList[j].photonView.owner.GetTeam() == PlayerInput.instance.PlayerTeam)
				{
					list.Add(ControllerManager.ControllerList[j]);
				}
			}
			else if (!ControllerManager.ControllerList[j].photonView.owner.GetDead())
			{
				list.Add(ControllerManager.ControllerList[j]);
			}
		}
		firstPerson.selectPlayerIndex++;
		if (firstPerson.selectPlayerIndex > list.Count - 1)
		{
			firstPerson.selectPlayerIndex = 0;
		}
		if (list.Count != 0)
		{
			if (firstPerson.selectPlayer != null)
			{
				firstPerson.selectPlayer.PlayerSkin.PlayerAnimator.rootPos = Vector3.zero;
			}
			firstPerson.selectPlayer = list[firstPerson.selectPlayerIndex];
			if (firstPerson.selectPlayer == null)
			{
				UpdateFirstSelectPlayers();
			}
			firstPerson.selectPlayerID = firstPerson.selectPlayer.photonView.ownerId;
			firstPerson.selectPlayer.PlayerSkin.PlayerAnimator.rootPos = Vector3.back * 2f;
			cachedTransform.localPosition = firstPerson.selectPlayer.PlayerSkin.PhotonPosition + firstPerson.headPos;
			cachedTransform.localRotation = firstPerson.selectPlayer.PlayerSkin.PhotonRotation * Quaternion.Euler(firstPerson.selectPlayer.PlayerSkin.Rotate * -60f, 0f, 0f);
			UpdateWeaponFirst();
			if (SelectPlayerEvent != null)
			{
				selectedPlayer = firstPerson.selectPlayer.photonView.ownerId;
				SelectPlayerEvent(selectedPlayer);
			}
		}
		else
		{
			ActiveStaticCamera();
			if (SelectPlayerEvent != null)
			{
				selectedPlayer = -1;
				SelectPlayerEvent(selectedPlayer);
			}
		}
		list = null;
	}

	private void UpdateWeaponFirst()
	{
		foreach (KeyValuePair<int, FPWeaponShooter> weapon in firstPerson.weaponList)
		{
			weapon.Value.Deactive();
		}
		if (firstPerson.selectPlayer.PlayerSkin.SelectWeapon != null)
		{
			TPWeaponShooter selectWeapon = firstPerson.selectPlayer.PlayerSkin.SelectWeapon;
			if (!firstPerson.weaponList.ContainsKey(selectWeapon.WeaponID))
			{
				WeaponData weaponData = WeaponManager.GetWeaponData(selectWeapon.WeaponID);
				GameObject fpsPrefab = weaponData.FpsPrefab;
				fpsPrefab = Utils.AddChild(fpsPrefab, cachedTransform);
				firstPerson.selectWeapon = fpsPrefab.GetComponent<FPWeaponShooter>();
				firstPerson.selectWeapon.FPWeapon.Spectator = true;
				firstPerson.weaponList.Add(weaponData.ID, firstPerson.selectWeapon);
				fpsPrefab.SetActive(true);
			}
			else
			{
				firstPerson.selectWeapon = firstPerson.weaponList[selectWeapon.WeaponID];
				firstPerson.selectWeapon.FPWeapon.Activate();
			}
			firstPerson.selectWeapon.UpdateWeaponData(selectWeapon.WeaponID, selectWeapon.WeaponSkin, selectWeapon.GetStickers(), selectWeapon.FireStatValue);
			firstPerson.selectWeapon.UpdateHandAtlas(firstPerson.selectPlayer.PlayerSkin.PlayerTeam, firstPerson.selectPlayer.PlayerSkin.BodySkin);
			UICrosshair.SetAccuracy(WeaponManager.GetWeaponData(selectWeapon.WeaponID).Accuracy);
		}
	}

	private void SetWeaponEventFirst(int playerID, int weapon, int skin)
	{
		if (instance.SelectCameraType == CameraType.FirstPerson && firstPerson.selectPlayer != null && firstPerson.selectPlayerID == playerID)
		{
			UpdateWeaponFirst();
		}
	}

	private void SetDeadEventFirst(int playerID, bool dead)
	{
		if (instance.SelectCameraType != CameraType.FirstPerson || !(firstPerson.selectPlayer != null) || firstPerson.selectPlayerID != playerID)
		{
			return;
		}
		if (dead)
		{
			foreach (KeyValuePair<int, FPWeaponShooter> weapon in firstPerson.weaponList)
			{
				weapon.Value.Deactive();
			}
			return;
		}
		UpdateWeaponFirst();
	}

	private void SetFireEventFirst(int playerID)
	{
		if (instance.SelectCameraType == CameraType.FirstPerson && firstPerson.selectPlayer != null && firstPerson.selectPlayerID == playerID && firstPerson.selectWeapon != null)
		{
			firstPerson.selectWeapon.Fire();
		}
	}

	private void SetReloadEventFirst(int playerID)
	{
		if (instance.SelectCameraType == CameraType.FirstPerson && firstPerson.selectPlayer != null && firstPerson.selectPlayerID == playerID && firstPerson.selectWeapon != null)
		{
			firstPerson.selectWeapon.Reload();
		}
	}

	private void UpdateSpectate()
	{
		if (!(spectate.point == null))
		{
			spectate.distance = spectate.distanceMax;
			spectate.value.y = ClampAngle(spectate.value.y, -20f, 80f);
			spectate.rotation = Quaternion.Euler(spectate.value.y, spectate.value.x, 0f);
			spectate.ray.origin = spectate.point.position;
			spectate.ray.direction = (cachedTransform.position - spectate.point.position).normalized;
			if (Physics.SphereCast(spectate.ray.origin, 0.25f, spectate.ray.direction, out spectate.raycastHit, spectate.distance))
			{
				spectate.distance = Mathf.Clamp(spectate.raycastHit.distance, spectate.distanceMin, spectate.distanceMax);
			}
			spectate.negDistance = new Vector3(0f, 0f, 0f - spectate.distance);
			spectate.position = spectate.rotation * spectate.negDistance + spectate.point.position;
			cachedTransform.position = spectate.position;
			cachedTransform.rotation = spectate.rotation;
			SkyboxManager.GetCamera().rotation = spectate.rotation;
		}
	}

	private void UpdateSpectateSelectPlayers()
	{
		UpdateSpectateSelectPlayers(null);
	}

	private void UpdateSpectateSelectPlayers(PhotonPlayer player)
	{
		if (player != null)
		{
			for (int i = 0; i < ControllerManager.ControllerList.Count; i++)
			{
				if (ControllerManager.ControllerList[i].photonView.ownerId == player.ID)
				{
					if (ControllerManager.ControllerList[i].PlayerSkin != null && ControllerManager.ControllerList[i].PlayerSkin.isPlayerActive)
					{
						spectate.point = ControllerManager.ControllerList[i].PlayerSkin.PlayerSpectatePoint;
						if (SelectPlayerEvent != null)
						{
							selectedPlayer = player.ID;
							SelectPlayerEvent(selectedPlayer);
						}
						return;
					}
					break;
				}
			}
		}
		spectate.players.Clear();
		for (int j = 0; j < ControllerManager.ControllerList.Count; j++)
		{
			if (ControllerManager.ControllerList[j].PlayerSkin != null && ControllerManager.ControllerList[j].PlayerSkin.isPlayerActive)
			{
				spectate.players.Add(ControllerManager.ControllerList[j].PlayerSkin);
			}
		}
		if (teamCamera)
		{
			List<PlayerSkin> list = new List<PlayerSkin>();
			Team playerTeam = PlayerInput.instance.PlayerTeam;
			for (int k = 0; k < spectate.players.Count; k++)
			{
				if (spectate.players[k].PlayerTeam == playerTeam)
				{
					list.Add(spectate.players[k]);
				}
			}
			spectate.selectPoint++;
			if (spectate.selectPoint > list.Count - 1)
			{
				spectate.selectPoint = 0;
			}
			if (list.Count != 0)
			{
				spectate.point = list[spectate.selectPoint].PlayerSpectatePoint;
				if (SelectPlayerEvent != null)
				{
					selectedPlayer = list[spectate.selectPoint].Controller.photonView.ownerId;
					SelectPlayerEvent(selectedPlayer);
				}
			}
			else if (PhotonNetwork.player.GetTeam() != 0)
			{
				ActiveStaticCamera();
				if (SelectPlayerEvent != null)
				{
					selectedPlayer = -1;
					SelectPlayerEvent(selectedPlayer);
				}
			}
			list = null;
			return;
		}
		spectate.selectPoint++;
		if (spectate.selectPoint > spectate.players.Count - 1)
		{
			spectate.selectPoint = 0;
		}
		if (spectate.players.Count != 0)
		{
			spectate.point = spectate.players[spectate.selectPoint].PlayerSpectatePoint;
			if (SelectPlayerEvent != null)
			{
				selectedPlayer = spectate.players[spectate.selectPoint].Controller.photonView.ownerId;
				SelectPlayerEvent(selectedPlayer);
			}
		}
		else if (PhotonNetwork.player.GetTeam() != 0)
		{
			ActiveStaticCamera();
			if (SelectPlayerEvent != null)
			{
				selectedPlayer = -1;
				SelectPlayerEvent(selectedPlayer);
			}
		}
	}

	private void UpdateDead()
	{
		SkyboxManager.GetCamera().rotation = cachedTransform.rotation;
	}

	public static Transform GetActiveCamera()
	{
		if (instance.SelectCameraType == CameraType.None)
		{
			if (PlayerInput.instance == null)
			{
				return null;
			}
			return PlayerInput.instance.FPCamera.Transform;
		}
		return instance.cachedTransform;
	}

	public static CameraType GetSelectCameraType()
	{
		return instance.SelectCameraType;
	}

	private float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f)
		{
			angle += 360f;
		}
		if (angle > 360f)
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}
}
