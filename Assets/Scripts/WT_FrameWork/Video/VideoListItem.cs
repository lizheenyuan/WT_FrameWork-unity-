using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.WT_FrameWork.Video
{
    public class VideoListItem : MonoBehaviour
    {

        public string ShowName
        {
            set { transform.Find("Text").GetComponent<Text>().text = value; }
        }
        public string FilePath;
        private Button _btn;

        public Button Btn
        {
            get
            {
                if (_btn==null)
                {
                    _btn = transform.GetComponent<Button>();
                }
                return _btn;
            }
        }
    }
}
