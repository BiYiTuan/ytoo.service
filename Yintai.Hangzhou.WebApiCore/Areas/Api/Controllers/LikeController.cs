using System;
using System.Web.Mvc;
using Yintai.Architecture.Common.Models;
using Yintai.Architecture.Common.Web.Mvc.ActionResults;
using Yintai.Architecture.Common.Web.Mvc.Controllers;
using Yintai.Hangzhou.Contract.DTO.Request.Like;
using Yintai.Hangzhou.Contract.Like;
using Yintai.Hangzhou.Model;
using Yintai.Hangzhou.WebSupport.Binder;
using Yintai.Hangzhou.WebSupport.Mvc;

namespace Yintai.Hangzhou.WebApiCore.Areas.Api.Controllers
{
    public class LikeController : RestfulController
    {
        private readonly ILikeDataService _likeDataService;

        public LikeController(ILikeDataService likeDataService)
        {
            this._likeDataService = likeDataService;
        }

        [RestfulAuthorize]
        //[HttpPost]
        public RestfulResult Create(FormCollection formCollection, LikeCreateRequest request, int? authuid, UserModel authUser)
        {
            request.AuthUid = authuid.Value;
            request.AuthUser = authUser;

            return new RestfulResult { Data = this._likeDataService.Like(request) };
        }

        [RestfulAuthorize(true)]
        public RestfulResult List(GetLikeListRequest request, int? authuid, UserModel authUser, int? userId, [FetchUser(KeyName = "userid", IsCanMissing = true)]UserModel showUser)
        {
            //����userid��ȡ��ǰ��Ҫ��ʾ��like list ���Ϊ�գ���ô�Ͳ�ȡ
            if (authUser == null && showUser == null)
            {
                return new RestfulResult { Data = new ExecuteResult() { StatusCode = StatusCode.ClientError, Message = "�û���������" } };
            }

            if (showUser != null)
            {
                request.AuthUid = showUser.Id;
                request.AuthUser = showUser;
            }
            else
            {
                if (authUser != null && authuid != null)
                {
                    request.AuthUid = authuid.Value;
                    request.AuthUser = authUser;
                }
            }

            if (!String.IsNullOrEmpty(request.Type) && request.AuthUser != null)
            {
                switch (request.Type.ToLower())
                {
                    case "1": //1
                        return new RestfulResult { Data = this._likeDataService.GetLikeMeList(request) };
                    case "0": //0
                        return new RestfulResult { Data = this._likeDataService.GetILikeList(request) };
                    default:
                        Logger.Warn("����like�б� û���ṩ methods");
                        break;

                }
            }
            else
            {
                Logger.Warn("����like�б� û���ṩ methods");
            }

            return new RestfulResult { Data = new ExecuteResult() { StatusCode = StatusCode.ClientError, Message = "��������" } };
        }

        [RestfulAuthorize]
        [HttpPost]
        public RestfulResult Destroy(FormCollection formCollection, LikeDestroyRequest request, int? authuid, UserModel authUser)
        {
            request.AuthUid = authuid.Value;
            request.AuthUser = authUser;

            return new RestfulResult { Data = this._likeDataService.Destroy(request) };
        }

        [RestfulAuthorize]
        [HttpGet]
        public RestfulResult Destroy(LikeDestroyRequest request, int? authuid, UserModel authUser)
        {
            request.AuthUid = authuid.Value;
            request.AuthUser = authUser;

            return new RestfulResult { Data = this._likeDataService.Destroy(request) };
        }

        [RestfulAuthorize]
        [HttpGet]
        public RestfulResult Create(LikeCreateRequest request, int? authuid, UserModel authUser)
        {
            request.AuthUid = authuid.Value;
            request.AuthUser = authUser;

            return new RestfulResult { Data = this._likeDataService.Like(request) };
        }
    }
}