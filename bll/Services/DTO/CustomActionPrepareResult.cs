﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Services.DTO
{
	public class CustomActionPrepareResult
	{
		public CustomAction CustomAction { get; set; }
		public bool IsActionAccessable { get; set; }
		public string SecurityErrorMesage { get; set; }
	}
}
