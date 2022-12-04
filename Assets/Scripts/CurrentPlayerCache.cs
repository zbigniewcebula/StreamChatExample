using StreamChat.Core.LowLevelClient.Models;
using StreamChat.Core.LowLevelClient.Requests;
using StreamChat.Core.StatefulModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class CurrentPlayerCache
{
	public static string ClanID => CurrentClan == null ?
		string.Empty : CurrentClan.Id;
	public static IStreamChannel CurrentClan { get; private set; }

	public static async Task FetchCurrentClanAsync(
		Action<IStreamChannel> onComplete = null,
		Action onFail = null
	)
	{
		var channelResponse = await StreamManager.Client.QueryChannelsAsync(
			new Dictionary<string, object>()
			{
				{
					"members", new Dictionary<string, object>()
					{
						{ "$in", new[] {
							StreamManager.Client.LocalUserData.UserId
						} }
					}
				}
			}
		);

		CurrentClan = channelResponse.FirstOrDefault(
			c => c.Id.StartsWith("clan_")
		);

		if(CurrentClan != null)
			onComplete?.Invoke(CurrentClan);
		else
			onFail?.Invoke();
	}

	public static async Task FetchClanAsync(
		string id, Action<IStreamChannel> onComplete = null
	)
	{
		//var channelResponse = await StreamManager.Client.QueryChannelsAsync(
		//	new QueryChannelsRequest()
		//	{
		//		FilterConditions = new Dictionary<string, object>()
		//		{
		//			{
		//				"id", new Dictionary<string, object>()
		//				{
		//					{ "$eq", id }
		//				}
		//			}
		//		}
		//	}
		//);
		var channelResponse = await StreamManager.Client.QueryChannelsAsync(
			new Dictionary<string, object>()
			{
				{
					"id", new Dictionary<string, object>()
					{
						{ "$eq", id }
					}
				}
			}
		);

		if(channelResponse.Count() == 0)
			onComplete?.Invoke(null);

		var clan = channelResponse.FirstOrDefault();
		onComplete?.Invoke(clan);
	}

	public static async Task FetchClanMembersAsync(
		Action<IEnumerable<IStreamChannelMember>> onComplete,
		Action onFail = null
	)
	{
		var memberResponse = await CurrentClan.QueryMembers(
			new Dictionary<string, object>()
		);

		//var memberResponse = await StreamManager.Client.QueryMembersAsync(
		//	new QueryMembersRequest()
		//	{
		//		Id = CurrentClan.Channel.Id,
		//		Type = "messaging",
		//		FilterConditions = new Dictionary<string, object>(),
		//		Sort = new List<SortParam> {
		//			new SortParam { Field = "user_id" }
		//		},
		//		Limit = 10,
		//		Offset = 0,
		//	}
		//);

		onComplete.Invoke(memberResponse);
	}
}