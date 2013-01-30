using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OracleClient;
using System.Collections;
using System.Configuration;

/******************************************************************************************************
 * �����ߣ�
 * ����ʱ�䣺
 * ����������
 * �޶�������
 * �޸��ߣ�
 * �޸�ʱ�䣺
 * ��ϸ����������oracle���ݿ���ʲ�����
 * ****************************************************************************************************/
namespace Dosoft.DAL
{
    /// <summary>
    /// oracle���ݿ���ʲ�����
    /// </summary>
    public abstract class OracleHelper
    {

        // �������ݿ������ַ���
        public static readonly string connectionString = ConfigurationManager.ConnectionStrings["apps"].ConnectionString;
        
        //Create a hashtable for the parameter cached
        private static Hashtable parmCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// ִ�в�ѯ��䣬����DataSet
        /// </summary>
        /// <param name="SQLString">��ѯ���</param>
        /// <returns>DataSet</returns>
        public static DataSet Query(string SQLString)
        {
            if (SQLString != null && SQLString.Trim() != "")
            {
                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        connection.Open();
                        OracleDataAdapter command = new OracleDataAdapter(SQLString, connection);
                        command.Fill(ds, "ds");
                    }
                    catch (System.Data.OracleClient.OracleException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        connection.Close();
                    }
                    return ds;
                }
            }
            else
            {
                return null;
            }
        }

       
    }
}
