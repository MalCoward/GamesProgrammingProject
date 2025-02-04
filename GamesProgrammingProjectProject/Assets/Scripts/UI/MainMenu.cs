﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

	void Start()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		// Reset Generation Values
		GlobalControl.instance.ResetValues();
	}

	public void PlayGame()
	{
		SceneManager.LoadScene(1);
	}

	public void QuitGame()
	{
		Debug.Log("Quit!");
		Application.Quit();
	}
}
