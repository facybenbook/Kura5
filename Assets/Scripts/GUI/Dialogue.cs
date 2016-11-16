﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour {

	//Portraits
	public Transform rightPortrait;
	public Transform leftPortrait;
	private SpriteRenderer rightSprite;
	private SpriteRenderer leftSprite;
	public Animator rightAnimator;
	public Animator leftAnimator;
	public GameObject canvas; //For hiding GUI
	public Text displayText;
	public GameObject displayBox;
	public GameObject arrow;

	public GUISkin skin;
	public float textRate = 80; // how fast the text appears
	private float fastTextRate = 150;
	public float normalTextRate = 80;
	public AudioClip tickSound; // audio
	public AudioClip finishSound;
	public int border=5, height=300; // size of dialog 
	
	float textCounter = 0;
	Texture2D leftImage, rightImage; // images
	string theText = null; // the text (null means hidden)
	Rect mainRect; // rectangle for box
	private bool labelMode = false;

	//audiosources
	public AudioSource tickAudio;
	public AudioSource finishAudio;
	
	public bool isFinished { get { return theText==null; } }

	void Awake() {
		AudioSource[] sources = GetComponents<AudioSource> ();
		tickAudio = sources [0];
		finishAudio = sources [1];
		leftSprite = leftPortrait.GetComponent<SpriteRenderer>();
		leftAnimator = leftPortrait.GetComponent<Animator>();
		rightSprite = rightPortrait.GetComponent<SpriteRenderer>();
		rightAnimator = rightPortrait.GetComponent<Animator>();

		// if sound add the audio
		if (tickSound!=null)
		{
			audio.clip=tickSound;
		}
	}

	public void Show(string txt, Sprite left, Sprite right, bool isLabel=false)
	{
		Debug.Log(left);
		if(isLabel) {
			textRate = fastTextRate;
			labelMode = true;
		}
		else {
			textRate = normalTextRate;
			labelMode = false;
		}
		arrow.SetActive (false);
		if(displayBox!=null) displayBox.SetActive (true);
		theText = txt;
		if(left != null) {
			leftSprite.sprite = left;
			if(!leftAnimator.IsInTransition(0))leftAnimator.SetTrigger(Animator.StringToHash("Transition"));
		}
		if(right != null) {
			rightSprite.sprite = right;
			if(!rightAnimator.IsInTransition(0))rightAnimator.SetTrigger(Animator.StringToHash("Transition"));
		}
		textCounter = 0;
	}
	
	public void Hide()
	{
		theText=null;
		displayText.text = theText;
		if(displayBox!=null) displayBox.SetActive (false);
	}

	
	// Update is called once per frame
	void Update() 
	{
		if(isFinished) {
			leftSprite.sprite = null;
			rightSprite.sprite = null;
			return;
		}
		
		int oldCounter=Mathf.FloorToInt(textCounter); // for use later in audio
		if (Input.anyKey)   // any key makes it faster

			textCounter += (textRate*6)*Time.unscaledDeltaTime;
		else
			textCounter += (textRate*2)*Time.unscaledDeltaTime;
		// tick sound when displaying
		if (tickSound!=null && textCounter < theText.Length)
		{
			if (oldCounter!=Mathf.FloorToInt(textCounter)) // if new text
			{
				if(!audio.isPlaying) {
					makeSound(tickAudio, tickSound);
				}
			}
		}
		// if finished & space bar
		if (textCounter >= theText.Length)
		{   
			if(!labelMode) arrow.SetActive(true);
			else arrow.SetActive (false);
			bool push = Input.GetButtonDown("Confirm") || Input.GetButtonDown("Deny");
			if (push && !labelMode) {
				Hide();
				makeSound(finishAudio, finishSound);
			}
		}

		//Write out the text
		if (theText != null)
		{
			if(theText.Contains("^^")) theText = filterText(theText);
			string s = theText;
			if ((int)textCounter < theText.Length)
				s = getDisplayText(theText.Substring(0, (int)textCounter));
			else s = getDisplayText(theText);
			displayText.text = s;
		}
	}

	string filterText(string txt) {
		string[] c = JoystickUtil.getControlScheme();

		txt = txt.Replace("^^","");
		txt = txt.Replace("TARGET",c[0]);
		txt = txt.Replace("ROLL",c[1]);
		txt = txt.Replace("CHARGE",c[2]);
		txt = txt.Replace("ATTACK",c[3]);
		txt = txt.Replace("SWAP",c[4]);
		txt = txt.Replace("MOVE",c[5]);
		txt = txt.Replace("SWITCH",c[6]);
		txt = txt.Replace("START",c[7]);
		txt = txt.Replace("BLOCK",c[9]);

		txt = txt.Replace("USER",System.Environment.UserName);
		return txt;
	}

	string getDisplayText(string txt) {
		bool red = false;
		string s = "";
		bool stopNext = false;
		foreach (char letter in txt.ToCharArray()) {
			if(stopNext) {
				red=false;
				stopNext=false;
			}
			if(letter.ToString().Equals("[")) {
				red = true;
				//make the bracket itself red
			}
			if(letter.ToString().Equals("]")) {
				stopNext = true;
			}

			string section = "";

			if(red) {
				section = "<color=\"red\">"+letter+"</color>";
			}
			else section = letter.ToString();

			s += section;
		}

		return s;
	}

	public void makeSound(AudioSource s, AudioClip clip) {
		if(!audio.isPlaying) {
			s.clip = clip;
			s.Play ();
		}
	}
}
