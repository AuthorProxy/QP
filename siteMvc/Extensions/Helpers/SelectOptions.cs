﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public class SelectOptions
    {
		public SelectOptions()
		{
			ReadOnly = false;
		}
		
		public bool ReadOnly { get; set; }
		public string DefaultOption { get; set; }
		public EntityDataListArgs EntityDataListArgs { get; set; }
	}
}
