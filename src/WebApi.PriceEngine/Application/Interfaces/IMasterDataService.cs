using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VCM.Shared.API;
using WebApi.PriceEngine.Models.Master;

namespace WebApi.PriceEngine.Application.Interfaces
{
    public interface IMasterDataService
    {
        public Task<IList<object>> SyncGetDataByTable(string appCode, string tableName, int maxCounter);
        public Task<FileContentResult> GetFileMasterDataAsync(string appCode, string tableName, int maxCounter, string path, string contentType);
        public ResponseClient GetDataTableMaster();
        public Task<ResponseClient> GetFeaturedItemMasterAsync(string AppCode,string StoreNo, bool IsPromotion);
        public Task<ResponseClient> GetComboItemMasterAsync(string AppCode, string StoreNo);
    }
}
