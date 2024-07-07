using UnityEngine;

public class mFriendsElement : MonoBehaviour
{
	public UIWidget Widget;

	public UILabel Name;

	public UISprite StatusSprite;

	public UITexture InRoomTexture;

	private int playerID;

	public FriendInfo info;

	public int ID
	{
		get
		{
			return playerID;
		}
	}

	public void SetData(int id)
	{
		if (!Widget.cachedGameObject.activeSelf)
		{
			return;
		}
		playerID = id;
		string @string = CryptoPrefs.GetString("Friend_#" + id, "#" + id);
		Name.text = @string;
		if (PhotonNetwork.Friends != null)
		{
			for (int i = 0; i < PhotonNetwork.Friends.Count; i++)
			{
				if (PhotonNetwork.Friends[i].Name == @string)
				{
					info = PhotonNetwork.Friends[i];
					StatusSprite.cachedGameObject.SetActive(true);
					StatusSprite.color = ((!info.IsOnline) ? new Color(0.83f, 0.18f, 0.18f, 1f) : new Color(0.31f, 0.71f, 0.32f, 1f));
					InRoomTexture.cachedGameObject.SetActive(info.IsInRoom);
					name = ((!info.IsOnline) ? ("1-" + info.Name) : "0");
				}
			}
		}
		else
		{
			StatusSprite.cachedGameObject.SetActive(false);
		}
	}

	public void UpdateStatus(FriendInfo info)
	{
		if (!(info.Name != Name.text) && Widget.cachedGameObject.activeSelf)
		{
			StatusSprite.cachedGameObject.SetActive(true);
			StatusSprite.color = ((!info.IsOnline) ? new Color(0.83f, 0.18f, 0.18f, 1f) : new Color(0.31f, 0.71f, 0.32f, 1f));
			InRoomTexture.cachedGameObject.SetActive(info.IsInRoom);
			name = ((!info.IsOnline) ? ("1-" + info.Name) : "0");
		}
	}
}
