using System.Linq;
using OpenAuth.App.Interface;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository;
using OpenAuth.Repository.Domain.Material;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Infrastructure.HuaweiOBS;

namespace OpenAuth.App.Material
{
    public class ZWJAndXwjMGMTApp: OnlyUnitWorkBaeApp
    {
        public ZWJAndXwjMGMTApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
        }

        #region 中位机版本管理
        /// <summary>
        /// 是否存在中位机默认版本
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> IsExistsDefaultZWJVersion()
        {
            var result = new TableData();

            var data = await UnitWork.Find<ZWJSoftwareVersion>(null).FirstOrDefaultAsync(x => x.DefaultVersion == true);
            result.Data = data?.Id;

            return result;
        }

        /// <summary>
        /// 新增中位机软件版本记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> AddZWJSoftwareVersion(AddOrUpdateZWJSoftwareInfoReq req)
        {
            var result = new TableData();
            //中位机软件版本和中位机硬件为一对多的关系,即一个软件版本可适配多个硬件
            //中位机软件
            var zwjObj = new ZWJSoftwareVersion()
            {
                ProjectName = req.ProjectName,
                DefaultVersion = req.DefaultVersion,
                ZWJSoftwareVersionName = req.ZWJSoftwareVersionName,
                Remark = req.Remark,
                CreateUser = _auth.GetCurrentUser()?.User?.Id ?? "",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                FilePath = req.FilePath,
                FileName = req.FileName,
            };
            //中位机硬件
            var zwjHardwares = new List<ZWJHardware>();
            req.ZWJSns?.ForEach(x =>
                zwjHardwares.Add(new ZWJHardware { ZWJSn = x })
            );

            //开启事务
            using var tran = UnitWork.GetDbContext<ZWJSoftwareVersion>().Database.BeginTransaction();
            try
            {
                //如果是默认版本,则覆盖之前的版本(默认版本只能有一个)
                if (zwjObj.DefaultVersion)
                {
                    UnitWork.Update<ZWJSoftwareVersion>(x => x.DefaultVersion == true, e => new ZWJSoftwareVersion
                    {
                        DefaultVersion = false
                    });
                }

                //新增软件版本
                await UnitWork.AddAsync(zwjObj);
                await UnitWork.SaveAsync();

                //将硬件和软件对应起来
                zwjHardwares.All(x =>
                {
                    x.ZWJSoftwareVersionId = zwjObj.Id;
                    return true;
                });

                await UnitWork.BatchAddAsync(zwjHardwares.ToArray());
                await UnitWork.SaveAsync();

                await tran.CommitAsync();
            }
            catch(Exception ex)
            {
                await tran.RollbackAsync();
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// 查询中位机软件版本记录
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetZWJSoftwareVersions(QueryZWJSoftwareListReq req)
        {
            var result = new TableData();
            var data = UnitWork.Find<ZWJSoftwareVersion>(null)
                        .WhereIf(!string.IsNullOrWhiteSpace(req.ZWJSoftwareVersionName), x => x.ZWJSoftwareVersionName == req.ZWJSoftwareVersionName)
                        .WhereIf(!string.IsNullOrWhiteSpace(req.ZWJSn), x => x.ZWJHardwares.Any(h => h.ZWJSn == req.ZWJSn))
                        .Select(x => new
                        {
                            x.Id,
                            x.ProjectName,
                            x.ZWJSoftwareVersionName,
                            x.DefaultVersion,
                            x.FilePath,
                            x.FileName,
                            Count = x.ZWJHardwares.Count(),
                            x.Remark,
                            x.CreateTime,
                            Sns = x.ZWJHardwares.Select(x => x.ZWJSn)
                        });

            result.Data = await data.OrderByDescending(d => d.CreateTime).Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
            result.Count = await data.CountAsync();

            return result;
        }

        /// <summary>
        /// 修改中位机软件版本记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> UpdateZWJSoftwareVersion(AddOrUpdateZWJSoftwareInfoReq req)
        {
            var result = new TableData();
            var softwareObj = await UnitWork.Find<ZWJSoftwareVersion>(null).Include(x => x.ZWJHardwares).FirstOrDefaultAsync(x => x.Id == req.Id);
            if (softwareObj == null)
            {
                throw new Exception("中位机软件信息不存在");
            }
            var sns = softwareObj?.ZWJHardwares.Select(x => x.ZWJSn); //数据库表里面的硬件序列号
            var addSns = new List<string>(); //硬件序列号中要新增的记录
            var deleteSns = new List<string>(); //硬件序列号中要删除的记录
            if (req.ZWJSns != null)
            {
                addSns = req.ZWJSns.Except(sns).ToList(); 
                deleteSns = sns.Except(req.ZWJSns).ToList(); 
            }

            softwareObj.ProjectName = req.ProjectName;
            softwareObj.DefaultVersion = req.DefaultVersion;
            softwareObj.ZWJSoftwareVersionName = req.ZWJSoftwareVersionName;
            softwareObj.Remark = req.Remark;
            softwareObj.UpdateTime = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(req.FilePath))
            {
                softwareObj.FilePath = req.FilePath;
                softwareObj.FileName = req.FileName;
            }
            //开启事务
            using var tran = UnitWork.GetDbContext<ZWJSoftwareVersion>().Database.BeginTransaction();
            try
            {
                //如果是默认版本,则覆盖之前的版本(默认版本只能有一个)
                if (req.DefaultVersion)
                {
                    UnitWork.Update<ZWJSoftwareVersion>(x => x.DefaultVersion == true, e => new ZWJSoftwareVersion
                    {
                        DefaultVersion = false
                    });
                }

                //更新软件信息
                await UnitWork.UpdateAsync<ZWJSoftwareVersion>(z => z.Id == req.Id, e => new ZWJSoftwareVersion
                {
                    ProjectName = softwareObj.ProjectName,
                    DefaultVersion = softwareObj.DefaultVersion,
                    ZWJSoftwareVersionName = softwareObj.ZWJSoftwareVersionName,
                    Remark = softwareObj.Remark,
                    UpdateTime = softwareObj.UpdateTime,
                    FilePath = softwareObj.FilePath,
                    FileName = softwareObj.FileName,
                });

                //更新对应的硬件序列号信息
                if (addSns.Count() > 0)
                {
                    var zwjHardwares = addSns.Select(x => new ZWJHardware
                    {
                        ZWJSoftwareVersionId = req.Id,
                        ZWJSn = x
                    });
                    await UnitWork.BatchAddAsync<ZWJHardware>(zwjHardwares.ToArray());
                }

                if (deleteSns.Count() > 0) 
                {
                    UnitWork.Delete<ZWJHardware>(x => deleteSns.Contains(x.ZWJSn));
                }

                await UnitWork.SaveAsync();
                await tran.CommitAsync();
            }
            catch(Exception ex)
            {
                await tran.RollbackAsync();
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// 删除中位机版本记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TableData> DeleteZWJSoftwareVersion(int id)
        {
            var result = new TableData();

            var softwareObj = await UnitWork.Find<ZWJSoftwareVersion>(null).Include(x => x.ZWJHardwares).FirstOrDefaultAsync(x => x.Id == id);
            var sns = softwareObj?.ZWJHardwares;
            if (sns != null && sns.Count() > 0)
            {
                throw new Exception("该程序版本仍有硬件使用.");
            }
            //开启事务
            using var tran = UnitWork.GetDbContext<ZWJSoftwareVersion>().Database.BeginTransaction();
            try
            {
                await UnitWork.DeleteAsync<ZWJSoftwareVersion>(x => x.Id == id);
                await UnitWork.DeleteAsync<ZWJHardware>(x => x.ZWJSoftwareVersionId == id);
                
                await UnitWork.SaveAsync();
                if (!string.IsNullOrWhiteSpace(softwareObj?.FileName))
                {
                    //删除文件
                    new HuaweiOBSHelper().DeleteObject(softwareObj?.FileName, null);
                }

                await tran.CommitAsync();
            }
            catch(Exception ex)
            {
                await tran.RollbackAsync();
                throw ex;
            }

            return result;
        }

        #endregion

        #region 下位机版本管理

        /// <summary>
        /// 新增下位机软件版本记录
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> AddXWJSoftwareVersion(AddOrUpdateXWJSoftwareInfoReq req)
        {
            var result = new TableData();

            if(UnitWork.Find<XWJSoftwareVersion>(null).Any(x=>x.Alias == req.Alias))
            {
                throw new Exception("此别名已存在");
            }

            //下位机软件信息
            var xwjObject = new XWJSoftwareVersion
            {
                Alias = req.Alias,
                XWJSoftwareVersionName = req.XWJSoftwareVersionName,
                FilePath = req.FilePath,
                FileName = req.FileName,
                Remark = req.Remark,
                CreateUser = _auth.GetCurrentUser()?.User?.Id ?? "",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
            };

            await UnitWork.AddAsync(xwjObject);
            await UnitWork.SaveAsync();

            return result;
        }

        /// <summary>
        /// 查询下位机软件版本记录
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetXWJSoftwareVersions(QueryXWJSoftwareListReq req)
        {
            var result = new TableData();
            //根据条件查询下位机软件版本记录
            var data = UnitWork.Find<XWJSoftwareVersion>(null)
                       .WhereIf(!string.IsNullOrWhiteSpace(req.Alias), x => x.Alias == req.Alias)
                       .WhereIf(!string.IsNullOrWhiteSpace(req.XWJSoftwareVersionName), x => x.XWJSoftwareVersionName == req.XWJSoftwareVersionName)
                       .Select(x => new
                       {
                           x.Id,
                           x.Alias,
                           x.XWJSoftwareVersionName,
                           x.FilePath,
                           x.FileName,
                           x.Remark,
                           x.CreateTime
                       });

            result.Data = await data.OrderByDescending(d => d.CreateTime).Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
            result.Count = await data.CountAsync();

            return result;
        }

        /// <summary>
        /// 修改下位机软件版本记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> UpdateXWJSoftwareVersion(AddOrUpdateXWJSoftwareInfoReq req)
        {
            var result = new TableData();

            var xwjSoftware = await UnitWork.Find<XWJSoftwareVersion>(null).FirstOrDefaultAsync(x => x.Id == req.Id);
            if(xwjSoftware == null)
            {
                throw new Exception("下位机软件信息不存在");
            }
            //判断原有别名是否被使用
            var existsXWJHardware = (from s in UnitWork.Find<XWJSoftwareVersion>(null)
                                     join h in UnitWork.Find<XWJHardware>(null)
                                     on s.Alias equals h.Alias
                                     where s.Id == req.Id
                                     select s.Id).Any();
            if (existsXWJHardware && xwjSoftware.Alias != req.Alias)
            {
                throw new Exception("该程序版本原有别名仍有硬件使用.");
            }

            if (UnitWork.Find<XWJSoftwareVersion>(null).Any(x => x.Id != req.Id && x.Alias == req.Alias))
            {
                throw new Exception("此别名已存在");
            }

            xwjSoftware.Alias = req.Alias;
            xwjSoftware.XWJSoftwareVersionName = req.XWJSoftwareVersionName;
            xwjSoftware.Remark = req.Remark;
            xwjSoftware.UpdateTime = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(req.FilePath))
            {
                xwjSoftware.FilePath = req.FilePath;
                xwjSoftware.FileName = req.FileName;
            }

            //根据主键修改信息
            await UnitWork.UpdateAsync<XWJSoftwareVersion>(x => x.Id == req.Id, e => new XWJSoftwareVersion
            {
                Alias = xwjSoftware.Alias,
                XWJSoftwareVersionName = xwjSoftware.XWJSoftwareVersionName,
                Remark = xwjSoftware.Remark,
                UpdateTime = xwjSoftware.UpdateTime,
                FilePath = xwjSoftware.FilePath,
                FileName = xwjSoftware.FileName,
            });

            await UnitWork.SaveAsync();

            return result;
        }

        /// <summary>
        /// 删除下位机软件版本记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TableData> DeleteXWJSoftwareVersion(int id)
        {
            var result = new TableData();
            var existsXWJHardware = await (from s in UnitWork.Find<XWJSoftwareVersion>(null)
                                           join h in UnitWork.Find<XWJHardware>(null)
                                           on s.Alias equals h.Alias
                                           where s.Id == id
                                           select s.FileName).AnyAsync();
            if (existsXWJHardware)
            {
                throw new Exception("该程序版本仍有硬件使用.");
            }

            var xwjVersion = await UnitWork.Find<XWJSoftwareVersion>(null).FirstOrDefaultAsync(x => x.Id == id);

            //开启事务
            using var tran = UnitWork.GetDbContext<ZWJSoftwareVersion>().Database.BeginTransaction();
            try
            {
                await UnitWork.DeleteAsync<XWJSoftwareVersion>(x => x.Id == id);
                await UnitWork.SaveAsync();
                if (!string.IsNullOrWhiteSpace(xwjVersion?.FileName))
                {
                    //删除文件
                    new HuaweiOBSHelper().DeleteObject(xwjVersion?.FileName, null);
                }

                await tran.CommitAsync();
            }
            catch(Exception ex)
            {
                await tran.RollbackAsync();
                throw ex;
            }

            return result;
        }

        #endregion

        #region 下位机版本映射

        /// <summary>
        /// 获取下位机软件版本别名
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetXWJSoftwareVersionAlias()
        {
            var result = new TableData();

            var data = UnitWork.Find<XWJSoftwareVersion>(null).OrderByDescending(x => x.CreateTime).Select(x => x.Alias).Distinct();

            result.Data = data;
            result.Count = await data.CountAsync();

            return result;
        }

        /// <summary>
        /// 新增下位机版本映射
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> AddXWJHardwareMap(AddOrUpdateXWJMapReq req)
        {
            var result = new TableData();

            var xwjHardwareMap = new XWJHardware
            {
                XWJSn = req.XWJSn,
                Alias = req.Alias,
                AliasEn = req.AliasEn ?? "",
                Remark = req.Remark,
                CreateUser = _auth.GetCurrentUser()?.User?.Id ?? "",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now
            };

            await UnitWork.AddAsync(xwjHardwareMap);
            await UnitWork.SaveAsync();

            return result;
        }

        /// <summary>
        /// 查询下位机版本映射记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetXWJHarewareMaps(QueryXWJHarewareListReq req)
        {
            var result = new TableData();

            var data = from h in UnitWork.Find<XWJHardware>(null)
                       .WhereIf(!string.IsNullOrWhiteSpace(req.XWJSn), x => x.XWJSn == req.XWJSn)
                       join s in UnitWork.Find<XWJSoftwareVersion>(null)
                       .WhereIf(!string.IsNullOrWhiteSpace(req.XWJSoftwareVersionName), x => x.XWJSoftwareVersionName == req.XWJSoftwareVersionName)
                       on h.Alias equals s.Alias
                       join e in UnitWork.Find<XWJSoftwareVersion>(null)
                       on h.AliasEn equals e.Alias into temp
                       from t in temp.DefaultIfEmpty()
                       select new
                       {
                           h.XWJSn,
                           s.XWJSoftwareVersionName,
                           s.Alias,
                           XWJSoftwareVersionNameEn = t == null ? "" : t.XWJSoftwareVersionName,
                           AliasEn = t == null ? "" : t.Alias,
                           h.Remark,
                           h.CreateTime,
                           h.Id
                       };

            result.Data = await data.OrderByDescending(d => d.CreateTime).Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
            result.Count = await data.CountAsync();

            return result;
        }

        /// <summary>
        /// 修改下位机版本映射
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> UpdateXWJHarewareMap(AddOrUpdateXWJMapReq req)
        {
            var result = new TableData();

            await UnitWork.UpdateAsync<XWJHardware>(x => x.Id == req.Id, e => new XWJHardware
            {
                XWJSn = req.XWJSn,
                Alias = req.Alias,
                AliasEn = req.AliasEn ?? "",
                Remark = req.Remark,
                UpdateTime = DateTime.Now
            });

            await UnitWork.SaveAsync();

            return result;
        }

        /// <summary>
        /// 删除下位机版本映射
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TableData> DeleteXWJHarewareMap(int id)
        {
            var result = new TableData();

            await UnitWork.DeleteAsync<XWJHardware>(x => x.Id == id);
            await UnitWork.SaveAsync();

            return result;
        }
        #endregion

        #region 临时版本管理
        /// <summary>
        /// 获取中位机程序版本
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetZWJSoftwareVersionNames()
        {
            var result = new TableData();

            var data = UnitWork.Find<ZWJSoftwareVersion>(null)
                        .OrderByDescending(x => x.CreateTime)
                        .Select(x => new { x.Id, x.ZWJSoftwareVersionName });

            result.Data = await data.ToListAsync();
            result.Count = await data.CountAsync();

            return result;
        }

        /// <summary>
        /// 获取下位机程序版本
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetXWJSoftwareVersionNames()
        {
            var result = new TableData();

            var data = UnitWork.Find<XWJSoftwareVersion>(null)
                        .OrderByDescending(x => x.CreateTime)
                        .Select(x => new { x.Id, x.Alias });
            result.Data = await data.ToListAsync();
            result.Count = await data.CountAsync();

            return result;
        }

        /// <summary>
        /// 新增临时版本管理记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> AddTempVersion(AddOrUpdateTempVersionReq req)
        {
            var result = new TableData();

            var tempObject = new TempVersion
            {
                ContractNo = req.ContractNo,
                HardwareType = req.HardwareType,
                SoftwareVersionId = req.SoftwareVersionId,
                Remark = req.Remark,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
            };

            await UnitWork.AddAsync(tempObject);
            await UnitWork.SaveAsync();

            return result;
        }

        /// <summary>
        /// 查询临时版本记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetTempVesionInfo(QueryTempVersionReq req)
        {
            var result = new TableData();

            var query1 = from t in UnitWork.Find<TempVersion>(null)
                         join z in UnitWork.Find<ZWJSoftwareVersion>(null)
                         on t.SoftwareVersionId equals z.Id
                         where t.HardwareType == "中位机"
                         select new
                         {
                             t.Id,
                             t.ContractNo,
                             t.HardwareType,
                             VersionId = z.Id,
                             VersionName = z.ZWJSoftwareVersionName,
                             VersionRemark = z.Remark,
                             t.Remark,
                             t.CreateTime
                         };

            var query2 = from t in UnitWork.Find<TempVersion>(null)
                         join x in UnitWork.Find<XWJSoftwareVersion>(null)
                         on t.SoftwareVersionId equals x.Id
                         where t.HardwareType == "下位机"
                         select new
                         {
                             t.Id,
                             t.ContractNo,
                             t.HardwareType,
                             VersionId = x.Id,
                             VersionName = x.XWJSoftwareVersionName,
                             VersionRemark = x.Remark,
                             t.Remark,
                             t.CreateTime
                         };
            var query3 = query1.Concat(query2);

            result.Data = await query3.OrderByDescending(x => x.CreateTime).Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
            result.Count = await query3.CountAsync();

            return result;
        }

        /// <summary>
        /// 修改临时版本记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> UpdateTempVersion(AddOrUpdateTempVersionReq req)
        {
            var result = new TableData();

            await UnitWork.UpdateAsync<TempVersion>(x => x.Id == req.Id, e => new TempVersion
            {
                ContractNo = req.ContractNo,
                HardwareType = req.HardwareType,
                SoftwareVersionId = req.SoftwareVersionId,
                Remark = req.Remark,
                UpdateTime = DateTime.Now
            });
            await UnitWork.SaveAsync();

            return result;
        }

        /// <summary>
        /// 删除临时版本记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TableData> DeleteTempVersion(int id)
        {
            var result = new TableData();

            await UnitWork.DeleteAsync<TempVersion>(x => x.Id == id);
            await UnitWork.SaveAsync();

            return result;
        }
        #endregion
    }
}
