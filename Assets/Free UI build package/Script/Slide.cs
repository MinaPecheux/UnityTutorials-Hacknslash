using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slide : MonoBehaviour {
	Image Filler;
	public Slider slider;

	// Use this for initialization
	void Start () {
		Filler = GetComponent<Image>();
	}
	
	// Update is called once per frame
	void Update () {
		Filler.fillAmount = slider.value;
	}
}
