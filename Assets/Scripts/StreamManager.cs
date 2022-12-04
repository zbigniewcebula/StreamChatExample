using StreamChat.Core;
using StreamChat.Core.Auth;
using StreamChat.Libs.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

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

	private void Client_ConnectionStateChanged(ConnectionState oldState, ConnectionState newState)
	{
		if(newState == ConnectionState.Connected)
			_ = CurrentPlayerCache.FetchCurrentClanAsync();
	}
}