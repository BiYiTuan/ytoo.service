using System.Collections.Generic;
using Yintai.Architecture.Common.Models;
using Yintai.Hangzhou.Data.Models;
using Yintai.Hangzhou.Model.Enums;

namespace Yintai.Hangzhou.Repository.Contract
{
    public interface IFavoriteRepository : IRepository<FavoriteEntity, int>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="sourceType"></param>
        /// <param name="sourceid"></param>
        /// <returns></returns>
        FavoriteEntity GetItem(int userid, SourceType sourceType, int sourceid);

        /// <summary>
        /// ��ҳ
        /// </summary>
        /// <param name="userid"> </param>
        /// <param name="pagerRequest"></param>
        /// <param name="totalCount"></param>
        /// <param name="sortOrder"> </param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        List<FavoriteEntity> GetPagedList(int userid, PagerRequest pagerRequest, out int totalCount, FavoriteSortOrder sortOrder, SourceType sourceType);
    }
}