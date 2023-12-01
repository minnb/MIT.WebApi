using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VCM.Shared.API.PhucLongV2;

namespace VCM.Common.Validation
{
    public static class OrderValidation
    {
        public static bool OrderResponse(OrderResponseBody orderRequestBody, ref string error_mess)
        {
            bool flag = true;
            try
            {
                var paymentAmount = orderRequestBody.PaymentAmount;
                decimal lineAmountInclVat = orderRequestBody.Items.Where(x => x.UnitPrice > 0).Sum(x =>x.LineAmount);
                if(lineAmountInclVat > 0 && lineAmountInclVat != orderRequestBody.TotalAmount)
                {
                    flag = false;
                    error_mess = @"Đơn hàng " + orderRequestBody.OrderNo + @" tổng tiền đơn hàng: " + lineAmountInclVat.ToString("0,###") + " sai lệch tổng thanh toán: " + orderRequestBody.TotalAmount.ToString("0,###");
                }
            }
            catch (Exception ex)
            {
                error_mess = ex.Message.ToString();
                flag = false;
            }
            return flag;
        }
        public static bool OrderRequest(OrderRequestBody orderRequestBody, ref string error_mess)
        {
            bool flag = true;
            try
            {
                var paymentAmount = orderRequestBody.PaymentAmount;
                decimal lineAmountInclVat = orderRequestBody.Items.Where(x => x.LineAmount > 0).Sum(x => x.LineAmount);
                
                if (orderRequestBody.Payments != null && orderRequestBody.Payments.Count > 0)
                {
                    var paymentAmountKIOS = orderRequestBody.Payments.Sum(x => x.AmountTendered);
                    var sumAmount = orderRequestBody.TotalAmount - paymentAmountKIOS;
                    if (paymentAmount != sumAmount)
                    {
                        flag = false;
                        error_mess = @"Sai lệch số tiền phải thanh toán: PaymentAmount != TotalAmount - SUM(AmountTendered)";
                    }
                    else if (lineAmountInclVat != paymentAmountKIOS)
                    {
                        flag = false;
                        error_mess = @"Tổng tiền đơn hàng: " + lineAmountInclVat.ToString("0,###") + " sai lệch tổng thanh toán: " + paymentAmountKIOS.ToString("0,###");
                    }

                }
                else if (orderRequestBody.Payments == null && paymentAmount > 0)
                {
                    error_mess = @"Sai lệch số tiền phải thanh toán PaymentAmount";
                    flag = false;
                }

            }
            catch (Exception ex)
            {
                error_mess = ex.Message.ToString();
                flag = false;
            }
            return flag;
        }
    }
}
