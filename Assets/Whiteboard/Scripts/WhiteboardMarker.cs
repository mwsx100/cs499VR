using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WhiteboardMarker : MonoBehaviour
{
    [SerializeField] private Transform _tip;
    [SerializeField] private int _penSize = 5; //size of pen

    private Renderer _renderer; //renderer
    private Color[] _colors; //color array
    private float _tipHeight; 

    private RaycastHit _touch;
    private Whiteboard _whiteboard;
    private Vector2 _touchPos, _lastTouchPos;
    private bool _touchedLastFrame;
    private Quaternion _lastTouchRot;
   
    void Start()
    {
        _renderer = _tip.GetComponent<Renderer>(); //gets renderer
        _colors = Enumerable.Repeat(_renderer.material.color, _penSize * _penSize).ToArray(); //sets up colors array
        _tipHeight = _tip.localScale.y;
    }

    
    void Update()
    {
        Draw(); //calls draw method every frame
    }

    private void Draw()
    {
        if (Physics.Raycast(origin:_tip.position, direction:transform.up, out _touch, _tipHeight))
        {
            if(_touch.transform.CompareTag("Whiteboard")) //checks if item touched is a whiteboard
            {
                if (_whiteboard == null)
                {
                    _whiteboard = _touch.transform.GetComponent<Whiteboard>();
                }
                _touchPos = new Vector2(_touch.textureCoord.x, _touch.textureCoord.y); //touch position

                var x = (int)(_touchPos.x * _whiteboard.textureSize.x - (_penSize / 2));
                var y = (int)(_touchPos.y * _whiteboard.textureSize.y - (_penSize / 2));

                if (y < 0 || y > _whiteboard.textureSize.y || x < 0 || x > _whiteboard.textureSize.x) return; //if touched outside of whiteboard, exit

                if (_touchedLastFrame)
                {
                    _whiteboard.texture.SetPixels(x, y, blockWidth: _penSize, blockHeight: _penSize, _colors);

                    for (float f = 0.01f; f < 1.00f; f+= 0.01f) //interpolate from the last point touched to current
                    {
                        var lerpX = (int)Mathf.Lerp(a:_lastTouchPos.x, b:x, t:f);
                        var lerpY = (int)Mathf.Lerp(a:_lastTouchPos.y, b:y, t:f);
                        _whiteboard.texture.SetPixels(lerpX, lerpY, blockWidth: _penSize, blockHeight: _penSize, _colors);

                    }

                    transform.rotation = _lastTouchRot; //locks rotation of pen so it doesnt snap up while drawing

                    _whiteboard.texture.Apply();
                }

                _lastTouchPos = new Vector2(x, y); //caches data for the next frame loop
                _lastTouchRot = transform.rotation;
                _touchedLastFrame = true;
                return;
            }
        }

        _whiteboard = null;
        _touchedLastFrame = false;
    }
}
