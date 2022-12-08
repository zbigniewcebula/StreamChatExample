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
	[SerializeField] private int initialOffset = 0;

	//Events

	//Private
	private List<ClanEntry> entriesCache = new List<ClanEntry>();

	private Task currentSearch = null;

	private Task infiniteScroll = null;
	private float lastInfRefresh = 0f;

	private int offset = 0;

	//Methods
	private void Awake()
	{
		offset = initialOffset;
	}

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
		SearchAsync();
	}

	public void Search()
	{
		//Clean offset for new search results
		initialOffset = 0;
		//Hide advanced options
		advancedSearchOptions.gameObject.SetActive(false);

		Dictionary<string, object> filterParams = new Dictionary<string, object>()
		{
			{
				"id", new Dictionary<string, object>
				{
					{ "$eq", $"clan_{searchingField.text}" }
				}
			}
		};

		foreach(var entry in entriesCache)
			Destroy(entry.gameObject);
		entriesCache.Clear();

		currentSearch = SearchAsync(filterParams: filterParams);
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
			infiniteScroll = SearchAsync();

			int currentEntriesCount = entriesCache.Count;
			infiniteScroll.ContinueWith(t => {
				if(currentEntriesCount == entriesCache.Count)
					initialOffset = orgOffset;
			});

			lastInfRefresh = Time.timeSinceLevelLoad;
		}
	}

	private async Task SearchAsync(
		Func<IEnumerable<IStreamChannel>, IEnumerable<IStreamChannel>> filterFunc = null,
		Dictionary<string, object> filterParams = null
	)
	{
		if(filterParams == null)
		{
			filterParams = new Dictionary<string, object>
			{
				{
					"hidden", new Dictionary<string, object>
					{
						{ "$eq", false }
					}
				}
			};
		}

		//Response contains list of channel states that matched the query
		var response = await StreamManager.Client.QueryChannelsAsync(
			filterParams, elementsPerPage, offset
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
			if(filterFunc != null)
				response = filterFunc.Invoke(response);

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