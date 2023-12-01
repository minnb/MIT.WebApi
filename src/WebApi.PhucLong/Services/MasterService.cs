using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using VCM.PhucLong.API.Database;
using VCM.PhucLong.API.Queries.POS;
using VCM.Shared.Dtos.Odoo.Queries;

namespace VCM.PhucLong.API.Services
{
    public class MasterService : IMasterService
    {
        private readonly ILogger<MasterService> _logger;
        private readonly DapperOdooContext _context;
        public MasterService
            (
                ILogger<MasterService> logger,
                DapperOdooContext context
            )
        {
            _logger = logger;
            _context = context;
        }
        public List<GetPosConfig> GetPosConfig(string pos_name, int set)
        {
            try
            {
                List<GetPosConfig> lstData = new List<GetPosConfig>();
                using (IDbConnection conn = _context.CreateConnection(set))
                {
                    conn.Open();
                    return conn.Query<GetPosConfig>(PosConfigQuery.QueryPosConfig(pos_name)).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception " + ex.Message.ToString());
                throw;
            }
        }
    }
}
