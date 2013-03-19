using Passbook.Generator;
using Passbook.Generator.Fields;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Yintai.Architecture.Common;
using Yintai.Architecture.Common.Web.Mvc.ActionResults;
using Yintai.Architecture.Common.Web.Mvc.Controllers;
using Yintai.Architecture.Framework.ServiceLocation;
using Yintai.Hangzhou.Contract.Brand;
using Yintai.Hangzhou.Contract.Coupon;
using Yintai.Hangzhou.Contract.DTO.Request.Brand;
using Yintai.Hangzhou.Contract.DTO.Request.Coupon;
using Yintai.Hangzhou.Contract.DTO.Request.Pass;
using Yintai.Hangzhou.Contract.DTO.Response.Brand;
using Yintai.Hangzhou.Contract.DTO.Response.Coupon;
using Yintai.Hangzhou.Contract.DTO.Response.Store;
using Yintai.Hangzhou.Model;
using Yintai.Hangzhou.Model.Enums;
using Yintai.Hangzhou.WebSupport.Configuration;
using Yintai.Hangzhou.WebSupport.Mvc;

namespace Yintai.Hangzhou.WebApiCore.Areas.Api.Controllers
{
    public class CouponPassGeneratorRequest : PassGeneratorRequest
    {
        public CouponPassGeneratorRequest()
        {
            this.Style = PassStyle.Coupon;
        }

        public override void PopulateFields()
        {
            //this.AddPrimaryField(new NumberField("discount", "Discount", 0.01m, FieldNumberStyle.PKNumberStylePercent));
            //this.AddAuxiliaryField(new StandardField("expires", "expires", ""));
        }
    }

    public class PassHelper
    {
        // private const string IosUrlSchemesPre = "ytfs://";

        //private readonly ICouponDataService _couponDataService;
        //private readonly IStoreDataService _storeDataService;
        private readonly IBrandDataService _brandDataService;
        private readonly ICouponDataService _couponDataService;


        public PassHelper()
            : this(ServiceLocator.Current.Resolve<IBrandDataService>())
        {
            //_storeDataService = ServiceLocator.Current.Resolve<IStoreDataService>();
        }

        public PassHelper(IBrandDataService brandDataService)
        {
            _brandDataService = brandDataService;
            _couponDataService = ServiceLocator.Current.Resolve<ICouponDataService>();
        }

        public byte[] GetPass(HttpContextBase context, int couponId, string couponCode, int userId)
        {
            var coupon = _couponDataService.Get(new CouponInfoGetRequest()
                {
                    AuthUid = userId,
                    AuthUser = null,
                    CouponCode = couponCode,
                    CouponId = couponId,
                    Method = DefineRestfulMethod.Get,
                    Token = null,
                    Client_Version = null
                });

            if (coupon != null && coupon.IsSuccess && coupon.Data != null)
            {
                return GetPass(context, coupon.Data);
            }

            return null;
        }

        /// <summary>
        ///  ����pass
        /// </summary>
        /// <param name="context"></param>
        /// <param name="coupponInfo"></param>
        /// <returns></returns>
        public byte[] GetPass(HttpContextBase context, CouponInfoResponse coupponInfo)
        {
            var generator = new PassGenerator();

            var request = new CouponPassGeneratorRequest
                {
                    Identifier = ConfigurationManager.AppSettings["Identifier"],
                    CertThumbprint =
                        ConfigurationManager.AppSettings["PassBookCertificateThumbprint"].Replace(" ", String.Empty),
                    CertLocation = System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
                    SerialNumber = coupponInfo.Id.ToString(CultureInfo.InvariantCulture),
                    Description = coupponInfo.ProductDescription,
                    OrganizationName = ConfigurationManager.AppSettings["OrganizationName"],
                    TeamIdentifier = ConfigurationManager.AppSettings["TeamIdentifier"],
                    BackgroundColor = "rgb(229,1,80)",
                    ForegroundColor = "rgb(255,255,255)",
                    LabelColor = "rgb(255,255,255)",
                    AssociatedStoreIdentifiers = new List<int>(1)
                        {
                            ConfigManager.AppleAppid
                        }
                };
            //������


            //TODO:???�޸� ����Ӧ��ʹ��

            //var returnAppUrlSchemes = IosUrlSchemesPre + coupponInfo.User_Id.ToString(CultureInfo.InvariantCulture);

            //if (!String.IsNullOrWhiteSpace(returnAppUrlSchemes))
            //{
            //    //request.AddBackField(new StandardField("return app", "�����������ӷ���Ӧ��", "lafasogroupbuy://test"));
            //    request.AddBackField(new StandardField("return app", "�����������ӷ���Ӧ��", returnAppUrlSchemes));
            //}


            //request.Locale = "zh";

            // override icon and icon retina
            request.Images.Add(PassbookImage.Icon, System.IO.File.ReadAllBytes(context.Server.MapPath("~/Icons/coupon/icon.png")));
            //request.Images.Add(PassbookImage.IconRetina, System.IO.File.ReadAllBytes(context.Server.MapPath("~/Icons/coupon/icon@2x.png")));


            //request.Images.Add(PassbookImage.Strip, System.IO.File.ReadAllBytes(context.Server.MapPath("~/Icons/coupon/strip.png")));
            //request.Images.Add(PassbookImage.StripRetina, System.IO.File.ReadAllBytes(context.Server.MapPath("~/Icons/coupon/strip@2x.png")));

            request.Images.Add(PassbookImage.Logo, System.IO.File.ReadAllBytes(context.Server.MapPath("~/Icons/coupon/logo.png")));
            //request.Images.Add(PassbookImage.LogoRetina, System.IO.File.ReadAllBytes(context.Server.MapPath("~/Icons/coupon/logo@2x.png")));

            request.AddBarCode(coupponInfo.CouponId, BarcodeType.PKBarcodeFormatPDF417, "iso-8859-1" /*"UTF-8"*/, coupponInfo.CouponId);
            request.SuppressStripeShine = false;

            //��ȡƷ�� ����ȡ STORE
            var store = GetStore(coupponInfo);
            var brands = GetBrand(coupponInfo);

            string bName = String.Empty;
            //�ۿ۲���
            if (brands.Count > 0)
            {
                bName = brands[0].Name;
            }

            if (coupponInfo.SourceType == (int)SourceType.Product)
            {
                request.AddPrimaryField(new StandardField("primary1", bName, coupponInfo.ProductName));
            }

            if (coupponInfo.SourceType == (int)SourceType.Promotion)
            {
                request.AddPrimaryField(new StandardField("primary1", String.Empty, coupponInfo.ProductName));
            }

            //��Ч��
            //request.RelevantDate = coupponInfo.ValidEndDate;
            //request.IsRelative = true;
            request.AddSecondaryField(new DateField("secondary1", "��Ч��", coupponInfo.ValidStartDate, FieldDateTimeStyle.PKDateStyleLong, FieldDateTimeStyle.PKDateStyleNone));
            //
            request.AddSecondaryField(new StandardField("secondary2", String.Empty, "-"));

            request.AddSecondaryField(new DateField("secondary3", String.Empty, coupponInfo.ValidEndDate, FieldDateTimeStyle.PKDateStyleLong, FieldDateTimeStyle.PKDateStyleNone));

            request.AddBackField(new StandardField("back info1", coupponInfo.ProductName, coupponInfo.ProductDescription));
            request.AddBackField(new StandardField("back info2", "��Ч��", String.Format("{0} - {1}", coupponInfo.ValidStartDate.ToString("yyyy-M-d"), coupponInfo.ValidEndDate.ToString("yyyy-M-d"))));

            if (store != null)
            {
                request.LogoText = store.Name;//couponName;

                request.AddBackField(new StandardField("address", "��ַ", store.Location));
                request.AddBackField(new StandardField("ad2", "��ϵ�绰", store.Tel));
                var relevantest = String.Format("��������{0}�������������������鿴", store.Name);
                request.Locations.Add(
                    new Location
                        {
                            Latitude = store.Latitude,
                            Longitude = store.Longitude,
                            RelevantText = relevantest
                        });

                if (store.GpsLat != null && store.GpsLng != null)
                {
                    request.Locations.Add(new Location
                        {
                            Altitude = store.GpsAlt,
                            Latitude = store.GpsLat.Value,
                            Longitude = store.GpsLng.Value,
                            RelevantText = relevantest
                        });
                }
            }

            byte[] generatedPass = generator.Generate(request);

            return generatedPass;

            //return null;
        }

        private List<BrandInfoResponse> GetBrand(CouponInfoResponse couponInfo)
        {
            if (couponInfo.ProductInfoResponse != null)
            {
                if (couponInfo.ProductInfoResponse.BrandInfoResponse != null)
                {
                    return new List<BrandInfoResponse>(1) { couponInfo.ProductInfoResponse.BrandInfoResponse };
                }
            }

            if (couponInfo.PromotionInfoResponse != null)
            {
                if (couponInfo.PromotionInfoResponse.BrandIds != null &&
                    couponInfo.PromotionInfoResponse.BrandIds.Count > 0)
                {
                    var t = _brandDataService.GetAll(new BrandAllRequest
                    {
                        Refreshts = DateTime.Now
                    });

                    if (t.IsSuccess && t.Data != null && t.Data.Count > 0)
                    {
                        var b = t.Data.Where(v => couponInfo.PromotionInfoResponse.BrandIds.Any(s => s == v.Id)).ToList();

                        if (b.Count > 0)
                        {
                            return b;
                        }
                    }
                }
            }

            return new List<BrandInfoResponse>(0);
        }

        private static StoreInfoResponse GetStore(CouponInfoResponse couponInfo)
        {
            if (couponInfo.ProductInfoResponse != null)
            {
                return couponInfo.ProductInfoResponse.StoreInfoResponse;
            }

            if (couponInfo.PromotionInfoResponse != null)
            {
                return couponInfo.PromotionInfoResponse.StoreInfoResponse;
            }

            return null;
        }
    }

    public class PassController : RestfulController
    {
        private const string IosUrlSchemesPre = "ytfs://";

        private readonly ICouponDataService _couponDataService;

        private readonly PassHelper _passHelper;

        public PassController(ICouponDataService couponDataService, IBrandDataService brandDataService)
        {
            _couponDataService = couponDataService;
            _passHelper = new PassHelper(brandDataService);
        }

        [RestfulAuthorize]
        [HttpPost]
        public ActionResult Create(PassCreateRequest request, int? authuid, UserModel authUser)
        {
            var result =
                this._couponDataService.Get(new CouponInfoGetRequest()
                    {
                        AuthUid = authuid.Value,
                        CouponCode = request.CouponCode,
                        CouponId = request.CouponId,
                        Token = request.Token,
                        AuthUser = authUser
                    });

            if (result.IsSuccess && result.Data != null)
            {
                return Coupon(ControllerContext, result.Data);
            }

            return new RestfulResult { Data = result };
        }


        private ActionResult Coupon(ControllerContext controllerContext, CouponInfoResponse couponInfoResponse)
        {
            var p = _passHelper.GetPass(controllerContext.HttpContext, couponInfoResponse);

            return new FileContentResultV2(p, "application/vnd.apple.pkpass", DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture) + ".pkpass");
        }
    }

    public class FileContentResultV2 : FileContentResult
    {
        public FileContentResultV2(byte[] fileContents, string contentType)
            : base(fileContents, contentType)
        {
        }

        public FileContentResultV2(byte[] fileContents, string contentType, string fileName)
            : base(fileContents, contentType)
        {
            FileDownloadName = fileName;
        }
    }
}