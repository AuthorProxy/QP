﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Helpers
{
	public class AppSettingsItem
	{
		public string Key { get; set; }

		public string Value { get; set; }

		public bool Invalid { get; set; }

		public void Validate(RulesException<Db> errors, int index, string[] dupNames)
		{
			List<string> messages = new List<string>();
			if (string.IsNullOrWhiteSpace(Key))
			{
				messages.Add(String.Format(DBStrings.KeyRequired, index));
			}

			if (dupNames.Contains(Key))
			{
				messages.Add(String.Format(DBStrings.KeyNotUnique, index));
			}

			if (string.IsNullOrWhiteSpace(Value))
			{
				messages.Add(String.Format(DBStrings.ValueRequired, index));
			}

			if (Key.Length > 255)
			{
				messages.Add(String.Format(DBStrings.KeyMaxLengthExceeded, index));
			}

			if (Value.Length > 255)
			{
				messages.Add(String.Format(DBStrings.ValueMaxLengthExceeded, index));
			}
			
			Invalid = messages.Any();
			foreach (var message in messages)
				errors.ErrorForModel(message);

		}
	}
}
