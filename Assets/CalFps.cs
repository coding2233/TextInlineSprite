using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalFps : MonoBehaviour {

    private float starttime;

    private float endtime;

    private int framecnt;

	// Use this for initialization
	void Start () {
        Reset();
    }

    public void Reset()
    {
        starttime = Time.timeSinceLevelLoad;
        framecnt = 0;
        endtime = -1;
    }
	
	// Update is called once per frame
	void Update () {
		if(Time.timeSinceLevelLoad - starttime <5f)
        {
            framecnt++;
        }
        else
        {
            if (endtime < 0)
            {
                endtime = Time.timeSinceLevelLoad;
                float fps = framecnt / (endtime - starttime);
                Debug.LogError(fps);
            }
        }
    }
}
