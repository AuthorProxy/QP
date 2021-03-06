﻿using AutoMapper;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.Scheduler.Notification.Data;

namespace Quantumart.QP8.Scheduler.Notification
{
    public class NotificationMapperProfile : Profile
    {
        protected override void Configure()
        {
            CreateMap<SystemNotificationModel, SystemNotificationDto>();
        }
    }
}
