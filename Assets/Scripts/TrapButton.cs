using UnityEngine;

public class TrapButton : MonoBehaviour
{
	public Team PlayerTeam = Team.Red;

	[Range(1f, 30f)]
	public int Key;

	public KeyCode Keycode;

	public MeshRenderer ButtonRenderer;

	public Transform ButtonRedBlock;

	[Header("Weapon Settings")]
	public bool Weapon;

	[SelectedWeapon(WeaponType.Rifle)]
	public int SelectWeapon;

	private bool isTrigger;

	private bool isClickButton;

	private bool Active = true;

	private void Start()
	{
		EventManager.AddListener("StartRound", StartRound);
		EventManager.AddListener("WaitPlayer", StartRound);
		PhotonEvent.AddListener(PhotonEventTag.ClickButton, DeactiveButton);
	}

	private void OnEnable()
	{
		InputManager.GetButtonDownEvent += GetButtonDown;
	}

	private void OnDisable()
	{
		InputManager.GetButtonDownEvent -= GetButtonDown;
	}

	private void GetButtonDown(string name)
	{
		if (name == "Fire" && GameManager.GetRoundState() == RoundState.PlayRound && isTrigger && !isClickButton && ButtonRenderer.isVisible)
		{
			ClickButton();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!isClickButton && other.CompareTag("Player") && PlayerInput.instance.PlayerTeam == PlayerTeam)
		{
			isTrigger = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!isClickButton && other.CompareTag("Player") && PlayerInput.instance.PlayerTeam == PlayerTeam)
		{
			isTrigger = false;
		}
	}

	private void StartRound()
	{
		isClickButton = false;
		isTrigger = false;
		Active = false;
		TimerManager.In(nValue.float02, delegate
		{
			Active = true;
			if (ButtonRedBlock != null)
			{
				ButtonRedBlock.localPosition = Vector3.zero;
			}
		});
	}

	private void ClickButton()
	{
		if (!Active)
		{
			return;
		}
		PhotonEvent.RPC(PhotonEventTag.ClickButton, PhotonTargets.All, Key);
		isClickButton = true;
		isTrigger = false;
		if (Weapon)
		{
			GameManager.GetController().PlayerInput.PlayerWeapon.CanFire = false;
			TimerManager.In(nValue.float005, delegate
			{
				WeaponManager.SetSelectWeapon(WeaponType.Rifle, SelectWeapon);
				GameManager.GetController().PlayerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Rifle);
				GameManager.GetController().PlayerInput.PlayerWeapon.CanFire = true;
			});
		}
	}

	public void DeactiveButton(PhotonEventData data)
	{
		if ((int)data.parameters[nValue.int0] == Key)
		{
			DeactiveButton();
			EventManager.Dispatch("Button" + Key);
		}
	}

	public void DeactiveButton()
	{
		isClickButton = true;
		isTrigger = false;
		if (ButtonRedBlock != null)
		{
			ButtonRedBlock.localPosition = Vector3.down * nValue.float005;
		}
	}

	[ContextMenu("Click Button")]
	private void GetClickButton()
	{
		ClickButton();
	}
}
