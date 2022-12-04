using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour
{
	//Static

	//Accessor

	//Public/Inspector
	[Header("Clock")]
	[SerializeField] private Text clockLabel = null;

	[Header("Battery")]
	[SerializeField] private Text batteryLabel = null;
	[SerializeField] private Image batteryIcon = null;

	[Header("Cellular")]
	[SerializeField] private Image cellularIcon = null;

	[Header("Wifi")]
	[SerializeField] private Image wifiIcon = null;

	//Events

	//Private

	//Methods
	private void Update()
	{
		clockLabel.text = System.DateTime.Now.ToString("h:mm tt");

		if(SystemInfo.batteryLevel > 0)
		{
			batteryLabel.text = SystemInfo.batteryLevel.ToString("0%");
			batteryIcon.fillAmount = SystemInfo.batteryLevel;
		}
		else
		{
			batteryLabel.text = "100%";
			batteryIcon.fillAmount = 1f;
		}
	}
}