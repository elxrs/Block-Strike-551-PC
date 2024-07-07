using System;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
	[Serializable]
	public class WeaponAnimData
	{
		public Key[] defaultValue;

		public Key[] maxRotateValue;

		public Key[] minRotateValue;

		public Keys[] reloadValue;

		public int reloadIndex;

		public int maxReloadIndex;

		public bool reloading;

		public bool active;

		public void SetDefault()
		{
			for (int i = 0; i < defaultValue.Length; i++)
			{
				defaultValue[i].target.localPosition = defaultValue[i].pos;
				defaultValue[i].target.localRotation = defaultValue[i].rot;
			}
		}

		public void SetRotate(float rotate)
		{
			if (rotate >= 0f)
			{
				for (int i = 0; i < maxRotateValue.Length; i++)
				{
					maxRotateValue[i].target.localPosition = Vector3Lerp(defaultValue[i + 3].pos, maxRotateValue[i].pos, rotate);
					maxRotateValue[i].target.localRotation = Quaternion.Lerp(defaultValue[i + 3].rot, maxRotateValue[i].rot, rotate);
				}
				return;
			}
			rotate = Mathf.Abs(rotate);
			for (int j = 0; j < minRotateValue.Length; j++)
			{
				minRotateValue[j].target.localPosition = Vector3Lerp(defaultValue[j + 3].pos, minRotateValue[j].pos, rotate);
				minRotateValue[j].target.localRotation = Quaternion.Lerp(defaultValue[j + 3].rot, minRotateValue[j].rot, rotate);
			}
		}

		public void StartReload()
		{
			reloadIndex = 0;
			reloading = true;
		}

		public void StopReload()
		{
			reloadIndex = 0;
			reloading = false;
			SetDefault();
		}

		public void UpdateReload()
		{
			reloadIndex++;
			if (reloadIndex >= maxReloadIndex)
			{
				StopReload();
				return;
			}
			for (int i = 0; i < reloadValue.Length; i++)
			{
				reloadValue[i].target.localPosition = reloadValue[i].pos[reloadIndex];
				reloadValue[i].target.localRotation = reloadValue[i].rot[reloadIndex];
			}
		}

		private Vector3 Vector3Lerp(Vector3 from, Vector3 to, float t)
		{
			from.x += (to.x - from.x) * t;
			from.y += (to.y - from.y) * t;
			from.z += (to.z - from.z) * t;
			return from;
		}
	}

	[Serializable]
	public class LegsAnimData
	{
		public Key[] defaultValue;

		public Key[] groundedValue;

		public Keys[] moveValue;

		public int moveIndex;

		public bool active;

		public void SetDefault()
		{
			for (int i = 0; i < defaultValue.Length; i++)
			{
				defaultValue[i].target.localPosition = defaultValue[i].pos;
				defaultValue[i].target.localRotation = defaultValue[i].rot;
			}
		}

		public void SetGrounded(bool grounded)
		{
			if (!grounded)
			{
				for (int i = 0; i < defaultValue.Length; i++)
				{
					groundedValue[i].target.localPosition = groundedValue[i].pos;
					groundedValue[i].target.localRotation = groundedValue[i].rot;
				}
			}
			else
			{
				SetDefault();
			}
		}

		public void UpdateMove(float speed)
		{
			if (speed >= 0f)
			{
				moveIndex++;
				if (moveIndex >= moveValue[0].pos.Length)
				{
					moveIndex = 0;
				}
			}
			else
			{
				moveIndex--;
				if (moveIndex <= 0)
				{
					moveIndex = moveValue[0].pos.Length - 1;
				}
				speed = Mathf.Abs(speed);
			}
			if (active)
			{
				for (int i = 0; i < moveValue.Length; i++)
				{
					moveValue[i].target.localPosition = Vector3Lerp(defaultValue[i].pos, moveValue[i].pos[moveIndex], speed);
					moveValue[i].target.localRotation = Quaternion.Lerp(defaultValue[i].rot, moveValue[i].rot[moveIndex], speed);
				}
			}
		}

		private Vector3 Vector3Lerp(Vector3 from, Vector3 to, float t)
		{
			from.x += (to.x - from.x) * t;
			from.y += (to.y - from.y) * t;
			from.z += (to.z - from.z) * t;
			return from;
		}
	}

	[Serializable]
	public class Key
	{
		public Transform target;

		public Vector3 pos;

		public Quaternion rot;
	}

	[Serializable]
	public class Keys
	{
		public Transform target;

		public Vector3[] pos;

		public Quaternion[] rot;
	}

	public WeaponType selectWeapon;

	public WeaponAnimData rifle;

	public WeaponAnimData pistol;

	public WeaponAnimData knife;

	public LegsAnimData legs;

	public Transform root;

	private float cachedRotate;

	private bool cachedGrounded;

	private float cachedMove;

	private bool cachedReload;

	private Vector3 rootPosition = Vector3.zero;

	public float rotate
	{
		get
		{
			return cachedRotate;
		}
		set
		{
			if (cachedRotate != value)
			{
				cachedRotate = value;
				if (!cachedReload)
				{
					GetSelectWeapon().SetRotate(value);
				}
			}
		}
	}

	public bool grounded
	{
		get
		{
			return cachedGrounded;
		}
		set
		{
			if (cachedGrounded != value)
			{
				cachedGrounded = value;
				legs.SetGrounded(value);
			}
		}
	}

	public bool reload
	{
		get
		{
			return cachedReload;
		}
		set
		{
			if (selectWeapon == WeaponType.Knife)
			{
				return;
			}
			if (value)
			{
				if (cachedReload)
				{
					GetSelectWeapon().StopReload();
				}
				cachedReload = value;
				GetSelectWeapon().StartReload();
			}
			else
			{
				cachedReload = value;
				GetSelectWeapon().StopReload();
			}
		}
	}

	public float move
	{
		get
		{
			return cachedMove;
		}
		set
		{
			if (value == 0f)
			{
				if (cachedMove != 0f && cachedGrounded)
				{
					legs.UpdateMove(value);
					cachedMove = value;
				}
			}
			else
			{
				cachedMove = value;
			}
		}
	}

	public Vector3 rootPos
	{
		get
		{
			return rootPosition;
		}
		set
		{
			rootPosition = value;
			root.localPosition = rootPosition;
		}
	}

	private void OnEnable()
	{
		rifle.active = true;
		pistol.active = true;
		knife.active = true;
		legs.active = true;
	}

	private void OnDisable()
	{
		rifle.active = false;
		pistol.active = false;
		knife.active = false;
		legs.active = false;
	}

	private void FixedUpdate()
	{
		if (cachedMove != 0f && cachedGrounded)
		{
			legs.UpdateMove(cachedMove);
		}
		if (cachedReload)
		{
			GetSelectWeapon().UpdateReload();
			if (!GetSelectWeapon().reloading)
			{
				GetSelectWeapon().StopReload();
				GetSelectWeapon().SetRotate(cachedRotate);
				cachedReload = false;
			}
		}
	}

	private WeaponAnimData GetSelectWeapon()
	{
		switch (selectWeapon)
		{
		case WeaponType.Rifle:
			return rifle;
		case WeaponType.Pistol:
			return pistol;
		case WeaponType.Knife:
			return knife;
		default:
			return rifle;
		}
	}

	public void SetWeapon(WeaponType type)
	{
		if (type != selectWeapon)
		{
			if (GetSelectWeapon().reloading)
			{
				GetSelectWeapon().StopReload();
			}
			selectWeapon = type;
			reload = false;
			switch (type)
			{
			case WeaponType.Rifle:
				rifle.SetDefault();
				break;
			case WeaponType.Pistol:
				pistol.SetDefault();
				break;
			case WeaponType.Knife:
				knife.SetDefault();
				break;
			}
		}
	}

	public void SetDefault()
	{
		root.localPosition = rootPos;
		root.localEulerAngles = Vector3.zero;
		SetWeapon(selectWeapon);
		cachedRotate = 0f;
		cachedReload = false;
		cachedMove = 0f;
		cachedGrounded = true;
		WeaponAnimData weaponAnimData = GetSelectWeapon();
		weaponAnimData.SetDefault();
		weaponAnimData.SetRotate(rotate);
		legs.SetGrounded(grounded);
	}
}
