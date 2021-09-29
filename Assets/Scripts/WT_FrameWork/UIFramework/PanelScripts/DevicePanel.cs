using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.WT_FrameWork.Data;
using Assets.Scripts.WT_FrameWork.Managers;
using Assets.Scripts.WT_FrameWork.UIFramework.Base;
using Assets.Scripts.WT_FrameWork.Util;
using UnityEngine.UI;

public class DevicePanel : BasePanel
{
    private Text _contentText, _titleText;
    private Button _closeBtn;
    private Dropdown _dropdown;
    private DevWTData _dev;
    private List<DevItemWTData> _devItems;
    protected override void FindObject()
    {
        base.FindObject();
        _dropdown = GetComponent<Dropdown>("bg/Dropdown");
        _contentText = GetComponent<Text>("bg/Scroll View/Viewport/Content");
        _titleText = GetComponent<Text>("bg/title");
        _closeBtn = GetComponent<Button>("bg/closebtn");
    }

    protected override void InitDB()
    {
        // Arg = 1;
        base.InitDB();
        if (Arg ==null)
        {
            OnExit();
            return;
        }
        List<DevWTData> l = CsvDataManager.instance.GetDataTable<DevWTData>();
        List<DevItemWTData> li = CsvDataManager.instance.GetDataTable<DevItemWTData>();
        _dev = l.FirstOrDefault((_) =>_.DevId == Convert.ToUInt32(Arg));
        _devItems = new List<DevItemWTData>();
        string[] ss = _dev.Items.Split(Common.StrSpliter);
        foreach (var s in ss)
        {
            var t = li.FirstOrDefault((_) => _.DId == int.Parse(s));
            if (t!=null)
            {
                _devItems.Add(t);
            }
        }
        _dropdown.options.Clear();
        foreach (var devItemWtData in _devItems)
        {
            _dropdown.options.Add(new Dropdown.OptionData(devItemWtData.ItemName));
        }
        _titleText.text = _dev.DevName;
        _dropdown.captionText.text = _dropdown.options[0].text;
        _dropdown.value = 0;
        OnValueChanged(0);
    }

    protected override void RigistEvent()
    {
        base.RigistEvent();
        _dropdown.onValueChanged.AddListener(OnValueChanged);
        _closeBtn.onClick.AddListener(ClosePanel);
    }

    protected override void UnRigistEvent()
    {
        base.UnRigistEvent();
        _dropdown.onValueChanged.RemoveListener(OnValueChanged);
        _closeBtn.onClick.RemoveListener(ClosePanel);
    }
    private void OnValueChanged(int arg0)
    {
        _contentText.text = _devItems[arg0].ItemContent.Replace("\\n","\n").Replace("\\r","\r");
    }
}