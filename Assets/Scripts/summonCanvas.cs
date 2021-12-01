using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
public class summonCanvas : MonoBehaviour
{
    public GameObject player;
    public GameObject canvas;
    public Vector3 spawnCanvasPosition;

    private int canvasCt;
    private InputDevice targetDevice;
    private bool bHeld;
    private bool started;
    private Whiteboard _whiteboard;
    private GameObject clone;
    // Start is called before the first frame update
    void Start()
    {
        bHeld = false;
        List<InputDevice> devices = new List<InputDevice>();
        InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        // InputDevices.GetDevices(devices);
        InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristics, devices);
        canvasCt = 0;
        foreach (var item in devices)
        {
            Debug.Log(item.name + item.characteristics);
        }

        if (devices.Count > 0)
        {
            targetDevice = devices[0];
            started = true;
        }


    }

    // Update is called once per frame
    void Update()
    {
        if (started)
        {
            SpawnCanvas();
        }
        else //in case the program is started and the righthand controller hasn't been woken up yet, this block of code will retrieve it once it is woken
        {
            List<InputDevice> devices = new List<InputDevice>();
            InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
            InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristics, devices);
            foreach (var item in devices)
            {
                Debug.Log(item.name + item.characteristics);
            }
            if (devices.Count > 0)
            {
                targetDevice = devices[0];
                started = true;
            }
        }
       


    }


    void SpawnCanvas()
    {
       
        targetDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bHeld);
      //  if (canvasCt > 0) return;

        if (bHeld)
        {
            spawnCanvasPosition = new Vector3(player.gameObject.transform.position.x, player.gameObject.transform.position.y, player.gameObject.transform.position.z + 3);

            if (canvasCt == 0)
            {
                clone = Instantiate(canvas, spawnCanvasPosition, new Quaternion(0, 0, 180, 1));
                canvasCt++;
            }
            return;
        }
        else
        {
            Destroy(clone);
            canvasCt--;
            return;
        }
    }
}
