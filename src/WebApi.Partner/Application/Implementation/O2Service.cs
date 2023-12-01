using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Partner.API.Application.Interfaces;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Shared.API;
using VCM.Shared.API.O2;
using VCM.Shared.API.Wintel;
using VCM.Shared.Entity.Partner;
using WebApi.Partner.Application.Interfaces;
using WebApi.Partner.ViewModels.O2;

namespace WebApi.Partner.Application.Implementation
{
    public class O2Service : IO2Service
    {
        private readonly ILogger<O2Service> _logger;
        private readonly ITransService _transService;
        private readonly IMemoryCacheService _memoryCacheService;
        public O2Service(
          ILogger<O2Service> logger,
          ITransService transService,
          IMemoryCacheService memoryCacheService
          )
        {
            _logger = logger;
            _transService = transService;
            _memoryCacheService = memoryCacheService;
        }

        public async Task<ResponseClient> CheckPhoneNumberAsync(WebApiViewModel webApiInfo, ValidateKitStatusRequest request, List<Item> itemDto)
        {
            await Task.Delay(1);
            TokenO2 tokenO2 = CreateTokenO2A(webApiInfo);
            try
            {
                if (tokenO2 != null)
                {
                    var result = CallApiO2(webApiInfo, tokenO2, null, "O2_CHECK_MEMBER", "GET", "?phone=" + request.Serial);
                    _logger.LogWarning("result: " + result);
                    if (!string.IsNullOrEmpty(result))
                    {
                        var objRsp = JsonConvert.DeserializeObject<QualifyO2Response>(result);
                        if (objRsp != null && objRsp.Data != null)
                        {
                            if (objRsp.Message.Contains("unauthorized") || objRsp.Verdict == "invalid_token")
                            {
                                tokenO2 = _memoryCacheService.TokenO2Async(webApiInfo, true).Result;
                                result = CallApiO2(webApiInfo, tokenO2, null, "O2_CHECK_MEMBER", "GET", "?phone_number=" + request.Serial);
                                objRsp = JsonConvert.DeserializeObject<QualifyO2Response>(result);
                            }

                            if (objRsp != null && objRsp.Data != null)
                            {
                                if (!string.IsNullOrEmpty(objRsp.Data.Phone_number))
                                {
                                    return ResponseHelper.RspOK(MappingQualifyO2(objRsp, itemDto));
                                }
                                else
                                {
                                    //số điện thoại không tồn tại trên O2 thì sẽ trả về NORMAL
                                    _logger.LogWarning("O2 => Số điện thoại không tồn tại " + objRsp.Message);

                                    string phone_test = StringHelper.Right(request.Serial, 9);
                                    if (StringHelper.Left(request.Serial, 1) == "0")
                                    {
                                        phone_test = "84" + phone_test;
                                    }
                                    return ResponseHelper.RspOK(new UserO2()
                                    {
                                        Phone_number = phone_test,
                                        Tier = "NORMAL",
                                        Potential_vip = false,
                                        Evo = false,
                                        Wintel = false,
                                        Tpay = false,
                                        QuotaMoney = 500000
                                    });
                                }
                            }
                            else
                            {
                                _logger.LogWarning("O2 => objRsp.Data = null không có dữ liệu Data từ O2 trả về || " + objRsp.Message);
                                return ResponseHelper.RspNotWarning(201, objRsp.Message);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("O2 => objRsp = null || " + result);
                            return ResponseHelper.RspNotFoundData("O2 Dữ liệu trả về không đúng định dạng");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("O2 => không nhận được response từ O2");
                        return ResponseHelper.RspNotAuth("Không nhận được response từ O2");
                    }
                }
                else
                {
                    return ResponseHelper.RspNotAuth("Lỗi đăng nhập API O2");
                   // return ResponseHelper.RspOK(FakeUserO2(request.Serial));

                }
            }
            catch (Exception ex)
            {
                _logger.LogError("O2 Exception CHECK_MEMBER" + ex.Message.ToString());
                return ResponseHelper.RspNotFoundData("Request timeout O2 ");
            }
        }
        public Task<ResponseClient> LoginO2Async(WebApiViewModel webApiInfo)
        {
            throw new System.NotImplementedException();
        }
        public async Task<ResponseClient> UpdateTierMemberO2Async(WebApiViewModel webApiInfo, UpdateTierMemberO2 request)
        {
            await Task.Delay(1);
            var dataFake = _transService.UpdateDataTestAsync(request.PartnerCode, request.MemberNumber, request.DataUpdate, 1);
            if (dataFake != null)
            {
                return new ResponseClient()
                {
                    Meta = new Meta()
                    {
                        Code = 200,
                        Message = "Update Tier member thành công"
                    },
                    Data = new UserO2()
                    {
                        Phone_number = dataFake.ItemNo,
                        Tier = dataFake.Test1,
                        Evo = dataFake.Test2 == "1"
                    }
                };
            }
            else
            {
                return new ResponseClient()
                {
                    Meta = new Meta()
                    {
                        Code = 201,
                        Message = "Update Tier member không thành công"
                    },
                    Data = null
                };
            }

        }      
        private UserO2 MappingQualifyO2(QualifyO2Response o2Response, List<Item> itemDto)
        {
            UserO2 resultMapping = new UserO2()
            {
                Phone_number = o2Response.Data.Phone_number,
                Potential_vip = o2Response.Data.Potential_vip,
                Evo = o2Response.Data.Products.Evo,
                Wintel = o2Response.Data.Products.Wintel,
                Tpay = o2Response.Data.Products.Tpay,
            };

            if (!o2Response.Data.Potential_vip && (o2Response.Data.Tier.ToUpper() == "STANDARD" || o2Response.Data.Tier.ToUpper() == "NORMAL"))
            {
                resultMapping.Tier = "NORMAL";
            }
            else if (o2Response.Data.Potential_vip && (o2Response.Data.Tier.ToUpper() == "STANDARD" || o2Response.Data.Tier.ToUpper() == "NORMAL"))
            {
                resultMapping.Tier = "POTENTIAL";
            }
            else if (o2Response.Data.Tier.ToUpper() == "VIP")
            {
                resultMapping.Tier = "VIP";
            }
            var checkQuota = itemDto.Where(x => x.PartnerItem == resultMapping.Tier).FirstOrDefault();

            if(checkQuota == null)
            {
                resultMapping.QuotaMoney = 0;
            }
            else
            {
                resultMapping.QuotaMoney = checkQuota.VatPercent;
            }           

            return resultMapping;
        }
        private string CallApiO2(WebApiViewModel webApiInfo, TokenO2 dataToken, object wsRequest, string func, string httpMethod, string param)
        {
            try
            {
                var routeApi = webApiInfo.WebRoute.Where(x => x.Name == func).FirstOrDefault();
                var url_request = webApiInfo.Host + routeApi.Route.ToString();

                if (!string.IsNullOrEmpty(param) && httpMethod == "GET")
                {
                    url_request += param;
                }
                string token = null;
                if (dataToken != null)
                {
                    token = "Bearer " + dataToken.Data.Token;
                }
                Console.WriteLine(url_request);
                _logger.LogWarning(url_request);
                ApiHelper api = new ApiHelper(
                    url_request,
                    null,
                    token,
                    httpMethod,
                    wsRequest != null ? JsonConvert.SerializeObject(wsRequest) : null,
                    true,
                    webApiInfo.HttpProxy,
                    new string[] { webApiInfo.Bypasslist }
                    );
                return api.InteractWithApi();
            }
            catch(Exception ex)
            {
                _logger.LogWarning("CallApi O2 Exception: " + ex.Message.ToString());
                return null;
            }
        }
        public async Task<ResponseClient> PingO2(WebApiViewModel webApiInfo)
        {
            var result = CallApiO2(webApiInfo, null, null, "O2_PING", "GET", null);
            _logger.LogWarning("result: " + result);
            var meta = ResponseHelper.MetaOK(200, result);
            await Task.Delay(1);
            return new ResponseClient()
            {
                Meta = meta,
                Data = null
            };
        }
        private TokenO2 CreateTokenO2A(WebApiViewModel webApiInfo)
        {
            TokenO2 dataToken = new TokenO2();
            try
            {
                var routeApi = webApiInfo.WebRoute.Where(x => x.Name == "Login").FirstOrDefault();
                var url_request = webApiInfo.Host + routeApi.Route.ToString();
                var request = new UserLoginO2()
                {
                    ConsumerKey = webApiInfo.UserName,
                    Password = webApiInfo.Password
                };

                string[] byPass = new string[] { webApiInfo.Bypasslist };

                //_logger.LogWarning(url_request);
                //_logger.LogWarning("Proxy: " + webApiInfo.HttpProxy);
                //_logger.LogWarning("byPass: " + webApiInfo.Bypasslist);
                ApiHelper api = new ApiHelper(
                    url_request,
                    "",
                    "",
                    "POST",
                    JsonConvert.SerializeObject(request),
                    false,
                    webApiInfo.HttpProxy,
                    byPass
                    );
                var strResponse = api.InteractWithApiResponse();
                _logger.LogWarning("Login O2 response: " + strResponse);
                if (strResponse != null && strResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //_logger.LogWarning("O2 error response:  " + JsonConvert.SerializeObject(strResponse));
                    string result = string.Empty;
                    using (Stream stream = strResponse.GetResponseStream())
                    {
                        StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.UTF8);
                        result = streamReader.ReadToEnd();
                    }
                    _logger.LogWarning("CreateToken O2: " + result);
                    if (!string.IsNullOrEmpty(result))
                    {
                        var objRsp = JsonConvert.DeserializeObject<TokenO2>(result);
                        if (objRsp != null)
                        {
                            return objRsp;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (WebException ex)
            {
                using WebResponse response = ex.Response;
                HttpWebResponse httpResponse = (HttpWebResponse)response;
                using Stream data = response.GetResponseStream();
                using var reader = new StreamReader(data);
                _logger.LogWarning("Exception CreateToken O2: " + reader.ReadToEnd());
                return null;
            }

        }
    }
}
