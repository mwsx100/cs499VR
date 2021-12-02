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
    private Vector2 _hitPos, _lastHitPos, hitPosVec, lastRPos, lastQPos;
    private Vector2[] cornerCutPos;
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
               // cornerCutPos = cornerCut(_lastHitPos, _hitPos);
                //var q = cornerCutPos[0];
               // var r = cornerCutPos[1];
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


    private void DoubleCornerCutDraw()
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
                hitPosVec = new Vector2(x, y);
                cornerCutPos = cornerCut(lastQPos, lastRPos);
                var q0 = cornerCutPos[0]; 
                var r0 = cornerCutPos[1];
             

                cornerCutPos = cornerCut(_lastHitPos, hitPosVec);
                var q1 = cornerCutPos[0];
                var r1 = cornerCutPos[1];

                cornerCutPos = cornerCut(lastRPos, q1);
                var s0 = cornerCutPos[0];
                var t0 = cornerCutPos[1];


                cornerCutPos = cornerCut(q1, r1);
                var s1 = cornerCutPos[0];
                var t1 = cornerCutPos[1];
               


               // PixelCircle2((int)r.x, (int)r.y, _whiteboard, _penSize, pressure, _colors);
                float pressureInc = Math.Abs((oldPressure - pressure) / 200); //gets the absolute value of the difference between the old and new pressure, divided by the number of increments in the following for loop
                float temp = pressure; //temporary variable to keep track of the pressure variable
                pressure = oldPressure; //sets pressure to old pressure
                for (float f = 0.01f; f < 1.00f; f += 0.025f) //interpolate from the last point touched to current
                {
                    if (pressure > temp) pressure -= pressureInc; //if old pressure was more, gradually decrement
                    else pressure += pressureInc; //else gradually increment
                    var lerpRx = (int)Mathf.Lerp(a: (int)q0.x, b: (int)r0.x, t: f);
                    var lerpRy = (int)Mathf.Lerp(a: (int)q0.y, b: (int)r0.y, t: f);
                    PixelCircle2(lerpRx, lerpRy, _whiteboard, _penSize, pressure, _colors);
                }
                for (float f = 0.01f; f < 1.00f; f += 0.025f) //interpolate from the last point touched to current
                {
                    if (pressure > temp) pressure -= pressureInc; //if old pressure was more, gradually decrement
                    else pressure += pressureInc; //else gradually increment           
                    var lerpX = (int)Mathf.Lerp(a: (int)r0.x, b: (int)s0.x, t: f);
                    var lerpY = (int)Mathf.Lerp(a: (int)r0.y, b: (int)s0.y, t: f);
                    PixelCircle2(lerpX, lerpY, _whiteboard, _penSize, pressure, _colors);
                }
                for (float f = 0.01f; f < 1.00f; f += 0.025f) //interpolate from the last point touched to current
                {
                    if (pressure > temp) pressure -= pressureInc; //if old pressure was more, gradually decrement
                    else pressure += pressureInc; //else gradually increment
                    var lerpRx = (int)Mathf.Lerp(a: (int)s0.x, b: (int)t0.x, t: f);
                    var lerpRy = (int)Mathf.Lerp(a: (int)s0.y, b: (int)t0.y, t: f);
                    PixelCircle2(lerpRx, lerpRy, _whiteboard, _penSize, pressure, _colors);
                }
                for (float f = 0.01f; f < 1.00f; f += 0.025f) //interpolate from the last point touched to current
                {
                    if (pressure > temp) pressure -= pressureInc; //if old pressure was more, gradually decrement
                    else pressure += pressureInc; //else gradually increment
                    var lerpRx = (int)Mathf.Lerp(a: (int)t0.x, b: (int)s1.x, t: f);
                    var lerpRy = (int)Mathf.Lerp(a: (int)t0.y, b: (int)s1.y, t: f);
                    PixelCircle2(lerpRx, lerpRy, _whiteboard, _penSize, pressure, _colors);
                }

                for (float f = 0.01f; f < 1.00f; f += 0.025f) //interpolate from the last point touched to current
                {
                    if (pressure > temp) pressure -= pressureInc; //if old pressure was more, gradually decrement
                    else pressure += pressureInc; //else gradually increment
                    var lerpRx = (int)Mathf.Lerp(a: (int)s1.x, b: (int)t1.x, t: f);
                    var lerpRy = (int)Mathf.Lerp(a: (int)s1.y, b: (int)t1.y, t: f);
                    PixelCircle2(lerpRx, lerpRy, _whiteboard, _penSize, pressure, _colors);
                }




                pressure = temp; //reset pressure
                _whiteboard.texture.Apply(); //apply changes
                lastRPos = r1;
                lastQPos = q1;
            }
            _lastHitPos = new Vector2(x, y); //caches data for the next frame loop
            if (!_hitLastFrame)
            {

                lastRPos = _lastHitPos;
                lastQPos = _lastHitPos;
            }
               
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



    private void PixelCircle2(int x, int y, Whiteboard _whiteboard, int _penSize, float pressure, Color[] _colors)//circle generated using the coordinates of the unit circle  
    {
        _whiteboard.texture.SetPixels(x, y, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //center/origin square
        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure), y, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //_penSize is multiplied by the pressure of the trigger press
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure), y, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //positive x
        _whiteboard.texture.SetPixels(x, y + (int)(_penSize * pressure), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //positive y
        _whiteboard.texture.SetPixels(x, y - (int)(_penSize * pressure ), blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); //negative y

        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure * Math.Sqrt(2)) / 2, y + (int)(_penSize * pressure * Math.Sqrt(2)) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors); /*"corners" of the circle*/
        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure * Math.Sqrt(2)) / 2, y - (int)(_penSize * pressure * Math.Sqrt(2)) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure * Math.Sqrt(2)) / 2, y - (int)(_penSize * pressure * Math.Sqrt(2)) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure * Math.Sqrt(2)) / 2, y + (int)(_penSize * pressure * Math.Sqrt(2)) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);

        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure) / 2, y + (int)(_penSize * pressure * Math.Sqrt(3))/2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);/*more pixels to smooth it out.*/
        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure * Math.Sqrt(3))/2, y + (int)(_penSize * pressure)/2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);

        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure * Math.Sqrt(3))/2, y - (int)(_penSize * pressure) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x - (int)(_penSize * pressure) / 2, y - (int)(_penSize * pressure * Math.Sqrt(3))/2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);

        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure * Math.Sqrt(3))/2, y - (int)(_penSize * pressure) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure) / 2, y - (int)(_penSize * pressure * Math.Sqrt(3))/2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);

        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure * Math.Sqrt(3))/2, y + (int)(_penSize * pressure) / 2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);
        _whiteboard.texture.SetPixels(x + (int)(_penSize * pressure) / 2, y + (int)(_penSize * pressure * Math.Sqrt(3))/2, blockWidth: (int)(_penSize * pressure), blockHeight: (int)(_penSize * pressure), _colors);

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

    private Vector2[] cornerCut(Vector2 p1, Vector2 p2)
    {
        Vector2 q = .75f*p1 + .25f*p2;
        Vector2 r = .25f*p1 + .75f*p2;
        Vector2[] ret = new Vector2[] { q, r };
        return ret;

    }

   /* private void Bezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        float pt = p0 * Math.Pow(t, 2) + p1 * 2 * t * (1 - t) + p2 * Math.Pow((1 - t), 2);

    }*/
}