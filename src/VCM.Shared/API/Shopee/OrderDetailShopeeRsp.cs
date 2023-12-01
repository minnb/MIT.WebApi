using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API.Shopee
{
    public class OrderDetailShopeeRsp
    {
        public string result { get; set; }
        public OrderDetailShopee reply { get; set; }
    }
    public class OrderDetailShopee
    {
        public int status { get; set; }
        public DateTime delivery_time { get; set; }
        public string code { get; set; }
        public order_value order_value { get; set; }
        public DateTime order_time { get; set; }
        public Restaurant restaurant { get; set; }
        public int confirm_remaining_time { get; set; }
        public extra_fee extra_fee { get; set; }
        public total_value total_value { get; set; }
        public bool allow_cancel { get; set; }
        public List<dish_groups> dish_groups { get; set; }
        public total_merchant_discount total_merchant_discount { get; set; }
        public bool allow_confirm { get; set; }
        public string confirm_expired_time { get; set; }
        public DateTime pick_time { get; set; }
        public string serial { get; set; }
        public pay_to_merchant pay_to_merchant { get; set; }
        public object merchant_discounts { get; set; }
        public string note { get; set; }
        public string merchant_note { get; set; }
        public string note_for_shipper { get; set; }
        public string cs_note { get; set; }
        public vat_info vat_info { get; set; }
        public commission_amount commission_amount { get; set; }
        public shipping shipping { get; set; }
        public customer customer { get; set; }
        public customer_bill customer_bill { get; set; }
    }
    public class shipping
    {
        public int type { get; set; }
        public string name { get; set; }
    }
    public class vat_info
    {
        public int id { get; set; }
        public string tax_number { get; set; }
        public string address { get; set; }
        public string company_name { get; set; }
        public int status { get; set; }
    }
    public class merchant_discounts
    {
        public string name { get; set; }
        public string code { get; set; }
        public merchant_discount merchant_discount { get; set; }

    }
    public class merchant_discount:temp_value
    {

    }
    public class dish_groups
    {
        public int dish_group_id { get; set; }
        public string group_name { get; set; }
        public List<dishes> dishes { get; set; }
    }
    public class dishes
    {
        public int dish_id { get; set; }
        public int quantity { get; set; }
        public price price { get; set; }
        public List<topping_groups> topping_groups { get; set; }
        public int order_dish_id { get; set; }
        public string note { get; set; }
        public string dish_name { get; set; }
        public string partner_dish_id { get; set; }
        public total total { get; set; }
        public int out_of_stock { get; set; }
        public original_price original_price { get; set; }
    }
    public class topping_groups
    {
        public int topping_group_id { get; set; }
        public string group_name { get; set; }
        public string partner_topping_group_id { get; set; }
        public List<topping> toppings { get; set; }
    }
    public class topping
    {
        public string partner_topping_id { get; set; }
        public original_price original_price { get; set; }
        public int topping_id { get; set; }
        public price price { get; set; }
        public string topping_name { get; set; }
        public int quantity { get; set; }
    }
    public class original_price : temp_value { }
    public class price : temp_value
    {
    }
    public class total : temp_value
    {
    }
    public class extra_fee : temp_value
    {
    }
    public class total_merchant_discount: temp_value
    {

    }
    public class commission_amount : temp_value
    {

    }
    public class customer
    {
        public string phone { get; set; }
        public string name { get; set; }
    }
    public class total_amount : temp_value { }
    public class surcharge_fee : temp_value { }
    public class packing_fee : temp_value { }
    public class hand_deliver_fee : temp_value { }
    public class service_fee : temp_value { }
    public class total_discount : temp_value { }
    public class foody_discount : temp_value { }
    public class shipping_fee : temp_value { }

    public class customer_bill
    {
        public total_amount total_amount { get; set; }
        public surcharge_fee surcharge_fee { get; set; }
        public packing_fee packing_fee { get; set; }
        public hand_deliver_fee hand_deliver_fee { get; set; }
        public service_fee service_fee { get; set; }
        public total_discount total_discount { get; set; }
        public foody_discount foody_discount { get; set; }
        public shipping_fee shipping_fee { get; set; }
        public merchant_discount merchant_discount { get; set; }
    }
}
