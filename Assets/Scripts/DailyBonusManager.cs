using System;
using UnityEngine;

public class DailyBonusManager : MonoBehaviour
{
	[Serializable]
	public class BonusClass
	{
		public CryptoInt Money;

		public CryptoInt Gold;

		public CryptoInt BaseCase;

		public CryptoInt ProfessionalCase;

		public CryptoInt LegendaryCase;
	}

	public BonusClass[] Bonus;

	private static DailyBonusManager instance;

	private void Start()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}
}
