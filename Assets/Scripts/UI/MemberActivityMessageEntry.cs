using UnityEngine;
using UnityEngine.UI;

public class MemberActivityMessageEntry : MonoBehaviour
{
	//Static

	//Accessor

	//Public/Inspector
	[Header("Bindings")]
	[SerializeField] private Text message = null;
	[SerializeField] private Image background = null;

	[Header("Settings/Join")]
	[SerializeField] private string joinMessage = "{0} joined the clan";
	[SerializeField] private Sprite joinBG = null;

	[Header("Settings/Left")]
	[SerializeField] private string leftMessage = "{0} has left the clan";
	[SerializeField] private Sprite leftBG = null;

	//Events

	//Private

	//Methods
	public void SetJoin(string nickname)
	{
		message.text = string.Format(joinMessage, nickname);
		background.sprite = joinBG;
	}

	public void SetLeft(string nickname)
	{
		message.text = string.Format(leftMessage, nickname);
		background.sprite = leftBG;
	}
}