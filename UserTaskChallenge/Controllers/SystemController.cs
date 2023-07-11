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

        

    }

    public class Property
    {
        public string key { get; set; }
        public dynamic value { get; set; }
    }

}

