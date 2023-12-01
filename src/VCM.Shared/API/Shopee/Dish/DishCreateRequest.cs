using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API.Shopee.Dish
{
    public class DishBulkCreateRequest
    {
        public int restaurant_id { get; set; }
        public string partner_restaurant_id { get; set; }
        public List<ObjDishCreate> dishes { get; set; }
    }
    public class DishCreateRequest: DishCreateDto
    {
        public bool is_apply_all { get; set; }
    }
    public class DishCreateAll: DishCreateDto
    {
        public bool is_apply_all { get; set; }
        public int[] branch_ids { get; set; }
    }
    public class DishCreateDto
    {
        [Required]
        public int restaurant_id { get; set; }
        [Required]
        public string partner_restaurant_id { get; set; }
        [Required]
        public string partner_dish_id { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string partner_dish_group_id { get; set; }
        [Required]
        public decimal price { get; set; }
        public string name_en { get; set; }
        public int display_order { get; set; }
        public string description { get; set; }
        public int picture_id { get; set; }
        //public bool is_apply_all { get; set; }
        //public int[] branch_ids { get; set; }
    }
    public class ObjDishCreate
    {
        public string partner_dish_id { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string partner_dish_group_id { get; set; }
        [Required]
        public decimal price { get; set; }
        public string name_en { get; set; }
        public int display_order { get; set; }
        public string description { get; set; }
        public int picture_id { get; set; }
        public bool is_apply_all { get; set; }
    }
    public class DishCreateResponse
    {
        public int dish_id { get; set; }
        public bool is_pending { get; set; }
    }
}
