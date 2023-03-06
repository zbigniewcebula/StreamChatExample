using StreamChat.Core;
using StreamChat.Core.LowLevelClient.Models;
using StreamChat.Core.Requests;
using StreamChat.Core.StatefulModels;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ChatPanel : MonoBehaviour
{
	//Static
	public static IStreamChannel CurrentClan { get; private set; }

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
	[SerializeField] private MemberActivityMessageEntry memberActivityEntryPrefab = null;

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

		foreach(IStreamMessage msg in state.Messages)
			OnNewMessageAdded(state, msg);
	}

	private void OnNewMessageAdded(IStreamChannel channel, IStreamMessage msg)
	{
		if(msg.MentionedUsers.Count == 1
			&& msg.CustomData.TryGet("special", out string type)
			)
		{
			var entry = Instantiate(
				memberActivityEntryPrefab, messagesBoxParent
			);
			if(type == "join")
				entry.SetJoin(msg.MentionedUsers[0].Id);
			else if(type == "leave")
				entry.SetLeft(msg.MentionedUsers[0].Id);
			return;
		}

		var box = Instantiate(
			msg.User.Id == StreamManager.Client.LocalUserData.UserId ?
			currentUserMessageEntryPrefab : messageEntryPrefab,
			messagesBoxParent
		).GetComponent<ChatMessageBox>();
		box.Init(msg);

		box.Message = msg.Text;
		box.Author = msg.User.Id; //TODO: Username fetching
		if(msg.CreatedAt != null)
			box.CreationTime = msg.CreatedAt;

		if(msg.User.CustomData.ContainsKey("rank"))
		{
			string rank = msg.User.CustomData.Get<string>("rank");
			if(!System.Enum.TryParse(rank, out MemberEntry.MemberEntryData.Rank temp))
				box.Rank = temp;
			else
				box.Rank = MemberEntry.MemberEntryData.Rank.Member;
		}
		else
		{
			box.Rank = MemberEntry.MemberEntryData.Rank.Member;
			System.Collections.Generic.Dictionary<string, object> data = new()
			{
				{ "rank", MemberEntry.MemberEntryData.Rank.Member }
			};
			StreamManager.Client.UpsertUsers(new StreamUserUpsertRequest[] {
				new()
				{
					Id = msg.User.Id,
					CustomData = new StreamCustomDataRequest(data)
				}
			});
		}
	}

	private void SendMessage()
	{
		if(!chatStateScreen.activeSelf)
			return;

		_ = SendMessageAsync(messageTextField.text);
		messageTextField.text = string.Empty;
	}

	private async Task SendMessageAsync(string msg)
	{
		var response = await CurrentPlayerCache.CurrentClan
			.SendNewMessageAsync(msg);
	}
}