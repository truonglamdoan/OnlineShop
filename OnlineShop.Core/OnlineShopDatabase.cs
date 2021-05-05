using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data;
using EnterpriseDatabase = Microsoft.Practices.EnterpriseLibrary.Data.Database;
using System.ComponentModel.DataAnnotations.Schema;

using System.Web.Configuration;
using System.Net.Http;
using Newtonsoft.Json;

namespace OnlineShop.Core
{
    public class OnlineShopDatabase
    {
        private static EnterpriseDatabase _Instance = null;
        private const string PARA_CREATEUSERID = "CreateUserID";
        private const string PARA_LASTMODIFYUSERID = "LastModifyUserID";
        private static Dictionary<int, object> _RowMappers = new Dictionary<int, object>();
        public static DbCommand GetSqlStringCommand(string query, bool autoAddDivisionID = true, bool isUsingCondition = true)
        {
            //// Tạo đối tượng command
            DbCommand result = Instance.GetSqlStringCommand(query);

            // Bỏ timeout
            result.CommandTimeout = 0;

            return result;
        }

        public static EnterpriseDatabase Instance
        {
            //get
            //{
            //    if (_Instance == null)
            //    {

            //        //_Instance = EnterpriseLibraryContainer.Current.GetInstance<EnterpriseDatabase>("DongADbContext");
            //        _Instance = EnterpriseLibraryContainer.Current.GetInstance<EnterpriseDatabase>("OracleDbContext");
            //    }
            //    return _Instance;
            //}
            //set { _Instance = value; }

            get
            {
                if (_Instance == null)
                {
                    _Instance = EnterpriseLibraryContainer.Current.GetInstance<EnterpriseDatabase>("OnlineShopDbContext");
                }
                return _Instance;
            }
            set { _Instance = value; }
        }

        public static IDataReader ExecuteReader(DbCommand command, OnlineShopBaseDAL dal)
        {
            // Kiểm tra, xử lý khi có transaction và không có transaction
            var result = dal.Transaction == null
                             ? Instance.ExecuteReader(command)
                             : Instance.ExecuteReader(command, dal.Transaction);

            return result;
        }

        public static IDataReader ExecuteReader(DbCommand command, OnlineShopBaseDAL dal, string ConnectionString)
        {
            var instance = EnterpriseLibraryContainer.Current.GetInstance<EnterpriseDatabase>(ConnectionString);
            // Kiểm tra, xử lý khi có transaction và không có transaction
            var result = dal.Transaction == null
                             ? instance.ExecuteReader(command)
                             : instance.ExecuteReader(command, dal.Transaction);

            return result;
        }

        /// <summary>
        /// Tạo danh sách từ IDataReader
        /// </summary>
        /// <typeparam name="T">Kiểu đối tượng trong danh sách.</typeparam>
        /// <param name="reader">Đối tượng IDataReader</param>
        /// <param name="sqlName">Tên đầy đủ của câu sql. VD: Sql.BT0000.GetAll</param>
        /// <returns>Nếu không có record nào thì trả về danh sách rỗng.</returns>
        public static List<T> ToList<T>(IDataReader reader, string sqlName = "") where T : new()
        {
            var mapper = GetRowMapper<T>(reader, sqlName);

            var result = new List<T>();
            while (reader.Read())
            {
                result.Add(mapper.MapRow(reader));
            }
            return result;
        }

        /// <summary>
        /// Tạo đối tượng từ IDataReader
        /// </summary>
        /// <typeparam name="T">Kiểu đối tượng</typeparam>
        /// <param name="reader">Đối tượng IDataReader</param>
        /// <param name="sqlName">Tên đầy đủ của câu sql. VD: Sql.BT0000.GetAll</param>
        /// <returns>Nếu không có record nào thì trả về null.</returns>
        public static T FirstOrDefault<T>(IDataReader reader, string sqlName = "") where T : new()
        {
            var mapper = GetRowMapper<T>(reader, sqlName);

            T result = default(T);
            if (reader.Read())
            {
                result = mapper.MapRow(reader);
            }
            return result;
        }

        /// <summary>
        /// Tạo mapper
        /// </summary>
        /// <typeparam name="T">Kiểu đối tượng</typeparam>
        /// <param name="reader">Đối tượng IDataReader</param>
        /// <param name="sqlName">Tên đầy đủ của câu sql. VD: Sql.BT0000.GetAll</param>
        /// <returns></returns>
        private static IRowMapper<T> GetRowMapper<T>(IDataReader reader, string sqlName = "") where T : new()
        {
            IRowMapper<T> result = null;

            Type type = typeof(T);
            bool cache = !string.IsNullOrEmpty(sqlName);
            int hashCode = (type.FullName + sqlName).GetHashCode();

            // Kiểm tra mapper đã được cache hay chưa
            if (cache && RowMappers.ContainsKey(hashCode))
            {
                result = (IRowMapper<T>)RowMappers[hashCode];
            }
            // Nếu không được cache thì tạo mới
            else
            {
                IMapBuilderContext<T> context = MapBuilder<T>.MapNoProperties();

                PropertyInfo property = null;
                string propertyName = string.Empty;
                for (int i = reader.FieldCount - 1; i >= 0; i--)
                {

                    propertyName = reader.GetName(i);
                    property = type.GetProperty(propertyName);
                    // Trường hợp không lấy đc property thì lấy theo column name
                    if (property == null)
                    {
                        // Lấy dữ liệu property theo column của Object
                        property = type.GetProperties().FirstOrDefault(prop =>
                                        prop.GetCustomAttributes(false)
                                            .OfType<ColumnAttribute>()
                                            .Any(attribute => attribute.Name == propertyName));
                    }

                    // Chỉ map những field vừa có trong câu sql vừa có trong đối tượng
                    if (property != null && property.CanRead && property.CanWrite)
                        context.MapByName(property);
                }

                result = context.Build();

                // Nếu có yêu cầu cache thì lưu vào cache
                if (reader.FieldCount > 0 && cache)
                {
                    RowMappers.Add(hashCode, result);
                }
            }

            return result;
        }

        private static Dictionary<int, object> RowMappers
        {
            get { return _RowMappers; }
            set { _RowMappers = value; }
        }

        public static DataSet ExecuteDataSet(DbCommand command, OnlineShopBaseDAL dal)
        {
            // Kiểm tra, xử lý khi có transaction và không có transaction
            var result = dal.Transaction == null
                             ? Instance.ExecuteDataSet(command)
                             : Instance.ExecuteDataSet(command, dal.Transaction);

            return result;
        }

        public static DbCommand GetStoredProcCommand(string storedProcedureName)
        {
            DbCommand result = Instance.GetStoredProcCommand(storedProcedureName);
            result.CommandTimeout = 0;
            return result;
        }

        #region ---- Parameters ----

        public static void AddInParameter(DbCommand command, string name, DbType dbType, object value)
        {
            AddInParameter(command, name, dbType, value, false);
        }

        public static void AddInParameter(DbCommand command, string name, DbType dbType, object value,
                                          bool overrideValue)
        {
            // Đảm bảo tên tham số đúng chuẩn
            if (!name.StartsWith("@")) name = "@" + name;

            if (!command.Parameters.Contains(name))
            {
                Instance.AddInParameter(command, name, dbType, value);
            }
            else if (overrideValue)
            {
                Instance.SetParameterValue(command, name, value);
            }
        }

        public static void AddOutParameter(DbCommand command, string name, DbType dbType)
        {
            // Đảm bảo tên tham số đúng chuẩn
            if (!name.StartsWith("@")) name = "@" + name;

            if (!command.Parameters.Contains(name))
            {
                Instance.AddOutParameter(command, name, dbType, -1);
            }
        }

        public static void AddInOutParameter(DbCommand command, string name, DbType dbType, object value)
        {
            // Đảm bảo tên tham số đúng chuẩn
            if (!name.StartsWith("@")) name = "@" + name;

            if (!command.Parameters.Contains(name))
            {
                Instance.AddParameter(command, name, dbType,
                                      ParameterDirection.InputOutput, string.Empty, DataRowVersion.Current, value);
            }
        }

        /// <summary>
        /// Lấy out parameter nếu cần
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        public static object GetParameterValue(DbCommand command, string name)
        {
            return Instance.GetParameterValue(command, name);
        }

        /// <summary>
        /// Parse array value to string value
        /// Usual use for execute StoreProcedure
        /// return "'ValueA','ValueB'"
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="dbType"></param>
        /// <param name="value"></param>
        public static void StringParseParameter(DbCommand command, string name, DbType dbType, object value)
        {
            // Parse value sang chuỗi
            string valueText = null;
            var count = 0;
            var sb = new StringBuilder();

            //Parse chuỗi -> mảng trong trường hợp value là kiểu string
            if (value != null)
            {
                if (value.GetType().FullName == "System.String")
                {
                    value = value.ToString().Split(',');
                }

                var items = (IEnumerable<string>)value;
                foreach (string item in items)
                {
                    sb.AppendFormat("{0}", item);
                    if (count != ((items.Count() - 1)))
                    {
                        sb.Append("','");
                    }
                    count++;
                }
                valueText = sb.ToString().Trim(',');
            }
            //else //TODO:Đúng trong trường hợp filter xuyên đơn vị, Sai trong trường hợp xóa,...
            //{
            //    value = new string[] { ASOFTEnvironment.DivisionID };
            //}

            AddInParameter(command, name, dbType, valueText, false);
        }

        /// <summary>
        /// Thêm parameter bằng cách thay thế chuỗi.\n
        /// - Nếu là danh sách chuỗi thì chuỗi thay thế có dạng N'abc', N'def', ..., N'xyz'. 
        /// - Nếu không là danh sách chuỗi thì mặc định xem như là danh sách số khi đó chuỗi thay thế có dạng 9999, 43, 67, ..., 99.
        /// - Nếu là 1 đối tượng thì thay tên biến bằng chính giá trị truyền vào.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void ReplaceParameter(DbCommand command, string name, object value)
        {

            // Đảm bảo tên tham số đúng chuẩn
            if (!name.StartsWith("@")) name = "@" + name;

            // Text dùng để thay vào vị trí tham số
            string valueText = string.Empty;

            // Tạo text thay thế
            if (value != null)
            {
                // Nếu là danh sách chuỗi thì chuỗi thay thế có dạng
                // 'abc', 'def', ..., 'xyz'
                if (value is IEnumerable<string> || value is string[])
                {
                    var sb = new StringBuilder();
                    var items = (IEnumerable<string>)value;
                    foreach (string item in items)
                    {
                        //TODO cần escape các ký tự đặc biệt
                        sb.AppendFormat("N'{0}',", item);
                    }
                    valueText = sb.ToString().Trim(',');
                }
                // Nếu là danh sách Guid
                // '0000-0000-00000000-0000', '0000-0000-00000000-0000', ..., '0000-0000-00000000-0000'
                else if (value is IEnumerable<Guid> || value is Guid[])
                {
                    var sb = new StringBuilder();
                    var items = (IEnumerable<Guid>)value;
                    foreach (Guid item in items)
                    {
                        sb.AppendFormat("'{0}',", item);
                    }
                    valueText = sb.ToString().Trim(',');
                }
                // Nếu không là danh sách chuỗi thì mặc định xem như là danh sách số
                // khi đó chuỗi thay thế có dạng 9999, 43, 67, ..., 99
                else if (value is IEnumerable<object> || value is object[])
                {
                    var sb = new StringBuilder();
                    var items = (IEnumerable<object>)value;
                    foreach (object item in items)
                    {
                        sb.AppendFormat("{0},", item);
                    }
                    valueText = sb.ToString().Trim(',');
                }
                // Nếu là 1 đối tượng thì thay tên biến bằng chính giá trị truyền vào
                else
                {
                    valueText = value.ToString();
                }
            }


            // Thay text vào vị trí tham số
            command.CommandText = command.CommandText.Replace(name, valueText);
        }

        /// <summary>
        /// Kiểm tra SQLInJection
        /// </summary>
        /// <param name="inputSQL"></param>
        /// <returns></returns>
        public static string SafeSqlLiteral(string inputSQL)
        {
            return inputSQL.Replace("'", "''");
        }

        /// <summary>
        /// Thêm ký tự [] cho các field
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="value"></param>
        public static void EscapesParameter(StringBuilder sb, string value)
        {
            char c = ' ';
            for (int i = 0; i < value.Length; i++)
            {
                c = value[i];
                if (c == '*' || c == '%' || c == '[' || c == ']')
                    sb.Append("[").Append(c).Append("]");
                else if (c == '\'')
                    sb.Append("''");
                else
                    sb.Append(c);
            }
        }

        /// <summary>
        /// Lấy DbType tương ứng với item được truyền vào
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <history>
        ///     [Vĩnh Tâm]   Created [10/09/2019]
        /// </history>
        public static DbType GetDbTypeObject(object item)
        {
            Type typeObject = item.GetType();

            // Không dùng được switch case vì typeof(Type) không phải là Hằng số
            if (typeof(DateTime).Equals(typeObject))
            {
                return DbType.DateTime;
            }
            else if (typeof(int).Equals(typeObject))
            {
                return DbType.Int32;
            }
            else if (typeof(short).Equals(typeObject))
            {
                return DbType.Int16;
            }
            else if (typeof(decimal).Equals(typeObject))
            {
                return DbType.Decimal;
            }

            return DbType.String;
        }

        #endregion ---- Parameters ----

        public static int ExecuteNonQuery(DbCommand command, OnlineShopBaseDAL dal)
        {
            //Kiểm tra, xử lý khi có transaction và không có transaction
            var result = dal.Transaction == null
                             ? Instance.ExecuteNonQuery(command)
                             : Instance.ExecuteNonQuery(command, dal.Transaction);

            return result;
        }

        public static int ExecuteNonQuery(DbCommand command, OnlineShopBaseDAL dal, string ConnectionString)
        {
            //Kiểm tra, xử lý khi có transaction và không có transaction
            var instance = EnterpriseLibraryContainer.Current.GetInstance<EnterpriseDatabase>(ConnectionString);
            var result = dal.Transaction == null
                             ? instance.ExecuteNonQuery(command)
                             : instance.ExecuteNonQuery(command, dal.Transaction);


            return result;
        }

        /// <summary>
        /// Đọc API theo Now
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="controllerAPI"></param>
        /// <param name="function"></param>
        /// <returns></returns>

        public static List<T> ToDataAPIObject<T>(string controllerAPI, string function, string reportTypeID = null) where T : new()
        {
            var result = new List<T>();

            string Baseurl = WebConfigurationManager.AppSettings["urlAPI"];

            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(Baseurl);

                client.DefaultRequestHeaders.Clear();

                //Sending request to find web api REST service resource GetAllEmployees using HttpClient
                HttpResponseMessage Res = client.GetAsync(string.Format("/api/{0}/{1}?reportTypeID={2}", controllerAPI, function, reportTypeID)).Result;

                //Checking the response is successful or not which is sent using HttpClient  
                if (Res.IsSuccessStatusCode)
                {
                    //Storing the response details recieved from web api   
                    var AccResponse = Res.Content.ReadAsStringAsync().Result;

                    //Deserializing the response recieved from web api and storing into the Employee list  
                    result = JsonConvert.DeserializeObject<List<T>>(AccResponse);
                }
            }

            return result;
        }

        /// <summary>
        /// Search data API theo from date và toDate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="controllerAPI"></param>
        /// <param name="function"></param>
        /// <param name="param1"></param>
        /// <param name="fromDate"></param>
        /// <param name="param2"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public static List<T> ToDataAPIObject<T>(string controllerAPI, string function, string param1, DateTime fromDate, string param2, DateTime toDate, string param3 = null, string reportTypeID = null, string param4 = null, string marketID = null) where T : new()
        {
            var result = new List<T>();

            string Baseurl = WebConfigurationManager.AppSettings["urlAPI"];

            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(Baseurl);

                client.DefaultRequestHeaders.Clear();

                HttpResponseMessage Res = new HttpResponseMessage();
                //Sending request to find web api REST service resource GetAllEmployees using HttpClient
                if (string.IsNullOrEmpty(marketID))
                {
                    Res = client.GetAsync(string.Format("/api/{0}/{1}?{2}={3}&{4}={5}&{6}={7}", controllerAPI, function, param1, fromDate.ToString("MM/dd/yyyy"), param2, toDate.ToString("MM/dd/yyyy"), param3, reportTypeID)).Result;
                }
                else
                {
                    Res = client.GetAsync(string.Format("/api/{0}/{1}?{2}={3}&{4}={5}&{6}={7}&{8}={9}", controllerAPI, function, param1, fromDate.ToString("MM/dd/yyyy"), param2, toDate.ToString("MM/dd/yyyy"), param3, reportTypeID, param4, marketID)).Result;
                }

                //Checking the response is successful or not which is sent using HttpClient  
                if (Res.IsSuccessStatusCode)
                {
                    //Storing the response details recieved from web api   
                    var AccResponse = Res.Content.ReadAsStringAsync().Result;

                    //Deserializing the response recieved from web api and storing into the Employee list  
                    result = JsonConvert.DeserializeObject<List<T>>(AccResponse);
                }
            }

            return result;
        }

        /// <summary>
        /// Search data API theo giai đoạn và so sánh
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="controllerAPI"></param>
        /// <param name="function"></param>
        /// <param name="param1"></param>
        /// <param name="typeID"></param>
        /// <param name="param2"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public static List<T> ToDataAPIObject<T>(string controllerAPI, string function, string param1, int typeID, string param2, int year, string param3 = null, string reportTypeID = null, string param4 = null, string marketID = null) where T : new()
        {
            var result = new List<T>();

            string Baseurl = WebConfigurationManager.AppSettings["urlAPI"];

            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(Baseurl);

                client.DefaultRequestHeaders.Clear();

                //Sending request to find web api REST service resource GetAllEmployees using HttpClient
                HttpResponseMessage Res = new HttpResponseMessage();
                if (!string.IsNullOrEmpty(param4) && !string.IsNullOrEmpty(marketID))
                {
                    Res = client.GetAsync(string.Format("/api/{0}/{1}?{2}={3}&{4}={5}&{6}={7}&{8}={9}", controllerAPI, function, param1, typeID, param2, year, param3, reportTypeID, param4, marketID)).Result;
                }
                else
                {
                    Res = client.GetAsync(string.Format("/api/{0}/{1}?{2}={3}&{4}={5}&{6}={7}", controllerAPI, function, param1, typeID, param2, year, param3, reportTypeID)).Result;
                }

                //Checking the response is successful or not which is sent using HttpClient  
                if (Res.IsSuccessStatusCode)
                {
                    //Storing the response details recieved from web api   
                    var AccResponse = Res.Content.ReadAsStringAsync().Result;

                    //Deserializing the response recieved from web api and storing into the Employee list  
                    result = JsonConvert.DeserializeObject<List<T>>(AccResponse);
                }
            }

            return result;
        }

        public static List<T> ToDataAPIMarketObject<T>(string controllerAPI, string function, string param1, string value1, string reportTypeID = null) where T : new()
        {
            var result = new List<T>();

            string Baseurl = WebConfigurationManager.AppSettings["urlAPI"];

            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(Baseurl);

                client.DefaultRequestHeaders.Clear();

                //Sending request to find web api REST service resource GetAllEmployees using HttpClient
                HttpResponseMessage Res = client.GetAsync(string.Format("/api/{0}/{1}?{2}={3}&reportTypeID={4}", controllerAPI, function, param1, value1, reportTypeID)).Result;

                //Checking the response is successful or not which is sent using HttpClient  
                if (Res.IsSuccessStatusCode)
                {
                    //Storing the response details recieved from web api   
                    var AccResponse = Res.Content.ReadAsStringAsync().Result;

                    //Deserializing the response recieved from web api and storing into the Employee list  
                    result = JsonConvert.DeserializeObject<List<T>>(AccResponse);
                }
            }

            return result;
        }
    }
}
