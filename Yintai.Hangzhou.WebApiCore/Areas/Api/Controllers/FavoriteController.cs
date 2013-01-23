using System;
using Yintai.Architecture.Common;
using Yintai.Architecture.Common.Models;
using Yintai.Architecture.Common.Web.Mvc.ActionResults;
using Yintai.Architecture.Common.Web.Mvc.Controllers;
using Yintai.Hangzhou.Contract.DTO.Request.Favorite;
using Yintai.Hangzhou.Contract.Favorite;
using Yintai.Hangzhou.Contract.Request.Favorite;
using Yintai.Hangzhou.Model;
using Yintai.Hangzhou.WebSupport.Binder;
using Yintai.Hangzhou.WebSupport.Mvc;

namespace Yintai.Hangzhou.WebApiCore.Areas.Api.Controllers
{
    public class FavoriteController : RestfulController
    {
        private readonly IFavoriteDataService _favoriteDataService;

        public FavoriteController(IFavoriteDataService favoriteDataService)
        {
            this._favoriteDataService = favoriteDataService;
        }

        [RestfulAuthorize]
        public RestfulResult Create(FavoriteCreateRequest request, int? authuid)
        {
            request.AuthUid = authuid.Value;
            return new RestfulResult { Data = this._favoriteDataService.Create(request) };
        }

        [RestfulAuthorize]
        public RestfulResult List(GetFavoriteListRequest request, int? authuid, UserModel authUser)
        {
            //�жϵ�ǰ����ȡ���ղ��б��USER���Ƿ��Ǵ���or �곤
            if (authUser == null)
            {
                return new RestfulResult { Data = new ExecuteResult { StatusCode = StatusCode.ClientError, Message = "userδ�ҵ�" } };
            }
            request.UserModel = authUser;

            return new RestfulResult { Data = this._favoriteDataService.GetFavoriteList(request) };
        }

        public RestfulResult Daren(DarenFavoriteListRequest request, [FetchUser(KeyName = "userid")]UserModel showUser)
        {
            if (System.String.Compare(request.Method, DefineRestfulMethod.List, System.StringComparison.OrdinalIgnoreCase) != 0)
            {
                return new RestfulResult { Data = new ExecuteResult { StatusCode = StatusCode.ClientError, Message = "��������" } };
            }

            //�жϵ�ǰ����ȡ���ղ��б��USER���Ƿ��Ǵ���or �곤

            if (showUser == null)
            {
                return new RestfulResult { Data = new ExecuteResult { StatusCode = StatusCode.ClientError, Message = "userδ�ҵ�" } };
            }

            request.UserModel = showUser;

            return new RestfulResult { Data = this._favoriteDataService.GetDarenFavoriteList(request) };
        }

        [RestfulAuthorize]
        public RestfulResult Destroy(FavoriteDestroyRequest request, int? authuid, UserModel authUser)
        {
            request.AuthUid = authuid.Value;
            request.AuthUser = authUser;

            return new RestfulResult { Data = this._favoriteDataService.Destroy(request) };
        }
    }
}