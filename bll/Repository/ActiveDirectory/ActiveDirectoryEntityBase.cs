﻿using System.DirectoryServices;
using System.Linq;

namespace Quantumart.QP8.BLL.Repository.ActiveDirectory
{
	internal abstract class ActiveDirectoryEntityBase
	{
		public string Path { get; private set; }
		public string ReferencedPath { get; private set; }
		public string Name { get; private set; }
		public string[] MemberOf { get; private set; }

		public ActiveDirectoryEntityBase(SearchResult entity)
		{
			Path = entity.Path;
			ReferencedPath = Path.Replace("LDAP://", string.Empty);
			Name = GetValue<string>(entity, "cn");
			MemberOf = entity.Properties["memberOf"].OfType<string>().ToArray();
		}

		protected T GetValue<T>(SearchResult entity, string key)
		{
			return entity.Properties[key].OfType<T>().FirstOrDefault();
		}
	}
}