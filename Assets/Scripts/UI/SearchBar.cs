using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchBar : MonoBehaviour
{
	//Static

	//Accessor

	//Public/Inspector
	[Header("Bindings")]
	[SerializeField] private InputField input = null;
	[SerializeField] private Button deleteText = null;

	//Events

	//Private

	//Methods
	private void Awake()
	{
		input.onValueChanged.AddListener(OnTextEntered);
		OnTextEntered(input.text);
	}

	private void OnTextEntered(string txt)
	{
		deleteText.gameObject.SetActive(txt.Length > 0);
	}
}