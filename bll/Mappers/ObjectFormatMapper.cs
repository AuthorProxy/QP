﻿using AutoMapper;
using Quantumart.QP8.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Mappers
{
	internal class ObjectFormatMapper : GenericMapper<ObjectFormat, ObjectFormatDAL>
	{
		public override void CreateDalMapper()
		{
			Mapper.CreateMap<ObjectFormat, ObjectFormatDAL>()
				.ForMember(data => data.NetLanguages, opt => opt.Ignore())
				.ForMember(data => data.Notifications, opt => opt.Ignore())
				.ForMember(data => data.Object, opt => opt.Ignore())
				.ForMember(data => data.Object1, opt => opt.Ignore())
				.ForMember(data => data.ObjectFormatVersion, opt => opt.Ignore())
				.ForMember(data => data.PageTraceFormat, opt => opt.Ignore())
				.ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
				.ForMember(data => data.Locked, opt => opt.MapFrom(src => (src.LockedBy == 0) ? null : (DateTime?)src.Locked))
				.ForMember(data => data.LockedBy, opt => opt.MapFrom(src => (src.LockedBy == 0) ? null : Utils.Converter.ToNullableDecimal((int?)src.LockedBy)))
				.ForMember(data => data.LockedByUser, opt => opt.Ignore())
				.AfterMap(SetDalProperties)
				;
		}		

		private static void SetDalProperties(ObjectFormat bizObject, ObjectFormatDAL dataObject)
		{
			if (!bizObject.Assembled.HasValue)
			{
				using (new QPConnectionScope())
				{
					dataObject.Assembled = Common.GetSqlDate(QPConnectionScope.Current.DbConnection);
				}
			}
		}

	}	
}
