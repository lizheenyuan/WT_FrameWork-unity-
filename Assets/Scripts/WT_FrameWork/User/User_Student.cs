namespace Assets.Scripts.User
{
    public class User_Student : UserBase
    {
        private float _score;
        private string _icCardID;

        public User_Student(UserType utype, string uid, string uname,float score,string icCardID) : base(utype, uid, uname)
        {
            _score = score;
            _icCardID = icCardID;
        }

        public float Score
        {
            get { return _score; }
        }

        public string IcCardId
        {
            get { return _icCardID; }
        }
    }
}
