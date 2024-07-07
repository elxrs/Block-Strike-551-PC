using UnityEngine;
using UnityEditor;

public class Tools : MonoBehaviour
{
    [MenuItem("Tools/PlayerPrefs/DeleteAll")]
    private static void PFD(MenuCommand command)
    {
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("Tools/PlayerPrefs/Account/Set Max Money")]
    private static void SetMaxMoney(MenuCommand command)
    {
        SaveLoadManager.SetMoney(10000000);
        EventManager.Dispatch("AccountUpdate");
    }

    [MenuItem("Tools/PlayerPrefs/Account/Set Max Gold")]
    private static void SetMaxGold(MenuCommand command)
    {
        SaveLoadManager.SetGold(100000);
        EventManager.Dispatch("AccountUpdate");
    }

    [MenuItem("Tools/PlayerPrefs/Account/Set Level 250")]
    private static void Set250Level(MenuCommand command)
    {
        SaveLoadManager.SetPlayerLevel(250);
        EventManager.Dispatch("AccountUpdate");
    }
}
