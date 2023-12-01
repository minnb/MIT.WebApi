using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace WebApi.PriceEngine.Enums
{
    public enum PriceEngineAppCode
    {
        WIN_LIFE,
        WIN_DR,
        WIN_PLH,
        CV_LIFE,
        PLH_FL,
        PLH_KS,
        PLH_WIN
    }
    public enum PriceEngineEnum
    {
        [EnumMember(Value = "Tạo đơn hàng thành công")]
        OrderSuccessfull = 200,
        [EnumMember(Value = "Đơn hàng đã tồn tại")]
        OrderAlreadyExist = 210,
        [EnumMember(Value = "Lỗi tính toán đơn hàng")]
        OrderErrorCalc = 211,
        [EnumMember(Value = "Thông tin đơn hàng không đúng")]
        OrderException = 212,
        [EnumMember(Value = "Lỗi tạo dữ liệu đơn hàng")]
        OrderInsDatabase = 213,

        [EnumMember(Value = "Barcode chưa được cài giá bán")]
        BarcodeNotPrice = 220,
        [EnumMember(Value = "Barcode không tồn tại")]
        BarcodeNotFound = 221,
        [EnumMember(Value = "Barcode không đúng định dạng")]
        BarcodeException = 222,

        [EnumMember(Value = "Mã sản phẩm chưa được cài giá bán ")]
        ItemNotPrice = 230,
        [EnumMember(Value = "Mã sản phẩm không tồn tại")]
        ItemNotFound = 231,
        [EnumMember(Value = "Mã sản phẩm không đúng định dạng")]
        ItemException = 232
    }

}
