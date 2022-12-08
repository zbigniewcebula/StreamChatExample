using StreamChat.Core.Models;
using StreamChat.Core.StatefulModels;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MessageReaction : MonoBehaviour
{
	public Sprite Icon
	{
		get => image.sprite;
		set => image.sprite = value;
	}
	public int Count
	{
		get => counter;
		set
		{
			counter = value;
			count.text = value.ToString();
			if(value > 0)
				Show();
			else
				Hide();
		}
	}

	//Public/Inspector
	[Header("Bindings")]
	[SerializeField] private Button button = null;
	[SerializeField] private Image image = null;
	[SerializeField] private Text count = null;
	[SerializeField] private CanvasGroup canvasGroup = null;
	[SerializeField] private ReactionsDB reactionsDB = null;

	//Events

	//Private
	private int counter = 0;
	private IStreamMessage msg;

	//Methods
	public bool Init(string reactionID, IStreamMessage message)
	{
		Icon = reactionsDB.GetReactionIcon(reactionID);
		if(Icon == null)
		{
			Debug.LogError(
				$"[MessageReaction] No icon in reaction database under id: {reactionID}"
			);
			Destroy(gameObject);
			return false;
		}
		Count = 0;
		msg = message;

		msg.ReactionAdded += OnReactionAdded;
		msg.ReactionRemoved += OnReactionRemoved;

		if(msg.ReactionScores.TryGetValue(reactionID, out int temp))
			Count = temp;

		button.onClick.AddListener(SendAddReaction);

		return true;
	}

	private void SendAddReaction()
	{
		if(msg.OwnReactions.Count > 0)
		{
			foreach(var reaction in msg.OwnReactions)
				msg.DeleteReactionAsync(reaction.Type);
		}

		msg.SendReactionAsync(
			Icon.name, skipMobilePushNotifications: true
		);
	}

	private void OnReactionAdded(
		IStreamChannel channel, IStreamMessage message, StreamReaction reaction
	)
	{
		if(message.Id != msg.Id
		|| reaction.Type != Icon.name
		)
			return;

		++Count;
	}

	private void OnReactionRemoved(
		IStreamChannel channel, IStreamMessage message, StreamReaction reaction
	)
	{
		if(message.Id != msg.Id
		|| reaction.Type != Icon.name
		)
			return;

		--Count;
	}

	public void Show()
	{
		canvasGroup.alpha = 1f;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
	}

	public void Hide()
	{
		canvasGroup.alpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
	}
}