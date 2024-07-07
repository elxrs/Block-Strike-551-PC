using UnityEngine;

public class TicTacToeBlock : MonoBehaviour
{
	public TicTacToe target;

	public MeshEditor meshEditor;

	public int x;

	public int y;

	private void Damage(DamageInfo info)
	{
		target.Click(this, info);
	}
}
