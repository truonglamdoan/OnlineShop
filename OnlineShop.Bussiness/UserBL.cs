using OnlineShop.Core;
using OnlineShop.DataAccess;
using OnlineShop.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OnlineShop.Core.OnlineShopException;

namespace OnlineShop.Bussiness
{
    public class UserBL : OnlineShopBaseDAL
    {
        /// <summary>
        /// Thông báo cho những người tiếp theo sau khi người trước đó đã đánh giá
        /// </summary>
        /// <param name="APKMaster"></param>
        /// <param name="APK"></param>
        /// <param name="AssessOrder"></param>
        /// <returns></returns>
        /// <history>
        /// </history>
        public List<User> LoadDataMaster()
        {
            try
            {
                return new UserDAL().LoadDataMaster();
            }
            catch (Exception ex)
            {
                throw new OnlineShopException(OnlineShopLayer.Business, ex.Message, ex);
            }
        }
    }
}
