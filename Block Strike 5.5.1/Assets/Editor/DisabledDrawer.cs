using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(DisabledAttribute))]
public class DisabledDrawer : PropertyDrawer
{
	public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
	{
		using (new EditorGUI.DisabledScope(true))
		{
			EditorGUI.PropertyField(pos, property, label);
		}
	}
}
