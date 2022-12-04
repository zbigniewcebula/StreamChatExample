using StreamChat.Core;
using StreamChat.Core.LowLevelClient.Models;
using StreamChat.Core.LowLevelClient.Requests;
using StreamChat.Core.Models;
using StreamChat.Core.Requests;
using StreamChat.Core.StatefulModels;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ChatPanel : MonoBehaviour
{
	//Static
	public static ChannelState CurrentClan { get; private set; }

	//Accessor

	//Public/Inspector
	[Header("Bindings")]
	[SerializeField] private GameObject errorStateScreen = null;
	[SerializeField] private GameObject emptyStateScreen = null;
	[SerializeField] private GameObject chatStateScreen = null;

	[Header("Bindings/Messages")]
	[SerializeField] private RectTransform messagesBoxParent = null;
	[SerializeField] private Button sendMessageButton = null;
	[SerializeField] private InputField messageTextField = null;

	[Header("Bindings/Prefabs")]
	[SerializeField] private GameObject currentUserMessageEntryPrefab = null;
	[SerializeField] private GameObject messageEntryPrefab = null;

	//Events

	//Private

	//Methods
	private void Awake()
	{
		sendMessageButton.onClick.AddListener(SendMessage);
	}

	private IEnumerator Start()
	{
		while(StreamManager.Client.ConnectionState != ConnectionState.Connected)
			yield return null;

		ClanChatUpdate();
	}

	private async Task ClanChatUpdate()
	{
		errorStateScreen.SetActive(true);
		emptyStateScreen.SetActive(false);
		chatStateScreen.SetActive(false);

		//Fetch Clan channel
		if(CurrentPlayerCache.CurrentClan == null)
			await CurrentPlayerCache.FetchCurrentClanAsync(PrepareChat);
	}

	private void PrepareChat(IStreamChannel state)
	{
		if(state.Members == null
		|| state.Messages == null
		)
			return;

		errorStateScreen.SetActive(false);
		emptyStateScreen.SetActive(false);
		chatStateScreen.SetActive(true);

		state.MessageReceived += OnNewMessageAdded;

		foreach(Message msg in state.Messages)
		{
			//Create message entry
			var box = Instantiate(
				msg.User.Id == StreamManager.Client.LocalUserData.UserId ?
				currentUserMessageEntryPrefab : messageEntryPrefab,
				messagesBoxParent
			).GetComponent<ChatMessageBox>();

			box.Message = msg.Text;
			box.Author = msg.User.Name; //TODO: Username fetching
			if(msg.CreatedAt != null)
				box.CreationTime = msg.CreatedAt.Value;
			//box.Rank = //TODO: User rank fetching
		}
	}

	private void OnNewMessageAdded(IStreamChannel channel, IStreamMessage msg)
	{
		var box = Instantiate(
			msg.User.Id == StreamManager.Client.LocalUserData.UserId ?
			currentUserMessageEntryPrefab : messageEntryPrefab,
			messagesBoxParent
		).GetComponent<ChatMessageBox>();

		box.Message = msg.Text;
		box.Author = msg.User.Name; //TODO: Username fetching
		if(msg.CreatedAt != null)
			box.CreationTime = msg.CreatedAt;
		//box.Rank = //TODO: User rank fetching
	}

	private void SendMessage()
	{
		if(!chatStateScreen.activeSelf)
			return;

		SendMessageAsync(messageTextField.text);
	}

	private async Task SendMessageAsync(string msg)
	{
		var response = await CurrentPlayerCache.CurrentClan
			.SendNewMessageAsync(msg);

		//if(response)
	}
}