﻿
namespace Yintai.Hangzhou.Model.Enums
{
    /// <summary>
    /// 数据状态枚举，与数据库status 要保持一致，不允许修改 值
    /// </summary>
    public enum DataStatus
    {
        /// <summary>
        /// 已删除（逻辑删除）
        /// </summary>
        Deleted = -1,
        /// <summary>
        /// 默认状态
        /// </summary>
        Default = 0,
        /// <summary>
        /// 正常状态
        /// </summary>
        Normal = 1,

        //#region  业务状态
        //// 业务状态 1000起

        ////优惠券1001~1100

        ///// <summary>
        ///// 已使用 优惠券
        ///// </summary>
        //CouponUse = 1001



        //#endregion


    }
}
