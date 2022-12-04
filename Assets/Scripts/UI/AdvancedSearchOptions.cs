using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdvancedSearchOptions : MonoBehaviour
{
	//Static

	//Accessor
	public int SortDirection { get; private set; } = SearchPanel.SORT_ASC;
	public string FilterType { get; private set; } = "team";

	//Public/Inspector
	[Header("Bindings")]
	[SerializeField] private Toggle[] toggles = null;

	//Events

	//Private

	//Methods
	private void Awake()
	{
		foreach(var t in toggles)
		{
			var toggle = t;
			t.onValueChanged.AddListener(status =>
			{
				OnToggleValueChanged(toggle, status);
			});
		}
		gameObject.SetActive(false);
	}

	private void OnToggleValueChanged(Toggle t, bool status)
	{
		if(!status)
			return;

		SortDirection = t.name.Substring(0, 3) == "ASC" ?
			SearchPanel.SORT_ASC : SearchPanel.SORT_DSC;

		FilterType = t.name.Substring(3);
	}
}