using System.Collections.Generic;
using Photon;

public class TNTRunManager : PunBehaviour
{
	public TNTRunBlock[] Blocks;

	private List<short> DisabledBlocks = new List<short>();

	private static TNTRunManager instance;

	private void Awake()
	{
		PhotonClassesManager.Add(this);
	}

	private void Start()
	{
		instance = this;
		PhotonEvent.AddListener(1, PhotonDeleteBlock);
		PhotonEvent.AddListener(2, PhotonDeleteBlocks);
	}

	public override void OnPhotonPlayerConnected(PhotonPlayer playerConnect)
	{
		if (!PhotonNetwork.isMasterClient)
		{
			return;
		}
		TimerManager.In(1f, delegate
		{
			if (GameManager.GetRoundState() == RoundState.PlayRound)
			{
				PhotonEvent.RPC(2, PhotonTargets.All, DisabledBlocks.ToArray());
			}
		});
	}

	public static void DeleteBlock(int id)
	{
		PhotonEvent.RPC(1, PhotonTargets.All, (short)id);
	}

	private void PhotonDeleteBlock(PhotonEventData data)
	{
		short num = (short)data.parameters[0];
		Blocks[num].SetActive(false);
		DisabledBlocks.Add(num);
	}

	private void PhotonDeleteBlocks(PhotonEventData data)
	{
		if (GameManager.GetRoundState() == RoundState.PlayRound)
		{
			short[] array = (short[])data.parameters[0];
			for (int i = 0; i < array.Length; i++)
			{
				Blocks[array[i]].SetActive(false);
				DisabledBlocks.Add(array[i]);
			}
		}
	}

	public static void ResetBlocks()
	{
		for (int i = 0; i < instance.DisabledBlocks.Count; i++)
		{
			instance.Blocks[instance.DisabledBlocks[i]].SetActive(true);
		}
		instance.DisabledBlocks.Clear();
	}
}
