using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using VCM.Common.Helpers;

namespace PhucLong.Interface.Central.Database
{
    public class DBHelper
    {
        public DataSet ExecuteProcedure(string _dbConnectString, string procName, Hashtable parms)
        {
            DataSet ds = new DataSet("ExecuteProcedure");
            using (SqlConnection connection = new SqlConnection(_dbConnectString))
            {
                SqlCommand cmd = new SqlCommand
                {
                    CommandText = procName,
                    CommandType = CommandType.StoredProcedure
                };
                using (var da = new SqlDataAdapter(cmd.CommandText, connection))
                {
                    try
                    {
                        cmd.Connection = connection;
                        if (parms.Count > 0)
                        {
                            foreach (DictionaryEntry deparams in parms)
                            {
                                cmd.Parameters.AddWithValue(deparams.Key.ToString(), deparams.Value);
                            }
                        }
                        da.SelectCommand = cmd;
                        da.Fill(ds);
                    }
                    catch (Exception ex)
                    {
                        FileHelper.WriteLogs("Exception ExecuteProcedure: " + ex.ToString());
                    }
                }
            }
            return ds;
        }
        public DataSet ExecuteQuery(string _dbConnectString, string str_sql)
        {
            DataSet ds = new DataSet("dsQuery");
            using (SqlConnection connection = new SqlConnection(_dbConnectString))
            {
                SqlCommand cmd = new SqlCommand
                {
                    CommandText = str_sql,
                    CommandType = CommandType.Text,
                };
                using (var da = new SqlDataAdapter(cmd.CommandText, connection))
                {
                    try
                    {
                        cmd.Connection = connection;
                        da.SelectCommand = cmd;
                        da.Fill(ds);
                    }
                    catch (Exception ex)
                    {
                        FileHelper.WriteLogs("Exception ExecuteQuery: " + ex.ToString());
                    }
                }
            }
            return ds;
        }
        public SqlDataReader ExecuteReader(String connectionString, String commandText, CommandType commandType, params SqlParameter[] parameters)
        {
            SqlConnection conn = new SqlConnection(connectionString);

            using (SqlCommand cmd = new SqlCommand(commandText, conn))
            {
                cmd.CommandType = commandType;
                cmd.Parameters.AddRange(parameters);

                conn.Open();
                // When using CommandBehavior.CloseConnection, the connection will be closed when the  
                // IDataReader is closed. 
                SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return reader;
            }
        }
    }
}
