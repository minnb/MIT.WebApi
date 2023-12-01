using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Tools.Interface.Dtos.DRW;
using VCM.Common.Helpers;
using WCM.EntityFrameworkCore.EntityFrameworkCore.DrWin;

namespace Tools.Interface.Services.DrWin
{
    public static class InboundDrwService
    {
        public static void ReadXmlStockData(string jobName, List<ItemDrwDto> itemDrwDto, string fileName, ref List<M_ton_kho> lstData)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(fileName);
                var xml = XmlHelper.ReadXml(jobName, fileName);
                XmlNodeList nodeListCount = xml.GetElementsByTagName("LineItem");
                if (nodeListCount.Count > 1)
                {
                    //FileHelper.Write2Logs(jobName, JsonConvert.SerializeXmlNode(xml));
                    var result = JsonConvert.DeserializeObject<BatchStockXmlDrw>(JsonConvert.SerializeXmlNode(xml));
                    if (result.MT_BLUEPOS_BatchStock.LineItem.Count > 0)
                    {
                        foreach (var item in result.MT_BLUEPOS_BatchStock.LineItem)
                        {
                            if (!string.IsNullOrEmpty(item.Quantity))
                            {
                                item.Quantity.Trim();
                            }
                            else
                            {
                                FileHelper.Write2Logs(jobName, "Quantity IsNullOrEmpty: " + item.ArticleNumber);
                                continue;
                            }
                            var itemMaster = itemDrwDto.Where(x => x.ItemNo == item.ArticleNumber).FirstOrDefault();
                            if(itemMaster == null)
                            {
                                FileHelper.Write2Logs(jobName, "NotFound: " + item.ArticleNumber);
                                continue;
                            }
                            decimal qty = decimal.Parse(!string.IsNullOrEmpty(item.Quantity) ? item.Quantity.Replace(" ", "") : "0");
                            lstData.Add(new M_ton_kho()
                            {
                                Ma_sap = item.Site??"",
                                Ma_thuoc = item.ArticleNumber,
                                Don_vi_tinh = itemMaster.BaseUnit,
                                So_luong_ton_dau = (int)qty,
                                So_luong_xuat = 0,
                                So_luong_ton = (int)qty,
                                So_lo = item.Batch,
                                Han_dung = item.SLED,
                                Loai_phieu = "N",
                                Ma_phieu = "",
                                Ngay_xuat = DateTime.Now.AddDays(-1).Date,
                                Priority = int.Parse(!string.IsNullOrEmpty(item.Priority) ? item.Priority.Trim() : "0"),
                                IsProcess = false,
                                Thoi_gian = DateTime.Now
                            });
                        }
                    }
                }
                else if (nodeListCount.Count == 1)
                {
                    var result = JsonConvert.DeserializeObject<BatchStockXmlDrw_1>(JsonConvert.SerializeXmlNode(xml));
                    if (result.MT_BLUEPOS_BatchStock != null)
                    {
                        var item = result.MT_BLUEPOS_BatchStock.LineItem;
                        if (!string.IsNullOrEmpty(item.Quantity))
                        {
                            item.Quantity.Trim();
                        }
                        else
                        {
                            FileHelper.Write2Logs(jobName, "Quantity IsNullOrEmpty: " + item.ArticleNumber);
                        }
                        var itemMaster = itemDrwDto.Where(x => x.ItemNo == item.ArticleNumber).FirstOrDefault();
                        if (itemMaster == null)
                        {
                            FileHelper.Write2Logs(jobName, "NotFound: " + item.ArticleNumber);
                        }
                        decimal qty = decimal.Parse(!string.IsNullOrEmpty(item.Quantity) ? item.Quantity.Replace(" ", "") : "0");
                        lstData.Add(new M_ton_kho()
                        {
                            Ma_sap = item.Site ?? "",
                            Ma_thuoc = item.ArticleNumber,
                            Don_vi_tinh = itemMaster.BaseUnit,
                            So_luong_ton_dau = (int)qty,
                            So_luong_xuat = 0,
                            So_luong_ton = (int)qty,
                            So_lo = item.Batch,
                            Han_dung = item.SLED,
                            Loai_phieu = "N",
                            Ma_phieu = "",
                            Ngay_xuat = DateTime.Now.AddDays(-1).Date,
                            Priority = int.Parse(!string.IsNullOrEmpty(item.Priority) ? item.Priority.Trim() : "0"),
                            IsProcess = false,
                            Thoi_gian = DateTime.Now
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(jobName, "ReadXmlStockData Exception: " + ex.Message.ToString());
            }
        }
    }
}
