using UnityEngine;
using UnityEngine.UI;
using Rank = MemberEntry.MemberEntryData.Rank;
using DateTimeOffset = System.DateTimeOffset;

public class ChatMessageBox : MonoBehaviour
{
	//Static

	//Accessor
	public string Author
	{
		get => author.text;
		set => author.text = value;
	}
	public string Message
	{
		get => message.text;
		set => message.text = value;
	}
	public Rank Rank
	{
		get
		{
			if(!System.Enum.TryParse(rank.text, out Rank temp))
				return temp;
			return Rank.None;
		}
		set => rank.text = value.ToString();
	}
	public DateTimeOffset CreationTime
	{
		get => createdBy;
		set
		{
			createdBy = value;
			UpdateTimeSent();
		}
	}

	//Public/Inspector
	[Header("Bindings")]
	[SerializeField] private Text author = null;
	[SerializeField] private Text message = null;
	[SerializeField] private Text timeSent = null;
	[SerializeField] private Text rank = null;

	[Header("Settings"), Tooltip("Time in seconds")]
	[SerializeField] private float refreshInterval = 60f;

	//Events

	//Private
	private float nextRefresh = float.MinValue;
	private DateTimeOffset createdBy;

	//Methods
	private void LateUpdate()
	{
		if(nextRefresh < Time.timeSinceLevelLoad)
		{
			UpdateTimeSent();
			nextRefresh = Time.timeSinceLevelLoad + refreshInterval;
		}
	}

	private void UpdateTimeSent()
	{
		var diff = DateTimeOffset.UtcNow - createdBy;
		if(diff.TotalSeconds < 60)
		{
			timeSent.text = "Just now";
		}
		else if(diff.TotalMinutes < 60)
		{
			timeSent.text = $"{diff.Minutes}m ago";
		}
		else if(diff.TotalHours < 24)
		{
			timeSent.text = $"{diff.Hours}h ago";
		}
		else if(diff.TotalDays < 7)
		{
			timeSent.text = $"{diff.Days}d ago";
		}
		else
		{
			timeSent.text = $"{createdBy.Month}/{createdBy.Day}/{createdBy.Year} "
				+ $"{createdBy.Hour}:{createdBy.Minute}";
		}
	}
}