using StreamChat.Core;
using StreamChat.Core.Auth;
using StreamChat.Libs.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using System.Linq;
using StreamChat.Core.Requests;

public class StreamManager : MonoBehaviour
{
	//Static
	private static StreamManager instance = null;

	//Accessor
	public static IStreamChatClient Client => instance.client;

	//Public/Inspector
	[Header("Auth Data")]
	[SerializeField] private AuthCredentialsAsset authCredentialsAsset;

	//Events

	//Private
	private IStreamChatClient client = null;

	//Methods
	private void Awake()
	{
		//Registering instance
		instance = this;

		//Prepare client and auth credentials
		client = StreamChatClient.CreateDefaultClient();

		//Register events
		client.ConnectionStateChanged += Client_ConnectionStateChanged;

		//Initialize connection with the Stream Chat server
		client.ConnectUserAsync(authCredentialsAsset.Credentials);
	}

	private void OnDestroy()
	{
		//Unregister events
		client.ConnectionStateChanged -= Client_ConnectionStateChanged;

		//Unregistering instance
		instance = null;
	}

	private void Client_ConnectionStateChanged(
		ConnectionState oldState, ConnectionState newState
	)
	{
		if(newState == ConnectionState.Connected)
		{
			_ = CurrentPlayerCache.FetchCurrentClanAsync();
		}
	}

	private void Update()
	{
		if(Input.GetKeyDown(KeyCode.P) == false)
			return;

		/*
		var channelResponse = StreamManager.Client.QueryChannelsAsync(
			new Dictionary<string, object>()
			{
				{
					"id", new Dictionary<string, object>()
					{
						{ "$eq", "clan_50" }
					}
				}
			}
		);

		channelResponse.ContinueWith(
			task => {
				var channel = task.Result.FirstOrDefault();
				Debug.Log($"Mbr count (from IStreamChannel): {channel.MemberCount}");
				Debug.Log($"Mbr list count (from IStreamChannel): {channel.Members.Count}");
				int onl = channel.Members.Count(m => m.User.Online);
				Debug.Log($"Mbr list ONLINE count (from IStreamChannel): {onl}");

				channel.QueryMembersAsync(
					new Dictionary<string, object>() {
						{
							"created_at", new Dictionary<string, object>
							{
								{ "$gt", "1970-01-01T00:00:01.00Z" }
							}
						}
					}
				).ContinueWith(
					task2 => {
						var mbr = task2.Result.ToList();
						int online = mbr.Count(m => m.User.Online);
						Debug.Log($"Mbr count (from QueryMembersAsync): {mbr.Count}");
						Debug.Log($"Mbr count ONLINE (from QueryMembersAsync): {online}");
					}
				);
			}
		);
		*/

		StreamManager.Client.UpsertUsers(new StreamUserUpsertRequest[]
		{
				new StreamUserUpsertRequest()
				{
					Invisible = false,
					Id = Client.LocalUserData.UniqueId
				}
		});
	}
}