using UnityEngine;
using FairyGUI;
//using DataNodeV5;

namespace Game.L2Map
{
    /// <summary>
    /// 地图摄像机, 提供最基础的摄像机控制逻辑
    /// </summary>
    public class MapCamera : MonoBehaviour
    {
        // /// <summary>
        // /// 用于控制地图显示的摄像机范围
        // /// </summary>
        // public ICameraView CameraRect = null;
        // /// <summary>
        // /// 用于控制地图显示的摄像机范围
        // /// </summary>
        // /// <value></value>
        // public Rect viewRect { get; private set; }

        /// <summary>
        /// 导出摄像机的实际尺寸操作接口
        /// </summary>
        /// <value></value>
        public float orthographicSize
        {
            get => targetCamera.orthographicSize;
            set => targetCamera.orthographicSize = value;
        }

        /// <summary>
        /// 摄像机位置
        /// </summary>
        public Vector3 position => targetCamera.transform.position;

        /// <summary>
        /// 目标摄像机
        /// </summary>
        private Camera targetCamera;

        /// <summary>
        /// 
        /// </summary>
        private Vector2 halfCameraRectSize = new Vector2(1, 1);

        /// <summary>
        /// 控制者
        /// </summary>
        private string controller = null;
        

        /// <summary>
        /// 当前的地图范围, 以世界坐标计
        /// </summary>
        // [SerializeField]
        public Rect mapRange = new Rect(-1000, -1000, 2000, 2000);

        /// <summary>
        /// 摄像机缩放范围
        /// </summary>
        // [SerializeField]
        public Vector2 zoomRange = new Vector2(10, 50);

        /// <summary>
        /// 当前动画
        /// </summary>
        private GTweener moveTweener = null;
        private GTweener zoomTweener = null;

        /// <summary>
        /// 跟随目标
        /// </summary>
        private Transform followTarget = null;

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
            // Debug.Log($"{Screen.width} * {Screen.height}");
            

            zoomRange.x = 2f;
            zoomRange.y = 10f;
        }

        private void OnDestroy()
        {
            StopTween();
        }

        private void Update()
        {
            // Debug.Log($"camera update");
            if (followTarget == null)
                return;

            //  跟随获取后离开释放
            // if (Acquire("self", CameraControlLevel.Follow))
            if (followTarget.position != targetCamera.transform.position)
            {
                MoveTo(followTarget.position, 1, true);
                // Release("self");
            }
        }
        
        

        public Vector3 ScreenToWorldPoint(Vector3 position)
        {
            return targetCamera.ScreenToWorldPoint(position);
        }
        

        /// <summary>
        /// 摄像机跟随目标
        /// </summary>
        /// <param name="followTarget"></param>
        public void FollowTarget(Transform followTarget)
        {
            this.followTarget = followTarget;
            if (followTarget != null)
                StopTween(TweenType.MOVE);
        }


        /// <summary>
        /// 移动摄像机到目标位置
        /// </summary>
        /// <param name="pos">目标摄像机位置, 世界坐标</param>
        /// <param name="time">如果时间为0, 则立刻移动, 否则花时间做出对应的移动</param>
        /// <param name="force">是否强制移动</param>
        public void MoveTo(Vector3 pos, float time = 0, bool force = false)
        {
            StopTween(TweenType.MOVE);

            //  确保摄像机位置不要出边界
            if (force)
                pos.z = -10;
            else
                pos = ClampInRect(pos);
            if (time <= 0)
            {
                UpdatePosition(pos);
            }
            else
            {
                moveTweener = GTween.To(targetCamera.transform.position, pos, time).OnUpdate(tweener =>
                {
                    UpdatePosition(tweener.value.vec3);
                });
            }
        }

        public void UpdateViewRect()
        {
            var nearSize = orthographicSize * 1.3f;
            var farSize = orthographicSize * 1.6f;
            var pos = targetCamera.transform.position;
            var nearRect = new Rect(
                pos.x - nearSize,
                pos.y - nearSize,
                nearSize * 2,
                nearSize * 2
            );

            var farRect = new Rect(
                pos.x - farSize,
                pos.y - farSize,
                farSize * 2,
                farSize * 2
            );

            // Debug.Log($"current view rect {viewRect}, center at {viewRect.center}");
            //MapSupervisor.MapControl.UpdateViewRect(nearRect, farRect);
        }

        private void UpdatePosition(Vector3 pos)
        {
            targetCamera.transform.position = pos;
            UpdateViewRect();
        }

        /// <summary>
        /// 移动摄像机delta位置
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="time"></param>
        /// <param name="force"></param>
        public void Move(Vector2 delta, float time = 0, bool force = false)
        {
            MoveTo(targetCamera.transform.position + (Vector3)(delta * targetCamera.orthographicSize * 2.0f / Screen.height), time, force);
        }

        /// <summary>
        /// 调整摄像机尺寸到特定位置
        /// </summary>
        /// <param name="size"></param>
        /// <param name="time"></param>
        public void ZoomTo(float size, float time = 0)
        {
            StopTween(TweenType.ZOOM);

            size = Mathf.Clamp(size, zoomRange.x, zoomRange.y);
            if (time <= 0)
            {
                UpdateOrthographicSize(size);
            }
            else
            {
                zoomTweener = GTween.To(targetCamera.orthographicSize, size, time).OnUpdate(tweener =>
                {
                    UpdateOrthographicSize(tweener.value.x);
                });
            }
        }

        private void UpdateOrthographicSize(float size)
        {
            targetCamera.orthographicSize = size;
            UpdateCameraSize();
            targetCamera.transform.position = ClampInRect(targetCamera.transform.position);
            // CameraRect?.DymaticCullingCameraRect();
            UpdateViewRect();
        }

        /// <summary>
        /// 放大delta尺寸
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="time"></param>
        public void ZoomIn(float delta, float time = 0)
        {
            ZoomTo(targetCamera.orthographicSize - delta, time);
        }

        /// <summary>
        /// 缩小delta尺寸
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="time"></param>
        public void ZoomOut(float delta, float time = 0)
        {
            ZoomTo(targetCamera.orthographicSize + delta, time);
        }

        [System.Flags]
        public enum TweenType
        {
            MOVE = 1,
            ZOOM = 2,
            ALL = 3,
        }

        /// <summary>
        /// 停止所有动画
        /// </summary>
        private void StopTween(TweenType type = TweenType.ALL)
        {
            if (moveTweener != null && type.HasFlag(TweenType.MOVE))
            {
                moveTweener.Kill();
                moveTweener = null;
            }

            if (zoomTweener != null && type.HasFlag(TweenType.ZOOM))
            {
                zoomTweener.Kill();
                zoomTweener = null;
            }
        }

        /// <summary>
        /// 更新摄像机尺寸
        /// </summary>
        private void UpdateCameraSize()
        {
            halfCameraRectSize.x = (float)Screen.width / (float)Screen.height * targetCamera.orthographicSize;
            halfCameraRectSize.y = targetCamera.orthographicSize;
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

        private Rect CalculateBound(Vector2[] maskPoints)
        {
            float minx = maskPoints[0].x,
                      maxx = maskPoints[0].x,
                      miny = maskPoints[0].y,
                      maxy = maskPoints[0].y;

            Vector2 cellSize = new Vector2(3.2f, 1.0f);

            for (int i = 1; i < maskPoints?.Length; i++)
            {
                if (maskPoints[i].x < minx)
                    minx = maskPoints[i].x;
                else if (maskPoints[i].x > maxx)
                    maxx = maskPoints[i].x;

                if (maskPoints[i].y < miny)
                    miny = maskPoints[i].y;
                else if (maskPoints[i].y > maxy)
                    maxy = maskPoints[i].y;
            }

            //减去摄像机的边界
            var points = new float[4];
            points[0] = minx + cellSize.x / 2;
            points[1] = maxx - cellSize.x / 2;
            points[2] = miny - cellSize.y;
            points[3] = maxy + cellSize.y;
            return new Rect(points[0], points[2], points[1] - points[0], points[3] - points[2]);
        }
    }

}