using Photon;
using UnityEngine;

public class mg_BoxRoad : PunBehaviour
{
	public GameObject box;

	public int boxCount = 20;

	public int index = -1;

	public int i;

	public float speed = 0.5f;

	private Vector3 pos;

	private Vector3 lastPos;

	private Vector3 lastPos2 = new Vector3(-1f, -1f, -1f);

	public bool create;

	public bool created;

	public bool useY;

	private GameObject[] boxList;

	private int startTimeID;

	private void Awake()
	{
		PhotonClassesManager.Add(this);
	}

	private void Start()
	{
		boxList = new GameObject[boxCount];
		for (int i = 0; i < boxCount; i++)
		{
			boxList[i] = Instantiate(box, Vector3.zero, Quaternion.identity);
			boxList[i].name = i.ToString();
		}
		UIGameManager.SetActiveScore(true, 20);
		GameManager.SetStartDamageTime(0.1f);
		UIPanelManager.ShowPanel("Display");
		GameManager.MaxScore = 999;
		CameraManager.ActiveStaticCamera();
		TimerManager.In(1f, delegate
		{
			GameManager.OnSelectTeam(Team.Blue);
			if (PhotonNetwork.isMasterClient)
			{
				ActivationWaitPlayer();
			}
			else if (GameManager.GetController().PlayerInput.Dead)
			{
				CameraManager.ActiveSpectateCamera();
			}
		});
		EventManager.AddListener<DamageInfo>("DeadPlayer", OnDeadPlayer);
	}

	private void ActivationWaitPlayer()
	{
		EventManager.Dispatch("WaitPlayer");
		GameManager.UpdateRoundState(RoundState.WaitPlayer);
		OnWaitPlayer();
		OnCreatePlayer();
	}

	private void OnWaitPlayer()
	{
		UIStatus.Add(Localization.Get("Waiting for other players"), true);
		TimerManager.In(4f, delegate
		{
			if (GameManager.GetRoundState() == RoundState.WaitPlayer)
			{
				if (PhotonNetwork.playerList.Length <= 1)
				{
					OnWaitPlayer();
				}
				else
				{
					GameManager.UpdateRoundState(RoundState.StartRound);
					TimerManager.In(4f, delegate
					{
						OnStartRound();
					});
				}
			}
		});
	}

	public override void OnPhotonPlayerConnected(PhotonPlayer playerConnect)
	{
		if (!PhotonNetwork.isMasterClient)
		{
		}
	}

	public override void OnPhotonPlayerDisconnected(PhotonPlayer playerDisconnect)
	{
		if (PhotonNetwork.isMasterClient)
		{
			CheckPlayers();
		}
	}

	private void OnCreatePlayer()
	{
		WeaponManager.SetSelectWeapon(WeaponType.Pistol, 0);
		WeaponManager.SetSelectWeapon(WeaponType.Rifle, 0);
		PlayerInput playerInput = GameManager.GetController().PlayerInput;
		playerInput.SetHealth(100);
		CameraManager.DeactiveAll();
		GameManager.GetController().ActivePlayer(GameManager.GetTeamSpawn().GetSpawnPosition(), GameManager.GetTeamSpawn().GetSpawnRotation());
		playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Knife);
	}

	private void OnStartRound()
	{
		if (PhotonNetwork.playerList.Length <= 1)
		{
			ActivationWaitPlayer();
		}
		else if (PhotonNetwork.isMasterClient)
		{
			GameManager.UpdateRoundState(RoundState.StartRound);
			photonView.RPC("StartTimer", PhotonTargets.All, (byte)Random.Range(1, 200));
		}
	}

	[PunRPC]
	private void StartTimer(byte seed, PhotonMessageInfo info)
	{
		print(seed);
		Random.InitState(seed);
		EventManager.Dispatch("StartRound");
		OnCreatePlayer();
		float num = 5f;
		num -= (float)(PhotonNetwork.time - info.timestamp);
		for (int i = 0; i < boxCount; i++)
		{
			boxList[i].transform.position = Vector3.zero;
		}
		startTimeID = TimerManager.In(num, delegate
		{
			GameManager.UpdateRoundState(RoundState.PlayRound);
			pos = Vector3.zero;
			lastPos = Vector3.zero;
			lastPos2 = new Vector3(-1f, -1f, -1f);
			InvokeRepeating("CreateBox", speed, speed);
		});
	}

	private void OnDeadPlayer(DamageInfo damageInfo)
	{
		if (GameManager.GetRoundState() == RoundState.PlayRound)
		{
			PhotonNetwork.player.SetDeaths1();
			PlayerRoundManager.SetDeaths1();
		}
		if (GameManager.GetRoundState() == RoundState.PlayRound)
		{
			string text = Utils.GetTeamHexColor(PhotonNetwork.player) + " [@]";
			UIStatus.Add(text, false, "died");
		}
		if (GameManager.GetRoundState() == RoundState.PlayRound)
		{
			Vector3 ragdollForce = Utils.GetRagdollForce(GameManager.GetController().PlayerInput.PlayerTransform.position, damageInfo.AttackPosition);
			CameraManager.ActiveDeadCamera(GameManager.GetController().PlayerInput.FPCamera.Transform.position, GameManager.GetController().PlayerInput.FPCamera.Transform.eulerAngles, ragdollForce * 100f);
			GameManager.GetController().DeactivePlayer(ragdollForce, damageInfo.HeadShot);
		}
		else
		{
			OnCreatePlayer();
		}
		if (GameManager.GetRoundState() != RoundState.PlayRound)
		{
			return;
		}
		photonView.RPC("CheckPlayers", PhotonTargets.MasterClient);
		TimerManager.In(3f, delegate
		{
			if (GameManager.GetController().PlayerInput.Dead)
			{
				CameraManager.ActiveSpectateCamera();
			}
		});
	}

	[PunRPC]
	private void OnFinishRound(PhotonMessageInfo info)
	{
		TimerManager.Cancel(startTimeID);
		GameManager.UpdateRoundState(RoundState.EndRound);
		CancelInvoke("CreateBox");
		float delay = 6f - (float)(PhotonNetwork.time - info.timestamp);
		TimerManager.In(delay, delegate
		{
			OnStartRound();
		});
	}

	[PunRPC]
	private void CheckPlayers()
	{
		if (!PhotonNetwork.isMasterClient || GameManager.GetRoundState() != RoundState.PlayRound)
		{
			return;
		}
		PhotonPlayer[] playerList = PhotonNetwork.playerList;
		PhotonPlayer photonPlayer = null;
		for (int i = 0; i < playerList.Length; i++)
		{
			if (!playerList[i].GetDead())
			{
				if (photonPlayer != null)
				{
					photonPlayer = null;
					break;
				}
				photonPlayer = playerList[i];
			}
		}
		if (photonPlayer != null)
		{
			GameManager.UpdateRoundState(RoundState.EndRound);
			UIMainStatus.Add(photonPlayer.NickName + " [@]", false, 5f, "Win");
			photonView.RPC("OnFinishRound", PhotonTargets.All);
		}
	}

	private void CreateBox()
	{
		do
		{
			int num = Random.Range(1, 5);
			int num2 = Random.Range(1, 5);
			int num3 = Random.Range(1, 20);
			if (num >= 4)
			{
				pos.x += 1f;
			}
			if (num2 >= 4)
			{
				pos.z += 1f;
			}
			if (num3 == 19 && !useY)
			{
				pos.y += 1f;
				useY = true;
			}
			if (num3 == 1)
			{
				pos.y -= 1f;
			}
			if (num3 >= 6 || create)
			{
				create = false;
				created = true;
			}
			else
			{
				create = true;
			}
		}
		while (pos == lastPos || lastPos2 == lastPos);
		if (created)
		{
			index++;
			i++;
			if (boxCount == index)
			{
				index = 0;
			}
			created = false;
			boxList[index].transform.position = pos;
			lastPos2 = lastPos;
			lastPos = pos;
			useY = false;
			if (i >= 25 && speed >= 0.1f)
			{
				i = 0;
				speed -= 0.05f;
				CancelInvoke("CreateBox");
				InvokeRepeating("CreateBox", speed, speed);
			}
		}
	}
}
