using UnityEngine;
using UnityEngine.UI;

public class EmblemIcon : MonoBehaviour
{
	//Static

	//Accessor
	public int IconID
	{
		get { return iconID; }
		set
		{
			try
			{
				image.sprite = icons[value % icons.Length];
				iconID = value % icons.Length;
			}
			catch(System.IndexOutOfRangeException)
			{
				throw new System.IndexOutOfRangeException(
					$"EmblemIcon.SetIcon: id [{value}] out of range [{icons.Length}]"
				);
			}
		}
	}

	//Public/Inspector
	[Header("Bindings")]
	[SerializeField] private Image image = null;

	[Header("Icons")]
	[SerializeField] private Sprite[] icons = null;

	//Events

	//Private
	private int iconID = 0;

	//Methods
}