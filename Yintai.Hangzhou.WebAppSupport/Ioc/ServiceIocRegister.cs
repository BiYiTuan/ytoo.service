using Yintai.Hangzhou.Contract.Apns;
using Yintai.Hangzhou.Contract.Brand;
using Yintai.Hangzhou.Contract.Card;
using Yintai.Hangzhou.Contract.Comment;
using Yintai.Hangzhou.Contract.Coupon;
using Yintai.Hangzhou.Contract.Customer;
using Yintai.Hangzhou.Contract.Favorite;
using Yintai.Hangzhou.Contract.Feedback;
using Yintai.Hangzhou.Contract.HotWord;
using Yintai.Hangzhou.Contract.Like;
using Yintai.Hangzhou.Contract.Point;
using Yintai.Hangzhou.Contract.Product;
using Yintai.Hangzhou.Contract.ProductComplex;
using Yintai.Hangzhou.Contract.Promotion;
using Yintai.Hangzhou.Contract.Share;
using Yintai.Hangzhou.Contract.SpecialTopic;
using Yintai.Hangzhou.Contract.Store;
using Yintai.Hangzhou.Contract.Tag;
using Yintai.Hangzhou.Service;
using Yintai.Hangzhou.Service.Contract;
using Yintai.Hangzhou.Service.Contract.Apis;
using Yintai.Hangzhou.Service.Impl;
using Yintai.Hangzhou.Service.Impl.Apis;
using Yintai.Hangzhou.Service.Manager;

namespace Yintai.Hangzhou.WebSupport.Ioc
{
    internal class ServiceIocRegister : BaseIocRegister
    {
        #region Implementation of IIocRegister

        public override void Register()
        {
            //dataservice
            Current.Register<ICustomerDataService, CustomerDataService>();
            Current.Register<IStoreDataService, StoreDataService>();
            Current.Register<IPromotionDataService, PromotionDataService>();
            Current.Register<IShareDataService, ShareDataService>();
            Current.Register<IFavoriteDataService, FavoriteDataService>();
            Current.Register<ICouponDataService, CouponDataService>();
            Current.Register<IBrandDataService, BrandDataService>();
            Current.Register<IApnsDataService, ApnsDataService>();
            Current.Register<ICommentDataService, CommentDataService>();
            Current.Register<IProductDataService, ProductDataService>();
            Current.Register<ITagDataService, TagDataService>();
            Current.Register<ILikeDataService, LikeDataService>();
            Current.Register<IPointDataService, PointDataService>();
            Current.Register<IFeedbackDataService, FeedbackDataService>();
            Current.Register<IItemsDataService, ItemsDataService>();
            Current.Register<ISpecialTopicDataService, SpecialTopicDataService>();

            //service
            Current.Register<Service.Contract.IAuthenticationService, AuthenticationService>();
            Current.Register<ICouponService, CouponService>();
            Current.Register<IFavoriteService, FavoriteService>();
            Current.Register<Service.Contract.ILikeService, LikeService>();
            Current.Register<Service.Contract.IPointService, PointService>();
            Current.Register<Service.Contract.IPromotionService, PromotionService>();
            Current.Register<IRemindService, RemindService>();
            Current.Register<Service.Contract.IResourceService, ResourceService>();
            Current.Register<IShareService, ShareService>();
            Current.Register<IUserService, UserService>();
            Current.Register<IUserRightService, UserRightService>();
            Current.Register<MappingManagerV2, MappingManagerV2>();

            Current.Register<ICardDataService, CardDataService>();
            Current.RegisterSingleton<IGroupCardService, GroupCardService>();

            Current.Register<IHotwordDataService, HotwordDataService>();
        }

        #endregion
    }
}