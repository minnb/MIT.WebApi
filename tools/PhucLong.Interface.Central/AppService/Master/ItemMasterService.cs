using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PhucLong.Interface.Central.Database;
using PhucLong.Interface.Central.Models.Master;
using PhucLong.Interface.Central.Models.Master.SAP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using VCM.Common.Helpers;

namespace PhucLong.Interface.Central.AppService
{
    public class ItemMasterService
    {
        private IConfiguration _config;
        private CentralDbContext _dbContext;
        private LoggingService _loggingDb;
        public ItemMasterService
          (
              IConfiguration config,
              CentralDbContext dbContext
          )
        {
            _config = config;
            _dbContext = dbContext;
            _loggingDb = new LoggingService(_config);
        }
        public void SaveItemMaster(string localPath, string backupPath, int number_of_process,bool isMoveFile, string procedures)
        {
            try
            {
                List<ItemMaster> lstData = new List<ItemMaster>();
                var lstFile = FileHelper.GetFileFromDir(localPath, "*.xml");
                var count = 0;
                foreach (string file in lstFile)
                {
                    ReadXmlData(localPath + file, ref lstData);
                    count += 1;
                    if (count == number_of_process)
                    {
                        break;
                    }
                }
                FileHelper.WriteLogs("Result: "  + lstData.Count.ToString());
                if(lstData.Count > 0)
                {
                    List<Temp_Item> temp_Items = new List<Temp_Item>();
                    foreach(var item in lstData)
                    {
                        temp_Items.Add(new Temp_Item()
                        {
                            No = item.No,
                            Description = item.Description??"",
                            LongDescription = item.LongDescription??"",
                            BaseUnitOfMeasure = item.BaseUnitOfMeasure??"EA",
                            TaxGroupCode = item.TaxGroupCode??"00",
                            MCH = item.MCH ?? "",
                            SIZE_DIM = item.SIZE_DIM ?? "",
                            BASIC_MATL = item.BASIC_MATL ?? "",
                            Blocked = item.Blocked,
                            UpdateFlg = "N",
                            Id = Guid.NewGuid().ToString(),
                            FileIdoc = item.FileIdoc
                        });
                    }
                   
                    if(temp_Items.Count > 0)
                    {
                        bool flg = false;
                        using var transaction = _dbContext.Database.BeginTransaction();
                        try
                        {

                            temp_Items.ForEach(n => _dbContext.Temp_Item.Add(n));
                            //FileHelper.WriteLogs(JsonConvert.SerializeObject(temp_Items));
                            _dbContext.SaveChanges();
                            transaction.Commit();
                            flg = true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            FileHelper.WriteLogs("SaveItemMaster._dbContext.Database.BeginTransaction: " + ex.Message.ToString());
                        }
                        if (flg)
                        {
                            foreach(var item in temp_Items)
                            {
                                if (isMoveFile) FileHelper.MoveFileToFolder(backupPath, localPath + item.FileIdoc);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("ItemMasterService.SaveItemMaster Exception " + ex.Message.ToString());
            }
        }
        private void ReadXmlData(string fileName, ref List<ItemMaster> lstData)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(fileName);
                var xml = FileHelper.ReadXml(fileName);
                XmlNodeList nodeListCount = xml.GetElementsByTagName("ns0:mt_cx_item");               
                if (nodeListCount.Count == 1)
                {
                    var result = JsonConvert.DeserializeObject<ItemXml>(JsonConvert.SerializeXmlNode(xml)).mt_cx_item;
                    if (result.Data != null)
                    {
                        lstData.Add(new ItemMaster()
                        {
                            No = result.Data.No,
                            Description = result.Data.Description,
                            LongDescription = result.Data.LongDescription,
                            BaseUnitOfMeasure = result.Data.BaseUnitOfMeasure,
                            TaxGroupCode = result.Data.TaxGroupCode,
                            MCH = result.Data.MCH,
                            SIZE_DIM = result.Data.SIZE_DIM??"",
                            BASIC_MATL = result.Data.BASIC_MATL??"",
                            Blocked = result.Data.Blocked,
                            FileIdoc = fileInfo.Name.ToString()
                        });
                    }
                }
                else if (nodeListCount.Count > 1)
                {
                    var result = JsonConvert.DeserializeObject<ItemXmlList>(JsonConvert.SerializeXmlNode(xml));
                    if (result.Data.Count > 0)
                    {
                        foreach (var item in result.Data)
                        {
                            lstData.Add(new ItemMaster()
                            {
                                No = item.No,
                                Description = item.Description,
                                LongDescription = item.LongDescription,
                                BaseUnitOfMeasure = item.BaseUnitOfMeasure,
                                TaxGroupCode = item.TaxGroupCode,
                                MCH = item.MCH,
                                SIZE_DIM = item.SIZE_DIM,
                                BASIC_MATL = item.BASIC_MATL,
                                Blocked = item.Blocked,
                                FileIdoc = fileName
                            });
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                 FileHelper.WriteLogs("ItemMasterService.ReadXmlData Exception " + ex.Message.ToString());
            }

        }
    }
}
