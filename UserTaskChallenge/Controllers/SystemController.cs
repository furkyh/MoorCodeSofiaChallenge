using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using System.Xml;
using UserTaskChallenge.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace UserTaskChallenge.Controllers
{
    public class SystemController : Controller
    {

        [HttpPost]
        public IActionResult TableOperations(string jsonData)
        {
            ReturnData ret = new ReturnData();
            string data = jsonData;
            if (!string.IsNullOrEmpty(data))
            {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.Formatting = Newtonsoft.Json.Formatting.Indented;
                settings.NullValueHandling = NullValueHandling.Include;
                settings.DefaultValueHandling = DefaultValueHandling.Ignore;

                try
                {
                    JObject jobject = JObject.Parse(data, new JsonLoadSettings()
                    {
                        CommentHandling = CommentHandling.Ignore,
                        LineInfoHandling = LineInfoHandling.Ignore
                    });

                    if (jobject != null)
                    {
                        string type = jobject["type"].ToString();//I = Insert, U = Update, D = Delete
                        string tableName = jobject["tableName"].ToString();

                        List<Property> keys = new List<Property>();
                        List<Property> values = new List<Property>();

                        if (jobject["keys"] != null)
                        {
                            foreach (JProperty jproperty in (IEnumerable<JToken>)jobject["keys"])
                            {
                                string key = jproperty.Name;
                                var value = JTokenToConventionalDotNetObject(jproperty.Value);

                                keys.Add(new Property { key = key, value = value });
                            }
                        }

                        if (jobject["values"] != null)
                        {
                            foreach (JProperty jproperty in (IEnumerable<JToken>)jobject["values"])
                            {
                                string key = jproperty.Name;
                                var value = JTokenToConventionalDotNetObject(jproperty.Value);

                                

                                values.Add(new Property { key = key, value = value });
                            }
                        }

                        string queryString = "";

                        Core.SQL.Conn sqlConn = new Core.SQL.Conn(StaticData.connStr);

                        if (type == "I")
                        {
                            if (values.Count <= 0)
                            {
                                ret.status = ReturnData.Status.Error;
                                ret.errors.Add("Values is required for [I]nsert.");
                            }
                            else
                            {
                                queryString = string.Format("INSERT INTO [{0}] ({1}) VALUES ({2})",
                                    tableName,
                                    string.Join(", ", values.Select(x => "[" + x.key + "]")),
                                    string.Join(", ", values.Select(x => "@" + x.key)));

                                List<SqlParameter> parameters = new List<SqlParameter>();
                                foreach (var item in values)
                                {
                                    parameters.Add(new SqlParameter(item.key, item.value));
                                }

                                bool sonuc = sqlConn.Set(queryString, System.Data.CommandType.Text, parameters.ToArray());
                                if (sonuc)
                                {
                                    ret.status = ReturnData.Status.Ok;
                                }
                                else
                                {
                                    ret.status = ReturnData.Status.Error;
                                    ret.errors.Add(sqlConn.lastError);
                                }
                            }
                        }
                        else if (type == "U")
                        {
                            if (tableName == "User" && values.Any(x => x.key == "deleted" && x.value == 1))
                            {
                                DataTable dt = sqlConn.Get("SELECT userID FROM [User] WHERE deleted = 0 AND parentUserID = @parentUserID", CommandType.Text, new SqlParameter("parentUserID", keys.First().value));
                                if (dt.Rows.Count > 0)
                                {
                                    ret.status = ReturnData.Status.Error;
                                    ret.errors.Add("This user have a student, can't delete.");
                                    return Ok(ret);
                                }
                            }

                            if (values.Count <= 0)
                            {
                                ret.status = ReturnData.Status.Error;
                                ret.errors.Add("Values is required for [U]pdate.");
                            }
                            else if (keys.Count <= 0)
                            {
                                ret.status = ReturnData.Status.Error;
                                ret.errors.Add("Keys is required for [U]pdate.");
                            }
                            else
                            {
                                if (values.Any(x => x.key == "deleted") && values.First(x => x.key == "deleted").value == 1)
                                {
                                    try
                                    {
                                        ret = DeleteControl(tableName, Convert.ToInt32(keys.First().value));
                                        if (ret.status != ReturnData.Status.Ok)
                                            return Ok(ret);
                                    }
                                    catch { }
                                }

                                queryString = string.Format("UPDATE [{0}] SET {1} WHERE {2}",
                                    tableName,
                                    string.Join(", ", values.Select(x => "[" + x.key + "] = @V_" + x.key)),
                                    string.Join(" AND ", keys.Select(x => "[" + x.key + "] = @K_" + x.key)));

                                List<SqlParameter> parameters = new List<SqlParameter>();
                                foreach (var item in values)
                                {
                                    parameters.Add(new SqlParameter("V_" + item.key, item.value));
                                }
                                foreach (var item in keys)
                                {
                                    parameters.Add(new SqlParameter("K_" + item.key, item.value));
                                }

                                bool sonuc = sqlConn.Set(queryString, System.Data.CommandType.Text, parameters.ToArray());
                                if (sonuc)
                                {
                                    ret.status = ReturnData.Status.Ok;
                                }
                                else
                                {
                                    ret.status = ReturnData.Status.Error;
                                    ret.errors.Add(sqlConn.lastError);
                                }
                            }
                        }
                        else if (type == "D")
                        {
                            if (keys.Count <= 0)
                            {
                                ret.status = ReturnData.Status.Error;
                                ret.errors.Add("Keys is required for [D]elete.");
                            }
                            else
                            {
                                queryString = string.Format("DELETE FROM [{0}] WHERE {1}",
                                    tableName,
                                    string.Join(" AND ", keys.Select(x => "[" + x.key + "] = @" + x.key)));

                                List<SqlParameter> parameters = new List<SqlParameter>();
                                foreach (var item in keys)
                                {
                                    parameters.Add(new SqlParameter(item.key, item.value));
                                }

                                bool sonuc = sqlConn.Set(queryString, System.Data.CommandType.Text, parameters.ToArray());
                                if (sonuc)
                                {
                                    ret.status = ReturnData.Status.Ok;
                                }
                                else
                                {
                                    ret.status = ReturnData.Status.Error;
                                    ret.errors.Add(sqlConn.lastError);
                                }
                            }
                        }
                        else if (type == "S")
                        {
                            if (keys.Count <= 0)
                            {
                                ret.status = ReturnData.Status.Error;
                                ret.errors.Add("Keys is required for [S]elect.");
                            }
                            else
                            {
                                queryString = string.Format("SELECT * FROM [{0}] WHERE {1}",
                                    tableName,
                                    string.Join(" AND ", keys.Select(x => "[" + x.key + "] = @" + x.key)));

                                List<SqlParameter> parameters = new List<SqlParameter>();
                                foreach (var item in keys)
                                {
                                    parameters.Add(new SqlParameter(item.key, item.value));
                                }

                                var dt = sqlConn.Get(queryString, System.Data.CommandType.Text, parameters.ToArray());
                                if (string.IsNullOrEmpty(sqlConn.lastError))
                                {
                                    ret.data = dt;
                                    ret.status = ReturnData.Status.Ok;
                                }
                                else
                                {
                                    ret.status = ReturnData.Status.Error;
                                    ret.errors.Add(sqlConn.lastError);
                                }
                            }
                        }
                        else
                        {
                            ret.status = ReturnData.Status.Error;
                            ret.errors.Add("Type is invalid.");
                        }
                    }
                }
                catch (Exception e)
                {
                    ret.status = ReturnData.Status.Error;
                    ret.errors.Add(e.Message);
                }
            }

            return Ok(ret);
        }

        public object JTokenToConventionalDotNetObject(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return ((JObject)token).Properties()
                        .ToDictionary(prop => prop.Name, prop => JTokenToConventionalDotNetObject(prop.Value));
                case JTokenType.Array:
                    return token.Values().Select(JTokenToConventionalDotNetObject).ToList();
                default:
                    return token.ToObject<object>();
            }
        }

        public ReturnData DeleteControl(string tableName, int id)
        {
            ReturnData ret = new ReturnData();
            ret.status = ReturnData.Status.Ok;

            Core.SQL.Conn sqlConn = new Core.SQL.Conn(StaticData.connStr);
            if (tableName == "Author")
            {
                DataTable dt = sqlConn.Get("SELECT COUNT(*) FROM [Book] WHERE deleted = 0 AND authorID = @authorID", CommandType.Text, new SqlParameter("authorID", id));
                int cnt = Convert.ToInt32(dt.Rows[0][0]);

                if (cnt > 0)
                {
                    ret.status = ReturnData.Status.Error;
                    ret.errors.Add("There is a book by this author, this author cannot be deleted.");
                }
            }

            return ret;
        }

    }

    public class Property
    {
        public string key { get; set; }
        public dynamic value { get; set; }
    }

}

