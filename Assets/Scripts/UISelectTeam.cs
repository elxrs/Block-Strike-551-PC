using System.Text;
using UnityEngine;

public class UISelectTeam : MonoBehaviour
{
	public UILabel BluePlayersLabel;

	public UILabel RedPlayersLabel;

	public UILabel BlueCountLabel;

	public UILabel RedCountLabel;

	public UILabel BlueScoreLabel;

	public UILabel RedScoreLabel;

	public GameObject SpectatorsButton;

	private bool isSpectator;

	private int redPlayersCount;

	private int bluePlayersCount;

	private int Timer;

	private static UISelectTeam instance;

	private void Awake()
	{
		instance = this;
	}

	public static void OnStart()
	{
		UIPanelManager.ShowPanel("SelectTeam");
		instance.UpdateList();
		instance.Timer = TimerManager.In(0.1f, -1, 0.1f, instance.UpdateList);
	}

	public static void OnSpectator()
	{
		instance.isSpectator = true;
	}

	private void UpdateList()
	{
		PhotonPlayer[] otherPlayers = PhotonNetwork.otherPlayers;
		redPlayersCount = 0;
		bluePlayersCount = 0;
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		for (int i = 0; i < otherPlayers.Length; i++)
		{
			if (otherPlayers[i].GetTeam() == Team.Blue)
			{
				stringBuilder2.AppendLine(otherPlayers[i].NickName);
				bluePlayersCount++;
			}
			else if (otherPlayers[i].GetTeam() == Team.Red)
			{
				stringBuilder.AppendLine(otherPlayers[i].NickName);
				redPlayersCount++;
			}
		}
		BluePlayersLabel.text = stringBuilder2.ToString();
		RedPlayersLabel.text = stringBuilder.ToString();
		BlueCountLabel.text = bluePlayersCount + "/" + PhotonNetwork.room.MaxPlayers / 2;
		RedCountLabel.text = redPlayersCount + "/" + PhotonNetwork.room.MaxPlayers / 2;
		BlueScoreLabel.text = GameManager.BlueScore.ToString();
		RedScoreLabel.text = GameManager.RedScore.ToString();
		if (isSpectator)
		{
			if (bluePlayersCount > 1 && redPlayersCount > 0 && !PhotonNetwork.isMasterClient)
			{
				instance.SpectatorsButton.SetActive(true);
			}
		}
		else
		{
			instance.SpectatorsButton.SetActive(false);
		}
	}

	public void SelectTeam(int team)
	{
		if (HasConnectTeam((Team)team) || team == 0)
		{
			GameManager.OnSelectTeam((Team)team);
			TimerManager.Cancel(Timer);
		}
	}

	private bool HasConnectTeam(Team team)
	{
		switch (team)
		{
		case Team.Blue:
			if (bluePlayersCount - redPlayersCount >= 1)
			{
				return false;
			}
			if (PhotonNetwork.room.MaxPlayers / 2 == bluePlayersCount)
			{
				return false;
			}
			return true;
		case Team.Red:
			if (redPlayersCount - bluePlayersCount >= 1)
			{
				return false;
			}
			if (PhotonNetwork.room.MaxPlayers / 2 == redPlayersCount)
			{
				return false;
			}
			return true;
		default:
			return false;
		}
	}
}
