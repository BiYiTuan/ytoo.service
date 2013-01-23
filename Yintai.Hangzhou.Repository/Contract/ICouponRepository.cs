using System.Collections.Generic;
using Yintai.Architecture.Common.Models;
using Yintai.Hangzhou.Data.Models;
using Yintai.Hangzhou.Model.Enums;

namespace Yintai.Hangzhou.Repository.Contract
{
    public interface ICouponRepository : IRepository<CouponHistoryEntity, int>
    {
        /// <summary>
        /// �õ�ָ���û���COUPON
        /// </summary>
        /// <param name="pagerRequest"></param>
        /// <param name="totalCount"></param>
        /// <param name="userId"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        List<CouponHistoryEntity> GetPagedListByUserId(PagerRequest pagerRequest, out int totalCount, int userId, CouponSortOrder sortOrder);

        /// <summary>
        /// ��ȡ�û��Ż�ȯ�б�
        /// </summary>
        /// <param name="userId">�û�Id</param>
        /// <param name="sourceId">��ԴID</param>
        /// <param name="sourceType">��Դ����</param>
        /// <returns></returns>
        List<CouponHistoryEntity> GetListByUserIdSourceId(int userId, int sourceId, SourceType? sourceType);
    }
}