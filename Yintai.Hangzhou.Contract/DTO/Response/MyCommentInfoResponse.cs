﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Yintai.Hangzhou.Contract.DTO.Response.Customer;
using Yintai.Hangzhou.Contract.DTO.Response.Resources;
using Yintai.Hangzhou.Contract.Response;

namespace Yintai.Hangzhou.Contract.DTO.Response
{
    [DataContract]
    public class MyCommentInfoResponse:BaseResponse
    {
        [DataMember(Name = "commentid")]
        public int Id { get; set; }

        [DataMember(Name = "content")]
        public string Content { get; set; }

         [DataMember(Name = "sourceid")]
        public int SourceId { get; set; }

        [DataMember(Name="sourcetype")]
        public int SourceType { get; set; }


        [DataMember(Name = "createddate")]
        public System.DateTime CreatedDate { get; set; }

        [DataMember(Name = "replyuser")]
        public UserInfoResponse ReplyUser { get; set; }

        [DataMember(Name = "resource")]
        public ResourceInfoResponse Resource { get; set; }
    }
}
