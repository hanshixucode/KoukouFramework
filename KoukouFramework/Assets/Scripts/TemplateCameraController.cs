using System;
using System.Collections;
using System.Collections.Generic;
using FairyGUI;
using UnityEngine;

public class TemplateCameraController : MonoBehaviour
{
    
    /// <summary>
    /// 控制主摄像机
    /// </summary>
    public Camera mainCamera;

    /// <summary>
    /// 摄像机缩放范围
    /// </summary>
    [Header("摄像机缩放范围")]
    public Vector2 zoomRange = new Vector2(10, 50);
    /// <summary>
    /// 当前的地图范围, 以世界坐标计
    /// </summary>
    [Header("摄像机移动范围")]
    public Rect mapRange = new Rect(-1000, -1000, 2000, 2000);
    /// <summary>
    /// 摄像机正交状态远近
    /// </summary>
    [Header("控制摄像机正交默认尺寸")] 
    public float conrtrol_camera_size;
    /// <summary>
    /// 摄像机默认位置
    /// </summary>
    [Header("控制摄像机正交默认尺寸")] 
    public Vector3 control_camera_pos;                     
    
    /// <summary>
    /// 摄像机rect尺寸一半
    /// </summary>
    private Vector2 halfCameraRectSize = new Vector2(1, 1);
    
    float _portraitOrthographicSize = 0;
    /// <summary>
    /// 导出摄像机的实际尺寸操作接口
    /// </summary>
    /// <value></value>
    public float orthographicSize
    {
        get => _portraitOrthographicSize;
        set 
        {
            _portraitOrthographicSize = value;
            if (Screen.width > Screen.height)
            {
                //  屏幕横向状态
                value *= (float)Screen.height / (float)Screen.width;
            }

            mainCamera.orthographicSize = value;
        }
    }
    private PinchGesture pinch_gesture;
    private SwipeGesture swipe_gesture;
    private void Awake()
    {
        InitCameraSize();
        
        pinch_gesture = new PinchGesture(GRoot.inst);
        swipe_gesture = new SwipeGesture(GRoot.inst);
        swipe_gesture.onMove.AddCapture(Capture);
        pinch_gesture.onAction.AddCapture(Gesture);
        Stage.inst.onMouseWheel.AddCapture(MouseWheelZoom);

    }
    /// <summary>
    /// 滑动摄像机
    /// </summary>
    /// <param name="context"></param>
    private void Capture(EventContext context)
    {
        //if(Stage.isTouchOnUI) return;
        //var factor = mainCamera.orthographicSize / Screen.height * 2;
        //var tmp = swipe_gesture.delta;
        //tmp.x = -tmp.x;
        var factor = mainCamera.orthographicSize / Screen.height * 2;
        var tmp = new Vector2(-swipe_gesture.delta.x, swipe_gesture.delta.y) * factor;

        var pos = mainCamera.transform.position + (Vector3)tmp;
        mainCamera.transform.position = ClampInRect(pos);
    }
    /// <summary>
    /// 限制摄像机移动范围
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private Vector3 ClampInRect(Vector3 pos)
    {
        if (mapRange == null)
            return pos;

        // GameLog.Debug($"{pos}, {mapRange}, {halfCameraRectSize}");

        if (mapRange.width > halfCameraRectSize.x * 2)
            pos.x = Mathf.Clamp(pos.x, mapRange.xMin + halfCameraRectSize.x, mapRange.xMax - halfCameraRectSize.x);
        else
            pos.x = mapRange.center.x;

        if (mapRange.height > halfCameraRectSize.y * 2)
            pos.y = Mathf.Clamp(pos.y, mapRange.yMin + halfCameraRectSize.y, mapRange.yMax - halfCameraRectSize.y);
        else
            pos.y = mapRange.center.y;

        pos.z = -10;
        return pos;
    }



    /// <summary>
    /// 两指缩放
    /// </summary>
    /// <param name="context"></param>
    private void Gesture(EventContext context)
    {
        //if(Stage.isTouchOnUI) return;
        var size = Mathf.Clamp(mainCamera.orthographicSize - pinch_gesture.delta * 10, zoomRange.x, zoomRange.y);
        mainCamera.orthographicSize = size;
        mainCamera.transform.position = ClampInRect(mainCamera.transform.position);
    }

    /// <summary>
    /// 更新摄像机尺寸
    /// </summary>
    private void UpdateCameraSize()
    {
        halfCameraRectSize.x = (float)Screen.width / (float)Screen.height * mainCamera.orthographicSize;
        halfCameraRectSize.y = mainCamera.orthographicSize;
    }
    private void MouseWheelZoom(EventContext context)
    {
        //if(Stage.isTouchOnUI) return;
        mainCamera.orthographicSize = Miscs.wrap<float>(mainCamera.orthographicSize + 0.05f * context.inputEvent.mouseWheelDelta, zoomRange.x, zoomRange.y);
        mainCamera.transform.position = ClampInRect(mainCamera.transform.position);
    }
    
    /// <summary>
    /// 初始化摄像机的状态，位置，缩放
    /// </summary>
    public void InitCameraSize()
    {
        mainCamera.orthographicSize = conrtrol_camera_size;
        mainCamera.transform.localPosition = control_camera_pos;
    }
    /// <summary>
    /// 限制摄像机移动范围
    /// </summary>
    public void LimitPosition()
    {
        
    }
    /// <summary>
    /// 限制摄像机的缩放范围
    /// </summary>
    /// <param name="v2">v2.x = min; v2.y = max</param>
    public void LimitZoom(Vector2 v2)
    {
        
    }
    private void Update()
    {
        UpdateCameraSize();
        if (FairyGUI.Stage.isTouchOnUI) 
            return;
        ClickLive2d();
    }
        
    private void ClickLive2d()
    {
        if (Input.GetMouseButtonUp(0))
        {
            MouseRaycast();
        }
    }
    // 检测点击的是否是可点击的部件
    public void MouseRaycast()
    {
        //通知UI 返回上一级菜单
        Canle();
    }
    /// <summary>
    /// 返回上一级菜单键
    /// </summary>
    public void Canle()
    {
    }

    private void OnDestroy()
    {
        swipe_gesture.onMove.RemoveCapture(Capture);
        pinch_gesture.onAction.RemoveCapture(Gesture);
        Stage.inst.onMouseWheel.RemoveCapture(MouseWheelZoom);
    }

    private void OnGUI()
    {
        EditotGUITools.DrawRect(mapRange,Color.red);
    }
}