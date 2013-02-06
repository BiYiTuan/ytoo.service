using System;
using System.Collections.Generic;
using System.Linq;
using Yintai.Architecture.Common.Models;
using Yintai.Hangzhou.Contract.Coupon;
using Yintai.Hangzhou.Contract.DTO.Request.Coupon;
using Yintai.Hangzhou.Contract.DTO.Request.Promotion;
using Yintai.Hangzhou.Contract.DTO.Response.Promotion;
using Yintai.Hangzhou.Contract.Promotion;
using Yintai.Hangzhou.Data.Models;
using Yintai.Hangzhou.Model;
using Yintai.Hangzhou.Model.Enums;
using Yintai.Hangzhou.Repository.Contract;
using Yintai.Hangzhou.Service.Contract;

namespace Yintai.Hangzhou.Service
{
    public class PromotionDataService : BaseService, IPromotionDataService
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IShareService _shareService;
        private readonly IFavoriteService _favoriteService;
        private readonly ICouponDataService _couponDataService;
        private readonly IResourceService _resourceService;
        private readonly IPromotionBrandRelationRepository _promotionBrandRelationRepository;
        private readonly ICouponService _couponService;
        private readonly IPromotionService _promotionService;

        public PromotionDataService(IPromotionService promotionService, ICouponService couponService, IPromotionBrandRelationRepository promotionBrandRelationRepository, IPromotionRepository promotionRepository, IFavoriteService favoriteService, IShareService shareService, ICouponDataService couponDataService, IResourceService resourceService)
        {
            _promotionRepository = promotionRepository;
            _favoriteService = favoriteService;
            _shareService = shareService;
            _couponDataService = couponDataService;
            _resourceService = resourceService;
            _promotionBrandRelationRepository = promotionBrandRelationRepository;
            _couponService = couponService;
            _promotionService = promotionService;
        }

        private PromotionInfoResponse IsR(PromotionInfoResponse response, UserModel currentAuthUser, int entityId)
        {
            if (response == null || currentAuthUser == null)
            {
                return response;
            }

            //�Ƿ��ղ�
            var favoriteEntity = _favoriteService.Get(currentAuthUser.Id, entityId, SourceType.Promotion);
            if (favoriteEntity != null)
            {
                response.CurrentUserIsFavorited = true;
            }
            //�Ƿ��ȡ���Ż���
            var list = _couponService.Get(currentAuthUser.Id, entityId, SourceType.Promotion);
            if (list != null && list.Count > 0)
            {
                response.CurrentUserIsReceived = true;
            }

            return response;
        }

        #region Implementation of IPromotionService

        /// <summary>
        /// ע���ȡ���½ӿڵĵ��÷�ʽ
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ExecuteResult<PromotionCollectionResponse> GetPromotionList(GetPromotionListRequest request)
        {
            var pageRequest = new PagerRequest(request.Page, request.Pagesize);
            var timestamp = new Timestamp { TsType = TimestampType.Old, Ts = DateTime.Parse(request.RefreshTs) };

            int totalCount;
            List<PromotionEntity> entitys;

            switch (request.SortOrder)
            {
                case PromotionSortOrder.Near:
                    //���� ���̵���λ�ã��ҵ������д����ĵ���
                    //���ݵ���ɸ����Ʒ
                    entitys = _promotionRepository.GetPagedList(pageRequest.PageIndex, pageRequest.PageSize, out totalCount,
                                                           (int)request.SortOrder, request.Lng, request.Lat, timestamp);
                    break;
                case PromotionSortOrder.New:
                    /*��ѯ�߼�
                     * 1.���쿪ʼ�Ļ
                     * 2.��ǰ��ʼ�����컹�Խ��еĻ
                     * 3.������ʼ�Ļ��ʱ������ 24Сʱ�ڵ�
                     * 
                     * logic �� size40 
                     */

                    //1
                    entitys = _promotionRepository.GetPagedList(pageRequest, out totalCount, PromotionSortOrder.New, new DateTimeRangeInfo
                        {
                            StartDateTime = DateTime.Now,
                            EndDateTime = DateTime.Now

                        }, new CoordinateInfo(request.Lng, request.Lat), timestamp, null, PromotionFilterMode.New);

                    var t = pageRequest.PageIndex * pageRequest.PageSize;

                    var e2Size = 0;
                    var e2Index = 1;
                    List<PromotionEntity> e2;
                    var e2Count = 0;
                    var c = t - totalCount;
                    int? skipCount = null;
                    if (c <= 0)
                    {
                        e2Index = 1;
                        e2Size = 0;
                    }
                    else if (c > 0 && c < pageRequest.PageSize)
                    {
                        //1
                        e2Index = 1;
                        e2Size = c;
                    }
                    else
                    {
                        e2Index = (int)Math.Ceiling(c / (double)pageRequest.PageSize);
                        e2Size = pageRequest.PageSize;

                        if (e2Index > 1)
                        {
                            skipCount = c - (e2Index - 1) * e2Size + (pageRequest.PageSize * (e2Index - 2));
                        }
                    }

                    var p2 = new PagerRequest(e2Index, e2Size);

                    e2 = _promotionRepository.GetPagedList(p2, out e2Count, PromotionSortOrder.New, new DateTimeRangeInfo()
                    {
                        StartDateTime = DateTime.Now,
                        EndDateTime = DateTime.Now

                    }, new CoordinateInfo(request.Lng, request.Lat), timestamp, null, PromotionFilterMode.BeginStart, skipCount);

                    if (e2.Count != 0 && e2Size != 0)
                    {
                        entitys.AddRange(e2);
                    }

                    //�ܼ�¼��
                    totalCount = totalCount + e2Count;
                    //entitys = _promotionRepository.GetPagedList(pageRequest.PageIndex, pageRequest.PageSize,
                    //                    out totalCount, (int)request.SortOrder, timestamp);
                    break;
                default:
                    entitys = _promotionRepository.GetPagedList(pageRequest.PageIndex, pageRequest.PageSize,
                                                             out totalCount, (int)request.SortOrder, timestamp);
                    break;
            }

            var response = MappingManager.PromotionResponseMapping(entitys, request.CoordinateInfo);

            var result = new ExecuteResult<PromotionCollectionResponse>();
            var collection = new PromotionCollectionResponse(pageRequest, totalCount)
                                 {
                                     Promotions = response.ToList()
                                 };
            result.Data = collection;

            return result;

        }

        /// <summary>
        /// ˢ�½ӿ�
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ExecuteResult<PromotionCollectionResponse> GetPromotionListForRefresh(GetPromotionListForRefresh request)
        {
            var timestamp = new Timestamp { TsType = TimestampType.New, Ts = DateTime.Parse(request.RefreshTs) };

            //List<PromotionEntity> entitys = request.SortOrder == PromotionSortOrder.Near ? _promotionRepository.GetList(request.PagerRequest.PageSize, (int)request.SortOrder, request.Lng, request.Lat, timestamp) : _promotionRepository.GetList(request.PagerRequest.PageSize, (int)request.SortOrder, timestamp);

            List<PromotionEntity> entitys;
            var pageRequest = request.PagerRequest;

            switch (request.SortOrder)
            {
                case PromotionSortOrder.Near:
                    //���� ���̵���λ�ã��ҵ������д����ĵ���
                    //���ݵ���ɸ����Ʒ

                    entitys = _promotionRepository.GetList(request.PagerRequest.PageSize, (int)request.SortOrder,
                                                           request.Lng, request.Lat, timestamp);
                    break;
                case PromotionSortOrder.New:
                    /*��ѯ�߼�
                     * 1.���쿪ʼ�Ļ
                     * 2.��ǰ��ʼ�����컹�Խ��еĻ
                     * 3.������ʼ�Ļ��ʱ������ 24Сʱ�ڵ�
                     * 
                     * logic �� size40 
                     */

                    //1
                    var totalCount = 0;
                    entitys = _promotionRepository.GetPagedList(pageRequest, out totalCount, PromotionSortOrder.New, new DateTimeRangeInfo
                    {
                        StartDateTime = DateTime.Now,
                        EndDateTime = DateTime.Now

                    }, new CoordinateInfo(request.Lng, request.Lat), timestamp, null, PromotionFilterMode.New);

                    var t = pageRequest.PageIndex * pageRequest.PageSize;

                    var e2Size = 0;
                    var e2Index = 1;
                    List<PromotionEntity> e2;
                    var e2Count = 0;
                    var c = t - totalCount;
                    int? skipCount = null;
                    if (c <= 0)
                    {
                        e2Index = 1;
                        e2Size = 0;
                    }
                    else if (c > 0 && c < pageRequest.PageSize)
                    {
                        //1
                        e2Index = 1;
                        e2Size = c;
                    }
                    else
                    {
                        e2Index = (int)Math.Ceiling(c / (double)pageRequest.PageSize);
                        e2Size = pageRequest.PageSize;

                        if (e2Index > 1)
                        {
                            skipCount = c - (e2Index - 1) * e2Size + (pageRequest.PageSize * (e2Index - 2));
                        }
                    }

                    var p2 = new PagerRequest(e2Index, e2Size);

                    e2 = _promotionRepository.GetPagedList(p2, out e2Count, PromotionSortOrder.New, new DateTimeRangeInfo
                    {
                        StartDateTime = DateTime.Now,
                        EndDateTime = DateTime.Now

                    }, new CoordinateInfo(request.Lng, request.Lat), timestamp, null, PromotionFilterMode.BeginStart, skipCount);

                    if (e2.Count != 0 && e2Size != 0)
                    {
                        entitys.AddRange(e2);
                    }

                    //�ܼ�¼��
                    //totalCount = totalCount + e2Count;
                    //entitys = _promotionRepository.GetPagedList(pageRequest.PageIndex, pageRequest.PageSize,
                    //                    out totalCount, (int)request.SortOrder, timestamp);
                    break;
                default:
                    entitys = _promotionRepository.GetList(request.PagerRequest.PageSize, (int)request.SortOrder,
                                       timestamp);
                    break;
            }









            var response = MappingManager.PromotionResponseMapping(entitys, request.CoordinateInfo);

            var result = new ExecuteResult<PromotionCollectionResponse>();
            var collection = new PromotionCollectionResponse(request.PagerRequest, entitys.Count())
            {
                Promotions = response.ToList()
            };
            result.Data = collection;

            return result;
        }

        /// <summary>
        /// ��ȡ����������Ϣ
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ExecuteResult<PromotionInfoResponse> GetPromotionInfo(GetPromotionInfoRequest request)
        {
            var entity = _promotionRepository.GetItem(request.Promotionid);

            var response = MappingManager.PromotionResponseMapping(entity, request.CoordinateInfo);

            if (request.CurrentAuthUser != null && entity != null)
            {
                //�Ƿ��ղ�
                response = IsR(response, request.CurrentAuthUser, entity.Id);
            }

            var result = new ExecuteResult<PromotionInfoResponse>(response);

            return result;
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ExecuteResult<PromotionInfoResponse> CreateShare(PromotionShareCreateRequest request)
        {
            var shareEntity = _shareService.Create(new ShareHistoryEntity
                                             {
                                                 CreatedDate = DateTime.Now,
                                                 CreatedUser = request.AuthUid,
                                                 Description = request.Description,
                                                 Name = request.Name,
                                                 ShareTo = request.OutSiteType,
                                                 SourceId = request.Promotionid,
                                                 SourceType = (int)SourceType.Promotion,
                                                 Stauts = 1,
                                                 User_Id = request.AuthUid,
                                                 UpdatedDate = DateTime.Now,
                                                 UpdatedUser = request.AuthUid
                                             });

            var promotionEntity = _promotionRepository.GetItem(shareEntity.SourceId);
            promotionEntity.ShareCount = promotionEntity.ShareCount + 1;

            _promotionRepository.Update(promotionEntity);

            return GetPromotionInfo(new GetPromotionInfoRequest
                                 {
                                     Promotionid = shareEntity.SourceId,
                                     Lat = request.Lat,
                                     Lng = request.Lng
                                 });
        }

        /// <summary>
        /// �����ղ�
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ExecuteResult<PromotionInfoResponse> CreateFavor(PromotionFavorCreateRequest request)
        {
            var promotionEntity = _promotionRepository.GetItem(request.Promotionid);
            if (promotionEntity == null)
            {
                return new ExecuteResult<PromotionInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "�������" };
            }
            //����Ƿ��ղع�
            var favorEntity = _favoriteService.Get(request.AuthUid, request.Promotionid, SourceType.Promotion);
            if (favorEntity != null)
            {
                return new ExecuteResult<PromotionInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "���Ѿ���ӹ��ղ���" };
            }

            favorEntity = _favoriteService.Create(new FavoriteEntity
                                                              {
                                                                  CreatedDate = DateTime.Now,
                                                                  CreatedUser = request.AuthUid,
                                                                  Description = String.Empty,
                                                                  FavoriteSourceId = promotionEntity.Id,
                                                                  FavoriteSourceType = (int)SourceType.Promotion,
                                                                  Status = 1,
                                                                  User_Id = request.AuthUid,
                                                                  Store_Id = promotionEntity.Store_Id
                                                              });

            promotionEntity.FavoriteCount++;

            _promotionRepository.Update(promotionEntity);

            return GetPromotionInfo(new GetPromotionInfoRequest
            {
                Promotionid = favorEntity.FavoriteSourceId,
                CurrentAuthUser = request.AuthUser,
                Lat = request.Lat,
                Lng = request.Lng
            });
        }

        public ExecuteResult<PromotionInfoResponse> DestroyFavor(PromotionFavorDestroyRequest request)
        {
            var promotionEntity = _promotionRepository.GetItem(request.Promotionid);
            if (promotionEntity == null)
            {
                return new ExecuteResult<PromotionInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "�������" };
            }
            //����Ƿ��ղع�
            var favorEntity = _favoriteService.Get(request.AuthUid, request.Promotionid, SourceType.Promotion);
            if (favorEntity == null)
            {
                return new ExecuteResult<PromotionInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "û���ҵ��ղ�" };
            }

            _favoriteService.Del(favorEntity);

            promotionEntity.FavoriteCount--;

            _promotionRepository.Update(promotionEntity);

            return GetPromotionInfo(new GetPromotionInfoRequest
            {
                Promotionid = promotionEntity.Id,
                CurrentAuthUser = request.AuthUser,
                Lat = request.Lat,
                Lng = request.Lng
            });
        }

        /// <summary>
        /// �����Żݾ�
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ExecuteResult<PromotionInfoResponse> CreateCoupon(PromotionCouponCreateRequest request)
        {
            var promotionEntity = _promotionRepository.GetItem(request.PromotionId);
            if (promotionEntity == null)
            {
                return new ExecuteResult<PromotionInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������" };
            }

            var str = _promotionService.Verification(promotionEntity);
            if (!String.IsNullOrEmpty(str))
            {
                return new ExecuteResult<PromotionInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = str };
            }

            var coupon = _couponDataService.CreateCoupon(new CouponCouponRequest
                                                                  {
                                                                      AuthUid = request.AuthUid,
                                                                      SourceId = request.PromotionId,
                                                                      SourceType = (int)SourceType.Promotion,
                                                                      Token = request.Token,
                                                                      AuthUser = request.AuthUser,
                                                                      Method = request.Method,
                                                                      Client_Version = request.Client_Version
                                                                  });

            if (!coupon.IsSuccess)
            {
                return new ExecuteResult<PromotionInfoResponse>(null) { Message = coupon.Message, StatusCode = coupon.StatusCode };
            }

            promotionEntity.InvolvedCount = promotionEntity.InvolvedCount + 1;
            _promotionRepository.Update(promotionEntity);

            var re = MappingManager.PromotionResponseMapping(promotionEntity);
            re.CouponCodeResponse = coupon.Data;

            return new ExecuteResult<PromotionInfoResponse> { Data = re };
        }

        /// <summary>
        /// �����
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ExecuteResult<PromotionInfoResponse> CreatePromotion(CreatePromotionRequest request)
        {
            var promotionSourceType = request.AuthUser.UserLevel == (int)UserLevel.Daren ? RecommendSourceType.Daren : RecommendSourceType.StoreManager;
            //����Ǵ��ˣ���Ҫ�ϴ�storeId,����ǵ곤����ôȡ�곤������store
            // promotionSourceType == RecommendSourceType.Daren ? request.StoreId : request.AuthUser.Store_Id;
            var storeId = request.StoreId < 1 ? request.AuthUser.Store_Id : request.StoreId;
            var promotionEntity = _promotionRepository.Insert(new PromotionEntity
                {
                    CreatedDate = DateTime.Now,
                    CreatedUser = request.AuthUid,
                    Description = request.Description,
                    EndDate = request.EndDate,
                    FavoriteCount = 0,
                    InvolvedCount = 0,
                    LikeCount = 0,
                    Name = request.Name,
                    RecommendSourceId = request.RecommendUser == null ? request.AuthUid : request.RecommendUser.Value,
                    RecommendSourceType = (int)promotionSourceType,
                    ShareCount = 0,
                    StartDate = request.StartDate,
                    Status = 1,
                    Store_Id = storeId ?? 0,
                    UpdatedDate = DateTime.Now,
                    UpdatedUser = request.AuthUid,
                    RecommendUser = request.RecommendUser == null ? request.AuthUid : request.RecommendUser.Value,
                    Tag_Id = request.TagId ?? 0
                });
            //���� ͼƬ
            //�����ļ��ϴ�
            if (request.Files != null && request.Files.Count > 0)
            {
                _resourceService.Save(request.Files, request.AuthUid, 0, promotionEntity.Id, SourceType.Promotion);
            }

            //����Ʒ�ƹ�ϵ
            if (request.Brands.Length > 0)
            {
                var b = request.Brands.Distinct().Where(v => v > 0).ToList();
                var list = new List<PromotionBrandRelationEntity>(b.Count);
                foreach (var item in b)
                {
                    list.Add(new PromotionBrandRelationEntity
                        {
                            Brand_Id = item,
                            CreatedDate = DateTime.Now,
                            Promotion_Id = promotionEntity.Id
                        });
                }

                _promotionBrandRelationRepository.BatchInsert(list);
            }

            return GetPromotionInfo(new GetPromotionInfoRequest
                {
                    Promotionid = promotionEntity.Id,
                    CurrentAuthUser = request.AuthUser
                });
        }

        public ExecuteResult<PromotionInfoResponse> UpdatePromotion(UpdatePromotionRequest request)
        {
            if (request == null || request.PromotionId == null)
            {
                return new ExecuteResult<PromotionInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������" };
            }

            var entity = _promotionRepository.GetItem(request.PromotionId ?? 0);

            if (entity == null)
            {
                return new ExecuteResult<PromotionInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������,û���ҵ�ָ��promotion" };
            }
            var promotionSourceType = request.AuthUser.UserLevel == (int)UserLevel.Daren ? RecommendSourceType.Daren : RecommendSourceType.StoreManager;
            // promotionSourceType == RecommendSourceType.Daren ? request.StoreId : request.AuthUser.Store_Id;
            var storeId = request.StoreId < 1 ? request.AuthUser.Store_Id : request.StoreId;

            var source = MappingManager.PromotionEntityMapping(request);
            source.CreatedDate = entity.CreatedDate;
            source.CreatedUser = entity.CreatedUser;
            source.Status = entity.Status;
            source.FavoriteCount = entity.FavoriteCount;
            source.InvolvedCount = entity.InvolvedCount;
            source.LikeCount = entity.LikeCount;
            source.ShareCount = entity.ShareCount;
            source.Store_Id = storeId ?? 0;
            source.RecommendSourceType = (int)promotionSourceType;

            MappingManager.PromotionEntityMapping(source, entity);

            _promotionRepository.Update(entity);
            //Ʒ�ƹ�ϵ
            if (request.Brands == null || request.Brands.Length == 0)
            {
                _promotionBrandRelationRepository.Del(entity.Id);
            }
            else
            {
                _promotionBrandRelationRepository.Del(entity.Id);
                var b = request.BrandIds.Distinct().Where(v => v > 0).ToList();
                var list = new List<PromotionBrandRelationEntity>(b.Count);
                foreach (var item in b)
                {
                    list.Add(new PromotionBrandRelationEntity
                    {
                        Brand_Id = item,
                        CreatedDate = DateTime.Now,
                        Promotion_Id = entity.Id
                    });
                }

                _promotionBrandRelationRepository.BatchInsert(list);
            }

            return new ExecuteResult<PromotionInfoResponse>(MappingManager.PromotionResponseMapping(entity));
        }

        public ExecuteResult<PromotionInfoResponse> DestroyPromotion(DestroyPromotionRequest request)
        {
            if (request == null)
            {
                return new ExecuteResult<PromotionInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������" };
            }

            var entity = _promotionRepository.GetItem(request.PromotionId);

            if (entity == null)
            {
                return new ExecuteResult<PromotionInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������,û���ҵ�ָ��promotion" };
            }

            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedUser = request.AuthUid;
            entity.Status = (int)DataStatus.Deleted;

            _promotionRepository.Delete(entity);

            return new ExecuteResult<PromotionInfoResponse>(MappingManager.PromotionResponseMapping(entity));
        }

        public ExecuteResult<PromotionInfoResponse> CreateResourcePromotion(CreateResourcePromotionRequest request)
        {
            if (request == null)
            {
                return new ExecuteResult<PromotionInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������" };
            }

            var entity = _promotionRepository.GetItem(request.PromotionId);

            if (entity == null)
            {
                return new ExecuteResult<PromotionInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������,û���ҵ�ָ��promotion" };
            }

            //Add
            if (request.Files != null && request.Files.Count > 0)
            {
                _resourceService.Save(request.Files, request.AuthUid, request.DefaultNum, entity.Id, SourceType.Promotion);
            }

            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedUser = request.AuthUid;
            _promotionRepository.Update(entity);

            return new ExecuteResult<PromotionInfoResponse>(MappingManager.PromotionResponseMapping(entity));
        }

        public ExecuteResult<PromotionInfoResponse> DestroyResourcePromotion(DestroyResourcePromotionRequest request)
        {
            if (request == null)
            {
                return new ExecuteResult<PromotionInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������" };
            }

            var entity = _promotionRepository.GetItem(request.PromotionId);

            if (entity == null)
            {
                return new ExecuteResult<PromotionInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������,û���ҵ�ָ��promotion" };
            }

            var resources = _resourceService.Get(entity.Id, SourceType.Promotion);

            if (resources == null || resources.Count == 0)
            {
                return new ExecuteResult<PromotionInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������,û���ҵ�promotion����Դ" };
            }

            var resouce = resources.SingleOrDefault(v => v.Id == request.ResourceId);
            if (resouce == null)
            {
                return new ExecuteResult<PromotionInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������,û���ҵ�promotion��ָ����Դ" };
            }

            _resourceService.Del(resouce.Id);

            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedUser = request.AuthUid;

            _promotionRepository.Update(entity);

            return new ExecuteResult<PromotionInfoResponse>(MappingManager.PromotionResponseMapping(entity));
        }

        #endregion
    }
}