using UnityEngine;
using UnityEngine.UI;
using Rank = MemberEntry.MemberEntryData.Rank;
using DateTimeOffset = System.DateTimeOffset;
using System.Collections.Generic;
using StreamChat.Core.StatefulModels;
using StreamChat.Core.Models;
using UnityEngine.EventSystems;

public class ChatMessageBox : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	//Static
	private static ReactionsDB reactionsDB = null;

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
	[SerializeField] private RectTransform reactionsParent = null;
	[SerializeField] private MessageReaction reactionPrefab = null;

	[Header("Settings"), Tooltip("Time in seconds")]
	[SerializeField] private float refreshInterval = 60f;

	//Events

	//Private
	private float nextRefresh = float.MinValue;
	private DateTimeOffset createdBy;

	private IStreamMessage msg;
	private Dictionary<string, MessageReaction> reactions = new();

	//Methods
	private void Awake()
	{
		if(reactionsDB == null)
			reactionsDB = Resources.Load<ReactionsDB>("ReactionsDB");
	}
	private void Start()
	{
		for(int i = 0; i < reactionsDB.Count; ++i)
			AddReaction(reactionsDB.reactions[i].name);
	}

	private void LateUpdate()
	{
		if(nextRefresh < Time.timeSinceLevelLoad)
		{
			UpdateTimeSent();
			nextRefresh = Time.timeSinceLevelLoad + refreshInterval;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		foreach(KeyValuePair<string, MessageReaction> pair in reactions)
			pair.Value.Show();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		foreach(KeyValuePair<string, MessageReaction> pair in reactions)
			if(pair.Value.Count <= 0)
				pair.Value.Hide();
	}

	private void OnReactionsUpdated(
		IStreamChannel channel, IStreamMessage message, StreamReaction reaction
	)
	{
		//refresh all reactions
	}

	public void Init(IStreamMessage message)
	{
		msg = message;

		msg.ReactionUpdated += OnReactionsUpdated;
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

	public void AddReaction(string reactionID, int count = 0)
	{
		MessageReaction reactionUI;
		if(!reactions.TryGetValue(reactionID, out reactionUI))
		{
			reactionUI = Instantiate(
				reactionPrefab, reactionsParent
			);
			if(reactionUI.Init(reactionID, msg))
				reactions.Add(reactionID, reactionUI);
		}
		reactionUI.Count += count;
	}

	public void RemoveReaction(string reactionID)
	{
		MessageReaction reactionUI;
		if(reactions.TryGetValue(reactionID, out reactionUI))
			if(reactionUI.Count >= 1)
				reactionUI.Count -= 1;
	}
}