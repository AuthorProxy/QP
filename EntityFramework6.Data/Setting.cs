// Code generated by a template
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
namespace Quantumart.QP8.EntityFramework6.Data
{
    public partial class Setting: IQPArticle
    {
        public Setting()
        {
            RelatedSettings = new HashSet<Setting>();
            BackwardForRelatedSettings = new HashSet<Setting>();
        }

        public virtual Int32 Id { get; set; }
        public virtual Int32 StatusTypeId { get; set; }
        public virtual bool Visible { get; set; }
        public virtual bool Archive { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual DateTime Modified { get; set; }
        public virtual Int32 LastModifiedBy { get; set; }
        public virtual StatusType StatusType { get; set; }

        public virtual String Title { get; set; }
        public virtual String ValueMapped { get; set; }
        public virtual Decimal? DecimalValue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ICollection<Setting> RelatedSettings { get; set; }
        /// <summary>
        /// Auto-generated backing property for 1657/RelatedSettings
        /// </summary>
        public ICollection<Setting> BackwardForRelatedSettings { get; set; }
    }
}
