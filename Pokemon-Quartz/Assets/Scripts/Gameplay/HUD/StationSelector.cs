using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StationSelector : MonoBehaviour
{
    //Create station box for choices.
    [SerializeField] TextSelector stationBox;

    //Import all radio station text.
    [SerializeField] StationPlaylist melloMusic;
    [SerializeField] StationPlaylist kricketuneClassics;
    [SerializeField] StationPlaylist chatotStat;

    [SerializeField] Image radioArt;
    [SerializeField] Sprite mellotte;
    [SerializeField] Sprite kricket;
    [SerializeField] Sprite chatot;
    [SerializeField] Sprite standard;

    //Tracks what section the player is on right now.
    int currentStation;

    //Stores current station playing.
    private StationPlaylist activeStation;

    //Runs the station section
    public void HandleUpdate()
    {
        stationSelection();
    }

    //Highlights and allows player to make a choice.
    public void stationSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ++currentStation;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            --currentStation;
        }

        //Always 3 choices.
        currentStation = Mathf.Clamp(currentStation, 0, 2);

        //Highlights player's selection.
        stationBox.UpdateSelection(currentStation);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            //Player selected melloStation.
            if (currentStation == 0)
            {
                selectStation(melloMusic, mellotte);
            }
            //Player selected kricketuneClassics.
            else if (currentStation == 1)
            {
                selectStation(kricketuneClassics, kricket);
            }
            //Player selected chatotStat.
            else if (currentStation == 2)
            {
                selectStation(chatotStat, chatot);
            }
        }
    }

    //Plays and stops radio music.
    public void selectStation(StationPlaylist station, Sprite stationArt)
    {
        //None or another radio station was active, play chosen station.
        if(activeStation != station || activeStation == null)
        {
            //Stop active station if there exists.
            if (activeStation != null)
            {
                activeStation.StopMusic();
                activeStation = null;
            }

            activeStation = station;
            radioArt.sprite = stationArt;
            station.GetComponent<StationPlaylist>().StartMusic();
        }
        //Current station is the active one. We want to stop the station.
        else
        {
            if (activeStation != null)
            {
                activeStation.StopMusic();
                activeStation = null;
                radioArt.sprite = standard;
            }
        }
    }
}

