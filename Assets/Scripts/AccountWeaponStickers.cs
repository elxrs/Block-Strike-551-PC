using System;
using System.Collections.Generic;

[Serializable]
public class AccountWeaponStickers
{
	public CryptoInt SkinID;

	public List<AccountWeaponStickerData> StickerData = new List<AccountWeaponStickerData>();

	public void SortWeaponStickerData()
	{
		StickerData.Sort(SortWeaponStickerDataComparer);
	}

	private int SortWeaponStickerDataComparer(AccountWeaponStickerData a, AccountWeaponStickerData b)
	{
		return ((int)a.Index).CompareTo(b.Index);
	}

	public int[] ToArray()
	{
		SortWeaponStickerData();
		if (StickerData.Count == 0)
		{
			return new int[0];
		}
		int[] array = new int[(int)StickerData[StickerData.Count - 1].Index];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = -1;
		}
		for (int j = 0; j < StickerData.Count; j++)
		{
			array[(int)StickerData[j].Index - 1] = StickerData[j].StickerID;
		}
		return array;
	}
}
