﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.DTO
{
	public class UserGroupListItem
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public bool SharedArticles { get; set; }

		public DateTime Created { get; set; }

		public DateTime Modified { get; set; }

		public int LastModifiedByUserId { get; set; }

		public string LastModifiedByUser { get; set; }


		public string SharedArticlesChecked { get { return SharedArticles ? "checked=\"checked\"" : null; } }
	}
}
