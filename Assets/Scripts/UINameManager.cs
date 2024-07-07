using UnityEngine;

public class UINameManager : MonoBehaviour
{
	public Camera m_Camera;

	private UILabel label;

	private int TimerID;

	private string lastName;

	private string hitName;

	private Ray ray;

	private void Start()
	{
		label = UIElements.Get<UILabel>("NameLabel");
	}

	private void OnEnable()
	{
		TimerID = TimerManager.In(0.5f, -1, 0.18f, UpdateName);
	}

	private void OnDisable()
	{
		try
		{
			TimerManager.Cancel(TimerID);
			label.text = string.Empty;
		}
		catch
		{
		}
	}

	private void UpdateName()
	{
		ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
		if (nRaycast.Raycast(ray, 100f, "PlayerName"))
		{
			hitName = nRaycast.hitTransform.root.name;
			if (hitName != lastName)
			{
				ControllerManager component = nRaycast.hitTransform.root.GetComponent<ControllerManager>();
				if (component.PlayerSkin.PlayerTeam == Team.Blue)
				{
					label.effectColor = Color.blue;
				}
				else
				{
					label.effectColor = Color.red;
				}
				lastName = hitName;
			}
			label.text = lastName;
		}
		else
		{
			label.text = string.Empty;
		}
	}
}
