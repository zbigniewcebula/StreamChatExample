using StreamChat.Core;
using StreamChat.Core.LowLevelClient.Models;
using StreamChat.Core.LowLevelClient.Requests;
using StreamChat.Core.Requests;
using StreamChat.Core.StatefulModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SearchPanel : MonoBehaviour
{
	//Const
	public const int SORT_ASC = 1;
	public const int SORT_DSC = -1;

	//Static

	//Public/Inspector
	[Header("Bindings")]
	[SerializeField] private GameObject entryPrefab = null;

	[SerializeField] private TabManager tabManager = null;
	[SerializeField] private ClanPanel clanPanel = null;

	[SerializeField] private RectTransform entriesContainer = null;
	[SerializeField] private GameObject emptyStateScreen = null;
	[SerializeField] private InputField searchingField = null;
	[SerializeField] private AdvancedSearchOptions advancedSearchOptions = null;
	[SerializeField] private ScrollRect scroll = null;

	[Header("Settings")]
	[SerializeField] private int elementsPerPage = 30;
	[SerializeField] private int offset = 0;

	//Events

	//Private
	private List<ClanEntry> entriesCache = new List<ClanEntry>();

	private QueryChannelsRequest query = null;

	private Task currentSearch = null;

	private Task infiniteScroll = null;
	private float lastInfRefresh = 0f;

	//Methods
	private IEnumerator Start()
	{
		while(StreamManager.Client.ConnectionState != ConnectionState.Connected)
			yield return null;

		InitialClanListAsync();

		scroll.onValueChanged.AddListener(v => SearchInfiniteScroll());
	}

	private void Update()
	{
	}

	private async Task InitialClanListAsync()
	{
		query = new QueryChannelsRequest
		{
			//Sort alphabetically by team name
			Sort = new List<SortParamRequest>
			{
				new SortParamRequest
				{
					Field = "team",
					Direction = SORT_ASC,
				}
			},

			//Ignore hidden clans/channels
			FilterConditions = new Dictionary<string, object>
			{
				{
					"hidden", new Dictionary<string, object>
					{
						{ "$eq", false }
					}
				}
			},

			Limit = elementsPerPage,
			Offset = offset,

			//Tell server to not watch channel (avoiding unnecessay channel events)
			Watch = false,
			State = false,
		};

		SearchAsync();
	}

	public void Search()
	{
		//Clean offset for new search results
		offset = 0;
		//Hide advanced options
		advancedSearchOptions.gameObject.SetActive(false);

		query = new QueryChannelsRequest
		{
			//Sort alphabetically by team name
			Sort = new List<SortParamRequest>
			{
				new SortParamRequest
				{
					Field = advancedSearchOptions.FilterType,
					Direction = advancedSearchOptions.SortDirection,
				}
			},

			//Ignore hidden clans/channels
			FilterConditions = new Dictionary<string, object>
			{
				{
					"hidden", new Dictionary<string, object>
					{
						{ "$eq", false }
					}
				}
			},

			Limit = elementsPerPage,
			Offset = offset,

			//Tell server to not watch channel (avoiding unnecessay channel events)
			Watch = false,
			State = false,
		};

		var filteringFunc = new Func<IEnumerable<IStreamChannel>, IEnumerable<IStreamChannel>>(
			channels => channels
		);

		foreach(var entry in entriesCache)
			Destroy(entry.gameObject);
		entriesCache.Clear();

		if(string.IsNullOrEmpty(searchingField.text))
			currentSearch = SearchAsync();
		else
			currentSearch = SearchAsync(filteringFunc);
	}

	private void SearchInfiniteScroll()
	{
		if(currentSearch != null && !currentSearch.IsCompleted)
			return;

		if(scroll.verticalNormalizedPosition <= 0
		&& (infiniteScroll == null || infiniteScroll.IsCompleted)
		&& (Time.timeSinceLevelLoad - lastInfRefresh) > 0.5f
		)
		{
			int orgOffset = offset;
			offset += elementsPerPage;
			query.Offset = offset;
			infiniteScroll = SearchAsync();

			int currentEntriesCount = entriesCache.Count;
			infiniteScroll.ContinueWith(t => {
				if(currentEntriesCount == entriesCache.Count)
					offset = orgOffset;
			});

			lastInfRefresh = Time.timeSinceLevelLoad;
		}
	}

	private async Task SearchAsync(
		Func<IEnumerable<IStreamChannel>, IEnumerable<IStreamChannel>> filter = null
	)
	{
		if(query == null)
			return;

		//Response contains list of channel states that matched the query
		var response = await StreamManager.Client.QueryChannelsAsync(
			new Dictionary<string, object>
			{
				{
					"hidden", new Dictionary<string, object>
					{
						{ "$eq", false }
					}
				}
			}
		);

		int count = response.Count();
		if(count == 0)
		{
			//Nothing found and list is empty
			emptyStateScreen.SetActive(entriesCache.Count == 0);
			return;
		}
		emptyStateScreen.SetActive(false);

		Debug.Log("[SearchPanel] Found " + count + " channels");

		try
		{
			//Local filter of channels
			if(filter != null)
				response = filter.Invoke(response);

			//Iteration through recieved channels and creating new entries in UI
			foreach(IStreamChannel channel in response)
			{
				GameObject go = Instantiate(entryPrefab, entriesContainer);
				var entry = go.GetComponent<ClanEntry>();
				entry.SetData(channel, Entry_OnShowProfile);
			}
		}
		catch(Exception ex)
		{
			Debug.LogError($"[SearchPanel] {ex.Message}\n{ex.StackTrace}");
		}
	}

	private void Entry_OnShowProfile(ClanEntry obj)
	{
		clanPanel.ShowClanInfo(obj.Channel);
		tabManager.ClanPanel();
	}
}