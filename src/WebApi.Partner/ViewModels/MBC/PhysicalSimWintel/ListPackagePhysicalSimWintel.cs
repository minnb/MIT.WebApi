using System;
using System.Collections.Generic;

namespace WebApi.Partner.ViewModels.MBC.PhysicalSimWintel
{
    public class ListPackagePhysicalSimWintel
    {
        public List<ListPackagePhysicalSim> listPackagePosWcm { get; set; }
    }
    public class ListPackagePhysicalSim
    {
        public int packageId { get; set; }
        public string ocsCode { get; set; }
        public string hlrCode { get; set; }
        public int price { get; set; }
        public int priceSim { get; set; }
        public int pricePhysicalSim { get; set; }
        public string originalPrice { get; set; }
        public string unit { get; set; }
        public int usageTime { get; set; }
        public StatusPackagePhysicalSim status { get; set; }
        public DisplayPackagePhysicalSim display { get; set; }
        public string imageThumbnail { get; set; }
        public string imageBanner { get; set; }
        public string appDisplay { get; set; }
        public DateTime createDate { get; set; }
        public DateTime updateAt { get; set; }
        public int statusCmp { get; set; }
        public int dataType { get; set; }
        public ObjectTypePackagePhysicalSim objectType { get; set; }
        public PackageTypePackagePhysicalSim packageType { get; set; }
        public CustomerTypePackagePhysicalSim customerType { get; set; }
        public SubscriptionTypePackagePhysicalSim subscriptionType { get; set; }
        public int customType { get; set; }
        public string awarded { get; set; }
        public string paramRecurring { get; set; }
        public int position { get; set; }
        public int isRemove { get; set; }
        public int groupId { get; set; }
        public string specialPackageType { get; set; }
        public string isMasanPackage { get; set; }
        public string numberVoucherMasan { get; set; }
        public int shopId { get; set; }
        public int isWinmemberPackage { get; set; }
        public string packageName { get; set; }

    }
    public class SubscriptionTypePackagePhysicalSim
    {
        public int subscriptionTypeId { get; set; }
        public string subscriptionTypeName { get; set; }
        public int status { get; set; }
    }
    public class PackageTypePackagePhysicalSim
    {
        public int packageTypeId { get; set; }
        public string packageTypeName { get; set; }
        public int status { get; set; }

    }
    public class CustomerTypePackagePhysicalSim
    {
        public int customerTypeId { get; set; }
        public string customerName { get; set; }
        public int status { get; set; }

    }
    public class StatusPackagePhysicalSim
    {
        public int statusId { get; set; }
        public string statusName { get; set; }
        public string statusTable { get; set; }

    }
    public class DisplayPackagePhysicalSim
    {
        public int statusId { get; set; }
        public string displayType { get; set; }
        public string description { get; set; }
        public string tableName { get; set; }

    }
    public class ObjectTypePackagePhysicalSim
    {
        public int objectTypeId { get; set; }
        public string objectTypeName { get; set; }
        public int status { get; set; }
        public int type { get; set; }

    }
}
