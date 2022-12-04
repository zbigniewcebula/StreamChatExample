using UnityEngine.UI;

public static class Extension
{
	public static void Invoke(this Button btn) => btn.onClick.Invoke();
}