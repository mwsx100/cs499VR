using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using UnityEngine;

public class PointToDraw : MonoBehaviour
{
    [SerializeField] private int _penSize = 5;
    [SerializeField] private Transform _controller;

    private InputDevice targetDevice;
    private XRRayInteractor _ray;
    private Renderer _renderer;
    private Color[] _colors;
    private ActionBasedController _remote;

    private RaycastHit _hit;
    private Whiteboard _whiteboard;
    private Bucket bucket;
    private Vector2 _hitPos, _lastHitPos;
    private bool _hitLastFrame;
    private bool triggerValue;
    private float oldPressure;
    private bool started = false;





    // Start is called before the first frame update
    void Start()
    {
        _renderer = _controller.GetComponent<Renderer>(); //gets renderer
        _colors = Enumerable.Repeat(_renderer.material.color, _penSize * _penSize).ToArray(); //sets up colors array
        _ray = _controller.GetComponent<XRRayInteractor>();
        _remote = _controller.GetComponent<ActionBasedController>(); //i think this line can be deleted
        List<InputDevice> devices = new List<InputDevice>();
        InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        // InputDevices.GetDevices(devices);
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

    // Update is called once per frame
    void Update()
    {
        if (started)
        {
            Draw();
            Swap();
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

    private void Draw()
    {
        targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float pressure);
        if (_ray.TryGetCurrent3DRaycastHit(out _hit) && pressure > 0.05f) 
        {

            if (_hit.transform.CompareTag("Whiteboard"))
            {
                if (_whiteboard == null)
                {
                    _whiteboard = _hit.transform.GetComponent<Whiteboard>();
                }
            }
            else return; //if object isn't a whiteboard, return

            _hitPos = new Vector2(_hit.textureCoord.x, _hit.textureCoord.y); //hit position

            var x = (int)(_hitPos.x * _whiteboard.textureSize.x - (_penSize / 2));
            var y = (int)(_hitPos.y * _whiteboard.textureSize.y - (_penSize / 2));

            if (y < 0 || y > _whiteboard.textureSize.y || x < 0 || x > _whiteboard.textureSize.x) return; //if touched outside of whiteboard, exit

            if (_hitLastFrame)
            {
                PixelCircle(x, y, _whiteboard, _penSize, pressure, _colors);
                float pressureInc = Math.Abs((oldPressure - pressure) / 100); //gets the absolute value of the difference between the old and new pressure, divided by the number of increments in the following for loop
                float temp = pressure; //temporary variable to keep track of the pressure variable
                pressure = oldPressure; //sets pressure to old pressure

                for (float f = 0.01f; f < 1.00f; f += 0.01f) //interpolate from the last point touched to current
                {
                    if (pressure > temp) pressure -= pressureInc; //if old pressure was more, gradually decrement
                    else pressure += pressureInc; //else gradually increment
                    var lerpX = (int)Mathf.Lerp(a: _lastHitPos.x, b: x, t: f); 
                    var lerpY = (int)Mathf.Lerp(a: _lastHitPos.y, b: y, t: f);
                    PixelCircle(lerpX, lerpY, _whiteboard, _penSize, pressure, _colors);
                }
                pressure = temp; //reset pressure
                _whiteboard.texture.Apply(); //apply changes
            }

            _lastHitPos = new Vector2(x, y); //caches data for the next frame loop
            _hitLastFrame = true; 
            oldPressure = pressure; //caches current pressure for next frame loop
            return;
        }
        _whiteboard = null;
        _hitLastFrame = false;
    }


    private void PixelCircle(int x, int y, Whiteboard _whiteboard, int _penSize, float pressure, Color[] _colors)//sets 17 squares positioned in a way that they look circular    
    {
        _whiteboard.texture.SetPixels(x, y, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //center/origin square
        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure*.9), y, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //_penSize is multiplied by the pressure of the trigger press
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure*.9), y, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //positive x
        _whiteboard.texture.SetPixels(x, y + (int)(_penSize * pressure*.9), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //positive y
        _whiteboard.texture.SetPixels(x, y - (int)(_penSize * pressure*.9), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //negative y

        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure) / 2, y + (int)(_penSize * pressure) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); /*"corners" of the circle*/
        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure) / 2, y - (int)(_penSize * pressure) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure) / 2, y - (int)(_penSize * pressure) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure) / 2, y + (int)(_penSize * pressure) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
     
        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure * 0.75), y + (int)(_penSize * pressure)/4, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);/*more pixels to smooth it out*/
        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure)/4, y + (int)(_penSize * pressure * 0.75), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);

        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure * 0.75), y - (int)(_penSize * pressure)/4, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure)/4, y - (int)(_penSize * pressure * 0.75), blockWidth: (int)(_penSize * pressure),blockHeight: (int)(_penSize * pressure), _colors);

        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure * 0.75), y - (int)(_penSize * pressure)/4, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure)/4, y - (int)(_penSize * pressure * 0.75), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);

        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure * 0.75), y + (int)(_penSize * pressure)/4, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure)/4, y + (int)(_penSize * pressure*.75), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);

    }

    private void Swap()
    {
        targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float pressure);
        if (_ray.TryGetCurrent3DRaycastHit(out _hit) && pressure > 0.1f)
        {
            Debug.Log("X" +_hit.transform.tag + "Y");
            if (_hit.transform.CompareTag("Bucket")) //if we are pointing at a bucket, change our current paint color to whatever color the bucket object has
            {
                bucket = _hit.transform.GetComponent<Bucket>(); //gets renderer
                _colors = Enumerable.Repeat(bucket.color, _penSize * _penSize).ToArray();
            }
        }
    }
}