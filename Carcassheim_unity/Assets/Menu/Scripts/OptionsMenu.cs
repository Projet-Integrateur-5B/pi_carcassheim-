/*using System.Collections;
using System.Collections.Generic;*/
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : Miscellaneous
{
	private Button _btnSon, _btnMusique;
	private AudioSource _soundCtrl, _musicCtrl;
	private Scrollbar _soundScroll, _musicScroll;
	private Text _pourcentSon, _pourcentMusique;
	private float _previousSoundVol = 0.0f;
	private float _previousMusicVol = 0.0f;
	private static bool s_tmpOnce = true;
	float lastSoundValue = 0;
	float lastMusicValue = 0;
	public Toggle toggle_french, toggle_english, toggle_german;
	private GameObject containerButtons;
	private Transform optionsMenu, OCB; // Options Container Buttons
	// Start is called before the first frame update
	void Start()
	{
		// INITIALISATION
		optionsMenu = GameObject.Find("SubMenus").transform.Find("OptionsMenu").transform;
		OCB = optionsMenu.Find("Buttons").transform;
		_soundScroll = optionsMenu.Find("Scrollbar Son").GetComponent<Scrollbar>();
		_musicScroll = optionsMenu.Find("Scrollbar Musique").GetComponent<Scrollbar>();
		_pourcentMusique = optionsMenu.Find("Pourcent Musique").GetComponent<Text>();
		_pourcentSon = optionsMenu.Find("Pourcent Son").GetComponent<Text>();
		_btnMusique = OCB.Find("SwitchMusic").GetComponent<Button>();
		_btnSon = OCB.Find("SwitchSound").GetComponent<Button>();
		_soundCtrl = GameObject.Find("SoundController").GetComponent<AudioSource>();
		_musicCtrl = GameObject.Find("MusicController").GetComponent<AudioSource>();
		DefaultMusicSound();
		//Subscribe to the Scrollbar event
		_soundScroll.onValueChanged.AddListener(SoundScrollbarCallBack);
		lastSoundValue = _soundScroll.value;
		_musicScroll.onValueChanged.AddListener(MusicScrollbarCallBack);
		lastMusicValue = _musicScroll.value;
	/*toggle_french = GameObject.Find("Toggle French").GetComponent<Toggle>();
		toggle_french.onValueChanged.AddListener(delegate
		{
			ToggleValueChanged(toggle_french);
		}); */
	}

	void Update()
	{
	}

	void ToggleValueChanged(Toggle change)
	{
		if (change == toggle_french && change.isOn)
		{
			Debug.Log("French");
		}

		if (change == toggle_english && change.isOn)
		{
			Debug.Log("English");
		}

		if (change == toggle_german && change.isOn)
		{
			Debug.Log("German");
		}

		GameObject.Find("SoundController").GetComponent<AudioSource>().Play();
	}

	public void FlagsToggle() //affiche la langue du toggle enclenche
	{
		// foreach
		if (GameObject.Find("Toggle French").GetComponent<Toggle>().isOn == true)
		{
			Debug.Log("French");
		}
		else if (GameObject.Find("Toggle English").GetComponent<Toggle>().isOn == true)
		{
			Debug.Log("English");
		}
		else if (GameObject.Find("Toggle German").GetComponent<Toggle>().isOn == true)
		{
			Debug.Log("German");
		}
	}

	//---------------------------- Music/Sound Begin ----------------------------//
	public void VolumeSound(float value)
	{
		_soundCtrl.volume = value;
		_pourcentSon.text = Mathf.RoundToInt(value * 100) + "%";
	}

	public void VolumeMusic(float value)
	{
		_musicCtrl.volume = value;
		_pourcentMusique.text = Mathf.RoundToInt(value * 100) + "%";
	}

	public void DisplayVolumeSound(float value)
	{
		if (value > 0 /* && soundCtrl.isPlaying */)
		{
			_btnSon.GetComponentInChildren<Text>().text = "Son 'ON'";
		}
		else
		{
			_btnSon.GetComponentInChildren<Text>().text = "Son 'OFF'";
		}

		VolumeSound(value);
	}

	public void DisplayVolumeMusic(float value)
	{
		if (value > 0)
		{
			_btnMusique.GetComponentInChildren<Text>().text = "Musique 'ON'";
		}
		else
		{
			_btnMusique.GetComponentInChildren<Text>().text = "Musique 'OFF'";
		}

		VolumeMusic(value);
	}

	public void DefaultMusicSound()
	{
		if (s_tmpOnce == true)
		{
			_soundScroll.numberOfSteps = _musicScroll.numberOfSteps = 11; // 0->10 = 11
			_soundCtrl.volume = _musicCtrl.volume = _soundScroll.value = _musicScroll.value = 0.2f;
			VolumeSound(_soundScroll.value);
			VolumeMusic(_musicScroll.value);
			s_tmpOnce = !s_tmpOnce;
		}
	}

	//Will be called when Scrollbar changes
	public void SoundScrollbarCallBack(float value)
	{
		DisplayVolumeSound(value);
	}

	//Will be called when Scrollbar changes
	public void MusicScrollbarCallBack(float value)
	{
		DisplayVolumeMusic(value);
	}

	public void SwitchSound()
	{
		if (_soundScroll.value != 0)
		{
			_previousSoundVol = _soundCtrl.volume;
			_soundScroll.value = 0.0f;
			SoundScrollbarCallBack(_soundScroll.value);
		}
		else
		{
			_soundScroll.value = _previousSoundVol;
			SoundScrollbarCallBack(_previousSoundVol);
		}
	}

	public void SwitchMusic()
	{
		if (_musicScroll.value != 0)
		{
			_previousMusicVol = _musicCtrl.volume;
			_musicScroll.value = 0.0f;
			MusicScrollbarCallBack(_musicScroll.value);
		}
		else
		{
			_musicScroll.value = _previousMusicVol;
			MusicScrollbarCallBack(_previousMusicVol);
		}
	}

	// -------------- Music/Sound End -----------------------//
	public void HideOptions()
	{
		ChangeMenu("OptionsMenu", "HomeMenu");
	}

	public void FullScreen()
	{
		Screen.fullScreen = !Screen.fullScreen;
		Debug.Log("Windowed");
	}

	public void Help()
	{
		Application.OpenURL("https://tinyurl.com/SlapDance");
	}

	public void ShowCredits()
	{
		ChangeMenu("OptionsMenu", "CreditsMenu");
	}
}