using System.Data;
using Microsoft.Data.SqlClient;

namespace API_TurboeSigner_V2.App_Dal
{
    public class DataClass
    {
        #region Variables
        private readonly IConfiguration _config;
        private SqlConnection con;
        private SqlCommand cmd;
        private SqlDataAdapter da;
        private SqlTransaction _transaction;

        #endregion

        public DataClass(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public void OpenConnection()
        {
            if (con == null || con.State != ConnectionState.Open)
            {
                var server = _config.GetConnectionString("SrvName");
                var uid = _config.GetConnectionString("dbUid");
                var pwd = _config.GetConnectionString("dbPwd");
                var dbName = _config.GetConnectionString("dbName");

                if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(dbName))
                {
                    throw new InvalidOperationException("Database configuration is missing.");
                }

                string constr = $"server={server};uid={uid};pwd={pwd};database={dbName};TrustServerCertificate=True";
                con = new SqlConnection(constr);
                con.Open();
            }
        }

        public void CloseConnection()
        {
            if (con?.State == ConnectionState.Open)
            {
                con.Close();
                con.Dispose();
            }
        }

        private SqlCommand CreateCommand(string procName, List<SqlParameter>? sqlParams)
        {
            cmd = new SqlCommand(procName, con)
            {
                CommandTimeout = 300,
                CommandType = CommandType.StoredProcedure,
                Transaction = _transaction
            };

            if (sqlParams != null)
            {
                foreach (SqlParameter param in sqlParams)
                {
                    cmd.Parameters.Add(param ?? throw new ArgumentNullException(nameof(param)));
                }
            }

            return cmd;
        }

        public void GetDataset(string procName, ref DataSet dataset, List<SqlParameter>? sqlParams = null)
        {
            if (dataset == null) throw new ArgumentNullException(nameof(dataset));

            OpenConnection();
            _transaction = con.BeginTransaction();

            try
            {
                using (cmd = CreateCommand(procName, sqlParams))
                {
                    da = new SqlDataAdapter(cmd);
                    da.Fill(dataset);
                    _transaction.Commit();
                }
            }
            catch (Exception e)
            {
                _transaction.Rollback();
                throw new Exception("Error fetching dataset", e);
            }
            finally
            {
                CloseConnection();
            }
        }

        public void GetDatatable(string procName, ref DataTable dt, List<SqlParameter>? sqlParams = null)
        {
            if (dt == null) throw new ArgumentNullException(nameof(dt));

            OpenConnection();
            _transaction = con.BeginTransaction();

            try
            {
                using (cmd = CreateCommand(procName, sqlParams))
                {
                    da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                    _transaction.Commit();
                }
            }
            catch (Exception e)
            {
                _transaction.Rollback();
                throw new Exception("Error fetching datatable", e);
            }
            finally
            {
                CloseConnection();
            }
        }

        public string ExecuteScalar(string procName, List<SqlParameter>? sqlParams)
        {
            string result = string.Empty;

            OpenConnection();
            _transaction = con.BeginTransaction();

            try
            {
                using (cmd = CreateCommand(procName, sqlParams))
                {
                    var res = cmd.ExecuteScalar();
                    result = res?.ToString() ?? string.Empty;
                    _transaction.Commit();
                }
            }
            catch (Exception e)
            {
                _transaction.Rollback();
                throw new Exception("Error executing scalar command", e);
            }
            finally
            {
                CloseConnection();
            }

            return result;
        }

        public int ExecuteNonQuery(string procName, List<SqlParameter>? sqlParams = null)
        {
            int result = 0;

            OpenConnection();
            _transaction = con.BeginTransaction();

            try
            {
                using (cmd = CreateCommand(procName, sqlParams))
                {
                    result = cmd.ExecuteNonQuery();
                    _transaction.Commit();
                }
            }
            catch (Exception e)
            {
                _transaction.Rollback();
                throw new Exception("Error executing non-query command", e);
            }
            finally
            {
                CloseConnection();
            }

            return result;
        }
    }
}
