using UnityEngine;
using UnityEngine.UI;

public class TabManager : MonoBehaviour
{
	//Static

	//Accessor

	//Public/Inspector
	[Header("Bindings")]
	[SerializeField] private GameObject[] screens = null;

	[SerializeField] private Button defaultMenu = null;
	[SerializeField] private Button searchMenu = null;
	[SerializeField] private Button clanPanel = null;

	//Events

	//Private

	//Methods
	private void Start()
	{
		defaultMenu.Invoke();
	}

	public void HideAllScreens()
	{
		foreach(var s in screens)
			s.SetActive(false);
	}

	public void ShowScreen(GameObject go)
	{
		HideAllScreens();
		go.SetActive(true);
	}

	public void SelectButton(Button btn)
	{
		btn.Select();
	}

	public void SearchTab()
	{
		searchMenu.Invoke();
	}
	
	public void ClanPanel()
	{
		clanPanel.Invoke();
	}
}