using UnityEngine;

public class UIPlayerStatisticsElement : MonoBehaviour
{
	public UILabel PlayerNameLabel;

	public UITexture AvatarTexture;

	public UILabel LevelLabel;

	public UILabel ClanTagLabel;

	public UILabel KillsLabel;

	public UILabel DeathsLabel;

	public UILabel PingLabel;

	public UIWidget Widget;

	public UIDragScrollView DragScroll;

	private Color32 AdminColor = new Color32(60, 181, 232, byte.MaxValue);

	private Color32 LocalPlayerColor = Color.green;

	public PhotonPlayer PlayerInfo;

	private int Timer;

	private Transform mTransform;

	public Transform cachedTransform
	{
		get
		{
			if (mTransform == null)
			{
				mTransform = transform;
			}
			return mTransform;
		}
	}

	private void OnDisable()
	{
		TimerManager.Cancel(Timer);
	}

	public void SetData(PhotonPlayer playerInfo)
	{
		PlayerInfo = playerInfo;
		string text = playerInfo.GetClan().ToUpper();
		ClanTagLabel.text = text;
		AvatarTexture.mainTexture = AvatarManager.Get(playerInfo.GetAvatarUrl());
		PlayerNameLabel.text = UpdatePlayerName(playerInfo);
		LevelLabel.text = playerInfo.GetLevel().ToString();
		KillsLabel.text = PlayerInfo.GetKills().ToString();
		DeathsLabel.text = PlayerInfo.GetDeaths().ToString();
		PingLabel.text = PlayerInfo.GetPing().ToString();
		name = PlayerNameLabel.text;
		if (playerInfo.GetDead())
		{
			Widget.alpha = 0.5f;
		}
		else
		{
			Widget.alpha = 1f;
		}
		Widget.Update();
		if (playerInfo.IsLocal)
		{
			PlayerNameLabel.color = LocalPlayerColor;
			LevelLabel.color = LocalPlayerColor;
			KillsLabel.color = LocalPlayerColor;
			DeathsLabel.color = LocalPlayerColor;
			PingLabel.color = LocalPlayerColor;
		}
		else if (playerInfo.IsMasterClient)
		{
			PlayerNameLabel.color = AdminColor;
			LevelLabel.color = AdminColor;
			KillsLabel.color = AdminColor;
			DeathsLabel.color = AdminColor;
			PingLabel.color = AdminColor;
		}
		else
		{
			PlayerNameLabel.color = Color.white;
			LevelLabel.color = Color.white;
			KillsLabel.color = Color.white;
			DeathsLabel.color = Color.white;
			PingLabel.color = Color.white;
		}
		if (!TimerManager.IsActive(Timer))
		{
			Timer = TimerManager.In(3f, -1, 3f, UpdateData);
		}
	}

	private string UpdatePlayerName(PhotonPlayer player)
	{
		if ((PhotonNetwork.room.GetGameMode() == GameMode.Bomb || PhotonNetwork.room.GetGameMode() == GameMode.Bomb2) && PhotonNetwork.player.GetTeam() != Team.Blue && BombManager.GetPlayerBombID() != -1 && player.ID == BombManager.GetPlayerBombID())
		{
			return PlayerInfo.NickName + "   " + Utils.GetSpecialSymbol(98);
		}
		return PlayerInfo.NickName;
	}

	private void UpdateData()
	{
		SetData(PlayerInfo);
	}
}
