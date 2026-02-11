using Dapper;
using DigitalSignatureApplication.Models;
using DigitalSignatureApplication.Shared;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DigitalSignatureApplication.Repository
{
    public class DbRepository
    {
        private readonly DapperContext _context;
        private readonly string _spQuery;

        public DbRepository()
        {
            _context = new DapperContext();
            _spQuery = "MTA_LIVE_06012026.DSC_SP";
        }
        public async Task<IEnumerable<PendingViewModel>> GetPendingFromView()
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("MethodName", "View", DbType.String);
            return await GetQueryResult<PendingViewModel>(parameters);
        }

        public async Task<IEnumerable<OutPathViewModel>> GetOutPath(string DocEntry, string DocType, string DatabaseName)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("MethodName", "OutPath", DbType.String);
            parameters.Add("DatabaseName", DatabaseName, DbType.String);
            parameters.Add("DocType", DocType, DbType.String);
            parameters.Add("DocEntry", DocEntry, DbType.String);
            return await GetQueryResult<OutPathViewModel>(parameters);
        }

        public async Task<IEnumerable<SignerViewModel>> GetSignerList(string DocEntry, string DocType)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("MethodName", "GetSignerList", DbType.String);
            parameters.Add("DocType", DocType, DbType.String);
            parameters.Add("DocEntry", DocEntry, DbType.String);
            return await GetQueryResult<SignerViewModel>(parameters);
        }

        public async Task<int> UpdateView(string DatabaseName,
            string DocUnqKey, string DocType, string FilePath)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("MethodName", "UpdateView", DbType.String);
            parameters.Add("DocType", DocType, DbType.String);
            parameters.Add("DocEntry", "", DbType.String);
            parameters.Add("DatabaseName", DatabaseName, DbType.String);
            parameters.Add("DSCPath", FilePath, DbType.String);
            parameters.Add("DocumentUnqKey", DocUnqKey, DbType.String);
            return await ExecuteAQuery(parameters);
        }

        private async Task<int> ExecuteAQuery(DynamicParameters parameters)
        {
            using (IDbConnection connection = _context.CreateConnection())
            {
                var result = await connection.ExecuteAsync(_spQuery, parameters, commandType: CommandType.StoredProcedure);
                return result;
            }
        }

        private async Task<IEnumerable<T>> GetQueryResult<T>(DynamicParameters parameters)
        {
            using (IDbConnection connection = _context.CreateConnection())
            {
                var result = await connection.QueryAsync<T>(_spQuery, parameters, commandType: CommandType.StoredProcedure);
                return result;
            }
        }
    }
}
