using StreamChat.Core.LowLevelClient.Models;
using StreamChat.Core.Models;
using StreamChat.Core.StatefulModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClanEntry : MonoBehaviour
{
	//Static

	//Accessor
	public IStreamChannel Channel { get; private set; } = null;

	//Public/Inspector
	[Header("Bindings")]
	[SerializeField] private Button showClanBtn = null;
	[SerializeField] private Text nameLabel = null;
	[SerializeField] private Text membersCount = null;
	[SerializeField] private Text score = null;
	[SerializeField] private EmblemIcon iconBG = null;
	[SerializeField] private EmblemIcon iconFG = null;

	//Events
	public event Action<ClanEntry> OnShowProfile = null;

	//Private

	//Methods
	public void SetData(IStreamChannel channel, Action<ClanEntry> onShowProfile)
	{
		if(Channel != null && channel.Id != Channel.Id)
		{
			OnShowProfile = null;
			channel.Updated -= InternalRefresh;
		}

		Channel = channel;
		Refresh();

		showClanBtn.onClick.RemoveAllListeners();
		showClanBtn.onClick.AddListener(ShowClanProfile);
		OnShowProfile += onShowProfile;

		channel.Updated += InternalRefresh;
	}

	public void Refresh()
	{
		nameLabel.text = Channel.CustomData.Get<string>("clanname");
		score.text = Channel.CustomData.Get<int>("score").ToString();
		try
		{
			iconBG.IconID = Channel.CustomData.Get<int>("iconFG");
			iconFG.IconID = Channel.CustomData.Get<int>("iconBG");
		}
		catch(Exception ex)
		{
			Debug.LogException(ex);
		}
		int memOnline = Channel.Members == null ? 0 : Channel.Members.Count;
		int memCount = Channel.CustomData.Get<int>("membersCount");
		membersCount.text = $"Online: {memOnline}/{memCount}";
	}

	private void InternalRefresh(IStreamChannel channel)
	{
		Channel = channel;
		Refresh();
	}

	public void ShowClanProfile()
	{
		if(Channel == null)
			return;

		OnShowProfile?.Invoke(this);
	}
}