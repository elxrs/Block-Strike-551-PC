using UnityEngine;

public class TicTacToe : MonoBehaviour
{
	public TicTacToeBlock[] blocks;

	public Color redTeamColor = Color.red;

	public Color blueTeamColor = Color.blue;

	public byte step;

	public int[,] grid = new int[3, 3];

	private void Start()
	{
		PhotonEvent.AddListener(107, PhotonClick);
	}

	private void PhotonClick(PhotonEventData data)
	{
		TicTacToeBlock ticTacToeBlock = blocks[(byte)data.parameters[0]];
		if (grid[ticTacToeBlock.x, ticTacToeBlock.y] != 0)
		{
			return;
		}
		if (step == 0 || step == (byte)data.parameters[1])
		{
			grid[ticTacToeBlock.x, ticTacToeBlock.y] = (byte)data.parameters[1];
			if (grid[ticTacToeBlock.x, ticTacToeBlock.y] == 1)
			{
				step = 2;
				ticTacToeBlock.meshEditor.meshRenderer.enabled = true;
				ticTacToeBlock.meshEditor.color = blueTeamColor;
				ticTacToeBlock.meshEditor.uvRect = new Rect(0.5f, 0f, 0.11f, 0.11f);
			}
			else
			{
				step = 1;
				ticTacToeBlock.meshEditor.meshRenderer.enabled = true;
				ticTacToeBlock.meshEditor.color = redTeamColor;
				ticTacToeBlock.meshEditor.uvRect = new Rect(0.625f, 0f, 0.11f, 0.11f);
			}
		}
		if (PhotonNetwork.isMasterClient)
		{
			PhotonEvent.RPC(108, PhotonTargets.All, data.parameters[0], data.parameters[1]);
		}
	}

	public void Click(TicTacToeBlock block, DamageInfo info)
	{
		if (grid[block.x, block.y] != 0)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < blocks.Length; i++)
		{
			if (block == blocks[i])
			{
				num = i;
				break;
			}
		}
		PhotonEvent.RPC(107, PhotonTargets.MasterClient, (byte)num, (byte)info.AttackerTeam);
	}

	public Team isWin()
	{
		if (grid[0, 0] != 0 && grid[1, 1] != 0 && grid[2, 2] != 0 && grid[0, 0] == grid[1, 1] && grid[0, 0] == grid[2, 2])
		{
			if (grid[0, 0] == 1)
			{
				return Team.Blue;
			}
			if (grid[0, 0] == 2)
			{
				return Team.Red;
			}
		}
		if (grid[0, 2] != 0 && grid[1, 1] != 0 && grid[2, 0] != 0 && grid[0, 2] == grid[1, 1] && grid[0, 2] == grid[2, 0])
		{
			if (grid[0, 2] == 1)
			{
				return Team.Blue;
			}
			if (grid[0, 2] == 2)
			{
				return Team.Red;
			}
		}
		for (int i = 0; i < 3; i++)
		{
			if (grid[i, 0] != 0 && grid[i, 1] != 0 && grid[i, 2] != 0 && grid[i, 0] == grid[i, 1] && grid[i, 0] == grid[i, 2])
			{
				if (grid[i, 0] == 1)
				{
					return Team.Blue;
				}
				if (grid[i, 0] == 2)
				{
					return Team.Red;
				}
			}
			if (grid[0, i] != 0 && grid[1, i] != 0 && grid[2, i] != 0 && grid[0, i] == grid[1, i] && grid[0, i] == grid[2, i])
			{
				if (grid[0, i] == 1)
				{
					return Team.Blue;
				}
				if (grid[0, i] == 2)
				{
					return Team.Red;
				}
			}
		}
		return Team.None;
	}
}
