﻿using System;
using System.ServiceProcess;
using Microsoft.Practices.Unity;
using QP8.Infrastructure.Logging.UnityExtensions;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Scheduler.API;
using Quantumart.QP8.Scheduler.Core;
using Quantumart.QP8.Scheduler.Notification;
using Quantumart.QP8.Scheduler.Users;

namespace Quantumart.QP8.Scheduler.Service
{
    internal class ServiceConfiguration : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Container.AddNewExtension<SchedulerUsersConfiguration>();
            Container.AddNewExtension<SchedulerNotificationConfiguration>();
            Container.AddNewExtension<SchedulerCoreConfiguration>();
            Container.AddNewExtension<NLogContainerExtension>();

            var descriptors = Container.ResolveAll<ServiceDescriptor>();
            foreach (var descriptor in descriptors)
            {
                Container.RegisterType<ServiceBase, SchedulerService>(descriptor.Name, new InjectionFactory(c => new SchedulerService(c.Resolve<Func<IUnityContainer>>(descriptor.Key), descriptor)));
            }

            QPContext.SetUnityContainer(Container);
        }
    }
}