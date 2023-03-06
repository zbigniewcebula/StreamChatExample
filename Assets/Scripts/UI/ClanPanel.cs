using StreamChat.Core;
using StreamChat.Core.Requests;
using StreamChat.Core.StatefulModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ClanPanel : MonoBehaviour
{
	//Static

	//Accessor

	//Public/Inspector
	[Header("Bindings")]
	[SerializeField] private GameObject errorStateScreen = null;
	[SerializeField] private GameObject emptyStateScreen = null;
	[SerializeField] private GameObject clanStateScreen = null;

	[Header("Bindings/Clan Info")]
	[SerializeField] private new Text name = null;
	[SerializeField] private Text description = null;
	[SerializeField] private Text members = null;
	[SerializeField] private Text score = null;
	[SerializeField] private EmblemIcon iconBG = null;
	[SerializeField] private EmblemIcon iconFG = null;
	[SerializeField] private Button joinButton = null;
	[SerializeField] private Button leaveButton = null;

	[Header("Bindings/Dialog")]
	[SerializeField] private GameObject leaveDialog = null;
	[SerializeField] private Button cancelLeaveButton = null;
	[SerializeField] private Button leaveConfirmationButton = null;

	[Header("Bindings/Members")]
	[SerializeField] private RectTransform memberListParent = null;

	[Header("Bindings/Prefabs")]
	[SerializeField] private MemberEntry memberEntryPrefab = null;

	//Events

	//Private
	private IStreamChannel currentChannel = null;

	private List<MemberEntry> pool = new();

	//Methods
	private IEnumerator Start()
	{
		while(StreamManager.Client.ConnectionState != ConnectionState.Connected)
			yield return null;

		_ = CurrentPlayerCache.FetchCurrentClanAsync(
			state => ClanInfoUpdate(state.Id)
		);

		leaveDialog.SetActive(false);

		joinButton.onClick.AddListener(JoinButtonClicked);
		leaveButton.onClick.AddListener(() => leaveDialog.SetActive(true));

		cancelLeaveButton.onClick.AddListener(() => leaveDialog.SetActive(false));
		leaveConfirmationButton.onClick.AddListener(LeaveButtonClicked);
	}

	private void OnMembersChanged(IStreamChannel chn, IStreamChannelMember member)
	{
		Debug.Log("change");
		RefreshMembers(chn);
	}

	private async Task ClanInfoUpdate(string clanID = null)
	{
		clanStateScreen.SetActive(false);
		emptyStateScreen.SetActive(true);

		if(clanID == null)
		{
			await ShowClanInfo(CurrentPlayerCache.CurrentClan);
		}
		else
		{
			await ShowClanInfo(clanID);
		}
	}

	public async Task ShowClanInfo(string clanID)
	{
		var channelResponse = await StreamManager.Client.QueryChannelsAsync(
			new Dictionary<string, object>()
			{
				{
					"id", clanID
				}
			}
		);

		var channel = channelResponse.FirstOrDefault(
			chn => chn.Id == clanID
		);
		if(channel != null)
			await ShowClanInfo(channel);
	}

	private void RegisterMemberEvents()
	{
		currentChannel.MemberAdded += OnMembersChanged;
		currentChannel.MemberUpdated += OnMembersChanged;
		currentChannel.MemberRemoved += OnMembersChanged;
	}

	private void UnregisterMemberEvents()
	{
		currentChannel.MemberAdded -= OnMembersChanged;
		currentChannel.MemberUpdated -= OnMembersChanged;
		currentChannel.MemberRemoved -= OnMembersChanged;
	}

	public async Task ShowClanInfo(IStreamChannel channel)
	{
		if(channel == null)
		{
			if(currentChannel != null)
			{
				UnregisterMemberEvents();
			}
			currentChannel = channel;

			clanStateScreen.SetActive(false);
			emptyStateScreen.SetActive(true);
			return;
		}
		else if(currentChannel != null && channel.Id != currentChannel.Id)
		{
			UnregisterMemberEvents();
			currentChannel = channel;
			RegisterMemberEvents();
		}
		else
			currentChannel = channel;

		name.text = channel.CustomData.Get<string>("clanname");
		description.text = channel.CustomData.Get<string>("description");
		score.text = channel.CustomData.Get<int>("score").ToString();

		try
		{
			iconFG.IconID = channel.CustomData.Get<int>("iconFG");
			iconBG.IconID = channel.CustomData.Get<int>("iconBG");
		}
		catch(System.Exception ex)
		{
			Debug.LogException(ex);
		}

		joinButton.gameObject.SetActive(true);
		leaveButton.gameObject.SetActive(false);

		emptyStateScreen.SetActive(false);
		errorStateScreen.SetActive(false);
		clanStateScreen.SetActive(true);

		RefreshMembers(channel);
		RegisterMemberEvents();
	}

	private async void RefreshMembers(IStreamChannel channel)
	{
		IEnumerable<IStreamChannelMember> membersList = await channel.QueryMembersEx();

		int membersOnline = membersList == null ?
			0 : membersList.Where(
				m => m.User != null && m.User.Online
			).Count();
		members.text = $"{membersOnline}/{channel.MemberCount}";

		if(membersList.Any(m =>
				m.User.Id == StreamManager.Client.LocalUserData.UserId
		))
		{
			joinButton.gameObject.SetActive(false);
			leaveButton.gameObject.SetActive(true);
		}

		int count = membersList.Count();
		if(count > pool.Count)
		{
			int diff = count - pool.Count;
			for(int i = 0; i < diff; ++i)
			{
				MemberEntry memberEntry = Instantiate(
					memberEntryPrefab, memberListParent
				);
				memberEntry.transform.SetParent(memberListParent);
				pool.Add(memberEntry);
			}
		}
		else if(count < pool.Count)
		{
			for(int i = 0; i < pool.Count; ++i)
				pool[i].gameObject.SetActive(i < count);
		}

		int idx = 0;
		try
		{
			foreach(IStreamUser member in membersList.Select(m => m.User))
				pool[idx++].SetData(member);
		}
		catch(System.Exception ex)
		{
			Debug.LogException(ex);
		}
	}

	private void JoinButtonClicked()
	{
		//Send message
		//Send join request
		//Update clan info
		Action join = () => {
			Debug.Log("[ClanPanel] Join new clan");
			currentChannel.JoinAsMemberAsync().ContinueWith(
				tt => {
					ButtonsRefresh();
					return Task.Delay(1000);
				}
			).ContinueWith(
				tt => {
					CurrentPlayerCache.CurrentClan = currentChannel;
					return ShowClanInfo(currentChannel);
				}
			);
		};

		SendSpecialMessage(currentChannel, true).ContinueWith(_t => {
			if(_t.IsFaulted)
			{
				Debug.LogError(
					$"[ClanPanel] Failed to join the clan! Reason: {_t.Exception.Message}"
				);
				return;
			}

			if(CurrentPlayerCache.CurrentClan == null)
			{
				join.Invoke();
				return;
			}

			string id = CurrentPlayerCache.CurrentClan.Id;
			CurrentPlayerCache.CurrentClan.RemoveMembersAsync(
				new string[] { StreamManager.Client.LocalUserData.UserId }
			).ContinueWith(t => {
				if(t.IsFaulted)
				{
					Debug.LogError("[ClanPanel] Failed to join the clan");
					if(t.Exception != null)
						Debug.LogException(t.Exception);
				}
				else
				{
					join.Invoke();
				}
			});
		});
	}

	private void LeaveButtonClicked()
	{
		leaveDialog.SetActive(false);

		//Send message
		//Send leave request
		//Update clan info
		string id = CurrentPlayerCache.CurrentClan.Id;
		SendSpecialMessage(currentChannel, false).ContinueWith(_t => {
			if(_t.IsFaulted)
			{
				Debug.LogError("[ClanPanel] Failed to leave the clan!");
				return;
			}

			CurrentPlayerCache.CurrentClan.RemoveMembersAsync(
				new string[] { StreamManager.Client.LocalUserData.UserId }
			).ContinueWith(t => {
				if(t.IsFaulted)
				{
					Debug.LogError("[ClanPanel] Failed to leave clan");
					if(t.Exception != null)
						Debug.LogException(t.Exception);
				}
				else
				{
					ButtonsRefresh();

					Debug.Log("[ClanPanel] Left clan");
					CurrentPlayerCache.CurrentClan = null;
					t.ContinueWith(tt => ClanInfoUpdate(id));
				}
			});
		});
	}

	private void ButtonsRefresh()
	{
		joinButton.gameObject.SetActive(CurrentPlayerCache.CurrentClan == null);
		leaveButton.gameObject.SetActive(CurrentPlayerCache.CurrentClan != null);
	}

	private async Task SendSpecialMessage(IStreamChannel channel, bool join = true)
	{
		var response = await channel.SendNewMessageAsync(
			new StreamSendMessageRequest()
			{
				ShowInChannel = false,
				MentionedUsers = new List<IStreamUser>() {
					StreamManager.Client.LocalUserData.User
				},
				CustomData = new StreamCustomDataRequest()
				{
					{ "special", join? "join": "leave" }
				}
			}
		);
	}
}