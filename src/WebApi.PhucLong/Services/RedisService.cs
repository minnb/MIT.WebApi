using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MIT.Utils;
using Newtonsoft.Json;
using RestSharp;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VCM.PhucLong.API.Database;
using VCM.Shared.Const;
using VCM.Shared.Dtos.POS;
using VCM.Shared.Entity.Partner;
using VCM.Shared.Entity.PhucLong;
using VCM.Shared.Entity.SalesPartner;
using WCM.EntityFrameworkCore.EntityFrameworkCore.Partner;
using WebApi.PhucLong.Enums;
using WebApi.PhucLong.Models;

namespace WebApi.PhucLong.Services
{
    public class RedisService : IRedisService
    {
        private readonly ILogger<RedisService> _logger;
        private readonly DapperOdooContext _context;
        private readonly IDistributedCache _distributeCache;
        private readonly PartnerMDDbContext _dbContext;
        public RedisService
            (
                ILogger<RedisService> logger,
                DapperOdooContext context,
                IDistributedCache distributeCache,
                PartnerMDDbContext dbContext
            )
        {
            _logger = logger;
            _context = context;
            _distributeCache = distributeCache;
            _dbContext = dbContext;
        }
        
        private async Task SetCacheRedisAsync(string key_redis, byte[] data)
        {
            var options = new DistributedCacheEntryOptions()
                                   .SetSlidingExpiration(TimeSpan.FromHours(RedisConst.Redis_cache_time));
                                    //.SetAbsoluteExpiration(DateTime.Now.AddHours(12));
            await _distributeCache.SetAsync(key_redis, data, options);
        }
        public async Task<List<EmployeeOdooDto>> GetEmployeeRedis(int set, bool isDelete = false)
        {
            string key = RedisConst.Redis_cache_odoo_employee + set.ToString();
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(key);
            }

            var redis_data = await _distributeCache.GetAsync(key);
            if (redis_data != null)
            {
                return JsonConvert.DeserializeObject<List<EmployeeOdooDto>>(Encoding.UTF8.GetString(redis_data));
            }
            else
            {
                using var conn = _context.CreateConnection(set);
                conn.Open();

                string query = @"select id, name, emp_card_id from public.hr_employee;";

                var result = await conn.QueryAsync<EmployeeOdooDto>(query).ConfigureAwait(false);

                if (result.ToList().Count > 0)
                {
                    redis_data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result));
                    await SetCacheRedisAsync(key, redis_data);
                }
                return result.ToList();
            }
        }
        public async Task<List<ResPartnerOdooDto>> GetMemberRedis(int set, bool isDelete = false)
        {
            string key = RedisConst.Redis_cache_odoo_member + set.ToString();
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(key);
            }

            var redis_data = await _distributeCache.GetAsync(key);
            if (redis_data != null)
            {
                return JsonConvert.DeserializeObject<List<ResPartnerOdooDto>>(Encoding.UTF8.GetString(redis_data));
            }
            else
            {
                using var conn = _context.CreateConnection(set);
                conn.Open();

                string query = @"select a.id, a.name, a.display_name, a.active, a.employee, email, a.mobile, a.commercial_partner_id, a.gender, a.marital, a.birthday, 
                                a.total_point_act, a.current_point_act, a.expired_date, a.loyalty_level_id, b.level_name
                                from res_partner a
                                inner join loyalty_level b on a.loyalty_level_id = b.id where a.active = true and a.expired_date is not null;";

                var result = conn.Query<ResPartnerOdooDto>(query).ToList();

                if (result.Count > 0)
                {
                    redis_data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result));
                    await SetCacheRedisAsync(key, redis_data);
                }
                return result;
            }
        }
        public async Task<List<PaymentMethodOdooDto>> GetPaymentMethodRedis(int set, bool isDelete = false)
        {
            string key = RedisConst.Redis_cache_odoo_payment_method + set.ToString();
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(key);
            }

            var redis_data = await _distributeCache.GetAsync(key);
            if (redis_data != null)
            {
                return JsonConvert.DeserializeObject<List<PaymentMethodOdooDto>>(Encoding.UTF8.GetString(redis_data));
            }
            else
            {
                using var conn = _context.CreateConnection(set);
                conn.Open();

                var result = await conn.QueryAsync<PaymentMethodOdooDto>(@"SELECT id, name from pos_payment_method;").ConfigureAwait(false);

                if (result.ToList().Count > 0)
                {
                    redis_data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result));
                    await SetCacheRedisAsync(key, redis_data);
                }
                return result.ToList();
            }
        }
        public async Task<List<StoreInfoOdooDto>> GetPosConfigRedis(int set, bool isDelete = false)
        {
            string key = RedisConst.Redis_cache_odoo_store + set.ToString();
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(key);
            }

            var redis_data = await _distributeCache.GetAsync(key);
            if (redis_data != null)
            {
                return JsonConvert.DeserializeObject<List<StoreInfoOdooDto>>(Encoding.UTF8.GetString(redis_data));
            }
            else
            {
                using var conn = _context.CreateConnection(set);
                conn.Open();

                string query = @"select b.id store_id, b.name store_name, a.id pos_id, c.code store_no, a.name pos_no, a.receipt_header, a.receipt_footer
                                From pos_config a
                                inner join stock_location b on a.stock_location_id = b.id 
                                inner join stock_warehouse c on c.id = a.warehouse_id and c.lot_stock_id = a.stock_location_id
                                where a.active = true;";

                var result = await conn.QueryAsync<StoreInfoOdooDto>(query).ConfigureAwait(false);

                if (result.ToList().Count > 0)
                {
                    //image base 64
                    //foreach (var item in result.ToList())
                    //{
                    //    string url_image = Regex.Match(item.receipt_footer, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase).Groups[1].Value;
                    //    if (!string.IsNullOrEmpty(url_image))
                    //    {
                    //        string img_base64 = await Base64Utils.GetImageAsBase64Url(url_image);
                    //        if (!string.IsNullOrEmpty(img_base64))
                    //        {
                    //            await SetCacheRedisAsync(RedisConst.Redis_cache_odoo_pos_config_qrcode, Encoding.UTF8.GetBytes(img_base64));
                    //        }
                    //        break;
                    //    }
                    //}

                    redis_data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result));
                    await SetCacheRedisAsync(key, redis_data);
                }
                return result.ToList();
            }
        }
        public async Task<IEnumerable<ProductOdooDto>> GetProductProductRedis(int set, bool isDelete = false)
        {
            string key = RedisConst.Redis_cache_odoo_product_product + set.ToString();
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(key);
            }

            var redis_data = await _distributeCache.GetAsync(key);
            if (redis_data != null)
            {
                return JsonConvert.DeserializeObject<List<ProductOdooDto>>(Encoding.UTF8.GetString(redis_data));
            }
            else
            {
                using var conn = _context.CreateConnection(set);
                conn.Open();

                string query = @"select a.id, a.default_code, t.name display_name, b.tax_id
                                from product_product a
                                inner join product_template t on t.id = a.product_tmpl_id
                                left join product_taxes_rel b on a.id = b.prod_id; ";

                var result = await conn.QueryAsync<ProductOdooDto>(query).ConfigureAwait(false);

                if (result.ToList().Count > 0)
                {
                    redis_data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result));
                    await SetCacheRedisAsync(key, redis_data);
                }
                return result;
            }
        }
        public async Task<IEnumerable<PromoHeaderOdooDto>> GetPromoHeaderRedis(int set, bool isDelete = false)
        {
            string key = RedisConst.Redis_cache_odoo_sale_promotion_header + set.ToString();
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(key);
            }

            var redis_data = await _distributeCache.GetAsync(key);
            if (redis_data != null)
            {
                return JsonConvert.DeserializeObject<List<PromoHeaderOdooDto>>(Encoding.UTF8.GetString(redis_data));
            }
            else
            {
                using var conn = _context.CreateConnection(set);
                conn.Open();

                string query = @"select id, name, list_type from sale_promo_header where active = true;";

                var result = await conn.QueryAsync<PromoHeaderOdooDto>(query).ConfigureAwait(false);

                if (result.ToList().Count > 0)
                {
                    redis_data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result));
                    await SetCacheRedisAsync(key, redis_data);
                }
                return result;
            }
        }
        public async Task<List<int>> GetPayment_VCM_Redis(List<int> method, bool isDelete = false)
        {
            string key = RedisConst.Redis_cache_odoo_payment_method_vcm;
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(key);
            }

            var redis_data = await _distributeCache.GetAsync(key);

            if (redis_data != null)
            {
                return JsonConvert.DeserializeObject<List<int>>(Encoding.UTF8.GetString(redis_data));
            }
            else
            {
                if (method != null && method.Count > 0)
                {
                    await SetCacheRedisAsync(key, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(method)));
                }
                return method;
            }
        }
        public async Task<List<int>> GetPayment_VCM_Detail_Redis(List<int> method, bool isDelete = false)
        {
            string key = RedisConst.Redis_cache_odoo_payment_method_detail_vcm;
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(key);
            }

            var redis_data = await _distributeCache.GetAsync(key);

            if (redis_data != null)
            {
                return JsonConvert.DeserializeObject<List<int>>(Encoding.UTF8.GetString(redis_data));
            }
            else
            {
                if (method != null && method.Count > 0)
                {
                    await SetCacheRedisAsync(key, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(method)));
                }
                return method;
            }
        }
        public async Task SetRedisKeyAsync(string key, string value)
        {
            if (await _distributeCache.GetAsync(key) == null)
            {
                var options = new DistributedCacheEntryOptions()
                                  .SetSlidingExpiration(TimeSpan.FromHours(12));
                await _distributeCache.SetAsync(key, Encoding.UTF8.GetBytes(value), options);
            }
        }
        public async Task RemoveRedisKeyAsync(string key)
        {
            await _distributeCache.RemoveAsync(key);
        }
        public async Task<string> GetRedisValueAsync(string key)
        {
            var redis_data = await _distributeCache.GetAsync(key);
            if (redis_data != null)
            {
                return Encoding.UTF8.GetString(redis_data);
            }
            else
            {
                return null;
            }
        }
        public async Task<IEnumerable<Pos_Staging>> GetListOrderRedis(string redis_server, string port, int location_id)
        {
            List<Pos_Staging> result = new List<Pos_Staging>();
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redis_server +":" + port + @",allowAdmin=true"))
            {
                IDatabase db = redis.GetDatabase();

                var keys = redis.GetServer(redis_server, int.Parse(port)).Keys();

                string[] keysArr = keys.Select(key => (string)key).ToArray();

                foreach (string key in keysArr)
                {
                    if (key.Contains(location_id.ToString()))
                    {
                        result.Add(JsonConvert.DeserializeObject<Pos_Staging>(await GetRedisValueAsync(key)));
                    }
                }
            }

            return result;
        }
        public async Task<List<CpnVchBOMHeaderDto>> GetCpnVchBOMHeaderAsync(bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_cache_pos_CpnVchBOMHeader);
            }

            var key_redis = await _distributeCache.GetAsync(RedisConst.Redis_cache_pos_CpnVchBOMHeader);
            if (key_redis != null)
            {
                return JsonConvert.DeserializeObject<List<CpnVchBOMHeaderDto>>(Encoding.UTF8.GetString(key_redis));
            }
            else
            {
                using var conn = _context.CreateConnDB;
                conn.Open();
                string query = @"select distinct ItemNo, ItemName,UnitOfMeasure, DiscountType, cast(DiscountValue as decimal) DiscountValue, cast(MaxAmount as decimal) MaxAmount, cast(ValueOfVoucher as decimal) ValueOfVoucher, ArticleType, StartingDate, EndingDate, Blocked, LimitQty, CpnVchType, MinAmount
                                        from CpnVchBOMHeader (nolock)
                                        where Blocked = 0";
                var data = conn.Query<CpnVchBOMHeaderDto>(query).ToList();
                if (data.Count > 0)
                {
                    await SetCacheRedisAsync(RedisConst.Redis_cache_pos_CpnVchBOMHeader, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
                }
                return data;
            }
        }
        public async Task<List<WebApiModel>> GetDataWebApiAsync(bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_cache_webapi_sys_webapi);
            }

            var dataCache = await _distributeCache.GetAsync(RedisConst.Redis_cache_webapi_sys_webapi);
            if (dataCache != null)
            {
                return JsonConvert.DeserializeObject<List<WebApiModel>>(Encoding.UTF8.GetString(dataCache));
            }
            else
            {
                List<WebApiModel> lstDataApi = new List<WebApiModel>();
                var infoApi = _dbContext.SysWebApi.Where(x => x.Blocked == false).ToList();
                var routeApi = _dbContext.SysWebRoute.Where(x => x.Blocked == false).ToList();
                if (infoApi.Count > 0)
                {
                    List<SysWebRoute> lstNewRoute = new List<SysWebRoute>();
                    foreach (var api in infoApi)
                    {
                        lstDataApi.Add(new WebApiModel()
                        {
                            Id = api.Id,
                            Host = api.Host,
                            AppCode = api.AppCode,
                            UserName = api.UserName,
                            Password = api.Password,
                            PublicKey = api.PublicKey,
                            PrivateKey = api.PrivateKey,
                            Blocked = api.Blocked,
                            Description = api.Description,
                            WebRoute = routeApi?.Where(x => x.AppCode == api.AppCode).ToList(),
                            HttpProxy = api.HttpProxy,
                            Bypasslist = api.Bypasslist
                        });
                        lstNewRoute.Clear();
                    }
                    await SetCacheRedisAsync(RedisConst.Redis_cache_webapi_sys_webapi, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(lstDataApi)));

                    return lstDataApi;
                }
                else
                {
                    return null;
                }
            }
        }
        public async Task<List<ShopeeRestaurant>> GetShopeeRestaurantAsync(bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_ShopeeRestaurant);
            }

            var key_redis = await _distributeCache.GetAsync(RedisConst.Redis_ShopeeRestaurant);
            if (key_redis != null)
            {
                return JsonConvert.DeserializeObject<List<ShopeeRestaurant>>(Encoding.UTF8.GetString(key_redis));
            }
            else
            {
                var data = _dbContext.ShopeeRestaurant.ToList();
                if (data.Count > 0)
                {
                    await SetCacheRedisAsync(RedisConst.Redis_ShopeeRestaurant, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
                }
                return data;
            }
        }
        public async Task<List<CpnVchBOMLineDto>> GetCpnVchBOMLineAsync(bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_cache_pos_CpnVchBOMLine);
            }

            var key_redis = await _distributeCache.GetAsync(RedisConst.Redis_cache_pos_CpnVchBOMLine);
            if (key_redis != null)
            {
                return JsonConvert.DeserializeObject<List<CpnVchBOMLineDto>>(Encoding.UTF8.GetString(key_redis));
            }
            else
            {
                using var conn = _context.CreateConnDB;
                conn.Open();
                string query = @"SELECT ItemNo, LineItemNo From CpnVchBOMLine";
                var data = conn.Query<CpnVchBOMLineDto>(query).ToList();
                if (data.Count > 0)
                {
                    await SetCacheRedisAsync(RedisConst.Redis_cache_pos_CpnVchBOMLine, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
                }
                return data;
            }
        }
        public async Task<bool> DeleteRedisCachePartnerAsync(WebApiModel webApiInfo, string keyRedis)
        {
            await Task.Delay(1);
            if (webApiInfo == null)
            {
                return false;
            }
            else
            {
                RestClient client = new RestClient(webApiInfo.Host)
                {
                    Timeout = 45000
                };
                string router = webApiInfo.WebRoute.Where(x => x.Name == "DeleteRedisCache").FirstOrDefault().Route;
                RestRequest restRequest = new RestRequest(router, Method.POST);
                restRequest.AddHeader("Accept", "application/json");
                restRequest.AddHeader("Authorization", webApiInfo.Description + " " + Convert.ToBase64String(Encoding.UTF8.GetBytes(webApiInfo.UserName + ":" + webApiInfo.Password)));
                restRequest.AddHeader("KeyRedis", keyRedis);

                var response = await client.ExecuteAsync(restRequest);

                if(response != null && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }
    }
}
