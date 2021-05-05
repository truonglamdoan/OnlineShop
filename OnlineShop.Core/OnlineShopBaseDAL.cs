using System;
using System.Data;
using System.Data.Common;

namespace OnlineShop.Core
{
    public class OnlineShopBaseDAL
    {
        public OnlineShopBaseDAL()
        {

        }
        public OnlineShopBaseDAL(DbTransaction transaction)
        {
            Transaction = transaction;
        }

        internal DbTransaction Transaction { get; private set; }
    }
}
