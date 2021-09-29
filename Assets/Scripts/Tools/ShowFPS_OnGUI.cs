#define USE_TESTCONSOLE 
using UnityEngine;
using System.Collections;
using WT_FrameWork;

public class ShowFPS_OnGUI : MonoBehaviour
{
#if USE_TESTCONSOLE
    public float fpsMeasuringDelta = 2.0f;

    private float timePassed;
    private int m_FrameCount = 0;
    private float m_FPS = 0.0f;
    private bool showFPS;
    private bool _useLowFPS;
    /// <summary>
    /// 对性能不佳设备启用低帧率模式
    /// tips:默认使用垂直同步 在检测到帧率低于35 时 开启低帧率模式，开启后本次运行不再调回垂直同步
    /// </summary>
    public bool UseLowFps
    {
        get { return _useLowFPS; }
        set
        {
            if (_useLowFPS != value)
            {
                _useLowFPS = value;
                if (value)
                {
                    ShowLowFps();
                }
            }
        }
    }

    private void ShowLowFps()
    {
        Debug.LogWarning("切换Fps30模式");
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
    }

    private void Start()
    {
        timePassed = 0.0f;
#if UNITY_EDITOR
        showFPS = true;
#else
        showFPS = false;
#endif
        _useLowFPS = false;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.F))
        {
            showFPS = !showFPS;
        }
        m_FrameCount = m_FrameCount + 1;
        timePassed = timePassed + Time.deltaTime;

        if (timePassed > fpsMeasuringDelta)
        {
            m_FPS = m_FrameCount / timePassed;

            timePassed = 0.0f;
            m_FrameCount = 0;
        }
    }
    private void OnGUI()
    {
        if (!showFPS)
        {
            return;
        }
        GUIStyle bb = new GUIStyle();
        bb.normal.background = null;    //这是设置背景填充的
        bb.normal.textColor = m_FPS > 40f ? Color.green : Color.red;
        bb.fontSize = 25;       //当然，这是字体大小
        if (m_FPS < 35 && m_FPS > 1)
        {
            UseLowFps = true;
        }
        //居中显示FPS
        GUI.Label(new Rect((Screen.width ) - 125, 0, 200, 200), "FPS: " + m_FPS.ToString("###.00"), bb);
    }
#endif
    }
