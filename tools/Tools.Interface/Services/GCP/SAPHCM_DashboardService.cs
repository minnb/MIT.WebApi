using Newtonsoft.Json;
using PhucLong.Interface.Central.Models.Master.SAP;
using PhucLong.Interface.Central.Models.Master;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using VCM.Common.Helpers;
using Tools.Interface.Models.GCP.SAPHCM;
using PhucLong.Interface.Central.AppService;
using PhucLong.Interface.Central.Database;
using Microsoft.Extensions.Configuration;
using VCM.Common.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Data;
using WCM.EntityFrameworkCore.EntityFrameworkCore.Dapper;

namespace Tools.Interface.Services.GCP
{
    public class SAPHCM_DashboardService
    {
        private VCM.Common.Database.DapperContext _dbContext;
        public SAPHCM_DashboardService()
        {
        }

        public void Save_SAPHCM_Dashboard(string jobtype, string connectString, string localPath, int number_of_process, bool isMoveFile = true)
        {
            try
            {
                List<SAPHCM_Dashboard_Dto> lstData = new List<SAPHCM_Dashboard_Dto>();
                var lstFile = FileHelper.GetFileFromDir(localPath, "*.xml");
                var count = 0;
                foreach (string file in lstFile)
                {
                    if (file.ToUpper().Contains(jobtype.ToUpper()))
                    {
                        ReadXmlData(jobtype, localPath + file, ref lstData);
                        FileHelper.WriteLogs(JsonConvert.SerializeObject(lstData));
                        count += 1;
                        if (count == number_of_process)
                        {
                            break;
                        }
                    }
                }

                FileHelper.WriteLogs("SAPHCM_Dashboard Result: " + lstData.Count.ToString());
                if (lstData.Count > 0)
                {
                    _dbContext = new VCM.Common.Database.DapperContext(connectString);
                    bool flg = false;
                    using IDbConnection conn = _dbContext.CreateConnDB;
                    using IDbTransaction transaction = conn.BeginTransaction();
                    try
                    {


                        transaction.Commit();
                        flg = true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        FileHelper.WriteLogs("SAPHCM_Dashboard.Database.BeginTransaction: " + ex.Message.ToString());
                    }
                    if (flg)
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("Save_SAPHCM_Dashboard Exception " + ex.Message.ToString());
            }
        }
        private void ReadXmlData(string jobtype, string fileName, ref List<SAPHCM_Dashboard_Dto> lstData)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(fileName);
                var xml = FileHelper.ReadXml(fileName);
                XmlNodeList nodeListCount = xml.GetElementsByTagName("n0:MT_SAPHR_Dashboard");
                if (nodeListCount.Count >= 1)
                {
                    List<SAPHCM_Dashboard_Dto> resultData = new List<SAPHCM_Dashboard_Dto>();

                    var result = JsonConvert.DeserializeObject<SAPHCM_Dashboard_Xml>(JsonConvert.SerializeObject(nodeListCount[0]));
                    if (result.MT_SAPHR_Dashboard != null && result.MT_SAPHR_Dashboard.Items.Count > 0)
                    {
                        foreach(var item in result.MT_SAPHR_Dashboard.Items)
                        {
                            item.XmlnsPrx = result.MT_SAPHR_Dashboard.Items.FirstOrDefault().XmlnsPrx;
                            resultData.Add(item);
                        }
                    }
                    else
                    {
                        FileHelper.Write2Logs(jobtype, "ItemMasterService.ReadXmlData.ERROR: " + JsonConvert.SerializeObject(result));
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(jobtype,"ItemMasterService.ReadXmlData Exception " + ex.Message.ToString());
            }
        }
    }
}
