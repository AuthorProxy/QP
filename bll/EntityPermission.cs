﻿using System.Linq;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.EntityPermissions;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL
{
    public class EntityPermission : EntityPermissionBase
    {
        public static readonly int UserMemberType = 1;
        public static readonly int GroupMemberType = 2;

        public IPermissionRepository Repository { get; private set; }

        public User User { get; set; }

        public UserGroup Group { get; set; }

        public EntityPermissionLevel PermissionLevel { get; set; }

        [LocalizedDisplayName("PropagateToItems", NameResourceType = typeof(EntityPermissionStrings))]
        public bool PropagateToItems { get; set; }

        [LocalizedDisplayName("MemberType", NameResourceType = typeof(EntityPermissionStrings))]
        public int MemberType { get; set; }

        [LocalizedDisplayName("ExplicitPermissionToRelatedContents", NameResourceType = typeof(EntityPermissionStrings))]
        public bool ExplicitPermissionToRelatedContents { get; set; }

        [LocalizedDisplayName("Hide", NameResourceType = typeof(EntityPermissionStrings))]
        public bool Hide { get; set; }

        internal static EntityPermission Create(int parentId, IPermissionRepository repository)
        {
            var permission = new EntityPermission();
            permission.InitNew(parentId, repository);
            return permission;
        }

        private void InitNew(int parentId, IPermissionRepository repository)
        {
            if (IsNew)
            {
                ParentEntityId = parentId;
            }

            Init(repository);
        }

        public void Init(IPermissionRepository repository)
        {
            Repository = repository;
            if (IsNew && !UserId.HasValue && !GroupId.HasValue)
            {
                MemberType = GroupMemberType;
                PermissionLevelId = CommonPermissionRepository.GetPermissionLevels().Single(l => l.Level == Constants.PermissionLevel.Read).Id;
            }
            else
            {
                if (UserId.HasValue)
                {
                    MemberType = UserMemberType;
                }
                else if (GroupId.HasValue)
                {
                    MemberType = GroupMemberType;
                }
            }
        }

        public void DoCustomBinding()
        {
            if (MemberType == UserMemberType)
            {
                GroupId = null;
            }
            else if (MemberType == GroupMemberType)
            {
                UserId = null;
            }
            else
            {
                GroupId = null;
                UserId = null;
            }

            if (IsNew)
            {
                if (MemberType == UserMemberType && UserId.HasValue)
                {
                    User = UserRepository.GetById(UserId.Value, true);
                }
                else if (MemberType == GroupMemberType && GroupId.HasValue)
                {
                    Group = UserGroupRepository.GetById(GroupId.Value);
                }
            }
        }

        public override void Validate()
        {
            var errors = new RulesException<EntityPermission>();
            base.Validate(errors);
            if (MemberType != UserMemberType && MemberType != GroupMemberType)
            {
                errors.ErrorForModel(EntityPermissionStrings.SystemError);
            }

            if (MemberType == UserMemberType && !UserId.HasValue)
            {
                errors.ErrorFor(m => m.UserId, EntityPermissionStrings.UserIsNotSelected);
            }

            if (MemberType == GroupMemberType && !GroupId.HasValue)
            {
                errors.ErrorFor(m => m.GroupId, EntityPermissionStrings.GroupIsNotSelected);
            }

            if (!errors.IsEmpty)
            {
                throw errors;
            }
        }

        protected override RulesException ValidateUnique(RulesException errors)
        {
            if (IsNew && !Repository.CheckUnique(this))
            {
                errors.ErrorForModel(EntityPermissionStrings.PermissionIsNotUnique);
            }

            return errors;
        }
    }
}
