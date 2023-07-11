using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace Core.SQL
{
    public class Conn
    {

        private string connectionString = "";
        public string lastError = "";

        public Conn(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public DataTable Get(string query, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    command.CommandType = commandType;
                    foreach (SqlParameter parameter in parameters)
                    {
                        if (parameter.Value == null)
                            parameter.Value = DBNull.Value;
                        command.Parameters.Add(parameter);
                    }

                    SqlDataAdapter da = new SqlDataAdapter(command);
                    da.Fill(dt);
                }
                catch (Exception e)
                {
                    lastError = e.Message;
                }
            }

            return dt;
        }


        public List<T> Get<T>(string query, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            DataTable dt = Get(query, commandType, parameters);
            List<T> data = new List<T>();
            try
            {
                foreach (DataRow row in dt.Rows)
                {
                    T item = GetItem<T>(row);
                    data.Add(item);
                }
            }
            catch (Exception e)
            {
                lastError = e.Message;
            }
            return data;
        }

        public bool Set(string query, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            bool retD = true;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    command.CommandType = commandType;
                    foreach (SqlParameter parameter in parameters)
                    {
                        if (parameter.Value == null)
                            parameter.Value = DBNull.Value;
                        command.Parameters.Add(parameter);
                    }

                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    retD = false;
                    lastError = e.Message;
                }
            }
            return retD;
        }

        private T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName] == System.DBNull.Value ? null : dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }

    }



}
