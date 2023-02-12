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
	public static IStreamChannel CurrentClan { get; set; }

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
		IStreamChannel channel,
		Action<IEnumerable<IStreamChannelMember>> onComplete,
		Action onFail = null
	)
	{
		var memberResponse = await channel.QueryMembers(
			new Dictionary<string, object>() {
				{
					"created_at", new Dictionary<string, object>
					{
						{ "$gt", "1970-01-01T00:00:01.00Z" }
					}
				}
			}
		);

		onComplete.Invoke(memberResponse);
	}
	public static async Task FetchCurrentClanMembersAsync(
		Action<IEnumerable<IStreamChannelMember>> onComplete,
		Action onFail = null
	) => await FetchClanMembersAsync(CurrentClan, onComplete, onFail);
}