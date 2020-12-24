using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;


public class LevelStartCutscene : MonoBehaviour
{
    public Transform dummyPlayerTransform;
    public Transform playerTransform;
    public Transform dummyStart;
    public CinemachineVirtualCamera chasmCam;
    public GameObject[] gameObjectsToActivateOnFall;
    public float chasmCamEndDist;
    public float timeTilZoomedOut;
    public float fadeInTime;
    public PostProcessVolume PPVolume;

    private CinemachineComponentBase ComponentBase;
    private float ChasmCamStartDist;

    private float Timer;
    
  

    void Awake()
    {
        dummyPlayerTransform.position = dummyStart.position;
        ComponentBase = chasmCam.GetCinemachineComponent(CinemachineCore.Stage.Body);
        if (ComponentBase is CinemachineFramingTransposer == false)
        {
            Debug.LogError("Cannot find starting camera's framing transposer");
        }

        Timer = 0;
        ChasmCamStartDist = (ComponentBase as CinemachineFramingTransposer).m_CameraDistance;
    }

    float WeightAtStartOfFadeIn;
    bool ZoomedOut = false;
    bool HasCut = false;
    bool HasFadedIn = false;
    bool SkipCutscene = false;
    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.R) && SkipCutscene == false)
        {
            SkipCutscene = true;
            fadeInTime = 0;
        }

        if (ZoomedOut == false && HasCut == false)
        {
            // setting cinemachine virtual camera distance value for gradual zoom out. 
            Timer += Time.deltaTime;
            float t = Timer / timeTilZoomedOut;
            (ComponentBase as CinemachineFramingTransposer).m_CameraDistance = 
                Mathf.Lerp(ChasmCamStartDist, chasmCamEndDist, t);


            PPVolume.weight = t;
            if (t >= 1)
            {
                ZoomedOut = true;
                Timer = 0;
            }
        }


        if ((dummyPlayerTransform.GetComponent<DummyPlayer>().hasLanded || SkipCutscene) && HasCut == false)
        {
            // cut to black. transition between cameras. Switch dummy player for real player.
            PPVolume.weight = 0.9f;
            WeightAtStartOfFadeIn = PPVolume.weight;
            dummyPlayerTransform.gameObject.SetActive(false);
            playerTransform.gameObject.SetActive(true);
            chasmCam.enabled = false;
            Timer = 0;
            HasCut = true;
        }
        else if (HasCut && HasFadedIn == false)
        { 
            // fade in.
            Timer += Time.deltaTime;
            float t = Timer / fadeInTime;
            PPVolume.weight = Mathf.Lerp(WeightAtStartOfFadeIn, 0.1f, t);

            if (t >= 1)
            {
                HasFadedIn = true;
            }
        }

        if (HasFadedIn)
        {
            for (int i = 0; i < gameObjectsToActivateOnFall.Length; i++)
            {
                gameObjectsToActivateOnFall[i].SetActive(true) ;
            }
            gameObject.SetActive(false);
        }
       
    }
}
