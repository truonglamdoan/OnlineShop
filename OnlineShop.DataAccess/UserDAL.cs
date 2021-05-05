using OnlineShop.Core;
using OnlineShop.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop.DataAccess
{
    public class UserDAL : OnlineShopBaseDAL
    {
        public UserDAL() { }
        public UserDAL(DbTransaction tran) : base(tran) { }

        //   /// <summary>
        //   /// Load dữ liệu theo APK
        //   /// </summary>
        //   private const string SQL_GETUSER_ASSESS = @"SELECT M.APK, M.DivisionID, M.TaskID, M.TaskName
        //		, M.ProjectID
        //		, M.ProcessID
        //		, M.StepID
        //		, M.ParentTaskID
        //		, M.PreviousTaskID
        //		, M.AssignedToUserID
        //		, M.IsViolated
        //		, M.SupportUserID
        //		, M.TaskTypeID
        //		, O9.ProjectName AS ProjectName
        //		, IIF (M.ProjectID != '''' AND M.ProjectID IS NOT NULL, O2.ObjectName, O4.ProcessName) AS ProcessName
        //		, IIF (M.ProjectID != '''' AND M.ProjectID IS NOT NULL AND M.ProcessID != '''' AND M.ProcessID IS NOT NULL, O3.ObjectName, O5.StepName) AS StepName
        //		, M.APKSettingTime
        //		, M.TargetTypeID
        //		, M.RelatedToTypeID
        //		, M.IsRepeat
        //		, O6.TaskName AS ParentTaskName
        //		, O7.TaskName AS PreviousTaskName
        //		, M.ReviewerUserID
        //		, A1.FullName AS AssignedToUserName
        //		, A2.FullName AS SupportUserName
        //		, A3.FullName AS ReviewerUserName
        //		, M.PlanStartDate, M.PlanEndDate, M.PlanTime
        //		, M.ActualStartDate , M.ActualEndDate , M.ActualTime
        //		, A4.Description AS PriorityID
        //		, M.PercentProgress
        //		, ISNULL(M.StatusID, 0) AS StatusID, O8.StatusName
        //		, M.Description
        //		, O8.Orders
        //		, ISNULL(M.IsAssessor, 0) AS IsAssessor
        //		, ISNULL(M.LockID, 0) AS LockID
        //		, M.CreateUserID, M.CreateDate, M.LastModifyUserID, M.LastModifyDate
        //FROM OOT2110 M WITH (NOLOCK)
        //		LEFT JOIN OOT2100 O1 WITH (NOLOCK) ON M.APKMaster = O1.APK
        //		LEFT JOIN OOT2102 O2 WITH (NOLOCK) ON M.ProcessID = O2.ObjectID
        //		LEFT JOIN OOT2102 O3 WITH (NOLOCK) ON M.StepID = O3.ObjectID
        //		LEFT JOIN OOT1020 O4 WITH (NOLOCK) ON M.ProcessID = O4.ProcessID
        //		LEFT JOIN OOT1021 O5 WITH (NOLOCK) ON M.StepID = O5.StepID
        //		LEFT JOIN OOT2110 O6 WITH (NOLOCK) ON M.ParentTaskID = O6.TaskID
        //		LEFT JOIN OOT2110 O7 WITH (NOLOCK) ON M.PreviousTaskID = O7.TaskID
        //		LEFT JOIN OOT1040 O8 WITH (NOLOCK) ON M.StatusID = O8.StatusID
        //		LEFT JOIN OOT2100 O9 WITH (NOLOCK) ON M.ProjectID = O9.ProjectID
        //		LEFT JOIN AT1103 A1 WITH (NOLOCK) ON M.AssignedToUserID = A1.EmployeeID
        //		LEFT JOIN AT1103 A2 WITH (NOLOCK) ON M.SupportUserID = A2.EmployeeID
        //		LEFT JOIN AT1103 A3 WITH (NOLOCK) ON M.ReviewerUserID = A3.EmployeeID
        //		LEFT JOIN CRMT0099 A4 WITH (NOLOCK) ON M.PriorityID = A4.ID AND A4.CodeMaster = N'CRMT00000006'
        //WHERE M.APK = @APK";

        /// <summary>
        /// Load dữ liệu theo APK
        /// </summary>
        private const string SQL_GETUSER_ASSESS = @"SELECT * FROM [User]";

        /// <summary>
        /// Lấy dữ liệu Master
        /// </summary>
        /// <param name="APK"></param>
        /// <returns></returns>
        /// <history>
        /// 
        /// </history>
        public List<User> LoadDataMaster()
        {
            DbCommand command = null;
            List<User> result = new List<User>();
            try
            {
                using (command = OnlineShopDatabase.GetSqlStringCommand(SQL_GETUSER_ASSESS))
                {
                    //OnlineShopDatabase.AddInParameter(command, "APK", DbType.String, APK);
                    using (var reader = OnlineShopDatabase.ExecuteReader(command, this))
                    {
                        result = OnlineShopDatabase.ToList<User>(reader, "UserDAL.SQL_GETUSER_ASSESS");
                    }
                }
            }
            catch (Exception ex)
            {
                throw OnlineShopException.FromCommand(command, ex);
            }
            return result;
        }
    }
}
