using System;
using UnityEngine;

public class CasinoManager : MonoBehaviour
{
	[Serializable]
	public class SlotClass
	{
		public UIScrollView slot;

		public bool active;

		public float pos;

		public int value;
	}

	public SlotClass[] slots;

	public Vector2 speed;

	public bool isActive;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F))
		{
			isActive = true;
			for (int i = 0; i < slots.Length; i++)
			{
				slots[i].active = true;
				int num = UnityEngine.Random.Range(1, 10);
				print(num);
				slots[i].pos = slots[i].pos + (num * 100) + ((i + 3) * 1000);
				slots[i].value += num;
				if (slots[i].value >= 10)
				{
					slots[i].value -= 10;
				}
			}
		}
		if (!isActive)
		{
			return;
		}
		for (int j = 0; j < slots.Length; j++)
		{
			if (!slots[j].active)
			{
				continue;
			}
			slots[j].slot.MoveRelative(speed);
			if (slots[j].slot.panel.cachedTransform.localPosition.y <= 0f - slots[j].pos)
			{
				slots[j].active = false;
				if (j == slots.Length - 1)
				{
					isActive = false;
				}
			}
		}
	}
}
