using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.PhucLong.API.Controllers;
using VCM.Shared.API;
using VCM.Shared.API.Shopee.Dish;
using VCM.Shared.API.Shopee.Topping;
using VCM.Shared.Const;
using VCM.Shared.Entity.SalesPartner;
using WebApi.Core.AppServices.ShopeeService;
using WebApi.PhucLong.Models;
using WebApi.PhucLong.Services;

namespace WebApi.PhucLong.Controllers
{
    public class NowFoodController : BaseController
    {
        private readonly ILogger<NowFoodController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IRedisService _redisService;
        private readonly IDishShopeeService _dishShopeeService;
        private readonly IToppingShopeeService _toppingShopeeService;
        private readonly IRestaurantShopeeService _restaurantShopeeService;
        private readonly IPhucLongShopeeService _phucLongShopeeService;
        private WebApiModel _webApiAirPayInfo;
        private string message_api = string.Empty;
        private string _proxyHttp = "";
        private string[] _bypassList = {};
        private string AppCode = "NOWFOOD";
        private string errorMessage = "";
        public NowFoodController(
            ILogger<NowFoodController> logger,
            IRedisService redisService,
            IDishShopeeService dishShopeeService,
            IConfiguration configuration,
            IToppingShopeeService toppingShopeeService,
            IRestaurantShopeeService restaurantShopeeService,
            IPhucLongShopeeService phucLongShopeeService
            )
        {
            _logger = logger;
            _redisService = redisService;
            _dishShopeeService = dishShopeeService;
            _configuration = configuration;
            _toppingShopeeService = toppingShopeeService;
            _restaurantShopeeService = restaurantShopeeService;
            _phucLongShopeeService = phucLongShopeeService;
            _webApiAirPayInfo = _redisService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == AppCode).SingleOrDefault();
            if (_configuration["AppSetting:Environment"].ToUpper() != "DEV")
            {
                _proxyHttp = _webApiAirPayInfo.HttpProxy;
                _bypassList = new string[] { _webApiAirPayInfo.Bypasslist };
            }
        }

        [HttpGet]
        [Route("api/v1/nowfood/restaurant/get_restaurant_info")]
        public ResponseClient GetRestaurantInfo([Required] string StoreNo = "2001")
        {
            _logger.LogWarning("===> proxy: " + _proxyHttp + "@" + JsonConvert.SerializeObject(_bypassList));
            var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "restaurant.get_restaurant_info").FirstOrDefault();
            if (route == null)
            {
                return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
            }
            string url = _webApiAirPayInfo.Host + route.Route;

            var result = _restaurantShopeeService.GetRestaurantInfo(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, StoreNo, _proxyHttp, _bypassList, ref message_api);
            if (result != null && result.Count > 0)
            {
                return new ResponseClient()
                {
                    Meta = new Meta()
                    {
                        Code = 200,
                        Message = message_api
                    },
                    Data = result
                };
            }
            else
            {
                return ResponseHelper.RspNotFoundData(message_api);
            }
        }

        [HttpGet]
        [Route("api/v1/nowfood/restaurant/get_operation_time_ranges")]
        public ResponseClient Get_operation_time_ranges([Required] string StoreNo = "2001")
        {
            _logger.LogWarning("===> proxy: " + _proxyHttp + "@" + JsonConvert.SerializeObject(_bypassList));
            var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "restaurant.get_operation_time_ranges").FirstOrDefault();
            if (route == null)
            {
                return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
            }
            string url = _webApiAirPayInfo.Host + route.Route;

            var result = _restaurantShopeeService.Get_Store_operation_time_ranges(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, StoreNo, _proxyHttp, _bypassList, ref message_api);
            if (result != null && result.Count > 0)
            {
                //_phucLongShopeeService.InsertShopeeRestaurant(result);
                return new ResponseClient()
                {
                    Meta = new Meta()
                    {
                        Code = 200,
                        Message = message_api
                    },
                    Data = result
                };
            }
            else
            {
                return ResponseHelper.RspNotFoundData(message_api);
            }
        }

        [HttpGet]
        [Route("api/v1/nowfood/dish/get")]
        public ResponseClient DishGet(string StoreNo = "2001")
        {
            var restaurant = _redisService.GetShopeeRestaurantAsync().Result?.Where(x => x.partner_restaurant_id == StoreNo).FirstOrDefault();
            if (restaurant == null)
            {
                return ResponseHelper.RspNotFoundData(StoreNo + @" chưa được khai báo trên NowFood");
            }
            else
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "dish.get").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _dishShopeeService.GetDishShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, restaurant.restaurant_id, _proxyHttp, _bypassList, ref message_api);

                if (result != null && result.Count > 0)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = result
                    };
                }
                else
                {
                    return ResponseHelper.RspNotFoundData(message_api);
                }
            }
        }
        
        [HttpGet]
        [Route("api/v1/nowfood/dish/get_detail")]
        public ResponseClient DishDetailGet([Required] int restaurant_id = 1000007107, [Required] string partner_restaurant_id = "2001", [Required] int dish_id = 0)
        {
            var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "dish.get_detail").FirstOrDefault();
            if (route == null)
            {
                return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
            }
            string url = _webApiAirPayInfo.Host + route.Route;

            var result = _dishShopeeService.Dish_get_detail_Shopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, restaurant_id, partner_restaurant_id, dish_id, ref message_api);

            if (result != null)
            {
                return new ResponseClient()
                {
                    Meta = new Meta()
                    {
                        Code = 200,
                        Message = message_api
                    },
                    Data = result
                };
            }
            else
            {
                return ResponseHelper.RspNotFoundData(message_api);
            }
        }

        [HttpPost]
        [Route("api/v1/nowfood/dish/create")]
        public async Task<ResponseClient> CreateDish([FromBody] DishCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                var restaurant = GetRestaurantFromRedis(request.partner_restaurant_id);
                if (!CheckStoreIsHeader(restaurant,request.partner_restaurant_id,ref errorMessage))
                {
                    return ResponseHelper.RspNotWarning(201, errorMessage);
                }

                restaurant = restaurant.Where(x => x.is_header == false).ToList();
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "dish.create").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;
                DishCreateResponse result = new DishCreateResponse();

                if (!request.is_apply_all)
                {
                    result = _dishShopeeService.CreateDishShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);
                }
                else
                {
                    var bodyRequest = new DishCreateAll()
                    {
                        restaurant_id = request.restaurant_id,
                        partner_restaurant_id = request.partner_restaurant_id,
                        partner_dish_group_id = request.partner_dish_group_id,
                        partner_dish_id = request.partner_dish_id,
                        name = request.name,
                        name_en = request.name_en,
                        description = request.description,
                        price = request.price,
                        display_order  = request.display_order,
                        picture_id = request.picture_id,
                        is_apply_all = request.is_apply_all,
                        branch_ids = restaurant.Where(x => x.is_header == false).Select(x => x.restaurant_id).ToArray()
                    };
                    result = _dishShopeeService.CreateDishShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, bodyRequest, ref message_api);
                }               
                
                if (result != null)
                {
                    var webApiInfoPartner = _redisService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == "PARTNER").SingleOrDefault();
                    if(webApiInfoPartner != null)
                    {
                        await _redisService.DeleteRedisCachePartnerAsync(webApiInfoPartner, RedisConst.Redis_ShopeeDish);
                    }
                    
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = result
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> DishMapping.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

        [HttpPost]
        [Route("api/v1/nowfood/dish/update")]
        public async Task<ResponseClient> UpdateDishAsync([FromBody] DishCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                var restaurant = GetRestaurantFromRedis(request.partner_restaurant_id);
                if (!CheckStoreIsHeader(restaurant, request.partner_restaurant_id, ref errorMessage))
                {
                    return ResponseHelper.RspNotWarning(201, errorMessage);
                }

                restaurant = restaurant.Where(x => x.is_header == false).ToList();
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "dish.update").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;
                bool result = false;
                if (!request.is_apply_all)
                {
                    result = _dishShopeeService.UpdateDishShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);
                }
                else
                {
                    var bodyRequest = new DishCreateAll()
                    {
                        restaurant_id = request.restaurant_id,
                        partner_restaurant_id = request.partner_restaurant_id,
                        partner_dish_group_id = request.partner_dish_group_id,
                        partner_dish_id = request.partner_dish_id,
                        name = request.name,
                        name_en = request.name_en,
                        description = request.description,
                        price = request.price,
                        display_order = request.display_order,
                        picture_id = request.picture_id,
                        is_apply_all = request.is_apply_all,
                        branch_ids = restaurant.Select(x => x.restaurant_id).ToArray()
                    };
                    result = _dishShopeeService.UpdateDishShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, bodyRequest, ref message_api);
                }
                if (result)
                {
                    var webApiInfoPartner = _redisService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == "PARTNER").SingleOrDefault();
                    if (webApiInfoPartner != null)
                    {
                        await _redisService.DeleteRedisCachePartnerAsync(webApiInfoPartner, RedisConst.Redis_ShopeeDish);
                    }

                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = null
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> UpdateDish.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

        [HttpPost]
        [Route("api/v1/nowfood/dish/delete")]
        public ResponseClient DeleteDish([FromBody] DishDeleteRequest request)
        {
            if (ModelState.IsValid)
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "dish.delete").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _dishShopeeService.DeleteDishShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);

                if (result)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = null
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> dish.delete.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

        [HttpPost]
        [Route("api/v1/nowfood/dish/bulk_create")]
        public ResponseClient BulkCreateDish([FromBody] DishBulkCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "s2s/dish/bulk_create").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _dishShopeeService.CreateBulkDishShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);

                if (result != null)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = result
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> BulkCreateDish.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

        [HttpPost]
        [Route("api/v1/nowfood/dish/bulk_update")]
        public ResponseClient BulkUpdateDish([FromBody] DishBulkCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "s2s/dish/update").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _dishShopeeService.UpdateDishBulkShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);

                if (result)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = null
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> bulk_update.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }


        //Mapping

        [HttpPost]
        [Route("api/v1/nowfood/dish/update_partner_id_mappings")]
        public ResponseClient DishMapping([FromBody] DishMappingsRequest request)
        {
            if (ModelState.IsValid)
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "dish.update_partner_id_mappings").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _dishShopeeService.MappingDishShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, request, _proxyHttp, _bypassList, ref message_api);

                if (result)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = null
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> DishMapping.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }

        }

        [HttpPost]
        [Route("api/v1/nowfood/dish/create_topping_mapping")]
        public ResponseClient DishCreateToppingMapping([FromBody] DishCreateToppingMappingRequest request)
        {
            if (ModelState.IsValid)
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name.Trim() == "dish.create_topping_mapping").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _dishShopeeService.DishCreateToppingMappingShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);

                if (result)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = null
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> DishCreateToppingMapping.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }

        }

        [HttpPost]
        [Route("api/v1/nowfood/dish/delete_topping_mapping")]
        public ResponseClient delete_topping_mapping([FromBody] DishDeleteToppingMappingRequest request)
        {
            if (ModelState.IsValid)
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name.Trim() == "dish.delete_topping_mapping").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _dishShopeeService.DeleteDishToppingMappingShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);

                if (result)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = null
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> DishCreateToppingMapping.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }

        }


        //Dish group
        [HttpGet]
        [Route("api/v1/nowfood/dish/get_groups")]
        public ResponseClient DishGroupGet([Required] string StoreNo = "2001")
        {
            var restaurant = _redisService.GetShopeeRestaurantAsync().Result?.Where(x => x.partner_restaurant_id == StoreNo).FirstOrDefault();
            if (restaurant == null)
            {
                return ResponseHelper.RspNotFoundData(StoreNo + @" chưa được khai báo trên NowFood");
            }
            else
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "dish.get_groups").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _dishShopeeService.GetDishGroupShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, restaurant.restaurant_id, _proxyHttp, _bypassList, ref message_api);

                if (result != null && result.Count > 0)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = result
                    };
                }
                else
                {
                    return ResponseHelper.RspNotFoundData(message_api);
                }
            }
        }

        [HttpGet]
        [Route("api/v1/nowfood/dish/group/get_group_detail")]
        public ResponseClient DishGroupDetailGet([Required] int restaurant_id = 1000007107, [Required] string partner_restaurant_id = "2001", [Required] int dish_group_id = 1, [Required] string partner_dish_group_id = "1")
        {
            var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "dish.get_group_detail").FirstOrDefault();
            if (route == null)
            {
                return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
            }
            string url = _webApiAirPayInfo.Host + route.Route;

            var result = _dishShopeeService.Dish_get_group_detail_Shopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList,restaurant_id, partner_restaurant_id, dish_group_id, partner_dish_group_id, ref message_api);

            if (result != null)
            {
                return new ResponseClient()
                {
                    Meta = new Meta()
                    {
                        Code = 200,
                        Message = message_api
                    },
                    Data = result
                };
            }
            else
            {
                return ResponseHelper.RspNotFoundData(message_api);
            }
        }

        [HttpPost]
        [Route("api/v1/nowfood/dish/group/create_group")]
        public ResponseClient CreateDishGroup([FromBody] DishGroupCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "dish.create_group").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _dishShopeeService.CreateDishGroupShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);

                if (result != null)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = result
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> DishMapping.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }

        }

        [HttpPost]
        [Route("api/v1/nowfood/dish/group/update_group")]
        public ResponseClient update_group([FromBody] DishGroupCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "dish.update_group").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _dishShopeeService.UpdateDishGroupShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);

                if (result)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = null
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> DishMapping.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }

        }

        [HttpPost]
        [Route("api/v1/nowfood/dish/group/update_partner_group_id_mappings")]
        public ResponseClient update_partner_group_id_mappings([FromBody] Dish_update_partner_group_id_mappings request)
        {
            if (ModelState.IsValid)
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "dish.update_partner_group_id_mappings").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _dishShopeeService.UpdatePartneGroupIdMappingShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);

                if (result)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = null
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> DishMapping.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }

        }

        [HttpGet]
        [Route("api/v1/nowfood/dish/get_partner_group_id_mappings")]
        public ResponseClient get_partner_group_id_mappings([Required] string StoreNo = "2001")
        {
            var restaurant = _redisService.GetShopeeRestaurantAsync().Result?.Where(x => x.partner_restaurant_id == StoreNo).FirstOrDefault();
            if (restaurant == null)
            {
                return ResponseHelper.RspNotFoundData(StoreNo + @" chưa được khai báo trên NowFood");
            }
            else
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "dish.get_partner_group_id_mappings").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _dishShopeeService.Get_dish_partner_group_id_mappings(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, restaurant.restaurant_id, _proxyHttp, _bypassList, ref message_api);

                if (result != null && result.Count > 0)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = result
                    };
                }
                else
                {
                    return ResponseHelper.RspNotFoundData(message_api);
                }
            }
        }

        [HttpPost]
        [Route("api/v1/nowfood/dish/group/delete_group")]
        public ResponseClient DeleteDishGroupMapping([FromBody] DishGroupDeleteRequest request)
        {
            if (ModelState.IsValid)
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name.Trim() == "dish.delete_group").FirstOrDefault();
                if(route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _dishShopeeService.DeleteDishGroupShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);

                if (result)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = null
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> DishCreateToppingMapping.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }

        }


        //Status
        [HttpPost]
        [Route("api/v1/nowfood/dish/set_statuses")]
        public ResponseClient DishSetStatusMapping([FromBody] DishSetStatusesRequest request)
        {
            if (ModelState.IsValid)
            {
                var restaurant = GetRestaurantFromRedis(request.partner_restaurant_id);
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "dish.set_statuses").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                bool result = false;
                if (!request.is_apply_all)
                {
                    _logger.LogWarning("DishSetStatusesRequest: " + JsonConvert.SerializeObject(request));
                    result = _dishShopeeService.DishSetStatusShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);
                }
                else
                {
                    if (!CheckStoreIsHeader(restaurant, request.partner_restaurant_id,ref errorMessage))
                    {
                        return ResponseHelper.RspNotWarning(201, errorMessage);
                    }
                    else
                    {
                        var bodyRequest = new DishSetStatusRequestAll()
                        {
                            partner_restaurant_id = request.partner_restaurant_id,
                            restaurant_id = request.restaurant_id,
                            is_apply_all = request.is_apply_all,
                            dishes = request.dishes,
                            branch_ids = restaurant.Where(x => x.is_header == false).Select(x => x.restaurant_id).ToArray()
                        };
                        _logger.LogWarning("DishSetStatusesRequest: " + JsonConvert.SerializeObject(bodyRequest));
                        result = _dishShopeeService.DishSetStatusShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, bodyRequest, ref message_api);
                    }
                }

                if (result)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = null
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> DishSetStatusMapping.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }

        }
        [HttpPost]
        [Route("api/v1/nowfood/dish/get_approval_status")]
        public ResponseClient Dish_Get_approval_status([FromBody] Get_approval_status request)
        {
            if (ModelState.IsValid)
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "dish.get_approval_status").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _dishShopeeService.Get_approval_status(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);

                if (result != null)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = result
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> Dish_Get_approval_status.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }

        }

        [HttpGet]
        [Route("api/v1/nowfood/dish/get_out_of_service")]
        public ResponseClient DishGetOutOfService([FromBody]  DishGetOutOfServiceRequest request)
        {
            var restaurant = _redisService.GetShopeeRestaurantAsync().Result?.Where(x => x.restaurant_id == request.restaurant_id).FirstOrDefault();
            if (restaurant == null)
            {
                return ResponseHelper.RspNotFoundData(request.restaurant_id + @" chưa được khai báo trên NowFood");
            }
            else
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "dish.get_out_of_service").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _dishShopeeService.GetOutOfServiceShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);

                if (result != null && result.Count > 0)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = result
                    };
                }
                else
                {
                    return ResponseHelper.RspNotFoundData(message_api);
                }
            }
        }

        //Picture
        [HttpPost]
        [Route("api/v1/nowfood/dish/picture/upload_picture")]
        public ResponseClient dish_upload_picture([FromBody] dish_upload_picture request)
        {
            if (ModelState.IsValid)
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name.Trim() == "dish.upload_picture").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _dishShopeeService.dish_upload_picture(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);

                if (result!= null)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = result
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> dish_upload_picture.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }

        }
        
        [HttpPost]
        [Route("api/v1/nowfood/dish/picture/delete_picture")]
        public ResponseClient dish_delete_picture([FromBody] dish_delete_picture request)
        {
            if (ModelState.IsValid)
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name.Trim() == "dish.delete_picture").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _dishShopeeService.Dish_delete_picture(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);

                if (result)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = null
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> dish_upload_picture.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }

        }


        /// <summary>
        /// TOPPING API
        /// </summary>
        /// <param name="Sunrise"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/v1/nowfood/topping/get_groups")]
        public ResponseClient ToppingGroupGet([Required] string StoreNo = "2001")
        {
            var restaurant = _redisService.GetShopeeRestaurantAsync().Result?.Where(x => x.partner_restaurant_id == StoreNo).FirstOrDefault();
            if (restaurant == null)
            {
                return ResponseHelper.RspNotFoundData(StoreNo + @" chưa được khai báo trên NowFood");
            }
            else
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "topping.get_groups").FirstOrDefault();
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _toppingShopeeService.GetToppingGroupShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, restaurant.restaurant_id, _proxyHttp, _bypassList, ref message_api);

                if (result != null && result.Count > 0)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = result
                    };
                }
                else
                {
                    return ResponseHelper.RspNotFoundData(message_api);
                }
            }
        }
        
        [HttpPost]
        [Route("api/v1/nowfood/topping/group/create_group")]
        public ResponseClient CreateToppingGroup([FromBody] ToppingCreateGroupRequest request)
        {
            if (ModelState.IsValid)
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name.ToLower() == "topping.create_group").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _toppingShopeeService.ToppingCreateGroupShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);
                if (result != null)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = result
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> CreateToppingGroup.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }
        
        [HttpPost]
        [Route("api/v1/nowfood/topping/group/update_group")]
        public ResponseClient Topping_update_group([FromBody] topping_update_group request)
        {
            if (ModelState.IsValid)
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name.ToLower() == "topping.update_group").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _toppingShopeeService.Topping_update_group_shopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);
                if (result)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = null
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> Topping_update_group.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

        [HttpPost]
        [Route("api/v1/nowfood/topping/group/delete_group")]
        public ResponseClient Topping_delete_group([FromBody] topping_delete_group request)
        {
            if (ModelState.IsValid)
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name.ToLower() == "topping.delete_group").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _toppingShopeeService.Topping_delete_group_shopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);
                if (result)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = null
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> Topping_update_group.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

        [HttpPost]
        [Route("api/v1/nowfood/topping/group/update_partner_group_id_mappings")]
        public ResponseClient ToppingGroupMapping([FromBody] ToppingGroupMappingRequest request)
        {
            if (ModelState.IsValid)
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name.ToLower() == "topping.update_partner_group_id_mappings").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _toppingShopeeService.MappingToppingGroupShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, request, _proxyHttp, _bypassList, ref message_api);
                if (result)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = null
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> ToppingGroupMapping.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }
        
        //Topping
        [HttpGet]
        [Route("api/v1/nowfood/topping/get")]
        public ResponseClient ToppingGet([Required] string StoreNo = "2001")
        {
            var restaurant = _redisService.GetShopeeRestaurantAsync().Result?.Where(x => x.partner_restaurant_id == StoreNo).FirstOrDefault();
            if (restaurant == null)
            {
                return ResponseHelper.RspNotFoundData(StoreNo + @" chưa được khai báo trên NowFood");
            }
            else
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "topping.get").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _toppingShopeeService.GetToppingShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, restaurant.restaurant_id, _proxyHttp, _bypassList, ref message_api);

                if (result != null && result.Count > 0)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = result
                    };
                }
                else
                {
                    return ResponseHelper.RspNotFoundData(message_api);
                }
            }
        }
        
        [HttpPost]
        [Route("api/v1/nowfood/topping/create")]
        public ResponseClient CreateTopping([FromBody] ToppingCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name.ToLower() == "topping.create").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _toppingShopeeService.CreateToppingShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);
                if (result != null)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = result
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> CreateTopping.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

        [HttpPost]
        [Route("api/v1/nowfood/topping/update")]
        public ResponseClient Topping_update([FromBody] topping_update request)
        {
            if (ModelState.IsValid)
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name.ToLower() == "topping.update").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _toppingShopeeService.Topping_update_shopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);
                if (result != null)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = result
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> CreateTopping.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }
        
        [HttpPost]
        [Route("api/v1/nowfood/topping/set_group_quantity")]
        public ResponseClient Topping_set_group_quantity([FromBody] topping_set_group_quantity request)
        {
            if (ModelState.IsValid)
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "topping.topping_set_group_quantity").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "API chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _toppingShopeeService.Topping_set_group_quantity(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);
                if (result)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = null
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> topping.update_prices.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

        [HttpPost]
        [Route("api/v1/nowfood/topping/update_prices")]
        public ResponseClient Topping_update_prices([FromBody] topping_update_prices request)
        {
            if (ModelState.IsValid)
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "topping.update_prices").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _toppingShopeeService.Topping_update_prices_shopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);
                if (result)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = null
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> topping.update_prices.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

        [HttpPost]
        [Route("api/v1/nowfood/topping/delete")]
        public ResponseClient Topping_delete([FromBody] topping_delete request)
        {
            if (ModelState.IsValid)
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "topping.delete").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _toppingShopeeService.Topping_delete_shopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);
                if (result)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = null
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> Topping_delete.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

        [HttpPost]
        [Route("api/v1/nowfood/topping/update_partner_id_mappings")]
        public ResponseClient ToppingMapping([FromBody] ToppingMappingRequest request)
        {
            if (ModelState.IsValid)
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "topping.update_partner_id_mappings").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _toppingShopeeService.MappingToppingShopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, request, _proxyHttp, _bypassList, ref message_api);
                if (result)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = null
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> ToppingMapping.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

        [HttpPost]
        [Route("api/v1/nowfood/topping/set_statuses")]
        public ResponseClient topping_set_statuses([FromBody] topping_set_statuses request)
        {
            if (ModelState.IsValid)
            {
                var route = _webApiAirPayInfo.WebRoute.Where(x => x.AppCode == AppCode && x.Name == "topping.set_statuses").FirstOrDefault();
                if (route == null)
                {
                    return ResponseHelper.RspNotWarning(201, "Api chưa được khai báo");
                }
                string url = _webApiAirPayInfo.Host + route.Route;

                var result = _toppingShopeeService.topping_set_statuses_shopee(url, _webApiAirPayInfo.UserName, _webApiAirPayInfo.Password, _proxyHttp, _bypassList, request, ref message_api);
                if (result)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = message_api
                        },
                        Data = null
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = message_api
                        },
                        Data = null
                    };
                }
            }
            else
            {
                _logger.LogWarning("===> topping_set_statuses.ErrorMessage: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

        private List<ShopeeRestaurant> GetRestaurantFromRedis(string partner_restaurant_id)
        {
            return _redisService.GetShopeeRestaurantAsync().Result?.Where(x => x.parent_restaurant == partner_restaurant_id).ToList();
        }
        private bool CheckStoreIsHeader(List<ShopeeRestaurant> restaurant, string partner_restaurant_id, ref string mess)
        {
            var checkStore = restaurant.Where(x => x.is_header == true && x.partner_restaurant_id == partner_restaurant_id).FirstOrDefault();
            if (checkStore == null)
            {
                mess = "Cửa hàng " + partner_restaurant_id + " không có quyền cập nhật menu";
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
