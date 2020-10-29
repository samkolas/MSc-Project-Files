using DefKit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerScript : MonoBehaviour
{

    public UnityEngine.UI.Text timerText;
    // Overall time the simulation takes
    private float timer;


    public void SetTimer(float t)
    {
        timer = t;
    }


    public float GetTimer()
    {
        return timer;
    }

    // Update is called once per frame
    public void Update()
    {
        // Transform time calculated into minutes and seconds
        string minutes = Mathf.Floor(timer / 60).ToString("00");
        string seconds = (timer % 60).ToString("00");

        // Time of entire simulation
        timerText.text = "Total Time:" + " " + string.Format("{0}:{1}", minutes, seconds);
    }



}