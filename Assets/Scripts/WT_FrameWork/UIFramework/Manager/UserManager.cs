using System;
using System.Collections.Generic;
using Assets.Scripts.User;
using Assets.Scripts.WT_FrameWork;
using Assets.Scripts.WT_FrameWork.SingleTon;
using Assets.Scripts.WT_FrameWork.Util;
using UnityEngine;

namespace Assets.Scripts.UIFramework.Manager
{
    public class UserManager : WT_Singleton<UserManager>
    {

        private string _startTime;
        private string _endTime;

        private UserBase _currentUser;
        private List<string> ty_result;
        public readonly string[] Fault_Item=new string[4]
        {
            "报警电话选择错误;",
            "逃生物品选择错误;",
            "逃生时误用电梯;",
            "逃生姿势错误;"
        };

        public override void Init()
        {
            base.Init();
            ty_result = new List<string>();
        }
        public UserBase CurrentUser
        {
            get
            {
                if (_currentUser==null)
                {
                    _currentUser = new UserBase(UserType.Visitor,"0","游客");
                }
                return _currentUser;
            }
        }

        public string StartTime
        {
            get { return _startTime; }
            set
            {
                if (CurrentUser.U_Type==UserType.Student)
                {
                    _startTime = value;
                }
               
            }
        }

        public string EndTime
        {
            get { return _endTime; }
            set
            {
                if (CurrentUser.U_Type == UserType.Student)
                {
                    _endTime = value;
                }
            }
        }

        public void SetCurrentUser(UserBase user)
        {
            if (user==null)
            {
                return;
            }
            _currentUser = user;
        }

        public void LogoutUser()
        {
            CurrentUser.U_Type = UserType.Visitor;
            ty_result.Clear();
            StartTime = "0";
            EndTime = "0";
        }

        public void SetUserScore(string value)
        {
            if (CurrentUser.U_Type==UserType.Student&&!ty_result.Contains(value))
            {
                ty_result.Add(value);
            }
        }

        public void SetUserTYStartTime()
        {
            StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public void SetUserTYEndTime()
        {
            EndTime= DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public string GetUserScore()
        {
            string uscore = "";
            if (ty_result.Count == 0)
            {
                return Util.GetSystemConfig("UScoreConfig", "ts_No_fault");
            }
            for (int i = 0; i < ty_result.Count; i++)
            {
                uscore += ty_result[i];
            }
            return uscore;
        }
    }
}
