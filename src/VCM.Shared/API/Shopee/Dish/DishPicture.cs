using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API.Shopee.Dish
{
    public class DishPicture
    {
        public int id { get; set; }
        public string url { get; set; }
    }
    public class DishPictureRsp
    {
        public int picture_id { get; set; }
        public string url { get; set; }
    }
}
