using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.WT_FrameWork;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.User
{
    public class UserBase
    {
        public UserType _userUType = UserType.UnKnown;
        public string _userId;
        public string _userName;

        public UserBase(UserType utype, string uid, string uname)
        {
            SetUserInfo(utype, uid, uname);
        }
        public UserType U_Type
        {
            get { return _userUType; }
            set
            {
                _userUType = value;
                if (value == UserType.Visitor)
                {
                    _userName = "游客";
                    _userId = "0";
                }
            }
        }

        public string UserId
        {
            get { return _userId; }
        }

        public string UserName
        {
            get { return _userName; }
        }

        protected virtual void SetUserInfo(UserType utype, string uid, string uname)//登录时赋值
        {
            _userId = uid;
            _userName = uname;
            _userUType = utype;
        }
    }   
    public enum UserType
    {
        Visitor,
        Student,
        UnKnown
    }
}