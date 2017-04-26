﻿using System;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Repository.ActionPermissions;
using Quantumart.QP8.BLL.Repository.EntityPermissions;

namespace Quantumart.QP8.BLL.Services.ActionPermissions
{
	public class ActionPermissionChangeService : ActionPermissionChangeServiceAbstract
	{
		private Lazy<IPermissionRepository> permissionRepository = new Lazy<IPermissionRepository>(() => new ActionPermissionRepository());
		public override IPermissionRepository PermissionRepository { get { return permissionRepository.Value; } }

		private Lazy<IActionPermissionChangeRepository> changeRepository = new Lazy<IActionPermissionChangeRepository>(() => new ActionPermissionChangeRepository());
		public override IActionPermissionChangeRepository ChangeRepository { get { return changeRepository.Value; } }

		public override IPermissionViewModelSettings ViewModelSettings
		{
			get
			{
				return new GenericPermissionViewModelSettings
				{
					ActionCode = ActionCode.ChangeActionPermission,
					EntityTypeCode = EntityTypeCode.EntityTypePermission,
					IsPropagateable = false
				};
			}
		} 
	}
}
