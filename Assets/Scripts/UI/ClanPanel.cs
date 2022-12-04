using StreamChat.Core;
using StreamChat.Core.StatefulModels;
using System.Collections;
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

	[Header("Bindings/Clan Info")]
	[SerializeField] private RectTransform memberListParent = null;

	[Header("Bindings/Prefabs")]
	[SerializeField] private GameObject memberEntryPrefab = null;

	//Events

	//Private

	//Methods
	private IEnumerator Start()
	{
		while(StreamManager.Client.ConnectionState != ConnectionState.Connected)
			yield return null;

		_ = CurrentPlayerCache.FetchCurrentClanAsync(
			state => ClanInfoUpdate(state.Id)
		);

		joinButton.onClick.AddListener(JoinButtonClicked);
		leaveButton.onClick.AddListener(LeaveButtonClicked);
	}

	private async Task ClanInfoUpdate(string clanID)
	{
		clanStateScreen.SetActive(false);
		emptyStateScreen.SetActive(true);

		if(CurrentPlayerCache.CurrentClan == null)
		{
			Debug.Log($"[ClanPanel] Fetch current clan info...");
			await CurrentPlayerCache.FetchCurrentClanAsync(
				ShowClanInfo, () => {
					errorStateScreen.SetActive(false);
					Debug.Log($"[ClanPanel] Cannot find clanid: {CurrentPlayerCache.ClanID}");
				}
			);
		}
		else
		{
			ShowClanInfo(CurrentPlayerCache.CurrentClan);
		}
	}

	public void ShowClanInfo(IStreamChannel channel)
	{
		if(channel == null)
		{
			clanStateScreen.SetActive(false);
			emptyStateScreen.SetActive(true);
			return;
		}

		name.text = channel.CustomData.Get<string>("clanname");
		description.text = channel.CustomData.Get<string>("description");
		int membersOnline = channel.Members == null ?
			0 : channel.Members.Where(
				m => m.User != null && m.User.Online
			).Count();
		members.text = $"{membersOnline}/{channel.MemberCount}";
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

		if(channel.Members != null)
		{
			foreach(IStreamUser member in channel.Members.Select(m => m.User))
			{
				GameObject go = Instantiate(memberEntryPrefab, memberListParent);
				var memberEntry = go.GetComponent<MemberEntry>();

				int pts = member.CustomData.Get<int>("points");
				int emblem = member.CustomData.Get<int>("rankEmblem");
				string rank = "Member";
				if(member.CustomData.ContainsKey("rank"))
					rank = member.CustomData.Get<string>("rank");

				string name = "UnkownName";
				if(member.CustomData.ContainsKey("name"))
					name = member.CustomData.Get<string>("name");

				memberEntry.SetData(new MemberEntry.MemberEntryData()
				{
					name = member.Id, //name
					id = member.Id,
					rank = rank,
					points = pts,
					rankEmblem = (MemberEntry.MemberEntryData.RankEmblem)emblem
				});

				go.transform.SetParent(memberListParent);
			}
		}
		else
		{
			Debug.LogError("[ClanPanel] Members list is null");
			errorStateScreen.SetActive(true);
			return;
		}

		joinButton.gameObject.SetActive(false);
		leaveButton.gameObject.SetActive(true);

		emptyStateScreen.SetActive(false);
		errorStateScreen.SetActive(false);
		clanStateScreen.SetActive(true);
	}

	private void JoinButtonClicked()
	{
	}

	private void LeaveButtonClicked()
	{
	}
}