using System;
using System.Collections.Generic;

namespace WebApi.Partner.ViewModels.MBC
{
    public class ListPackagePosWcm
    {
        public List<DataListPackagePosWcmRsp> listPackagePosWcm { get; set; }
        public decimal MainPackagePrice { get; set; }
        public decimal PriceSim { get; set; }
    }
    public class DataListPackagePosWcmRsp
    {
        public int packageId { get; set; }
        public int usageTime { get; set; }
        public string usageUnit { get; set; }
        public int packageType { get; set; }
        public int statusId { get; set; }
        public string shopId { get; set; }
        public string packageName { get; set; }
        public string ocsCode { get; set; }
        public int basicPackageId { get; set; }
        public string itemNo { get; set; }
        public string barcode { get; set; }
        public decimal price { get; set; }
        public decimal priceSim { get; set; }
        public decimal esimPrice { get; set; }
        public decimal pricePhysicalSim { get; set; }
        public decimal mainPackagePrice { get; set; }
        public decimal physicalSimPrice { get; set;}
        public decimal pricePackagePhysical { get; set; }
    }
    public class ListPackagePosWcmRsp_2
    {
        public string packageId { get; set; }
        public string ocsCode { get; set; }
        public string hlrCode { get; set; }
        public decimal price { get; set; }
        public decimal originalPrice { get; set; }
        public string unit { get; set; }
        public int usageTime { get; set; }
        public string usageUnit { get; set; }
        public statusPackage status { get; set; }
        public string display { get; set; }
        public string PackageId { get; set; }
        public string imageThumbnail { get; set; }
        public string imageBanner { get; set; }
        public string appDisplay { get; set; }
        public DateTime createDate { get; set; }
        public DateTime updateAt { get; set; }
        public int statusCmp { get; set; }
        public int dataType { get; set; }
        public objectTypePackage objectType { get; set; }
        public packageTypePackage packageType { get; set; }
        public customerTypePackage customerType { get; set; }
        public subscriptionTypePackage subscriptionType { get; set; }
        public int customType { get; set; }
        public string awarded { get; set; }
        public string paramRecurring { get; set; }
        public int position { get; set; }
        public int isRemove { get; set; }
        public int groupId { get; set; }
        public string specialPackageType { get; set; }
        public string isMasanPackage { get; set; }
        public string numberVoucherMasan { get; set; }
        public string shopId { get; set; }
        public string isWinmemberPackage { get; set; }
        public string packageName { get; set; }
        public decimal priceSim { get; set; }
        public decimal esimPrice { get; set; }
        public decimal pricePhysicalSim { get; set; }
        public decimal mainPackagePrice { get; set; }
        public decimal physicalSimPrice { get; set; }
        public decimal pricePackagePhysical { get; set; }
    }


    public class subscriptionTypePackage
    {
        public int subscriptionTypeId { get; set; }
        public string subscriptionTypeName { get; set; }
        public int status { get; set; }
    }

    public class customerTypePackage
    {
        public int customerTypeId { get; set; }
        public string customerName { get; set; }
        public int status { get; set; }
    }
    public class packageTypePackage
    {
        public int packageTypeId { get; set; }
        public string packageTypeName { get; set; }
        public int status { get; set; }
    }
    public class objectTypePackage
    {
        public int objectTypeId { get; set; }
        public string objectTypeName { get; set; }
        public int status { get; set; }
        public int type { get; set; }
    }

    public class statusPackage
    {
        public int statusId { get; set; }
        public string statusName { get; set; }
        public string statusTable { get; set; }
    }
    public class displayPackage
    {
        public int displayId { get; set; }
        public string displayType { get; set; }
        public string description { get; set; }
        public string tableName { get; set; }
    }
}
