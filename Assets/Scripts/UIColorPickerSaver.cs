using FreeJSON;
using UnityEngine;

public class UIColorPickerSaver : MonoBehaviour
{
	public UIColorPicker ColorPicker;

	public UISprite SelectColor;

	public UILabel SelectColorLabel;

	public GameObject ColorElement;

	public UIGrid ColorElementsGrid;

	public bool isDelete;

	private void Start()
	{
		JsonArray jsonArray = JsonArray.Parse(PlayerPrefs.GetString("WorkshopEditColors"));
		for (int i = 0; i < jsonArray.Length; i++)
		{
			AddColor(jsonArray.Get<Color32>(i), i);
		}
	}

	public void OnSelectColorPicker()
	{
		SelectColor.color = ColorPicker.value;
		Color32 color = SelectColor.color;
		string text = "R: " + color.r + " | G: " + color.g + " | B: " + color.b;
		SelectColorLabel.text = text;
	}

	private void OnClick()
	{
		JsonArray jsonArray = JsonArray.Parse(PlayerPrefs.GetString("WorkshopEditColors"));
		if (jsonArray.Length <= 50 && !jsonArray.Contains((Color32)SelectColor.color))
		{
			jsonArray.Add((Color32)SelectColor.color);
			PlayerPrefs.SetString("WorkshopEditColors", jsonArray.ToString());
			AddColor(SelectColor.color, jsonArray.Length - 1);
		}
	}

	private void AddColor(Color32 color, int id)
	{
		GameObject gameObject = NGUITools.AddChild(ColorElementsGrid.gameObject, ColorElement);
		gameObject.SetActive(true);
		gameObject.GetComponent<UISprite>().color = color;
		gameObject.name = id.ToString();
		ColorElementsGrid.repositionNow = true;
	}

	public void OnSelectColor(UISprite sprite)
	{
		if (isDelete)
		{
			JsonArray jsonArray = JsonArray.Parse(PlayerPrefs.GetString("WorkshopEditColors"));
			jsonArray.Remove((Color32)sprite.color);
			PlayerPrefs.SetString("WorkshopEditColors", jsonArray.ToString());
			Destroy(sprite.cachedGameObject);
			ColorElementsGrid.repositionNow = true;
		}
		else
		{
			ColorPicker.Select(sprite.color);
		}
	}
}
