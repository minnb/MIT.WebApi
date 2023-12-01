using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Common.Helpers
{
    public static class BarcodeHelper
    {
        public static string GetPluCode(string Barcode)
        {
            if (IsFreshFoodBarcode(Barcode))
            {
                switch(Barcode.Length)
                {
                    case 13:
                        return Barcode.Substring(0, 7) + "000000";
                    case 19:
                        Barcode = Barcode.Substring(0, 13);
                        return Barcode.Substring(0, 7) + "000000";
                    default: return Barcode;
                }
                
            }
            else
            {
                return Barcode;
            }
        }
        public static decimal GetQtyBarcode(string Barcode)
        {
            if (IsFreshFoodBarcode(Barcode))
            {
                if(Barcode.Length == 13)
                {
                    if(Barcode.Substring(0, 7) + "000000" == Barcode)
                    {
                        return 1;
                    }
                    else
                    {
                        decimal qty = decimal.Parse(Barcode.Substring(7, 5).ToString());
                        return Math.Round((decimal)(qty / 1000), 3);
                    }
                }
                else if(Barcode.Length == 19)
                {
                    return GetQtyFreshFoodBarcode(Barcode.Substring(0, 13));
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 1;
            }
        }
        private static decimal GetQtyFreshFoodBarcode(string Barcode)
        {
            if (Barcode.Length == 13)
            {
                if (Barcode.Substring(0, 7) + "000000" == Barcode)
                {
                    return 1;
                }
                else
                {
                    decimal qty = decimal.Parse(Barcode.Substring(7, 5).ToString());
                    return Math.Round((decimal)(qty / 1000), 3);
                }
            }
            else 
            {
                return 0;
            }
        }
        public static bool IsFreshFoodBarcode(string Barcode)
        {
            if (!string.IsNullOrEmpty(Barcode))
            {
                return Barcode.Substring(0, 2) == "26" ? true : false;
            }
            else
            {
                return false;
            }
        }
    }
}
