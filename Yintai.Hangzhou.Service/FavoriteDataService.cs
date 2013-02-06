using System;
using Yintai.Architecture.Common.Models;
using Yintai.Hangzhou.Contract.DTO.Request.Favorite;
using Yintai.Hangzhou.Contract.DTO.Response.Favorite;
using Yintai.Hangzhou.Contract.Favorite;
using Yintai.Hangzhou.Contract.Request.Favorite;
using Yintai.Hangzhou.Repository.Contract;

namespace Yintai.Hangzhou.Service
{
    public class FavoriteDataService : BaseService, IFavoriteDataService
    {
        private readonly IFavoriteRepository _favoriteRepository;

        public FavoriteDataService(IFavoriteRepository favoriteRepository)
        {
            this._favoriteRepository = favoriteRepository;
        }

        #region

        /// <summary>
        /// ��ȡ�ղ��б�
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private ExecuteResult<FavoriteCollectionResponse> GetFavoriteList(FavoriteListRequest request)
        {
            var pagerRequest = new PagerRequest(request.Page, request.PageSize);
            int totalCount;
            var entitys = this._favoriteRepository.GetPagedList(request.UserModel.Id, pagerRequest, out totalCount, request.SortOrder, request.SType);

            CoordinateInfo coordinate = null;
            if (request.Lng > 0 || request.Lng < 0)
            {
                coordinate = new CoordinateInfo(request.Lng, request.Lat);
            }

            var response = MappingManager.FavoriteCollectionResponseMapping(entitys, coordinate);
            response.Index = pagerRequest.PageIndex;
            response.Size = pagerRequest.PageSize;
            response.TotalCount = totalCount;

            return new ExecuteResult<FavoriteCollectionResponse>(response);
        }

        #endregion

        #region Implementation of IFavoriteDataService

        /// <summary>
        /// �ղ�
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ExecuteResult Create(FavoriteCreateRequest request)
        {
            //return new ExecuteResult();

            throw new NotImplementedException();
        }

        /// <summary>
        /// ��ȡ�ղ��б�
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ExecuteResult<FavoriteCollectionResponse> GetFavoriteList(GetFavoriteListRequest request)
        {
            return GetFavoriteList((FavoriteListRequest)request);
        }

        public ExecuteResult<FavoriteCollectionResponse> GetDarenFavoriteList(DarenFavoriteListRequest request)
        {
            return GetFavoriteList((FavoriteListRequest)request);
        }

        /// <summary>
        /// ɾ���ղ�
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ExecuteResult Destroy(FavoriteDestroyRequest request)
        {
            var favorEntity = this._favoriteRepository.GetItem(request.FavoriteId);
            if (favorEntity == null)
            {
                return new ExecuteResult() { StatusCode = StatusCode.ClientError, Message = "û���ҵ��ò�Ʒ" };
            }

            if (favorEntity.User_Id != request.AuthUid)
            {
                return new ExecuteResult() { StatusCode = StatusCode.ClientError, Message = "��û��Ȩ��ɾ�����˵��ղ�" };
            }

            this._favoriteRepository.Delete(favorEntity);

            return new ExecuteResult();
        }

        #endregion
    }
}