﻿using System.Collections.Generic;
using System.Data;
using AutoMapper;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
	public class DataRowMapper
	{
		public static void CreateMap<TModel>()
			where TModel : class
		{
			Mapper.CreateMap<DataRow, TModel>().ConvertUsing(src => Converter.ToModelFromDataRow<TModel>(src));
		}

		public TModel Map<TModel>(DataRow row)
			where TModel : class
		{
			return Mapper.Map<DataRow, TModel>(row);
		}

		public TModel[] Map<TModel>(IEnumerable<DataRow> rows)
			where TModel : class
		{
			return Mapper.Map<IEnumerable<DataRow>, TModel[]>(rows);
		}
	}
}
