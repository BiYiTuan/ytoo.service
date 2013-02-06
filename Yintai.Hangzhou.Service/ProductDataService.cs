using System;
using System.Collections.Generic;
using System.Linq;
using Yintai.Architecture.Common.Caching;
using Yintai.Architecture.Common.Models;
using Yintai.Hangzhou.Contract.Coupon;
using Yintai.Hangzhou.Contract.DTO.Request.Coupon;
using Yintai.Hangzhou.Contract.DTO.Request.Product;
using Yintai.Hangzhou.Contract.DTO.Request.Promotion;
using Yintai.Hangzhou.Contract.DTO.Response.Coupon;
using Yintai.Hangzhou.Contract.DTO.Response.Product;
using Yintai.Hangzhou.Contract.Product;
using Yintai.Hangzhou.Contract.Promotion;
using Yintai.Hangzhou.Data.Models;
using Yintai.Hangzhou.Model;
using Yintai.Hangzhou.Model.Enums;
using Yintai.Hangzhou.Model.Filters;
using Yintai.Hangzhou.Repository.Contract;
using Yintai.Hangzhou.Service.Contract;
using Yintai.Hangzhou.Service.Manager;

namespace Yintai.Hangzhou.Service
{
    public class ProductDataService : BaseService, IProductDataService
    {
        private readonly IProductRepository _productRepository;
        private readonly IShareService _shareService;
        private readonly IFavoriteService _favoriteService;
        private readonly ICouponDataService _couponDataService;
        private readonly IResourceService _resourceService;
        private readonly ICouponService _couponService;
        private readonly IPromotionService _promotionService;
        private readonly IPromotionDataService _promotionDataService;

        public ProductDataService(IPromotionDataService promotionDataService, IPromotionService promotionService, ICouponService couponService, IResourceService resourceService, IProductRepository productRepository, IShareService shareService, IFavoriteService favoriteService, ICouponDataService couponDataService)
        {
            _promotionDataService = promotionDataService;
            _productRepository = productRepository;
            _shareService = shareService;
            _favoriteService = favoriteService;
            _couponDataService = couponDataService;
            _resourceService = resourceService;
            _couponService = couponService;
            _promotionService = promotionService;
        }

        private ProductInfoResponse IsR(ProductInfoResponse response, UserModel currentAuthUser, int productId)
        {
            if (response == null || currentAuthUser == null)
            {
                return response;
            }

            //�Ƿ��ղ�
            var favoriteEntity = _favoriteService.Get(currentAuthUser.Id, productId, SourceType.Product);
            if (favoriteEntity != null)
            {
                response.CurrentUserIsFavorited = true;
            }
            //�Ƿ��ȡ���Ż���
            var list = _couponService.Get(currentAuthUser.Id, productId, SourceType.Product);
            if (list != null && list.Count > 0)
            {
                response.CurrentUserIsReceived = true;
            }

            return response;
        }

        /// <summary>
        /// ��ȡ��Ʒ�б�
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ExecuteResult<ProductCollectionResponse> GetProductList(GetProductListRequest request)
        {
            if (request == null)
            {
                return new ExecuteResult<ProductCollectionResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������" };
            }

            int totalCount;
            int? ruserId;
            List<int> tagIds = null;
            if (request.TagId != null)
            {
                tagIds = new List<int> { request.TagId.Value };
            }

            if (request.UserModel == null)
            {
                ruserId = null;
            }
            else
            {
                ruserId = request.UserModel.Id;
            }

            var filter = new ProductFilter
                {
                    BrandId = request.BrandId,
                    DataStatus = DataStatus.Normal,
                    ProductName = null,
                    RecommendUser = ruserId,
                    TagIds = tagIds,
                    Timestamp = request.Timestamp,
                    TopicId = request.TopicId,
                    PromotionId = request.PromotionId
                };

            var produtEntities = _productRepository.GetPagedList(request.PagerRequest, out totalCount,
                request.ProductSortOrder, filter);

            var response = new ProductCollectionResponse(request.PagerRequest, totalCount)
            {
                Products = MappingManager.ProductInfoResponseMapping(produtEntities).ToList()
            };
            var result = new ExecuteResult<ProductCollectionResponse> { Data = response };

            return result;
        }

        public ExecuteResult<ProductCollectionResponse> RefreshProduct(GetProductRefreshRequest request)
        {
            if (request == null)
            {
                return new ExecuteResult<ProductCollectionResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������" };
            }
            int totalCount;

            List<int> tagIds = null;
            if (request.TagId != null)
            {
                tagIds = new List<int> { request.TagId.Value };
            }

            var filter = new ProductFilter
            {
                BrandId = request.BrandId,
                DataStatus = DataStatus.Normal,
                ProductName = null,
                RecommendUser = null,
                TagIds = tagIds,
                Timestamp = request.Timestamp,
                TopicId = request.TopicId,
                PromotionId = request.PromotionId
            };

            var entities = _productRepository.GetPagedList(request.PagerRequest, out totalCount,
                request.ProductSortOrder, filter);

            //var entities = _productRepository.GetPagedList(request.PagerRequest, out totalCount, ProductSortOrder.Default,
            //                                     request.Timestamp, request.TagId, null, request.BrandId);

            var response = new ProductCollectionResponse(request.PagerRequest, totalCount)
            {
                Products = MappingManager.ProductInfoResponseMapping(entities).ToList()
            };
            var result = new ExecuteResult<ProductCollectionResponse> { Data = response };

            return result;
        }

        public ExecuteResult<ProductInfoResponse> GetProductInfo(GetProductInfoRequest request)
        {
            if (request == null)
            {
                return new ExecuteResult<ProductInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������" };
            }

            //var entity = _productRepository.GetItem(request.ProductId);

            //var response = MappingManager.ProductInfoResponseMapping(entity);

            string cacheKey;
            var s = CacheKeyManager.ProductInfoKey(request.ProductId, out cacheKey);

            var r = CachingHelper.Get(
              delegate(out ProductInfoResponse data)
              {
                  var objData = CachingHelper.Get(cacheKey);
                  data = (objData == null) ? null : (ProductInfoResponse)objData;

                  return objData != null;
              },
              () =>
              {
                  var entity = _productRepository.GetItem(request.ProductId);

                  return MappingManager.ProductInfoResponseMapping(entity);
              },
              data =>
              CachingHelper.Insert(cacheKey, data, s));

            if (r != null && request.CurrentAuthUser != null)
            {
                r = IsR(r, request.CurrentAuthUser, r.Id);
            }

            return new ExecuteResult<ProductInfoResponse>(r);
        }

        public ExecuteResult<ProductInfoResponse> CreateProduct(CreateProductRequest request)
        {
            if (request == null)
            {
                return new ExecuteResult<ProductInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������" };
            }

            if (request.RecommendUser == 0)
            {
                request.RecommendUser = request.AuthUid;

            }

            request.RecommendSourceId = request.RecommendUser;
            request.RSourceType = request.AuthUser.Level == UserLevel.Daren
                                              ? RecommendSourceType.Daren
                                              : RecommendSourceType.StoreManager;

            //�жϵ�ǰ�û��Ƿ��� ����Ա���� level �Ǵ��ˣ�
            var entity = _productRepository.Insert(MappingManager.ProductEntityMapping(request));
            //���� ͼƬ
            //�����ļ��ϴ�
            if (request.Files != null && request.Files.Count > 0)
            {
                _resourceService.Save(request.Files, request.AuthUid, 0, entity.Id, SourceType.Product);
            }

            return new ExecuteResult<ProductInfoResponse>(MappingManager.ProductInfoResponseMapping(entity));
        }

        public ExecuteResult<ProductInfoResponse> UpdateProduct(UpdateProductRequest request)
        {
            if (request == null)
            {
                return new ExecuteResult<ProductInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������" };
            }

            var entity = _productRepository.GetItem(request.ProductId);

            if (entity == null)
            {
                return new ExecuteResult<ProductInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������,û���ҵ�ָ��product" };
            }

            if (request.RecommendUser == 0)
            {
                request.RecommendUser = request.AuthUid;
            }

            var source = MappingManager.ProductEntityMapping(request);
            source.CreatedDate = entity.CreatedDate;
            source.CreatedUser = entity.CreatedUser;
            source.Status = entity.Status;
            source.FavoriteCount = entity.FavoriteCount;
            source.InvolvedCount = entity.InvolvedCount;
            source.ShareCount = entity.ShareCount;

            MappingManager.ProductEntityMapping(source, entity);

            _productRepository.Update(entity);

            return new ExecuteResult<ProductInfoResponse>(MappingManager.ProductInfoResponseMapping(entity));
        }

        public ExecuteResult<ProductInfoResponse> DestroyProduct(DestroyProductRequest request)
        {
            if (request == null)
            {
                return new ExecuteResult<ProductInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������" };
            }

            var entity = _productRepository.GetItem(request.ProductId);

            if (entity == null)
            {
                return new ExecuteResult<ProductInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������,û���ҵ�ָ��product" };
            }

            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedUser = request.AuthUid;
            entity.Status = (int)DataStatus.Normal;

            _productRepository.Delete(entity);

            return new ExecuteResult<ProductInfoResponse>(MappingManager.ProductInfoResponseMapping(entity));
        }

        public ExecuteResult<ProductInfoResponse> CreateResourceProduct(CreateResourceProductRequest request)
        {
            if (request == null)
            {
                return new ExecuteResult<ProductInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������" };
            }

            var entity = _productRepository.GetItem(request.ProductId);

            if (entity == null)
            {
                return new ExecuteResult<ProductInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������,û���ҵ�ָ��product" };
            }

            //Add
            if (request.Files != null && request.Files.Count > 0)
            {
                _resourceService.Save(request.Files, request.AuthUid, request.DefaultNum, entity.Id, SourceType.Product);
            }

            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedUser = request.AuthUid;
            _productRepository.Update(entity);

            return new ExecuteResult<ProductInfoResponse>(MappingManager.ProductInfoResponseMapping(entity));
        }

        public ExecuteResult<ProductInfoResponse> DestroyResourceProduct(DestroyResourceProductRequest request)
        {
            if (request == null)
            {
                return new ExecuteResult<ProductInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������" };
            }

            var entity = _productRepository.GetItem(request.ProductId);

            if (entity == null)
            {
                return new ExecuteResult<ProductInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������,û���ҵ�ָ��product" };
            }

            var resources = _resourceService.Get(entity.Id, SourceType.Product);

            if (resources == null || resources.Count == 0)
            {
                return new ExecuteResult<ProductInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������,û���ҵ�product����Դ" };
            }

            var resouce = resources.SingleOrDefault(v => v.Id == request.ResourceId);
            if (resouce == null)
            {
                return new ExecuteResult<ProductInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������,û���ҵ�product��ָ����Դ" };
            }

            _resourceService.Del(resouce.Id);

            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedUser = request.AuthUid;

            _productRepository.Update(entity);

            return new ExecuteResult<ProductInfoResponse>(MappingManager.ProductInfoResponseMapping(entity));
        }

        public ExecuteResult<ProductInfoResponse> CreateShare(CreateShareProductRequest request)
        {
            if (request == null)
            {
                return new ExecuteResult<ProductInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������" };
            }
            var product = _productRepository.GetItem(request.ProductId);
            if (product == null)
            {
                return new ExecuteResult<ProductInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������" };
            }

            _shareService.Create(new ShareHistoryEntity
                {
                    CreatedDate = DateTime.Now,
                    CreatedUser = request.AuthUid,
                    Description = request.Description,
                    Name = request.Name,
                    ShareTo = (int)request.OsiteType,
                    SourceId = request.ProductId,
                    SourceType = (int)SourceType.Product,
                    Stauts = 1,
                    User_Id = request.AuthUid,
                    UpdatedDate = DateTime.Now,
                    UpdatedUser = request.AuthUid
                });

            //+1
            product.ShareCount++;

            _productRepository.Update(product);

            return new ExecuteResult<ProductInfoResponse>(MappingManager.ProductInfoResponseMapping(product));
        }

        public ExecuteResult<ProductInfoResponse> CreateFavor(CreateFavorProductRequest request)
        {
            if (request == null)
            {
                return new ExecuteResult<ProductInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������" };
            }
            var product = _productRepository.GetItem(request.ProductId);
            if (product == null)
            {
                return new ExecuteResult<ProductInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������" };
            }

            var favorentity = _favoriteService.Get(request.AuthUid, product.Id, SourceType.Product);
            if (favorentity != null)
            {
                return new ExecuteResult<ProductInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "���Ѿ���ӹ��ղ���" };
            }

            _favoriteService.Create(new FavoriteEntity
                {
                    CreatedDate = DateTime.Now,
                    CreatedUser = request.AuthUid,
                    Description = String.Empty,
                    FavoriteSourceId = product.Id,
                    FavoriteSourceType = (int)SourceType.Product,
                    Status = 1,
                    User_Id = request.AuthUid,
                    Store_Id = product.Store_Id
                });

            //+1
            product.FavoriteCount++;

            _productRepository.Update(product);
            //TODO:û�и��µ�ǰ��isfavorite
            var response = MappingManager.ProductInfoResponseMapping(product);

            if (request.AuthUser != null)
            {
                response = IsR(response, request.AuthUser, product.Id);
            }

            return new ExecuteResult<ProductInfoResponse>(response);
        }

        public ExecuteResult<ProductInfoResponse> DestroyFavor(DestroyFavorProductRequest request)
        {
            if (request == null)
            {
                return new ExecuteResult<ProductInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������" };
            }
            var product = _productRepository.GetItem(request.ProductId);
            if (product == null)
            {
                return new ExecuteResult<ProductInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������" };
            }

            var favorentity = _favoriteService.Get(request.AuthUid, product.Id, SourceType.Product);

            if (favorentity == null)
            {
                return new ExecuteResult<ProductInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "�ղز�����" };
            }

            //del shoucang
            _favoriteService.Del(favorentity);
            //del product share count
            product.FavoriteCount--;
            _productRepository.Update(product);

            return GetProductInfo(new GetProductInfoRequest
                {
                    CurrentAuthUser = request.AuthUser,
                    ProductId = request.ProductId
                });
        }

        public ExecuteResult<ProductInfoResponse> CreateCoupon(CreateCouponProductRequest request)
        {
            if (request == null)
            {
                return new ExecuteResult<ProductInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������" };
            }
            var product = _productRepository.GetItem(request.ProductId);
            if (product == null)
            {
                return new ExecuteResult<ProductInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��������" };
            }

            //�ж������v1.0�汾 �����������Ż�ȯ��

            ExecuteResult<CouponCodeResponse> coupon;
            if (request.Client_Version != "1.0")
            {
                //�ж�
                if (!_promotionService.Exists(request.PromotionId ?? 0, request.ProductId))
                {
                    return new ExecuteResult<ProductInfoResponse>(null) { StatusCode = StatusCode.ClientError, Message = "��ǰ��Ʒû�вμӸû" };
                }

                var pr = _promotionDataService.CreateCoupon(new PromotionCouponCreateRequest
                    {
                        AuthUid = request.AuthUid,
                        AuthUser = request.AuthUser,
                        Client_Version = request.Client_Version,
                        IsPass = request.IsPass,
                        Method = request.Method,
                        PromotionId = request.PromotionId ?? 0,
                        Token = request.Token
                    });

                if (pr.IsSuccess && pr.Data != null && pr.Data.CouponCodeResponse != null)
                {
                    coupon = new ExecuteResult<CouponCodeResponse>(pr.Data.CouponCodeResponse);
                }
                else
                {
                    coupon = new ExecuteResult<CouponCodeResponse>(null) { Message = pr.Message, StatusCode = pr.StatusCode };
                }
            }
            else
            {
                coupon = _couponDataService.CreateCoupon(new CouponCouponRequest
                {
                    AuthUid = request.AuthUid,
                    SourceId = product.Id,
                    SourceType = (int)SourceType.Product,
                    Token = request.Token,
                    AuthUser = request.AuthUser,
                    Method = request.Method,
                    Client_Version = request.Client_Version
                });
            }

            if (!coupon.IsSuccess)
            {
                return new ExecuteResult<ProductInfoResponse>(null) { Message = coupon.Message, StatusCode = coupon.StatusCode };
            }

            //+1
            product.InvolvedCount++;
            _productRepository.Update(product);

            var response = MappingManager.ProductInfoResponseMapping(product);
            response.CouponCodeResponse = coupon.Data;

            if (request.AuthUser != null)
            {
                response = IsR(response, request.AuthUser, product.Id);
            }

            return new ExecuteResult<ProductInfoResponse>(response);
        }
    }
}