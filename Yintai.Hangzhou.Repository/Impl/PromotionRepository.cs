﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using Yintai.Architecture.Common.Data.EF;
using Yintai.Architecture.Common.Models;
using Yintai.Hangzhou.Data.Models;
using Yintai.Hangzhou.Model.Enums;
using Yintai.Hangzhou.Repository.Contract;

namespace Yintai.Hangzhou.Repository.Impl
{
    public class PromotionRepository : RepositoryBase<PromotionEntity, int>, IPromotionRepository
    {
        #region Overrides of RepositoryBase<PromotionEntity,int>

        /// <summary>
        /// 查找key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override PromotionEntity GetItem(int key)
        {
            return base.Find(key);
        }

        #endregion

        #region methods

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="sort"></param>
        /// <returns></returns>
        private static Func<IQueryable<PromotionEntity>, IOrderedQueryable<PromotionEntity>> OrderBy(PromotionSortOrder sort)
        {
            Func<IQueryable<PromotionEntity>, IOrderedQueryable<PromotionEntity>> orderBy = null;
            switch (sort)
            {
                case PromotionSortOrder.CreatedDateDesc:
                    orderBy = v => v.OrderByDescending(s => s.CreatedDate);
                    break;
                case PromotionSortOrder.Near:
                    break;
                case PromotionSortOrder.Hot://最热
                    orderBy = v => v.OrderByDescending(s => s.LikeCount);
                    break;
                //最新
                case PromotionSortOrder.New:
                    orderBy = v => v.OrderByDescending(s => s.StartDate);
                    break;
                case PromotionSortOrder.StartAsc:
                    orderBy = v => v.OrderBy(s => s.StartDate);
                    break;
                case PromotionSortOrder.StartDesc:
                case PromotionSortOrder.Default:
                default:
                    orderBy = v => v.OrderByDescending(s => s.StartDate);
                    break;
            }

            return orderBy;
        }

        /// <summary>
        /// 过滤 今天开始的活动
        /// </summary>
        /// <param name="dataStatus">数据状态</param>
        /// <param name="rangeInfo">时间范围</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="recommendUser"></param>
        /// <returns></returns>
        private static Expression<Func<PromotionEntity, bool>> Filter2(DataStatus? dataStatus, DateTimeRangeInfo rangeInfo, Timestamp timestamp, int? recommendUser)
        {

            /*查询逻辑
 * 1.今天开始的活动
 * 2.以前开始，今天还自进行的活动
 * 3.即将开始的活动，时间升序 24小时内的
 * 
 * logic 例 size40 
 */

            //2
            var filter = Filter(dataStatus, rangeInfo, timestamp, recommendUser);
            var start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            filter = filter.And(v => v.StartDate < start);

            return filter;
        }

        /// <summary>
        /// 过滤 今天开始的活动
        /// </summary>
        /// <param name="dataStatus">数据状态</param>
        /// <param name="rangeInfo">时间范围</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="recommendUser"></param>
        /// <returns></returns>
        private static Expression<Func<PromotionEntity, bool>> Filter3(DataStatus? dataStatus, DateTimeRangeInfo rangeInfo, Timestamp timestamp, int? recommendUser)
        {

            /*查询逻辑
 * 1.今天开始的活动
 * 2.以前开始，今天还自进行的活动
 * 3.即将开始的活动，时间升序 24小时内的
 * 
 * logic 例 size40 
 */
            //3

            var start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(1);

            var filter = Filter(dataStatus, rangeInfo, timestamp, recommendUser);

            filter = filter.And(v => v.StartDate >= start);


            return filter;
        }

        /// <summary>
        /// 过滤 今天开始的活动
        /// </summary>
        /// <param name="dataStatus">数据状态</param>
        /// <param name="rangeInfo">时间范围</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="recommendUser"></param>
        /// <returns></returns>
        private static Expression<Func<PromotionEntity, bool>> Filter1(DataStatus? dataStatus, DateTimeRangeInfo rangeInfo, Timestamp timestamp, int? recommendUser)
        {

            /*查询逻辑
 * 1.今天开始的活动
 * 2.以前开始，今天还自进行的活动
 * 3.即将开始的活动，时间升序 24小时内的
 * 
 * logic 例 size40 
 */

            var filter = Filter(dataStatus, rangeInfo, timestamp, recommendUser);
            var start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            var end = start.AddDays(1);

            filter = filter.And(v => v.StartDate >= start && v.StartDate <= end);


            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pagerRequest"></param>
        /// <param name="totalCount"></param>
        /// <param name="sortOrder"></param>
        /// <param name="dateTimeRangeInfo"></param>
        /// <param name="coordinateInfo"></param>
        /// <param name="timestamp"></param>
        /// <param name="recommendUser"></param>
        /// <returns></returns>
        private List<PromotionEntity> GetPagedListByNew(PagerRequest pagerRequest, out int totalCount, PromotionSortOrder sortOrder,
                                 DateTimeRangeInfo dateTimeRangeInfo, CoordinateInfo coordinateInfo, Timestamp timestamp, int? recommendUser)
        {
            /*查询逻辑
* 1.今天开始的活动
* 2.以前开始，今天还自进行的活动
* 3.即将开始的活动，时间升序 24小时内的
* 
* logic 例 size40 
*/

            var r = new DateTimeRangeInfo
                {
                    EndDateTime = DateTime.Now
                };

            var f1 = Filter1(DataStatus.Normal, r, timestamp, recommendUser);

            var f2 = Filter2(DataStatus.Normal, r, timestamp, recommendUser);

            var skip = (pagerRequest.PageIndex - 1) * pagerRequest.PageSize;

            var t = base.Get(f1)
            .Union(
                base.Get(f2));

            totalCount = t.Count();

            if (pagerRequest.PageSize == 1)
            {
                return t.OrderByDescending(v => v.StartDate).Take(pagerRequest.PageSize).ToList();
            }

            return t.OrderByDescending(v => v.StartDate).Skip(skip).Take(pagerRequest.PageSize).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skipCount"></param>
        /// <param name="pagerRequest"></param>
        /// <param name="totalCount"></param>
        /// <param name="sortOrder"></param>
        /// <param name="dateTimeRangeInfo"></param>
        /// <param name="coordinateInfo"></param>
        /// <param name="timestamp"></param>
        /// <param name="recommendUser"></param>
        /// <returns></returns>
        private List<PromotionEntity> GetPagedListByBeStart(int? skipCount, PagerRequest pagerRequest, out int totalCount, PromotionSortOrder sortOrder,
                         DateTimeRangeInfo dateTimeRangeInfo, CoordinateInfo coordinateInfo, Timestamp timestamp, int? recommendUser)
        {
            /*查询逻辑
* 1.今天开始的活动
* 2.以前开始，今天还自进行的活动
* 3.即将开始的活动，时间升序 24小时内的
* 
* logic 例 size40 
*/
            var f3 = Filter3(DataStatus.Normal, new DateTimeRangeInfo() { EndDateTime = DateTime.Now }, timestamp, recommendUser);


            if (skipCount == null)
            {
                return
               base.Get(f3, out totalCount, pagerRequest.PageIndex, pagerRequest.PageSize,
                        OrderBy(PromotionSortOrder.StartAsc)).ToList();
            }
            else
            {
                return
               base.Get(f3, out totalCount, pagerRequest.PageIndex, pagerRequest.PageSize,
                        OrderBy(PromotionSortOrder.StartAsc), skipCount.Value).ToList();
            }
        }


        /// <summary>
        /// 过滤
        /// </summary>
        /// <param name="dataStatus">数据状态</param>
        /// <param name="rangeInfo">时间范围</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="recommendUser"></param>
        /// <returns></returns>
        private static Expression<Func<PromotionEntity, bool>> Filter(DataStatus? dataStatus, DateTimeRangeInfo rangeInfo, Timestamp timestamp, int? recommendUser)
        {
            return Filter(dataStatus, rangeInfo, timestamp, recommendUser, null);
        }


        /// <summary>
        /// 过滤
        /// </summary>
        /// <param name="dataStatus">数据状态</param>
        /// <param name="rangeInfo">时间范围</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="recommendUser"></param>
        /// <param name="tagids"></param>
        /// <returns></returns>
        private static Expression<Func<PromotionEntity, bool>> Filter(DataStatus? dataStatus, DateTimeRangeInfo rangeInfo, Timestamp timestamp, int? recommendUser, List<int> tagids)
        {
            var filter = PredicateBuilder.True<PromotionEntity>();

            if (dataStatus != null)
            {
                filter = filter.And(v => v.Status == (int)dataStatus.Value);
            }

            //TODO: 这块值得商榷
            if (rangeInfo != null)
            {
                //开始时间必须大于当前时间，才叫开始
                //if (rangeInfo.StartDateTime != null)
                //{
                //    filter = filter.And(v => v.StartDate >= rangeInfo.StartDateTime);
                //}

                if (rangeInfo.EndDateTime != null)
                {
                    filter = filter.And(v => v.EndDate > rangeInfo.EndDateTime);
                }
            }

            if (timestamp != null)
            {
                switch (timestamp.TsType)
                {
                    case TimestampType.New:
                        filter = filter.And(v => v.UpdatedDate >= timestamp.Ts);
                        break;
                    case TimestampType.Old:
                    default:
                        filter = filter.And(v => v.UpdatedDate < timestamp.Ts);
                        break;
                }
            }

            if (recommendUser != null)
            {
                filter = filter.And(v => v.RecommendUser == recommendUser.Value);
            }

            if (tagids != null && tagids.Count > 0)
            {
                if (tagids.Count == 1)
                {
                    filter = filter.And(v => v.Tag_Id == tagids[0]);
                }
                else
                {
                    filter = filter.Or(v => tagids.Any(s => s == v.Tag_Id));
                }
            }

            return filter;
        }


        #endregion

        #region Implementation of IPromotionRepository

        public List<PromotionEntity> GetPagedList(PagerRequest pagerRequest, out int totalCount, PromotionSortOrder sortOrder,
                                 DateTimeRangeInfo dateTimeRangeInfo, CoordinateInfo coordinateInfo, Timestamp timestamp,
                                 int? recommendUser, PromotionFilterMode filterMode, int? specialSkipCount)
        {
            if (specialSkipCount == null || specialSkipCount.Value == 0 || filterMode != PromotionFilterMode.BeginStart)
            {
                return GetPagedList(pagerRequest, out totalCount, sortOrder, dateTimeRangeInfo, coordinateInfo,
                                    timestamp, recommendUser, filterMode);
            }

            var skipCount = specialSkipCount.Value;

            return GetPagedListByBeStart(skipCount,
                                         pagerRequest, out totalCount, sortOrder, dateTimeRangeInfo, coordinateInfo,
                                         timestamp, recommendUser);
        }

        public List<PromotionEntity> GetPagedList(PagerRequest pagerRequest, out int totalCount,
                                                  PromotionSortOrder sortOrder,
                                                  DateTimeRangeInfo dateTimeRangeInfo, CoordinateInfo coordinateInfo,
                                                  Timestamp timestamp, int? recommendUser)
        {
            return GetPagedList(pagerRequest, out totalCount, sortOrder, dateTimeRangeInfo, coordinateInfo, timestamp,
                                recommendUser, PromotionFilterMode.Default);
        }

        public List<PromotionEntity> GetPagedListForSearch(PagerRequest pagerRequest, out int totalCount, PromotionSortOrder sortOrder,
                                          DateTimeRangeInfo dateTimeRangeInfo, CoordinateInfo coordinateInfo, Timestamp timestamp,
                                          int? recommendUser, PromotionFilterMode? filterMode, string promotionName, List<int> tagids,
                                          int? brandId)
        {
            var filter = Filter(DataStatus.Normal, null, timestamp, recommendUser, tagids);
            if (!String.IsNullOrWhiteSpace(promotionName))
            {
                filter = filter.And(v => v.Name.StartsWith(promotionName));
            }

            return base.Get(filter, out totalCount, pagerRequest.PageIndex,
          pagerRequest.PageSize, OrderBy(sortOrder)).ToList();
        }

        public List<PromotionEntity> GetPagedList(PagerRequest pagerRequest, out int totalCount, PromotionSortOrder sortOrder,
                                 DateTimeRangeInfo dateTimeRangeInfo, CoordinateInfo coordinateInfo, Timestamp timestamp, int? recommendUser, PromotionFilterMode filterMode)
        {
            if (filterMode == PromotionFilterMode.New)
            {
                return GetPagedListByNew(pagerRequest, out totalCount, sortOrder, dateTimeRangeInfo, coordinateInfo,
                                         timestamp, recommendUser);
            }

            if (filterMode == PromotionFilterMode.BeginStart)
            {
                return GetPagedListByBeStart(null, pagerRequest, out totalCount, sortOrder, dateTimeRangeInfo, coordinateInfo,
                                         timestamp, recommendUser);
            }


            if (coordinateInfo != null)
            {
                //存储
                return GetPagedList(pagerRequest.PageIndex, pagerRequest.PageSize, out totalCount, (int)sortOrder,
                                    coordinateInfo.Longitude, coordinateInfo.Latitude, timestamp);
            }

            return
                base.Get(Filter(DataStatus.Normal, dateTimeRangeInfo, timestamp, recommendUser), out totalCount, pagerRequest.PageIndex,
                         pagerRequest.PageSize, OrderBy(sortOrder)).ToList();
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalCount"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public List<PromotionEntity> GetPagedList(int pageIndex, int pageSize, out int totalCount, int sort)
        {
            return GetPagedList(new PagerRequest(pageIndex, pageSize), out totalCount,
                                (PromotionSortOrder)sort, new DateTimeRangeInfo() { EndDateTime = DateTime.Now }, null,
                                null, null);

            //return base.Get(
            //    Filter(DataStatus.Normal, new DateTimeRangeInfo { StartDateTime = null, EndDateTime = DateTime.Now },
            //           null, null), out totalCount, pageIndex, pageSize, OrderBy(sort)).ToList();

            //return base.Get(v => v.Status == 1 && v.StartDate <= DateTime.Now && v.EndDate > DateTime.Now, out totalCount, pageIndex, pageSize, OrderBy(sort)).ToList();
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalCount"></param>
        /// <param name="sort"></param>
        /// <param name="timestamp">时间戳 </param>
        /// <returns></returns>
        public List<PromotionEntity> GetPagedList(int pageIndex, int pageSize, out int totalCount, int sort, Timestamp timestamp)
        {
            return GetPagedList(new PagerRequest(pageIndex, pageSize), out totalCount,
                    (PromotionSortOrder)sort, new DateTimeRangeInfo() { EndDateTime = DateTime.Now }, null,
                    timestamp, null);

            //        return base.Get(
            //Filter(DataStatus.Normal, new DateTimeRangeInfo { StartDateTime = null, EndDateTime = DateTime.Now },
            //       timestamp, null), out totalCount, pageIndex, pageSize, OrderBy(sort)).ToList();

            //if (timestamp == null)
            //{
            //    return GetPagedList(pageIndex, pageSize, out totalCount, sort);
            //}

            //switch (timestamp.TsType)
            //{
            //    case TimestampType.New:
            //        return base.Get(v => v.Status == 1 && v.StartDate <= DateTime.Now && v.EndDate > DateTime.Now && v.UpdatedDate >= timestamp.Ts, out totalCount, pageIndex, pageSize, OrderBy(sort)).ToList();
            //    case TimestampType.Old:
            //    default:
            //        return base.Get(v => v.Status == 1 && v.StartDate <= DateTime.Now && v.EndDate > DateTime.Now && v.UpdatedDate < timestamp.Ts, out totalCount, pageIndex, pageSize, OrderBy(sort)).ToList();
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalCount"></param>
        /// <param name="sort"></param>
        /// <param name="lng"></param>
        /// <param name="lat"></param>
        /// <param name="timestamp">时间戳 </param>
        /// <returns></returns>
        public List<PromotionEntity> GetPagedList(int pageIndex, int pageSize, out int totalCount, int sort, double lng, double lat, Timestamp timestamp)
        {
            if (timestamp == null)
            {
                return GetPagedList(pageIndex, pageSize, out totalCount, sort, lng, lat);
            }

            var output = new SqlParameter("@TotalCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var parames = new SqlParameter[7];
            var data = DateTime.Now;
            parames[0] = new SqlParameter("@Longitude", lng);
            parames[1] = new SqlParameter("@Latitude", lat);
            parames[2] = new SqlParameter("@StartDate", data);
            parames[3] = new SqlParameter("@PageSize", pageSize);
            parames[4] = new SqlParameter("@PageIndex", pageIndex);
            parames[5] = output;
            parames[6] = new SqlParameter("@TsDate", timestamp.Ts);


            var list = new List<PromotionEntity>(pageSize);
            using (var reader = SqlHelper.ExecuteReader(SqlHelper.GetConnection(), CommandType.StoredProcedure,
                                    "[dbo].[Promotion_GetPagedListByCoordinateAndTs]", parames))
            {
                while (reader.Read())
                {
                    var entity = ConvertEntity(reader);

                    list.Add(entity);
                }
            }

            totalCount = Int32.Parse(output.Value.ToString());

            return list;
        }

        public List<PromotionEntity> GetPagedList(int pageIndex, int pageSize, out int totalCount, int sort, double lng, double lat)
        {
            var parames = new SqlParameter[6];
            var data = DateTime.Now;
            parames[0] = new SqlParameter("@Longitude", lng);
            parames[1] = new SqlParameter("@Latitude", lat);
            parames[2] = new SqlParameter("@StartDate", data);
            parames[3] = new SqlParameter("@PageSize", pageSize);
            parames[4] = new SqlParameter("@PageIndex", pageIndex);
            parames[5] = new SqlParameter("@TotalCount", 0) { Direction = ParameterDirection.Output };

            var list = new List<PromotionEntity>(pageSize);

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.GetConnection(), CommandType.StoredProcedure,
                        "[dbo].[Promotion_GetPagedListByCoordinate]", parames))
            {

                while (reader.Read())
                {
                    var entity = ConvertEntity(reader);

                    list.Add(entity);
                }
            }

            totalCount = Int32.Parse(parames[5].Value.ToString());

            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="timestamp">时间戳 </param>
        /// <returns></returns>
        public List<PromotionEntity> GetList(int pageSize, int sort, Timestamp timestamp)
        {
            int totalCount;
            return GetPagedList(1, pageSize, out totalCount, sort, timestamp);

            //switch (timestamp.TsType)
            //{
            //    case TimestampType.New:
            //        return base.Get(v => v.Status == 1 && v.StartDate <= DateTime.Now && v.EndDate > DateTime.Now && v.UpdatedDate >= timestamp.Ts, OrderBy(sort)).Take(pageSize).ToList();
            //    case TimestampType.Old:
            //    default:
            //        return base.Get(v => v.Status == 1 && v.StartDate <= DateTime.Now && v.EndDate > DateTime.Now && v.UpdatedDate < timestamp.Ts, OrderBy(sort)).Take(pageSize).ToList();
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="lng"></param>
        /// <param name="lat"></param>
        /// <param name="timestamp">时间戳 </param>
        /// <returns></returns>
        public List<PromotionEntity> GetList(int pageSize, int sort, double lng, double lat, Timestamp timestamp)
        {
            var parames = new SqlParameter[6];
            var data = DateTime.Now;
            parames[0] = new SqlParameter("@Longitude", lng);
            parames[1] = new SqlParameter("@Latitude", lat);
            parames[2] = new SqlParameter("@StartDate", data);
            parames[3] = new SqlParameter("@PageSize", pageSize);
            parames[5] = new SqlParameter("@TsType", (int)timestamp.TsType);
            parames[4] = new SqlParameter("@TsDate", timestamp.Ts);

            var list = new List<PromotionEntity>(pageSize);

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.GetConnection(), CommandType.StoredProcedure,
                                    "[dbo].[Promotion_GetListByCoordinate]", parames))
            {
                while (reader.Read())
                {
                    var entity = ConvertEntity(reader);

                    list.Add(entity);
                }
            }

            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<PromotionEntity> GetList(List<int> ids)
        {
            return base.Get(v => ids.Any(s => s == v.Id) && v.Status == (int)DataStatus.Normal).ToList();
        }

        private static PromotionEntity ConvertEntity(IDataRecord record)
        {
            if (record == null)
            {
                return null;
            }

            var model = new PromotionEntity
                            {
                                Id = Int32.Parse(record["Id"].ToString()),
                                Name = record["Name"].ToString(),
                                Description = record["Description"].ToString(),
                                CreatedUser = Int32.Parse(record["CreatedUser"].ToString()),
                                CreatedDate = DateTime.Parse(record["CreatedDate"].ToString()),
                                UpdatedDate = DateTime.Parse(record["UpdatedDate"].ToString()),
                                UpdatedUser = Int32.Parse(record["UpdatedUser"].ToString()),
                                StartDate = DateTime.Parse(record["StartDate"].ToString()),
                                EndDate = DateTime.Parse(record["EndDate"].ToString()),
                                Status = Int32.Parse(record["Status"].ToString()),
                                RecommendSourceId = Int32.Parse(record["RecommendSourceId"].ToString()),
                                RecommendSourceType = Int32.Parse(record["RecommendSourceType"].ToString()),
                                LikeCount = Int32.Parse(record["LikeCount"].ToString()),
                                FavoriteCount = Int32.Parse(record["FavoriteCount"].ToString()),
                                ShareCount = Int32.Parse(record["ShareCount"].ToString()),
                                InvolvedCount = Int32.Parse(record["InvolvedCount"].ToString()),
                                Store_Id = Int32.Parse(record["Store_Id"].ToString()),
                                RecommendUser = Int32.Parse(record["RecommendUser"].ToString()),
                                Tag_Id = Int32.Parse(record["Tag_Id"].ToString())
                            };

            return model;
        }

        #endregion
    }
}
