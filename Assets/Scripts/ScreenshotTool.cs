using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScreenshotTool : MonoBehaviour
{
	//Private
	private string screenshotPath = null;

	private void Start()
	{
#if UNITY_EDITOR
		screenshotPath = Path.Combine(
			Application.dataPath + "/../SCREENSHOTS"
		);
#else
		screenshotPath = Path.Combine(
			Application.dataPath + "/SCREENSHOTS"
		);
#endif

		if(!Directory.Exists(screenshotPath))
			Directory.CreateDirectory(screenshotPath);
	}

	private void Update()
	{
		if(Input.GetKeyDown(KeyCode.F12))
		{
			string filename = Path.Combine(
				screenshotPath,
				$"Screenshot_{System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.png"
			);

			ScreenCapture.CaptureScreenshot(filename);
		}
	}
}