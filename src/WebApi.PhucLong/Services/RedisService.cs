using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MIT.Utils;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VCM.PhucLong.API.Database;
using VCM.Shared.Const;
using VCM.Shared.Entity.PhucLong;

namespace WebApi.PhucLong.Services
{
    public class RedisService : IRedisService
    {
        private readonly ILogger<RedisService> _logger;
        private readonly DapperContext _context;
        private readonly IDistributedCache _distributeCache;
        public RedisService
            (
                ILogger<RedisService> logger,
                DapperContext context,
                IDistributedCache distributeCache
            )
        {
            _logger = logger;
            _context = context;
            _distributeCache = distributeCache;
        }

        private async Task SetCacheRedisAsync(string key_redis, byte[] data)
        {
            var options = new DistributedCacheEntryOptions()
                                   .SetSlidingExpiration(TimeSpan.FromHours(RedisConst.Redis_cache_time));
                                    //.SetAbsoluteExpiration(DateTime.Now.AddHours(12));
            await _distributeCache.SetAsync(key_redis, data, options);
        }
        public async Task<List<EmployeeOdooDto>> GetEmployeeRedis(bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_cache_odoo_employee);
            }

            var redis_data = await _distributeCache.GetAsync(RedisConst.Redis_cache_odoo_employee);
            if (redis_data != null)
            {
                return JsonConvert.DeserializeObject<List<EmployeeOdooDto>>(Encoding.UTF8.GetString(redis_data));
            }
            else
            {
                using var conn = _context.CreateConnection(1);
                conn.Open();

                string query = @"select id, name, emp_card_id from public.hr_employee;";

                var result = await conn.QueryAsync<EmployeeOdooDto>(query).ConfigureAwait(false);

                if (result.ToList().Count > 0)
                {
                    redis_data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result));
                    await SetCacheRedisAsync(RedisConst.Redis_cache_odoo_employee, redis_data);
                }
                return result.ToList();
            }
        }
        public async Task<List<ResPartnerOdooDto>> GetMemberRedis(bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_cache_odoo_member);
            }

            var redis_data = await _distributeCache.GetAsync(RedisConst.Redis_cache_odoo_member);
            if (redis_data != null)
            {
                return JsonConvert.DeserializeObject<List<ResPartnerOdooDto>>(Encoding.UTF8.GetString(redis_data));
            }
            else
            {
                using var conn = _context.CreateConnection(1);
                conn.Open();

                string query = @"select a.id, a.name, a.display_name, a.active, a.employee, email, a.mobile, a.commercial_partner_id, a.gender, a.marital, a.birthday, 
                                a.total_point_act, a.current_point_act, a.expired_date, a.loyalty_level_id, b.level_name
                                from res_partner a
                                inner join loyalty_level b on a.loyalty_level_id = b.id where a.active = true and a.expired_date is not null;";

                var result = conn.Query<ResPartnerOdooDto>(query).ToList();

                if (result.Count > 0)
                {
                    redis_data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result));
                    await SetCacheRedisAsync(RedisConst.Redis_cache_odoo_member, redis_data);
                }
                return result;
            }
        }
        public async Task<List<PaymentMethodOdooDto>> GetPaymentMethodRedis(bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_cache_odoo_payment_method);
            }

            var redis_data = await _distributeCache.GetAsync(RedisConst.Redis_cache_odoo_payment_method);
            if (redis_data != null)
            {
                return JsonConvert.DeserializeObject<List<PaymentMethodOdooDto>>(Encoding.UTF8.GetString(redis_data));
            }
            else
            {
                using var conn = _context.CreateConnection(1);
                conn.Open();

                var result = await conn.QueryAsync<PaymentMethodOdooDto>(@"SELECT id, name from pos_payment_method;").ConfigureAwait(false);

                if (result.ToList().Count > 0)
                {
                    redis_data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result));
                    await SetCacheRedisAsync(RedisConst.Redis_cache_odoo_payment_method, redis_data);
                }
                return result.ToList();
            }
        }
        public async Task<List<StoreInfoOdooDto>> GetPosConfigRedis(bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_cache_odoo_store);
            }

            var redis_data = await _distributeCache.GetAsync(RedisConst.Redis_cache_odoo_store);
            if (redis_data != null)
            {
                return JsonConvert.DeserializeObject<List<StoreInfoOdooDto>>(Encoding.UTF8.GetString(redis_data));
            }
            else
            {
                using var conn = _context.CreateConnection(1);
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
                    await SetCacheRedisAsync(RedisConst.Redis_cache_odoo_store, redis_data);
                }
                return result.ToList();
            }
        }
        public async Task<IEnumerable<ProductOdooDto>> GetProductProductRedis(bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_cache_odoo_product_product);
            }

            var redis_data = await _distributeCache.GetAsync(RedisConst.Redis_cache_odoo_product_product);
            if (redis_data != null)
            {
                return JsonConvert.DeserializeObject<List<ProductOdooDto>>(Encoding.UTF8.GetString(redis_data));
            }
            else
            {
                using var conn = _context.CreateConnection(1);
                conn.Open();

                string query = @"select a.id, a.default_code, t.name display_name, b.tax_id
                                from product_product a
                                inner join product_template t on t.id = a.product_tmpl_id
                                left join product_taxes_rel b on a.id = b.prod_id; ";

                var result = await conn.QueryAsync<ProductOdooDto>(query).ConfigureAwait(false);

                if (result.ToList().Count > 0)
                {
                    redis_data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result));
                    await SetCacheRedisAsync(RedisConst.Redis_cache_odoo_product_product, redis_data);
                }
                return result;
            }
        }
        public async Task<IEnumerable<PromoHeaderOdooDto>> GetPromoHeaderRedis(bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_cache_odoo_sale_promotion_header);
            }

            var redis_data = await _distributeCache.GetAsync(RedisConst.Redis_cache_odoo_sale_promotion_header);
            if (redis_data != null)
            {
                return JsonConvert.DeserializeObject<List<PromoHeaderOdooDto>>(Encoding.UTF8.GetString(redis_data));
            }
            else
            {
                using var conn = _context.CreateConnection(1);
                conn.Open();

                string query = @"select id, name, list_type from sale_promo_header where active = true;";

                var result = await conn.QueryAsync<PromoHeaderOdooDto>(query).ConfigureAwait(false);

                if (result.ToList().Count > 0)
                {
                    redis_data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result));
                    await SetCacheRedisAsync(RedisConst.Redis_cache_odoo_sale_promotion_header, redis_data);
                }
                return result;
            }
        }
        public async Task<List<int>> GetPayment_VCM_Redis(List<int> method, bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_cache_odoo_payment_method_vcm);
            }

            var redis_data = await _distributeCache.GetAsync(RedisConst.Redis_cache_odoo_payment_method_vcm);

            if (redis_data != null)
            {
                return JsonConvert.DeserializeObject<List<int>>(Encoding.UTF8.GetString(redis_data));
            }
            else
            {
                if (method != null && method.Count > 0)
                {
                    await SetCacheRedisAsync(RedisConst.Redis_cache_odoo_payment_method_vcm, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(method)));
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
    }
}
