using System.Collections.Generic;
using VCM.Shared.Dtos.Odoo.Queries;

namespace VCM.PhucLong.API.Services
{
    public interface IMasterService
    {
        List<GetPosConfig> GetPosConfig(string pos_name);

    }
}
