#if !UNITY_WEBGL


/*
 *
 * lzy 2019.02.28
 * 1.播放器使用easymovietexture 为底层api
 * 2.经过使用测试发现 使用时需要完全关闭360相关软件
 * 3.经过测试发现播放视频之前需要加载，顾使用了延迟线程
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.WT_FrameWork.UIFramework.Base;
using Assets.Scripts.WT_FrameWork.Util;
using Assets.Scripts.WT_FrameWork.Video;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class VideoPanel : BasePanel
{
    private bool b_menushow;
    private bool b_volumeOn = true;
    public bool BAutoPlayFirst;
    private Button btn_play, btn_stop, btn_menu, btn_volume, btn_ret;
    private int curPlayTime;
    private MediaPlayerCtrl mpc;
    private Slider s_videoProgrss, s_volume, s_videoSpeed;
    private Text t_time;
    private Transform t_videoList, t_playtools, t_playListContent;
    private Tween tween_videolist, tween_playtools, tween_up;
    private List<VideoListItem> videoItems;
    private string videoPath;

    private int videotime;
    private Transform volumeOn, volumeOff, playOn, playOff;

    public bool BVolumeOn
    {
        get { return b_volumeOn; }
        set
        {
            b_volumeOn = value;
            volumeOn.gameObject.SetActive(value);
            volumeOff.gameObject.SetActive(!value);
            if (mpc.GetAudioSource() != null)
            {
                mpc.GetAudioSource().mute = !value;
                print("mute:" + value);
            }
        }
    }

    public override void OnEnter()
    {
        print(Arg);
        base.OnEnter();
        videotime = curPlayTime = 0;
        DOTween.defaultAutoPlay = AutoPlay.None;
        DOTween.defaultAutoKill = false;
        videoPath = Path.Combine(Application.streamingAssetsPath, "Video");
        mpc = transform.Find("VideoManager").GetComponent<MediaPlayerCtrl>();
        s_videoProgrss = transform.Find("tools/content/video_progress").GetComponent<Slider>();
        s_videoProgrss.value = 0f;
        t_videoList = transform.Find("videolist");
        t_playListContent = t_videoList.Find("Scroll View/Viewport/Content");
        t_playtools = transform.Find("tools/content");
        btn_play = transform.Find("tools/content/btn_play").GetComponent<Button>();
        btn_stop = transform.Find("tools/content/btn_stop").GetComponent<Button>();
        btn_play.onClick.AddListener(OnPlayBtnClick);
        playOn = btn_play.transform.Find("on");
        playOff = btn_play.transform.Find("off");
        btn_stop.onClick.AddListener(OnStopBtnClick);
        btn_menu = transform.Find("tools/content/menu").GetComponent<Button>();
        btn_menu.onClick.AddListener(OnMenuBtnClick);
        b_menushow = false;
        s_volume = transform.Find("tools/content/volume_progress").GetComponent<Slider>();
        s_volume.value = float.Parse(Util.GetSystemConfig("VideoConfig", "volume"));
        s_volume.onValueChanged.AddListener(Ons_VolumeValueChanged);
        btn_volume = s_volume.transform.Find("btn_volume").GetComponent<Button>();
        btn_volume.onClick.AddListener(OnBtn_VolumeClick);
        volumeOn = btn_volume.transform.Find("on");
        volumeOff = btn_volume.transform.Find("off");
        s_videoSpeed = transform.Find("tools/content/video_speed").GetComponent<Slider>();
        s_videoSpeed.onValueChanged.AddListener(OnVideoSpeedChanged);
        s_videoSpeed.value = 0;

#region 右侧和下侧动画

        var videolistMoveX = t_videoList.GetComponent<RectTransform>().sizeDelta.x;
        var videolistStartPos = t_videoList.GetComponent<RectTransform>().localPosition.x;
        tween_videolist = t_videoList.DOLocalMoveX(videolistStartPos - videolistMoveX, 0.5f);
        var play_toolsMoveY = t_playtools.GetComponent<RectTransform>().sizeDelta.y;
        var play_toolsStartPos = t_playtools.GetComponent<RectTransform>().localPosition.y;
        tween_playtools = t_playtools.DOLocalMoveY(play_toolsStartPos - play_toolsStartPos, 0.5f);
        var t_up = transform.Find("up/content");
        var upMoveY = t_up.GetComponent<RectTransform>().sizeDelta.y;
        var upStartY = t_up.GetComponent<RectTransform>().localPosition.y;
        tween_up = t_up.DOLocalMoveY(upMoveY - upStartY, 0.5f);
        BVolumeOn = true; //默认打开声音
        btn_ret = transform.Find("up/content/ret").GetComponent<Button>();
        btn_ret.onClick.AddListener(ClosePanel);

#endregion

#region 填充播放列表

        FillVideoList();

#endregion

        BAutoPlayFirst = int.Parse(Util.GetSystemConfig("VideoConfig", "b_autoplay")) > 0;
        if (BAutoPlayFirst) StartCoroutine(OnVideoItemPlay(videoItems[0]));
        SetPlayIcon(BAutoPlayFirst);
        t_time = transform.Find("tools/content/t_time").GetComponent<Text>();
        SetVideoTime();
        mpc.OnEnd += () =>
        {
            OnStopBtnClick();
            mpc.Load(mpc.m_strFileName);
        };
    }

    private void SetPlayIcon(bool b_isPlay)
    {
        playOn.gameObject.SetActive(!b_isPlay);
        playOff.gameObject.SetActive(b_isPlay);
    }

    private void SetVideoTime()
    {
        videotime = mpc.GetDuration();
        if (videotime == -1)
        {
            curPlayTime = 0;
            videotime = 0;
        }
        else
        {
            videotime /= 1000;
            curPlayTime = mpc.GetSeekPosition() / 1000;
        }

        t_time.text = ConvertInt2Time(curPlayTime) + "/" + ConvertInt2Time(videotime);
    }

    private string ConvertInt2Time(int t)
    {
        return (t / 3600).ToString("00") + ":" + (t / 60 % 60).ToString("00") + ":" + (t % 60).ToString("00");
    }

    private void OnVideoSpeedChanged(float playSpeed)
    {
        mpc.SetSpeed(1.0f + playSpeed * 0.1f);
        print(1.0f + playSpeed * 0.1f);
    }

    private void OnBtn_VolumeClick()
    {
        BVolumeOn = !BVolumeOn;
    }

    private void Ons_VolumeValueChanged(float v)
    {
        BVolumeOn = true;
        mpc.SetVolume(v);
        Util.SetSystemConfig("VideoConfig", "volume", v.ToString("0.00"));
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

#region 处理播放进度条

        if (m_bActiveDrag == false)
        {
            m_fDeltaTime += Time.deltaTime;
            if (m_fDeltaTime > m_fDragTime)
            {
                m_bActiveDrag = true;
                m_fDeltaTime = 0.0f;
            }
        }

        if (m_bUpdate == false)
            return;

        if (mpc != null)
            if (s_videoProgrss != null)
                s_videoProgrss.value = mpc.GetSeekBarValue();

#endregion

        SetVideoTime();
    }

    private void FillVideoList()
    {
        if (videoItems == null) videoItems = new List<VideoListItem>();
        if (videoItems.Count > 0)
            foreach (var videoListItem in videoItems)
                Destroy(videoListItem.gameObject);
        var vinfos = GetVideosInfo();
        if (vinfos != null)
        {
            var vitem = Resources.Load("UI/playitem") as GameObject;
            foreach (var vinfo in vinfos)
            {
                var vo = Instantiate(vitem, t_playListContent);
                var vli = vo.GetComponent<VideoListItem>();
                vli.ShowName = vinfo.Key;
                vli.FilePath = vinfo.Value;
                vli.Btn.onClick.AddListener(() =>
                {
                    //mpc.UnLoad();
                    StartCoroutine(OnVideoItemPlay(vli));
                });
                videoItems.Add(vli);
            }
        }
    }

    private IEnumerator OnVideoItemPlay(VideoListItem vli)
    {
        mpc.UnLoad();
        mpc.Load(vli.FilePath);
        yield return new WaitForSeconds(0.1f);
        mpc.Play();
        Ons_VolumeValueChanged(s_volume.value);
    }

    private void OnMenuBtnClick()
    {
        if (!b_menushow)
            tween_videolist.PlayForward();
        else
            tween_videolist.PlayBackwards();
        b_menushow = !b_menushow;
    }

    public void OnMouseExitPlayList()
    {
        if (b_menushow)
        {
            tween_videolist.PlayBackwards();
            b_menushow = !b_menushow;
        }
    }

    public void OnEnterPlayTools()
    {
        tween_playtools.PlayForward();
    }

    public void OnExitPlayTools()
    {
        tween_playtools.PlayBackwards();
    }

    private void OnStopBtnClick()
    {
        mpc.Stop();
    }

    private void OnPlayBtnClick()
    {
        if (mpc.GetCurrentState() == MediaPlayerCtrl.MEDIAPLAYER_STATE.PLAYING)
        {
            mpc.Pause();
            SetPlayIcon(false);
        }
        else
        {
            mpc.Play();
            SetPlayIcon(true);
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        OnStopBtnClick();
        btn_play.onClick.RemoveListener(OnPlayBtnClick);
        btn_stop.onClick.RemoveListener(OnStopBtnClick);
        btn_menu.onClick.RemoveListener(OnMenuBtnClick);
        s_volume.onValueChanged.RemoveListener(Ons_VolumeValueChanged);
        btn_volume.onClick.RemoveListener(OnBtn_VolumeClick);
        s_videoSpeed.onValueChanged.RemoveListener(OnVideoSpeedChanged);
        btn_ret.onClick.RemoveListener(ClosePanel);
        mpc.OnEnd = null;
        Util.SaveSystemConfig();
    }

    public Dictionary<string, string> GetVideosInfo()
    {
        Debug.Log("123");
        var videos = Directory.GetFiles(videoPath, "*.mp4");
        if (videos.Length > 0)
        {
            var videosInfo = new Dictionary<string, string>();
            for (var i = 0; i < videos.Length; i++)
            {
                var filename = Path.GetFileName(videos[i]);
                var filepath = Path.Combine("Video", filename);
                Debug.Log(filepath);
                videosInfo.Add(filename, filepath);
            }

            return videosInfo;
        }

        return null;
    }

#region 播放进度条参数

    public float m_fDragTime = 0.2f;
    private bool m_bActiveDrag = true;
    private bool m_bUpdate = true;
    private float m_fDeltaTime;
    private float m_fLastValue;
    private float m_fLastSetValue;

#endregion

#region 播放进度条事件

    public void OnSliderPointEnter()
    {
        m_bUpdate = false;
    }

    public void OnSliderPointExit()
    {
        m_bUpdate = true;
    }

    public void OnSliderPointDown()
    {
        //暂时没有用到
    }

    public void OnSliderPointUp()
    {
        mpc.SetSeekBarValue(s_videoProgrss.value);
    }

    public void OnSliderDrag()
    {
        if (m_bActiveDrag == false)
        {
            m_fLastValue = s_videoProgrss.value;
            return;
        }

        m_fLastSetValue = s_videoProgrss.value;
        m_bActiveDrag = false;
    }

#endregion

#region 上方内容显隐事件

    public void OnUpPoinerEnter()
    {
        tween_up.PlayForward();
    }

    public void OnUpPointerExit()
    {
        tween_up.PlayBackwards();
    }

#endregion
}
#endif