using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopInShop.Interface.Services
{
    public class MasterDataService
    {
        private readonly IConfiguration _configuration;
        public MasterDataService(IConfiguration configuration)
        {
            _configuration = configuration;
        }



    }
}
