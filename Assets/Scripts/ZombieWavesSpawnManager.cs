using Photon;
using UnityEngine;

public class ZombieWavesSpawnManager : PunBehaviour
{
	public int count = 5;

	public int maxInScene = 2;

	public int alive;

	public int deads;

	public Transform[] spawns;

	public GameObject zombiePrefab;

	private void Start()
	{
		TimerManager.In(1f, -1, 1f, CheckScene);
		PlayerAI.deadEvent += DeadZombie;
		PlayerAI.startEvent += StartZombie;
	}

	private void CheckScene()
	{
		if (count - deads > alive && PlayerAI.list.Count < maxInScene)
		{
			PhotonNetwork.InstantiateSceneObject("Player/PlayerAI", spawns[Random.Range(0, spawns.Length)].position, Quaternion.identity, 0, null);
		}
	}

	private void StartZombie(PlayerAI ai)
	{
		ai.dead = false;
		ai.health = 1000;
		alive++;
	}

	private void DeadZombie(PlayerAI ai)
	{
		deads++;
		alive--;
		if (count <= deads && alive == 0)
		{
			UIToast.Show("Pause 10 sec");
			TimerManager.In(10f, delegate
			{
				count += 5;
				maxInScene++;
				deads = 0;
			});
		}
	}

	private void CreateZombie()
	{
		if (PhotonNetwork.isMasterClient)
		{
			photonView.RPC("PhotonCreateZombie", PhotonTargets.All, (byte)Random.Range(0, spawns.Length));
		}
	}

	[PunRPC]
	private void PhotonCreateZombie(byte spawn)
	{
		PoolManager.Spawn("Zombie", zombiePrefab, spawns[spawn].position, Quaternion.identity.eulerAngles);
	}
}
