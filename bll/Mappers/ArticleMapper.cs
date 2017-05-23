﻿using System;
using AutoMapper;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ArticleMapper : GenericMapper<Article, ArticleDAL>
    {
        public override void CreateBizMapper()
        {
            Mapper.CreateMap<ArticleDAL, Article>()
                .ForMember(biz => biz.LockedBy, opt => opt.MapFrom(src => Converter.ToInt32(src.LockedBy)))
                .ForMember(data => data.WorkflowBinding, opt => opt.Ignore());
        }

        public override void CreateDalMapper()
        {
            Mapper.CreateMap<Article, ArticleDAL>()
                .ForMember(data => data.Locked, opt => opt.MapFrom(src => src.LockedBy == 0 ? null : (DateTime?)src.Locked))
                .ForMember(data => data.LockedBy, opt => opt.MapFrom(src => src.LockedBy == 0 ? null : (int?)src.LockedBy))
                .ForMember(data => data.AccessRules, opt => opt.Ignore())
                .ForMember(data => data.Content, opt => opt.Ignore())
                .ForMember(data => data.ContentData, opt => opt.Ignore())
                .ForMember(data => data.ItemToItem, opt => opt.Ignore())
                .ForMember(data => data.ItemToItemVersions, opt => opt.Ignore())
                .ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
                .ForMember(data => data.LockedByUser, opt => opt.Ignore())
                .ForMember(data => data.Schedules, opt => opt.Ignore())
                .ForMember(data => data.Status, opt => opt.Ignore())
                .ForMember(data => data.StatusHistory, opt => opt.Ignore())
                .ForMember(data => data.Versions, opt => opt.Ignore())
                .ForMember(data => data.WorkflowBinding, opt => opt.Ignore());
        }
    }
}
