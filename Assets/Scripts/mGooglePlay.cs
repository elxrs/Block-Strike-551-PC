using UnityEngine;

public class mGooglePlay : MonoBehaviour
{
	public UILabel SignInOutLabel;
	
	private void OnSignIn(string playerID)
	{
		SignInOutLabel.text = Localization.Get("Sign out");
	}

	private void OnSignInError(string error)
	{
		SignInOutLabel.text = Localization.Get("Sign in");
		UIToast.Show(error, 3f);
	}

	private void OnSignOut()
	{
		SignInOutLabel.text = Localization.Get("Sign in");
	}

	public void OnSignInOut()
	{
	}

	public void OnAchievement()
	{
	}
}
