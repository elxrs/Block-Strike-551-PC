using UnityEngine;

public class ZombieBlock : Photon.MonoBehaviour
{
	[Range(1f, 50f)]
	public int ID = 1;

	public int CountAttack = 50;

	[Range(1f, 20f)]
	public int Button = 1;

	public GameObject ActiveBlock;

	private int StartCountAttack = 50;

	private GameObject mGameObject;

	public GameObject cachedGameObject
	{
		get
		{
			if (mGameObject == null)
			{
				mGameObject = gameObject;
			}
			return mGameObject;
		}
	}

	public bool actived
	{
		get
		{
			return cachedGameObject.activeSelf;
		}
		set
		{
			cachedGameObject.SetActive(value);
			if (ActiveBlock != null)
			{
				ActiveBlock.SetActive(!value);
			}
		}
	}

	private void Start()
	{
		StartCountAttack = CountAttack;
		EventManager.AddListener("Button" + Button, ButtonClick);
		EventManager.AddListener("StartRound", StartRound);
		EventManager.AddListener("WaitPlayer", StartRound);
	}

	private void StartRound()
	{
		actived = false;
	}

	private void ButtonClick()
	{
		actived = true;
		CountAttack = StartCountAttack;
	}

	public void Damage(DamageInfo info)
	{
		if (info.AttackerTeam == Team.Red)
		{
			UICrosshair.Hit();
			ZombieMode.AddDamage((byte)ID);
		}
	}

	public void Attack()
	{
		CountAttack -= nValue.int1;
		CountAttack = Mathf.Clamp(CountAttack, nValue.int0, StartCountAttack);
	}
}
