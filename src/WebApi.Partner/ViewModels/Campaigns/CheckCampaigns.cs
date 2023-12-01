using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Partner.ViewModels.Campaigns
{
    public class CheckCampaignResponse
    {
        public string Channel { get; set; }
        public string StoreNo { get; set; }
        public string OrderNo { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime OrderDate { get; set; } //yyyy-MM-dd
        public List<CampaignData> Campaigns { get; set; }
    }
    public class CampaignData
    {
        public string Campaign { get; set; }
    }
    public class GetCampaignData : CampaignData
    {
        public string StoreNo { get; set; }
        public string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public string PhoneNumber { get; set; }
    }
}
