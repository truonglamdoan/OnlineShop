using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace OnlineShop.Core
{
    public class OnlineShopEnvironment
    {
        public const string KEY_USERID = "UserID";
        public const string KEY_USERNAME = "Fullname";
        public static string UserID
        {
            set { SessionState[KEY_USERID] = value; }
            get { return GetSession<string>(KEY_USERID); }
        }

        public static string Fullname
        {
            set { SessionState[KEY_USERNAME] = value; }
            get { return GetSession<string>(KEY_USERNAME); }
        }

        /// <summary>
        /// Chứa các biến cấp session
        /// </summary>
        internal static HttpSessionState SessionState
        {
            get
            {
                if (HttpContext.Current == null) return null;
                var session = HttpContext.Current.Session;

                if (session == null) return new Object() as HttpSessionState;
                return session;
            }
        }

        public static T GetSession<T>(string key)
        {
            if (SessionState == null)
                return default(T);

            var value = SessionState[key];
            T result = default(T);
            if (value != null && value is T) result = (T)value;
            return result;
        }
    }
}
