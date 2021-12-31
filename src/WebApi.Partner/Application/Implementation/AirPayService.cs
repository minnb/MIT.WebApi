using Microsoft.Extensions.Logging;
using MIT.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VCM.Common.Helpers;
using VCM.Shared.Partner;
using VCM.Shared.API;
using VCM.Partner.API.Application.Interfaces;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Shared.Entity.Partner;
using VCM.Partner.API.Common.Const;
using VCM.Shared.Dtos.POS;

namespace VCM.Partner.API.Application.Implementation
{
    public class AirPayService : IAirPayService
    {
        private readonly ILogger<AirPayService> _logger;
        private readonly ASCIIEncoding _byteConverter;
        private readonly ITransService _transService;
        public AirPayService(
            ILogger<AirPayService> logger,
            ITransService transService
            )
        {
            _logger = logger;
            _byteConverter = new ASCIIEncoding();
            _transService = transService;
        }
        public ResponseClient CallPurchaseCardV2(WebApiViewModel _webApiAirPayInfo, POSPurchaseCardRequest bodyData, List<Item> itemDto, string privateKeyVCM, string proxyHttp, string[] byPass)
        {
            ResponseClient resultObj = new ResponseClient();
            string requestId = Guid.NewGuid().ToString().Replace("-","");
            try
            {
                var posRequest = new PurchaseCard()
                {
                    brand_id = bodyData.PosNo.ToString(),
                    service_code = bodyData.ServiceCode.ToString(),
                    quantity = bodyData.Quantity.ToString(),
                    partner_id = _webApiAirPayInfo.UserName.ToString(),
                    reference_no = bodyData.OrderNo.ToString()
                };
                QueryStringHelper queryStringHelper = new QueryStringHelper();
                CreateQueryPurchaseCard(queryStringHelper, posRequest);

                var dataRequest = new PurchaseCardAirPay()
                {
                    brand_id = posRequest.brand_id,
                    service_code = posRequest.service_code,
                    quantity = posRequest.quantity,
                    partner_id = posRequest.partner_id,
                    reference_no = posRequest.reference_no,
                    signature = this.SignatureAirPay(queryStringHelper, privateKeyVCM)
                };

                ApiHelper api = new ApiHelper(
                        _webApiAirPayInfo.Host + _webApiAirPayInfo.WebRoute.Where(x => x.Name == "purchase_card").FirstOrDefault().Route.ToString(),
                        "",
                        "",
                        "POST",
                        JsonConvert.SerializeObject(dataRequest),
                        false,
                        proxyHttp,
                        byPass
                        );

                string strResponse = api.InteractWithApi();
                _logger.LogWarning("Response: " + strResponse);

                var rp = JsonConvert.DeserializeObject<RespCode>(strResponse);
                var rpPurchaseCard = JsonConvert.DeserializeObject<DataPurchaseCard>(strResponse);

                if (rpPurchaseCard != null)
                {
                    List<TransLineDto> transLines = new List<TransLineDto>();
                    var mapItem = itemDto.Where(x => x.PartnerItem == "PURCHASE_CARD").FirstOrDefault();
                    int i = 0;

                    foreach (var item in rpPurchaseCard.Cards)
                    {
                        i++;
                        var remark = new Remark()
                        {
                            Remark1 = DecrytionRSA(item.Pin, privateKeyVCM),
                            Remark2 = bodyData.PurchaseValue.ToString(),
                            Remark3 = i.ToString(),
                            Remark4 = item.Serial,
                            Remark5 = item.Expiry
                        };

                        var infoItemPartner = new InfoItemPartner()
                        {
                            ItemNo = mapItem.ItemNo,
                            Qty = 1,
                            UnitPrice = bodyData.PurchaseValue,
                            DiscountAmount = 0,
                            LineAmountInclVAT = bodyData.PurchaseValue,
                            IsLoyalty = false
                        };

                        transLines.Add(ObjExample.MappingTransLine(mapItem, i,0, infoItemPartner, remark));
                    }

                    resultObj.Meta = new Meta()
                    {
                        Code = 200,
                        Message = "Successfully"
                    };

                    resultObj.Data = new TransHeaderDto()
                    {
                        AppCode = bodyData.PartnerCode,
                        OrderNo = bodyData.OrderNo,
                        OrderTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        CustNo = "",
                        CustName = "",
                        CustPhone = "",
                        CustAddress = "",
                        CustNote = "",
                        DeliveryType = 1,
                        CardMember = "",
                        TotalAmount = bodyData.Quantity * bodyData.PurchaseValue,
                        PaymentAmount = 0,
                        Status = 0,
                        IsPromotion = false,
                        RefNo = rpPurchaseCard.apc_order_id,
                        CashierId = "",
                        CashierName = "",
                        TransLine = transLines.ToList(),
                        TransPaymentEntry = null
                    };

                    _transService.AddTransRaw(new TransRaw()
                    {
                        Id = Guid.NewGuid(),
                        AppCode = bodyData.PartnerCode,
                        StoreNo = bodyData.PosNo.Substring(0,4),
                        OrderNo = posRequest.reference_no,
                        ServiceType = bodyData.ServiceType,
                        ReferenceNo = rpPurchaseCard.apc_order_id??"",
                        RawData = JsonConvert.SerializeObject(rpPurchaseCard),
                        UpdateFlg = "N",
                        CrtDate = DateTime.Now,
                        CrtUser = bodyData.PosNo,
                        IPAddress = ""
                    });
                }
                else
                {
                    resultObj.Meta = ResponseHelper.RspTimeOut(bodyData.PartnerCode);
                }
                return resultObj;

            }
            catch(Exception ex)
            {
                string messageError = ex.Message.ToString();
                if(ex.InnerException != null)
                {
                    messageError += @"====> " + ex.InnerException.Message.ToString();
                }
                return new ResponseClient()
                {
                    Meta = new Meta { Code = 9999, Message = messageError },
                    Data = null
                };
            }
        }
        public ResponseClient GetCardV2(WebApiViewModel _webApiAirPayInfo, POSGetCardV2Request bodyData, string privateKeyVCM, string proxyHttp, string[] byPass)
        {
            try
            {
                GetCardV2Request getCardV2Request = new GetCardV2Request()
                {
                    partner_id = _webApiAirPayInfo.UserName.ToString(),
                    reference_no = bodyData.reference_no
                };

                QueryStringHelper queryStringHelper = new QueryStringHelper();
                CreateQueryGetCard(queryStringHelper, getCardV2Request);

                getCardV2Request.signature = this.SignatureAirPay(queryStringHelper, privateKeyVCM);

                ApiHelper api = new ApiHelper(
                        _webApiAirPayInfo.Host + _webApiAirPayInfo.WebRoute.Where(x => x.Name == "get_card").FirstOrDefault().Route.ToString(),
                        "",
                        "",
                        "POST",
                        JsonConvert.SerializeObject(getCardV2Request),
                        false,
                        proxyHttp,
                        byPass
                        );

                //_logger.LogWarning("Request: " + JsonConvert.SerializeObject(dataRequest));

                string strResponse = api.InteractWithApi();
                _logger.LogWarning("GetCardV2 Response: " + strResponse);

                var rp = JsonConvert.DeserializeObject<RespCode>(strResponse);
                var getCardV2Rsp = JsonConvert.DeserializeObject<GetCardV2Rsp>(strResponse);
                
                if(getCardV2Rsp != null)
                {
                    _transService.AddTransRaw(new TransRaw()
                    {
                        Id = Guid.NewGuid(),
                        AppCode = "APY",
                        StoreNo = "99999999",
                        OrderNo = bodyData.reference_no,
                        ReferenceNo = bodyData.reference_no??"",
                        RawData = JsonConvert.SerializeObject(getCardV2Rsp),
                        UpdateFlg = "N",
                        CrtDate = DateTime.Now,
                        CrtUser = "A",
                        IPAddress = ""
                    });
                }

                return new ResponseClient()
                {
                    Meta = new Meta { Code = rp.resp_code, Message = rp.resp_msg },
                    Data = null
                };
            }
            catch (Exception ex)
            {
                string messError = ex.Message.ToString();
                if (ex.InnerException != null)
                {
                    messError += messError + " =====>" + ex.InnerException.ToString();
                }
                return new ResponseClient()
                {
                    Meta = new Meta { Code = 9999, Message = messError },
                    Data = null
                };
            }
        }
        public ResponseClient GetBalance(WebApiViewModel _webApiAirPayInfo, string privateKeyVCM, string proxyHttp, string[] byPass)
        {
            try
            {
                GetBalanceRequest getBalanceRequest = new GetBalanceRequest() 
                {
                    partner_id = _webApiAirPayInfo.UserName.ToString()
                };
                QueryStringHelper queryStringHelper = new QueryStringHelper();
                CreateQueryGetBalance(queryStringHelper, getBalanceRequest);


                getBalanceRequest.signature = this.SignatureAirPay(queryStringHelper, privateKeyVCM);

                ApiHelper api = new ApiHelper(
                        _webApiAirPayInfo.Host + _webApiAirPayInfo.WebRoute.Where(x => x.Name == "get_balance").FirstOrDefault().Route.ToString(),
                        "",
                        "",
                        "POST",
                        JsonConvert.SerializeObject(getBalanceRequest),
                        false,
                        proxyHttp,
                        byPass
                        );

                //_logger.LogWarning("Request: " + JsonConvert.SerializeObject(dataRequest));

                string strResponse = api.InteractWithApi();
                _logger.LogWarning("GetBalance Response: " + strResponse);

                var rp = JsonConvert.DeserializeObject<RespCode>(strResponse);
                var getBalanceRsp = JsonConvert.DeserializeObject<GetBalanceRsp>(strResponse);

                if (getBalanceRsp != null)
                {
                    _transService.AddTransRaw(new TransRaw()
                    {
                        Id = Guid.NewGuid(),
                        AppCode = "APY",
                        StoreNo = "99999999",
                        OrderNo = getBalanceRequest.partner_id,
                        ReferenceNo = getBalanceRequest.partner_id??"",
                        RawData = JsonConvert.SerializeObject(getBalanceRsp),
                        UpdateFlg = "N",
                        CrtDate = DateTime.Now,
                        CrtUser = "A",
                        IPAddress = ""
                    });
                }

                return new ResponseClient()
                {
                    Meta = new Meta { Code = rp.resp_code, Message = rp.resp_msg },
                    Data =  null
                };
            }
            catch (Exception ex)
            {
                string messError = ex.Message.ToString();
                if (ex.InnerException != null)
                {
                    messError += messError + " =====>" + ex.InnerException.ToString();
                }
                return new ResponseClient()
                {
                    Meta = new Meta { Code = 9999, Message = messError },
                    Data = null
                };
            }
        }
        public ResponseClient GetTransactions(WebApiViewModel _webApiAirPayInfo, GetTransactionsRequest bodyData, string privateKeyVCM, string proxyHttp, string[] byPass)
        {
            try
            {
                GetTransaction getTransaction = new GetTransaction()
                {
                    partner_id = _webApiAirPayInfo.UserName.ToString(),
                    from_date = bodyData.from_date.ToString("yyyy-MM-dd 00:00:00"),
                    to_date = bodyData.to_date.ToString("yyyy-MM-dd 00:00:00")
                };
                QueryStringHelper queryStringHelper = new QueryStringHelper();
                CreateQueryGetTransaction(queryStringHelper, getTransaction);


                getTransaction.signature = this.SignatureAirPay(queryStringHelper, privateKeyVCM);

                ApiHelper api = new ApiHelper(
                        _webApiAirPayInfo.Host + _webApiAirPayInfo.WebRoute.Where(x => x.Name == "get_transactions").FirstOrDefault().Route.ToString(),
                        "",
                        "",
                        "POST",
                        JsonConvert.SerializeObject(getTransaction),
                        false,
                        proxyHttp,
                        byPass
                        );

                //_logger.LogWarning("Request: " + JsonConvert.SerializeObject(dataRequest));

                string strResponse = api.InteractWithApi();
                _logger.LogWarning("GetTransactions Response: " + strResponse);

                var rp = JsonConvert.DeserializeObject<RespCode>(strResponse);
                //var getBalanceRsp = JsonConvert.DeserializeObject<GetBalanceRsp>(strResponse);

                //if (getBalanceRsp != null)
                //{
                //    _transService.Add(new TransRaw()
                //    {
                //        Id = Guid.NewGuid(),
                //        AppCode = "APY",
                //        StoreNo = "99999999",
                //        TranNo = getTransaction.from_date.ToString("yyyyMMddHHMMSS"),
                //        ReferenceNo = getTransaction.to_date.ToString("yyyyMMddHHMMSS"),
                //        RawData = JsonConvert.SerializeObject(getBalanceRsp),
                //        UpdateFlg = "N",
                //        CrtDate = DateTime.Now,
                //        CrtUser = "A",
                //        IPAddress = ""
                //    });
                //}

                return new ResponseClient()
                {
                    Meta = new Meta { Code = rp.resp_code, Message = rp.resp_msg },
                    Data = null // getBalanceRsp != null ? getBalanceRsp : null
                };
            }
            catch (Exception ex)
            {
                string messError = ex.Message.ToString();
                if (ex.InnerException != null)
                {
                    messError += messError + " =====>" + ex.InnerException.ToString();
                }
                return new ResponseClient()
                {
                    Meta = new Meta { Code = 9999, Message = messError },
                    Data = null
                };
            }
        }

        private string DecrytionRSA(string signedData, string private_key)
        {
            //ASCIIEncoding ByteConverter = new ASCIIEncoding();
            byte[] decryptedData;
            var signByte = Convert.FromBase64String(signedData);
            decryptedData = RSA3Cryptography.RSASHA256DesCryption(signByte, private_key);

            return _byteConverter.GetString(decryptedData);

        }
        private  string SignatureAirPay(QueryStringHelper queryStringHelper, string private_key)
        {
            //ASCIIEncoding ByteConverter = new ASCIIEncoding();
            string rawData = queryStringHelper.GetRequestRaw();
            _logger.LogWarning(rawData);           

            byte[] originalData = _byteConverter.GetBytes(rawData);
            byte[] signedData;

            signedData = RSA3Cryptography.HashAndSignBytes(originalData, private_key);

            return Convert.ToBase64String(signedData);
        }

        private  void CreateQueryPurchaseCard(QueryStringHelper queryStringHelper, PurchaseCard pay)
        {
            queryStringHelper.AddRequestData("service_code", pay.service_code);
            queryStringHelper.AddRequestData("quantity", pay.quantity.ToString());
            queryStringHelper.AddRequestData("reference_no", pay.reference_no);
            queryStringHelper.AddRequestData("partner_id", pay.partner_id);
            queryStringHelper.AddRequestData("brand_id", pay.brand_id);
        }
        private void CreateQueryGetCard(QueryStringHelper queryStringHelper, GetCardV2Request pay)
        {
            queryStringHelper.AddRequestData("reference_no", pay.reference_no);
            queryStringHelper.AddRequestData("partner_id", pay.partner_id);
        }
        private void CreateQueryGetBalance(QueryStringHelper queryStringHelper, GetBalanceRequest pay)
        {
            queryStringHelper.AddRequestData("partner_id", pay.partner_id);
        }
        private void CreateQueryGetTransaction(QueryStringHelper queryStringHelper, GetTransaction pay)
        {
            queryStringHelper.AddRequestData("partner_id", pay.partner_id);
            queryStringHelper.AddRequestData("from_date", pay.from_date);
            queryStringHelper.AddRequestData("to_date", pay.to_date);
        }
    }
}
