﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Yintai.Architecture.Common.Models;

namespace Yintai.Hangzhou.Cms.WebSiteCoreV1.Models
{
    public class PointCollectionViewModel : PagerInfo, IViewModel
    {
        public PointCollectionViewModel(PagerRequest request)
            : base(request)
        {
        }

        public PointCollectionViewModel(PagerRequest request, int totalCount)
            : base(request, totalCount)
        {
        }

        public List<PointViewModel> Points { get; set; }
    }

    public class PointViewModel : BaseViewModel
    {
        [Key]
        public int Id { get; set; }

        [Range(0, Int32.MaxValue)]
        [Required]
        [Display(Name = "用户ID")]
        public int User_Id { get; set; }

        [Range(double.MinValue, double.MaxValue)]
        [Required]
        [Display(Name = "数量")]
        public decimal Amount { get; set; }

        [Range(0, Int32.MaxValue)]
        [Required]
        [Display(Name = "类型")]
        public int Type { get; set; }

        [StringLength(256, MinimumLength = 0)]
        [Display(Name = "标题")]
        public string Name { get; set; }

        [StringLength(1024, MinimumLength = 0)]
        [Display(Name = "说明")]
        public string Description { get; set; }

        [Range(0, Int32.MaxValue)]
        [Display(Name = "来源ID")]
        public int PointSourceId { get; set; }

        [Display(Name = "积分来源")]
        [Required]
        [Range(0, Int32.MaxValue)]
        public int PointSourceType { get; set; }


        [Display(Name = "状态")]
        public int Status { get; set; }
        [Display(Name = "创建人")]
        public int CreatedUser { get; set; }
        [Display(Name = "创建日期")]
        [DataType(DataType.DateTime)]
        public System.DateTime CreatedDate { get; set; }
        [Display(Name = "修改日期")]
        [DataType(DataType.DateTime)]
        public System.DateTime UpdatedDate { get; set; }
        [Display(Name = "修改人")]
        public int UpdatedUser { get; set; }
    }
}
