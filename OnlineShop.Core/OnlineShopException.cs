using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop.Core
{
    public class OnlineShopException : Exception
    {
        /// <summary>
        /// Get / Set layer occur exception
        /// </summary>
        public OnlineShopLayer Layer { get; private set; }

        /// <summary>
        /// Exception Message
        /// </summary>
        public string InnerMessage { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public static OnlineShopException FromCommand(DbCommand command, Exception innerException)
        {
            var message = new StringBuilder();

            if (command != null)
            {
                // Tham số
                if (command.Parameters != null)
                {
                    message.AppendLine("Parameters:");
                    foreach (DbParameter param in command.Parameters)
                    {
                        message.AppendFormat("\t{0}\t{1}\t{2}\t{3}\n",
                                             param.ParameterName, param.DbType, param.Value, param.Direction);
                    }
                }

                // Câu sql
                message.AppendFormat("CommandTimeout: {0}\nCommandText:\n{1}",
                                     command.CommandTimeout,
                                     command.CommandText);
            }

            // Tạo đối tượng ASOFTException
            var result = new OnlineShopException(OnlineShopLayer.DataAccess, innerException.Message, innerException)
            {
                InnerMessage = message.ToString()
            };

            // Giải phóng tài nguyên
            message = null;

            return result;
        }

        /// <summary>
        /// Enum define layer occur exception
        /// </summary>
        public enum OnlineShopLayer
        {
            Unknow = 0,
            DataAccess = 1,
            Business = 2,
            UI = 3
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public OnlineShopException(OnlineShopLayer layer, string message, Exception innerException)
            : base(message, innerException)
        {
            Layer = layer;
        }
    }
}
