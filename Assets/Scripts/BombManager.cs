using System.Collections.Generic;
using Photon;
using UnityEngine;

public class BombManager : PunBehaviour
{
	public GameObject Bomb;

	public Transform ZoneA;

	public Transform ZoneB;

	private int PlayerBomb = -1;

	private int PlayerDeactiveBomb = -1;

	private int Zone = -1;

	public static bool BombPlaced;

	public static bool BuyTime;

	private bool BombPlacing;

	public BombAudio BombAudio;

	public ParticleSystem Effect;

	public ControllerManager BombController;

	public Transform BombPlayerModel;

	public GameObject BombPlayerModel2;

	public GameObject LineBomb;

	private Vector3 BombPosition;

	private bool BuyZone;

	private static BombManager instance;

	private void Awake()
	{
		if (PhotonNetwork.offlineMode)
		{
			Destroy(this);
		}
		else if (PhotonNetwork.room.GetGameMode() != GameMode.Bomb && PhotonNetwork.room.GetGameMode() != GameMode.Bomb2)
		{
			Resources.UnloadAsset(BombAudio.BombAudioClip);
			Resources.UnloadAsset(Effect.GetComponent<AudioSource>().clip);
			Destroy(gameObject);
		}
		else
		{
			PhotonClassesManager.Add(this);
			instance = this;
			ZoneA.gameObject.SetActive(true);
			ZoneB.gameObject.SetActive(true);
		}
	}

	private void Start()
	{
		PhotonEvent.AddListener(1, SetStartRoundBomb);
		PhotonEvent.AddListener(2, PhotonMasterPickupBomb);
		PhotonEvent.AddListener(3, PhotonPickupBomb);
		PhotonEvent.AddListener(4, PhotonSetBomb);
		PhotonEvent.AddListener(5, PhotonDeactiveBomb);
		PhotonEvent.AddListener(6, PhotonDeactiveBoom);
		PhotonEvent.AddListener(7, PhotonDeactiveBombExit);
		PhotonEvent.AddListener(8, PhotonSetPosition);
		PhotonEvent.AddListener(9, PhotonOnPhotonPlayerConnected);
		EventManager.AddListener("StartRound", StartRound);
		EventManager.AddListener("WaitPlayer", StartRound);
	}

	private void OnEnable()
	{
		InputManager.GetButtonDownEvent += GetButtonDown;
		InputManager.GetButtonUpEvent += GetButtonUp;
	}

	private void OnDisable()
	{
		InputManager.GetButtonDownEvent -= GetButtonDown;
		InputManager.GetButtonUpEvent -= GetButtonUp;
	}

	private void GetButtonDown(string name)
	{
		if (name == "Use")
		{
			if (BuyZone)
			{
				UIControllerList.BuyWeapons.cachedGameObject.SetActive(true);
			}
			else if (PhotonNetwork.player.GetTeam() == Team.Red && Zone != -nValue.int1 && PlayerBomb == PhotonNetwork.player.ID && !BombPlaced)
			{
				UIGameManager.StartDuration(nValue.int5, true, SetBomb);
				PlayerInput.instance.SetMove(false);
				BombPlacing = true;
			}
			else if (PhotonNetwork.player.GetTeam() == Team.Blue && Zone != -nValue.int1 && PlayerDeactiveBomb == PhotonNetwork.player.ID && BombPlaced)
			{
				if (PhotonNetwork.room.GetGameMode() == GameMode.Bomb)
				{
					UIGameManager.StartDuration(nValue.int5, true, DeactiveBoom);
				}
				else
				{
					UIGameManager.StartDuration((!UIDefuseKit.GetDefuseKit()) ? nValue.int8 : nValue.int4, true, DeactiveBoom);
				}
				PlayerInput.instance.SetMove(false);
				BombPlacing = true;
			}
		}
		else if (BombPlacing)
		{
			UIGameManager.StopDuration();
			if (PlayerInput.instance != null)
			{
				PlayerInput.instance.SetMove(true);
			}
			BombPlacing = false;
		}
	}

	private void GetButtonUp(string name)
	{
		if (name == "Use" && !BuyZone)
		{
			UIGameManager.StopDuration();
			PlayerInput.instance.SetMove(true);
			BombPlacing = false;
		}
	}

	private void StartRound()
	{
		BombPlaced = false;
		BombPlacing = false;
		Bomb.SetActive(false);
		PlayerBomb = -nValue.int1;
		PlayerDeactiveBomb = -nValue.int1;
		Zone = -nValue.int1;
		BombPosition = Vector3.zero;
		BombAudio.Stop();
		Effect.Stop();
		Effect.GetComponent<AudioSource>().Stop();
		if (GameManager.GetRoundState() == RoundState.WaitPlayer)
		{
			return;
		}
		TimerManager.In(nValue.int1, delegate
		{
			if (GameManager.GetRoundState() != 0 && PhotonNetwork.isMasterClient)
			{
				List<PhotonPlayer> list = new List<PhotonPlayer>();
				for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
				{
					if (PhotonNetwork.playerList[i].GetTeam() == Team.Red)
					{
						list.Add(PhotonNetwork.playerList[i]);
					}
				}
				if (list.Count > nValue.int0)
				{
					PhotonPlayer photonPlayer;
					do
					{
						photonPlayer = list[Random.Range(nValue.int0, list.Count)];
					}
					while (photonPlayer.GetDead());
					PhotonEvent.RPC((byte)nValue.int1, PhotonTargets.All, photonPlayer.ID);
				}
			}
		});
	}

	private void SetStartRoundBomb(PhotonEventData data)
	{
		PlayerBomb = (int)data.parameters[nValue.int0];
		if (PlayerBomb == PhotonNetwork.player.ID)
		{
			UIToast.Show(Localization.Get("You have a bomb"));
			return;
		}
		if (BombPlayerModel == null)
		{
			BombPlayerModel = Instantiate(BombPlayerModel2).transform;
		}
		BombPlayerModel.parent = null;
		if (PhotonNetwork.player.GetTeam() == Team.Red)
		{
			UIToast.Show(PhotonPlayer.Find(PlayerBomb).NickName + " " + Localization.Get("picked up a bomb"));
		}
		BombController = ControllerManager.FindController(PlayerBomb);
		if (BombController != null)
		{
			BombPlayerModel.SetParent(BombController.PlayerSkin.PlayerWeaponContainers[2]);
			BombPlayerModel.localPosition = new Vector3(0.1f, 0f, 0.1f);
			BombPlayerModel.localEulerAngles = new Vector3(-90f, 0f, 0f);
		}
	}

	public override void OnPhotonPlayerConnected(PhotonPlayer playerConnect)
	{
		if (!PhotonNetwork.isMasterClient)
		{
			return;
		}
		TimerManager.In(nValue.float05, delegate
		{
			if (BombPlaced)
			{
				float num = UIGameManager.instance.ScoreTimer - Time.time;
				PhotonEvent.RPC((byte)nValue.int9, true, playerConnect, BombPlaced, BombPosition, num);
			}
			else
			{
				PhotonEvent.RPC((byte)nValue.int9, true, playerConnect, BombPlaced);
			}
		});
	}

	private void PhotonOnPhotonPlayerConnected(PhotonEventData data)
	{
		BombPlaced = (bool)data.parameters[nValue.int0];
		if (BombPlaced)
		{
			BombPosition = (Vector3)data.parameters[nValue.int1];
			float time = (float)data.parameters[nValue.int2] - (float)(PhotonNetwork.time - data.timestamp);
			UIGameManager.StartScoreTimer(time, Boom);
			BombAudio.Play(time);
			SetPosition(BombPosition);
		}
	}

	public override void OnPhotonPlayerDisconnected(PhotonPlayer playerDisconnect)
	{
		if (PhotonNetwork.isMasterClient && playerDisconnect.ID == PlayerBomb)
		{
			SetRandomPlayer();
		}
	}

	private void SetRandomPlayer()
	{
		List<PhotonPlayer> list = new List<PhotonPlayer>();
		for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
		{
			if (PhotonNetwork.playerList[i].GetTeam() == Team.Red && !PhotonNetwork.playerList[i].GetDead())
			{
				list.Add(PhotonNetwork.playerList[i]);
			}
		}
		if (list.Count > nValue.int0)
		{
			PhotonPlayer photonPlayer = list[Random.Range(nValue.int0, list.Count)];
			PhotonEvent.RPC((byte)nValue.int1, PhotonTargets.All, photonPlayer.ID);
		}
	}

	public static void DeadPlayer()
	{
		if (PhotonNetwork.player.ID == instance.PlayerBomb)
		{
			RaycastHit hitInfo;
			if (Physics.Raycast(PlayerInput.instance.PlayerTransform.position, Vector3.down, out hitInfo, nValue.int50))
			{
				PhotonEvent.RPC((byte)nValue.int8, PhotonTargets.All, hitInfo.point, hitInfo.normal);
			}
			else
			{
				instance.SetRandomPlayer();
			}
		}
		instance.BombPlacing = false;
		UIControllerList.Use.cachedGameObject.SetActive(false);
		UIControllerList.BuyWeapons.cachedGameObject.SetActive(false);
		UIGameManager.StopDuration();
		PlayerInput.instance.SetMove(true);
	}

	private void PhotonSetPosition(PhotonEventData data)
	{
		if (PlayerBomb != -nValue.int1 && PhotonNetwork.player.GetTeam() == Team.Red)
		{
			UIToast.Show(PhotonPlayer.Find(PlayerBomb).NickName + " " + Localization.Get("lost the bomb"));
		}
		PlayerBomb = -nValue.int1;
		SetPosition((Vector3)data.parameters[nValue.int0], (Vector3)data.parameters[nValue.int1]);
		if (BombPlayerModel == null)
		{
			BombPlayerModel = Instantiate(BombPlayerModel2).transform;
		}
		BombPlayerModel.parent = null;
		BombPlayerModel.position = Vector3.up * 1000f;
	}

	public static void SetPosition(Vector3 pos, Vector3 normal)
	{
		instance.Bomb.transform.position = pos + normal * nValue.float001;
		instance.Bomb.transform.rotation = Quaternion.LookRotation(normal);
		instance.Bomb.SetActive(true);
	}

	public static void SetPosition(Vector3 pos)
	{
		instance.Bomb.transform.position = pos;
		instance.Bomb.transform.rotation = Quaternion.Euler(-nValue.int90, Random.Range(nValue.int0, nValue.int360), nValue.int0);
		instance.Bomb.SetActive(true);
	}

	public void OnTriggerEnterBomb()
	{
		if (GameManager.GetRoundState() == RoundState.PlayRound)
		{
			PhotonEvent.RPC((byte)nValue.int2, PhotonTargets.MasterClient);
		}
	}

	public void OnTriggerExitBomb()
	{
		if (BombPlaced)
		{
			UIControllerList.Use.cachedGameObject.SetActive(false);
			if (PhotonNetwork.player.ID == PlayerDeactiveBomb)
			{
				PhotonEvent.RPC((byte)nValue.int7, PhotonTargets.All);
			}
		}
	}

	private void PhotonMasterPickupBomb(PhotonEventData data)
	{
		PhotonPlayer player = PhotonPlayer.Find(data.senderID);
		if (BombPlaced)
		{
			if (player.GetTeam() == Team.Blue && PlayerDeactiveBomb == -nValue.int1)
			{
				PhotonEvent.RPC((byte)nValue.int5, PhotonTargets.All, data.senderID);
			}
		}
		else if (player.GetTeam() == Team.Red && PlayerBomb == -nValue.int1)
		{
			PhotonEvent.RPC((byte)nValue.int3, PhotonTargets.All, data.senderID);
		}
	}

	private void PhotonPickupBomb(PhotonEventData data)
	{
		Bomb.SetActive(false);
		PlayerBomb = (int)data.parameters[nValue.int0];
		if (PlayerBomb == PhotonNetwork.player.ID)
		{
			UIToast.Show(Localization.Get("You have a bomb"));
			return;
		}
		if (BombPlayerModel == null)
		{
			BombPlayerModel = Instantiate(BombPlayerModel2).transform;
		}
		BombPlayerModel.parent = null;
		BombPlayerModel.position = Vector3.up * 1000f;
		if (PhotonNetwork.player.GetTeam() == Team.Red)
		{
			UIToast.Show(PhotonPlayer.Find(PlayerBomb).NickName + " " + Localization.Get("picked up a bomb"));
		}
		BombController = ControllerManager.FindController(PlayerBomb);
		if (BombController != null)
		{
			BombPlayerModel.SetParent(BombController.PlayerSkin.PlayerWeaponContainers[2]);
			BombPlayerModel.localPosition = new Vector3(0.1f, 0f, 0.1f);
			BombPlayerModel.localEulerAngles = new Vector3(-90f, 0f, 0f);
		}
	}

	private void PhotonDeactiveBomb(PhotonEventData data)
	{
		PlayerDeactiveBomb = (int)data.parameters[nValue.int0];
		if (PlayerDeactiveBomb == PhotonNetwork.player.ID)
		{
			UIControllerList.Use.cachedGameObject.SetActive(true);
		}
	}

	private void PhotonDeactiveBombExit(PhotonEventData data)
	{
		PlayerDeactiveBomb = -nValue.int1;
	}

	public void OnTriggerEnterZone(int zone)
	{
		Zone = zone;
		if (PhotonNetwork.player.GetTeam() == Team.Red && PlayerBomb == PhotonNetwork.player.ID && GameManager.GetRoundState() == RoundState.PlayRound)
		{
			UIControllerList.Use.cachedGameObject.SetActive(true);
		}
	}

	public void OnTriggerExitZone()
	{
		Zone = -nValue.int1;
		UIControllerList.Use.cachedGameObject.SetActive(false);
		UIGameManager.StopDuration();
		PlayerInput.instance.SetMove(true);
		BombPlacing = false;
	}

	private void SetBomb()
	{
		UIControllerList.Use.cachedGameObject.SetActive(false);
		UIGameManager.StopDuration();
		PlayerInput.instance.SetMove(true);
		if (GameManager.GetRoundState() == RoundState.PlayRound)
		{
			PhotonEvent.RPC((byte)nValue.int4, true, PhotonTargets.All, PlayerInput.instance.PlayerTransform.position - new Vector3(nValue.int0, nValue.float008, nValue.int0));
		}
	}

	private void PhotonSetBomb(PhotonEventData data)
	{
		if (GameManager.GetRoundState() == RoundState.PlayRound)
		{
			UIToast.Show(Localization.Get("The bomb has been planted"));
			BombPlacing = false;
			BombPlaced = true;
			if (BombPlayerModel == null)
			{
				BombPlayerModel = Instantiate(BombPlayerModel2).transform;
			}
			BombPlayerModel.parent = null;
			BombPlayerModel.position = Vector3.up * 1000f;
			BombController = null;
			PlayerBomb = -nValue.int1;
			float time = 35f - (float)(PhotonNetwork.time - data.timestamp);
			UIGameManager.StartScoreTimer(time, Boom);
			SetPosition((Vector3)data.parameters[nValue.int0]);
			BombAudio.Play(time);
		}
	}

	private void Boom()
	{
		BombAudio.Boom();
		Effect.Play();
		Effect.GetComponent<AudioSource>().Play();
		Effect.transform.position = Bomb.transform.position;
		BombAudio.Stop();
		Bomb.SetActive(false);
		BombPlacing = false;
		BombPlaced = false;
		PlayerBomb = -nValue.int1;
		PlayerDeactiveBomb = -nValue.int1;
		UIGameManager.instance.isScoreTimer = false;
		if (PhotonNetwork.room.GetGameMode() == GameMode.Bomb)
		{
			BombMode.instance.Boom();
		}
		else
		{
			BombMode2.instance.Boom();
		}
		UIGameManager.StopDuration();
		if (PlayerInput.instance.isAwake)
		{
			PlayerInput.instance.SetMove(true);
			int value = (nValue.int50 - (int)Vector3.Distance(PlayerInput.instance.PlayerTransform.position, Bomb.transform.position)) * nValue.int5;
			value = Mathf.Clamp(value, nValue.int0, nValue.int100);
			if (value > nValue.int0)
			{
				DamageInfo damageInfo = DamageInfo.Get(value, Vector3.zero, Team.None, nValue.int0, nValue.int0, -nValue.int1, false);
				PlayerInput.instance.Damage(damageInfo);
			}
			PlayerInput.instance.FPCamera.AddRollForce(Random.Range(-nValue.int3, nValue.int3));
		}
	}

	private void DeactiveBoom()
	{
		UIControllerList.Use.cachedGameObject.SetActive(false);
		UIGameManager.StopDuration();
		PlayerInput.instance.SetMove(true);
		PhotonEvent.RPC((byte)nValue.int6, PhotonTargets.All);
	}

	private void PhotonDeactiveBoom(PhotonEventData data)
	{
		BombAudio.Stop();
		Bomb.SetActive(false);
		Bomb.SetActive(false);
		BombPlacing = false;
		BombPlaced = false;
		PlayerBomb = -nValue.int1;
		PlayerDeactiveBomb = -nValue.int1;
		UIGameManager.instance.isScoreTimer = false;
		if (PhotonNetwork.room.GetGameMode() == GameMode.Bomb)
		{
			BombMode.instance.DeactiveBoom();
		}
		else
		{
			BombMode2.instance.DeactiveBoom();
		}
	}

	public void OnTriggerEnterZoneBuy(int team)
	{
		if (PhotonNetwork.room.GetGameMode() == GameMode.Bomb2 && PhotonNetwork.player.GetTeam() == (Team)team && GameManager.GetRoundState() == RoundState.PlayRound && (BuyTime || UIGameManager.instance.Timer > 90f))
		{
			UIControllerList.Use.cachedGameObject.SetActive(true);
			BuyZone = true;
		}
	}

	public void OnTriggerExitZoneBuy()
	{
		if (PhotonNetwork.room.GetGameMode() == GameMode.Bomb2)
		{
			UIControllerList.Use.cachedGameObject.SetActive(false);
			UIControllerList.BuyWeapons.cachedGameObject.SetActive(false);
			BuyZone = false;
		}
	}

	public static int GetPlayerBombID()
	{
		return instance.PlayerBomb;
	}
}
