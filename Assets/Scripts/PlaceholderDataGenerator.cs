using StreamChat.Core;
using StreamChat.Core.Requests;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class PlaceholderDataGenerator : MonoBehaviour
{
	//Const

	//Static

	//Public/Inspector
	[Header("Pseudo-Buttons")]
	public bool generateClanChannels = false;
	public bool deleteClanChannels = false;

	public bool generateUsers = false;

	//Events

	//Private

	//Methods
	private IEnumerator Start()
	{
		while(StreamManager.Client.ConnectionState != ConnectionState.Connected)
			yield return null;
	}

	private void Update()
	{
		if(generateClanChannels)
		{
			GenerateClanChannelsAsync();
			generateClanChannels = false;
		}
		if(deleteClanChannels)
		{
			DeleteClanChannelsAsync();
			deleteClanChannels = false;
		}
	}

	private async Task DeleteClanChannelsAsync()
	{
		//var lst = Enumerable
		//		.Range(0, 60)
		//		.Select(idx => $"clan_{idx}")
		//		.ToList();
		//var response = StreamManager.Client.ChannelApi.DeleteChannelsAsync(new DeleteChannelsRequest()
		//{
		//	Cids = lst,
		//	HardDelete = true
		//});
		//Debug.Log($"[Generator] Deleted clan channels: {response.IsCompleted}");
		//Debug.Log($"[Generator] lst: \n" + string.Join("\n", lst));
		//return;

		for(int i = 0; i < 100; ++i)
		{
			await Task.Delay(20);
			try
			{
				//StreamManager.Client.DeleteMultipleChannelsAsync
				//
				//var response = await StreamManager.Client.DeleteChannelAsync(
				//	channelType: "messaging", channelId: "clan_" + i
				//);
				//Debug.Log($"[Generator] Deleted clan channel: {response.Channel.Id}");
			}
			catch(StreamChat.Core.Exceptions.StreamApiException ex)
			{
				Debug.LogException(ex);
			}
			catch(Exception ex)
			{
				Debug.LogError($"[Generator] {ex.Message}\n{ex.StackTrace}");
			}
		}
	}

	private async Task GenerateClanChannelsAsync()
	{
		for(int i = 0; i < 100; ++i)
		{
			await Task.Delay(20);
			if(i == 50)
				await Task.Delay(500);

			try
			{
				//var req = new ChannelRequest()
				//{
				//	AdditionalProperties = new Dictionary<string, object>()
				//	{
				//		{ "clanname", $"Clan {i}" },
				//		{ "description", $"This is clan {i}." },
				//		{ "score", UnityEngine.Random.Range(1, 9999).ToString() },
				//		{ "iconFG", UnityEngine.Random.Range(0, 33).ToString() },
				//		{ "iconBG", UnityEngine.Random.Range(0, 6).ToString() },
				//		{ "membersCount", UnityEngine.Random.Range(10, 40).ToString() }
				//	},
				//	Team = i.ToString()
				//};
				//var channelState = await StreamManager.Client.GetOrCreateChannelAsync(
				//	channelType: "messaging",
				//	channelId: "clan_" + i,
				//	new ChannelGetOrCreateRequest()
				//	{
				//		Data = req
				//	}
				//);
				//Debug.Log($"[Generator] Created channel: {channelState.Channel.Id}");
			}
			catch(StreamChat.Core.Exceptions.StreamApiException ex)
			{
				Debug.LogError($"[Generator] {ex.Message}\n{ex.StackTrace}");
			}
			catch(Exception ex)
			{
				Debug.LogError($"[Generator] {ex.Message}\n{ex.StackTrace}");
			}
		}
	}
}