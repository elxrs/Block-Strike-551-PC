using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
	public static InputManager instance;

	public static event Action<string> GetButtonDownEvent;

	public static event Action<string> GetButtonEvent;

	public static event Action<string> GetButtonUpEvent;

	public static event Action<string, float> GetAxisEvent;

    public bool isCursor = true;

    private void Start()
	{
		instance = this;
		DontDestroyOnLoad(gameObject);
	}

	public static void Init()
	{
		if (instance == null)
		{
			GameObject gameObject = new GameObject("InputManager");
			gameObject.AddComponent<InputManager>();
		}
	}

    private void Update()
    {
        if (GameObject.Find("Display") != null)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                SetButtonDown("Fire");
            }
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                SetButtonUp("Fire");
            }
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                SetButtonDown("Aim");
            }
            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                SetButtonUp("Aim");
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                SetButtonDown("Chat");
            }
            if (Input.GetKeyUp(KeyCode.T))
            {
                SetButtonUp("Chat");
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                SetButtonDown("Use");
            }
            if (Input.GetKeyUp(KeyCode.E))
            {
                SetButtonUp("Use");
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                SetButtonDown("Use");
            }
            if (Input.GetKeyUp(KeyCode.B))
            {
                SetButtonUp("Use");
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                SetButtonDown("Reload");
            }
            if (Input.GetKeyUp(KeyCode.R))
            {
                SetButtonUp("Reload");
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SetButtonDown("Jump");
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                SetButtonUp("Jump");
            }
        }
        SetAxis("Mouse X", Input.GetAxis("Mouse X"));
        SetAxis("Mouse Y", Input.GetAxis("Mouse Y"));
        UpdateCursor();
    }

    private void UpdateCursor()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            isCursor = !isCursor;
        }
        Cursor.visible = isCursor;
        Cursor.lockState = isCursor ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public static void SetButtonDown(string name)
	{
		if (GetButtonDownEvent != null)
		{
			GetButtonDownEvent(name);
		}
	}

	public static void SetButtonUp(string name)
	{
		if (GetButtonUpEvent != null)
		{
			GetButtonUpEvent(name);
		}
	}

	public static void SetAxis(string name, float value)
	{
		if (GetAxisEvent != null)
		{
			GetAxisEvent(name, value);
		}
	}
}
