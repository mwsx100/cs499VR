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
    [SerializeField] private float opacity = 1f;

    private InputDevice targetDevice;
    private XRRayInteractor _ray;
    private Renderer _renderer;
    private Color[] _colors;
    private Color penColor;
    private ActionBasedController _remote;

    private RaycastHit _hit;
    private Whiteboard _whiteboard;
    private Bucket bucket;
    private Vector2 _hitPos, _lastHitPos, hitPosVec, lastRPos, lastQPos;
    private Vector2[] cornerCutPos;
    private bool _hitLastFrame;
    private bool triggerValue;
    private float oldPressure, pressure;
    private bool started = false;





    // Start is called before the first frame update
    void Start()
    {
        _renderer = _controller.GetComponent<Renderer>(); //gets renderer
        penColor = _renderer.material.color;

        Color newColor = new Color(penColor.r, penColor.g, penColor.b, opacity);
        penColor.a = opacity;
        Debug.Log(penColor.a);
        _colors = Enumerable.Repeat(newColor, _penSize * _penSize).ToArray(); //sets up colors array
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
          //  Draw();

          //  CornerCutDraw();
           DoubleCornerCutDraw();
           
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
        targetDevice.TryGetFeatureValue(CommonUsages.trigger, out pressure);
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
               
                PixelCircle2(x, y, _whiteboard, _penSize, pressure, _colors);
                float pressureInc = Math.Abs((oldPressure - pressure) / 100); //gets the absolute value of the difference between the old and new pressure, divided by the number of increments in the following for loop
                float temp = pressure; //temporary variable to keep track of the pressure variable
                pressure = oldPressure; //sets pressure to old pressure
               

                for (float f = 0.01f; f < 1.00f; f += 0.01f) //interpolate from the last point touched to current
                {
                    if (pressure > temp) pressure -= pressureInc; //if old pressure was more, gradually decrement
                    else pressure += pressureInc; //else gradually increment
                    var lerpX = (int)Mathf.Lerp(a: _lastHitPos.x, b: x, t: f); 
                    var lerpY = (int)Mathf.Lerp(a: _lastHitPos.y, b: y, t: f);
                    PixelCircle2(lerpX, lerpY, _whiteboard, _penSize, pressure, _colors);
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



    private void CornerCutDrawDotted() 
    {
        targetDevice.TryGetFeatureValue(CommonUsages.trigger, out pressure);
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
                Debug.Log(x + "hitPosx\n");
                Debug.Log(y + "hitPosy\n");
                hitPosVec = new Vector2(x, y);
                cornerCutPos = cornerCut(_lastHitPos, hitPosVec);
                var q = cornerCutPos[0];
                var r = cornerCutPos[1];

                Debug.Log(q.x + " q.x\n");
                Debug.Log(q.y + " q.y\n");

                Debug.Log(q.x + " r.x\n");
                Debug.Log(_lastHitPos.x + " last.x\n");
                Debug.Log(_lastHitPos.y + " last.y\n");              
                PixelCircle2((int)r.x, (int)r.y, _whiteboard, _penSize, pressure, _colors);           
                float pressureInc = Math.Abs((oldPressure - pressure) / 100); //gets the absolute value of the difference between the old and new pressure, divided by the number of increments in the following for loop
                float temp = pressure; //temporary variable to keep track of the pressure variable
                pressure = oldPressure; //sets pressure to old pressure


                for (float f = 0.01f; f < 1.00f; f += 0.01f) //interpolate from the last point touched to current
                {
                    if (pressure > temp) pressure -= pressureInc; //if old pressure was more, gradually decrement
                    else pressure += pressureInc; //else gradually increment
                    var lerpX = (int)Mathf.Lerp(a: (int)q.x, b: (int)r.x, t: f);
                    var lerpY = (int)Mathf.Lerp(a: (int)q.y, b: (int)r.y, t: f);
                    PixelCircle2(lerpX, lerpY, _whiteboard, _penSize, pressure, _colors);
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


    private void CornerCutDraw() //chaikin's corner cutting algorithm inmplemented for smoother line drawing 
    {
        targetDevice.TryGetFeatureValue(CommonUsages.trigger, out pressure);
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
                hitPosVec = new Vector2(x, y);
                cornerCutPos = cornerCut(_lastHitPos, hitPosVec);
                var q = cornerCutPos[0];
                var r = cornerCutPos[1];          
                PixelCircle2((int)r.x, (int)r.y, _whiteboard, _penSize, pressure, _colors);
                float pressureInc = Math.Abs((oldPressure - pressure) / 100); //gets the absolute value of the difference between the old and new pressure, divided by the number of increments in the following for loop
                float temp = pressure; //temporary variable to keep track of the pressure variable
                pressure = oldPressure; //sets pressure to old pressure            
                for (float f = 0.01f; f < 1.00f; f += 0.02f) //interpolate from the last point touched to current
                {
                    if (pressure > temp) pressure -= pressureInc; //if old pressure was more, gradually decrement
                    else pressure += pressureInc; //else gradually increment
                    var lerpRx = (int)Mathf.Lerp(a: (int)lastRPos.x, b: (int)q.x, t: f);
                    var lerpRy = (int)Mathf.Lerp(a: (int)lastRPos.y, b: (int)q.y, t: f);
                    PixelCircle2(lerpRx, lerpRy, _whiteboard, _penSize, pressure, _colors);
                 }
                for (float f = 0.01f; f < 1.00f; f += 0.02f) //interpolate from the last point touched to current
                {
                    if (pressure > temp) pressure -= pressureInc; //if old pressure was more, gradually decrement
                    else pressure += pressureInc; //else gradually increment           
                    var lerpX = (int)Mathf.Lerp(a: (int)q.x, b: (int)r.x, t: f);
                    var lerpY = (int)Mathf.Lerp(a: (int)q.y, b: (int)r.y, t: f);
                    PixelCircle2(lerpX, lerpY, _whiteboard, _penSize, pressure, _colors);
                 }
                pressure = temp; //reset pressure
                _whiteboard.texture.Apply(); //apply changes
                lastRPos = r;
            }             
            _lastHitPos = new Vector2(x, y); //caches data for the next frame loop
            if(!_hitLastFrame) lastRPos = _lastHitPos;
            _hitLastFrame = true;
            oldPressure = pressure; //caches current pressure for next frame loop
            return;
        }
        _whiteboard = null;
        _hitLastFrame = false;
    }


    private void DoubleCornerCutDraw() //corners are cut twice here, this is probably the best one
    {
        targetDevice.TryGetFeatureValue(CommonUsages.trigger, out pressure);
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
                hitPosVec = new Vector2(x, y);
                cornerCutPos = cornerCut(lastQPos, lastRPos); //corner cuts the previous corner cut
                var q0 = cornerCutPos[0]; //point1 start point
                var r0 = cornerCutPos[1]; //point2
                cornerCutPos = cornerCut(_lastHitPos, hitPosVec); //corner cuts last hit and current hit
                var q1 = cornerCutPos[0]; 
                var r1 = cornerCutPos[1];
                cornerCutPos = cornerCut(lastRPos, q1); //corner cuts the r coordinate from the previous corner cut and the q coordinate from the current corner cut
                var s0 = cornerCutPos[0]; //point3
                var t0 = cornerCutPos[1]; //point4
                cornerCutPos = cornerCut(q1, r1); //corner cuts the current corner cut
                var s1 = cornerCutPos[0]; //point5
                var t1 = cornerCutPos[1]; //point6 end end point
               
                float pressureInc = Math.Abs((oldPressure - pressure) / 200); //gets the absolute value of the difference between the old and new pressure, divided by the number of increments in the following for loop
                float temp = pressure; //temporary variable to keep track of the pressure variable
                pressure = oldPressure; //sets pressure to old pressure
                pressure = Interpolate(q0, r0, _whiteboard, _penSize, pressure, pressureInc, temp, 0.025f, _colors);
                pressure = Interpolate(r0, s0, _whiteboard, _penSize, pressure, pressureInc, temp, 0.025f, _colors);
                pressure = Interpolate(s0, t0, _whiteboard, _penSize, pressure, pressureInc, temp, 0.025f, _colors);
                pressure = Interpolate(t0, s1, _whiteboard, _penSize, pressure, pressureInc, temp, 0.025f, _colors);
                pressure = Interpolate(s1, t1, _whiteboard, _penSize, pressure, pressureInc, temp, 0.025f, _colors);
                pressure = temp; //reset pressure
                _whiteboard.texture.Apply(); //apply changes
                lastRPos = r1;
                lastQPos = q1;
            }
            _lastHitPos = new Vector2(x, y); //caches data for the next frame loop
            if (!_hitLastFrame)
            {
                lastRPos = _lastHitPos; //if q and r are the same, the interpolate loop will just interpolate in place
                lastQPos = _lastHitPos;
            }
               
            _hitLastFrame = true;
            oldPressure = pressure; //caches current pressure for next frame loop
            return;
        }
        _whiteboard = null;
        _hitLastFrame = false;
    }




    private float Interpolate(Vector2 p1, Vector2 p2, Whiteboard _whiteboard, int _penSize, float pressure, float pressureInc, float temp, float fInc, Color[] _colors)
    {
        for (float f = 0.01f; f < 1.00f; f += fInc) //interpolate from the last point touched to current
        {
            if (pressure > temp) pressure -= pressureInc; //if old pressure was more, gradually decrement
            else pressure += pressureInc; //else gradually increment           
            var lerpX = (int)Mathf.Lerp(a: (int)p1.x, b: (int)p2.x, t: f);
            var lerpY = (int)Mathf.Lerp(a: (int)p1.y, b: (int)p2.y, t: f);
           // _whiteboard.texture.SetPixels(lerpX, lerpY, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //center/origin square
      
            PixelCircle2(lerpX, lerpY, _whiteboard, _penSize, pressure, _colors);
          //  Spray(_whiteboard, _colors[0], lerpX, lerpY, (int)(_penSize * pressure));
            //  DrawCircle(_whiteboard, _colors[0], lerpX, lerpY, (int)(_penSize * pressure));
        }
        return pressure;

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
                Color newColor = new Color(bucket.color.r, bucket.color.g, bucket.color.b, opacity);
                _colors = Enumerable.Repeat(newColor, _penSize * _penSize).ToArray();
            }
        }
    }

    private Vector2[] cornerCut(Vector2 p1, Vector2 p2) //chaikin's corner cutting algorithm
    {
        Vector2 q = .75f*p1 + .25f*p2;
        Vector2 r = .25f*p1 + .75f*p2;
        Vector2[] ret = new Vector2[] { q, r };
        return ret;

    }



    private void PixelCircle(int x, int y, Whiteboard _whiteboard, int _penSize, float pressure, Color[] _colors)//sets 17 squares positioned in a way that they look circular    
    {
        _whiteboard.texture.SetPixels(x, y, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //center/origin square
        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure * .9), y, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //_penSize is multiplied by the pressure of the trigger press
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure * .9), y, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //positive x
        _whiteboard.texture.SetPixels(x, y + (int)(_penSize * pressure * .9), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //positive y
        _whiteboard.texture.SetPixels(x, y - (int)(_penSize * pressure * .9), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //negative y

        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure) / 2, y + (int)(_penSize * pressure) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); /*"corners" of the circle*/
        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure) / 2, y - (int)(_penSize * pressure) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure) / 2, y - (int)(_penSize * pressure) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure) / 2, y + (int)(_penSize * pressure) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);

        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure * 0.75), y + (int)(_penSize * pressure) / 4, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);/*more pixels to smooth it out*/
        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure) / 4, y + (int)(_penSize * pressure * 0.75), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);

        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure * 0.75), y - (int)(_penSize * pressure) / 4, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure) / 4, y - (int)(_penSize * pressure * 0.75), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);

        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure * 0.75), y - (int)(_penSize * pressure) / 4, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure) / 4, y - (int)(_penSize * pressure * 0.75), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);

        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure * 0.75), y + (int)(_penSize * pressure) / 4, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure) / 4, y + (int)(_penSize * pressure * .75), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);

    }



    private void PixelCircle2(int x, int y, Whiteboard _whiteboard, int _penSize, float pressure, Color[] _colors)//circle generated using the coordinates of the unit circle  
    {
        _whiteboard.texture.SetPixels(x, y, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //center/origin square
        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure), y, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //_penSize is multiplied by the pressure of the trigger press
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure), y, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //positive x
        _whiteboard.texture.SetPixels(x, y + (int)(_penSize * pressure), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //positive y
        _whiteboard.texture.SetPixels(x, y - (int)(_penSize * pressure), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //negative y

        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure * Math.Sqrt(2)) / 2, y + (int)(_penSize * pressure * Math.Sqrt(2)) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); /*"corners" of the circle*/
        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure * Math.Sqrt(2)) / 2, y - (int)(_penSize * pressure * Math.Sqrt(2)) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure * Math.Sqrt(2)) / 2, y - (int)(_penSize * pressure * Math.Sqrt(2)) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure * Math.Sqrt(2)) / 2, y + (int)(_penSize * pressure * Math.Sqrt(2)) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);

        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure) / 2, y + (int)(_penSize * pressure * Math.Sqrt(3)) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);/*more pixels to smooth it out.*/
        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure * Math.Sqrt(3)) / 2, y + (int)(_penSize * pressure) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);

        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure * Math.Sqrt(3)) / 2, y - (int)(_penSize * pressure) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure) / 2, y - (int)(_penSize * pressure * Math.Sqrt(3)) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);

        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure * Math.Sqrt(3)) / 2, y - (int)(_penSize * pressure) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure) / 2, y - (int)(_penSize * pressure * Math.Sqrt(3)) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);

        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure * Math.Sqrt(3)) / 2, y + (int)(_penSize * pressure) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure) / 2, y + (int)(_penSize * pressure * Math.Sqrt(3)) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);

    }

    private void PixelCircle3(int x, int y, Whiteboard _whiteboard, int _penSize, float pressure, Color[] _colors)//circle generated using the coordinates of the unit circle  with more angles, looks very slightly smoother
    {
        _whiteboard.texture.SetPixels(x, y, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //center/origin square
        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure), y, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //_penSize is multiplied by the pressure of the trigger press
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure), y, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //positive x
        _whiteboard.texture.SetPixels(x, y + (int)(_penSize * pressure), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //positive y
        _whiteboard.texture.SetPixels(x, y - (int)(_penSize * pressure), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //negative y

        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure * Math.Sqrt(2)) / 2, y + (int)(_penSize * pressure * Math.Sqrt(2)) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); /*"corners" of the circle*/
        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure * Math.Sqrt(2)) / 2, y - (int)(_penSize * pressure * Math.Sqrt(2)) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure * Math.Sqrt(2)) / 2, y - (int)(_penSize * pressure * Math.Sqrt(2)) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure * Math.Sqrt(2)) / 2, y + (int)(_penSize * pressure * Math.Sqrt(2)) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);

        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure) / 2, y + (int)(_penSize * pressure * Math.Sqrt(3)) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);/*more pixels to smooth it out.*/
        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure * Math.Sqrt(3)) / 2, y + (int)(_penSize * pressure) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);

        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure * (Math.Sqrt(6) - Math.Sqrt(2)) / 4), y + (int)(_penSize * pressure * (Math.Sqrt(6) + Math.Sqrt(2)) / 4), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);//105
        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure * (Math.Sqrt(6) + Math.Sqrt(2)) / 4), y + (int)(_penSize * pressure * (Math.Sqrt(6) - Math.Sqrt(2)) / 4), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);//165


        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure * Math.Sqrt(3)) / 2, y - (int)(_penSize * pressure) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure) / 2, y - (int)(_penSize * pressure * Math.Sqrt(3)) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);

        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure * (Math.Sqrt(6) - Math.Sqrt(2)) / 4), y - (int)(_penSize * pressure * (Math.Sqrt(6) + Math.Sqrt(2)) / 4), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); // 255
        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure * (Math.Sqrt(6) + Math.Sqrt(2)) / 4), y - (int)(_penSize * pressure * (Math.Sqrt(6) - Math.Sqrt(2)) / 4), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); // 195

        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure * Math.Sqrt(3)) / 2, y - (int)(_penSize * pressure) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure) / 2, y - (int)(_penSize * pressure * Math.Sqrt(3)) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);

        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure * (Math.Sqrt(6) - Math.Sqrt(2)) / 4), y - (int)(_penSize * pressure * (Math.Sqrt(6) + Math.Sqrt(2)) / 4), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); // 285
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure * (Math.Sqrt(6) + Math.Sqrt(2)) / 4), y - (int)(_penSize * pressure * (Math.Sqrt(6) - Math.Sqrt(2)) / 4), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); // 345

        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure * Math.Sqrt(3)) / 2, y + (int)(_penSize * pressure) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure) / 2, y + (int)(_penSize * pressure * Math.Sqrt(3)) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);

        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure * (Math.Sqrt(6) + Math.Sqrt(2)) / 4), y + (int)(_penSize * pressure * (Math.Sqrt(6) - Math.Sqrt(2)) / 4), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);//15
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure * (Math.Sqrt(6) - Math.Sqrt(2)) / 4), y + (int)(_penSize * pressure * (Math.Sqrt(6) + Math.Sqrt(2)) / 4), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);//75


    }


    private void DrawCircle(Whiteboard _whiteboard, Color color, int x, int y, int radius) //iteratively draws a circle. smoothest circle but will slow down the program the larger the circle is
    {
        float rSquared = radius * radius;
        for (int u = x - radius; u < x + radius + 1; u++)
            for (int v = y - radius; v < y + radius + 1; v++)
                if ((x - u) * (x - u) + (y - v) * (y - v) < rSquared)
                {
                    Color a = new Color(1, 1, 1);
                    a = _whiteboard.texture.GetPixel(u, v);
                    a.r = (a.r * (1 - opacity) + color.r * opacity);
                    a.g = (a.g * (1 - opacity) + color.g * opacity);
                    a.b = (a.b * (1 - opacity) + color.b * opacity);
                    // Debug.Log(a.r + ", " + a.g + ", " + a.b);
                    //  a.r = Math.Min(a.r, color.r);
                    // a.g = Math.Min(a.g, color.g);
                    //a.b = Math.Min(a.b, color.b);
                    _whiteboard.texture.SetPixel(u, v, a);
                    //  u+=1; v+=1;
                }
    }

    private void Spray(Whiteboard _whiteboard, Color color, int x, int y, int radius) //spray using system.random
    {
        var rand = new System.Random();
        int incr;
        float rSquared = radius * radius;
        for (int u = x - radius; u < x + radius + 1; u++)
            for (int v = y - radius; v < y + radius + 1; v++)
                if ((x - u) * (x - u) + (y - v) * (y - v) < rSquared)
                {
                    _whiteboard.texture.SetPixel(u, v, color);
                    incr = rand.Next(0, 20);
                    u += incr; v += incr;
                }
    }

    private void Spray2(Whiteboard _whiteboard, Color color, int x, int y, int radius) //spray using UnityEngine.Random
    {

        float rSquared = radius * radius;
        for (int u = x - radius; u < x + radius + 1; u++)
            for (int v = y - radius; v < y + radius + 1; v++)
                if ((x - u) * (x - u) + (y - v) * (y - v) < rSquared)
                {
                    Vector2 randPoint = UnityEngine.Random.insideUnitCircle * radius;
                    _whiteboard.texture.SetPixel((int)randPoint.x + x, (int)randPoint.y + y, color);
                    u++; v++;
                }
    }





    /* private void Bezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
     {
         float pt = p0 * Math.Pow(t, 2) + p1 * 2 * t * (1 - t) + p2 * Math.Pow((1 - t), 2);

     }*/
}