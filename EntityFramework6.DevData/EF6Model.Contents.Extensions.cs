// Code generated by a template
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Quantumart.QP8.EntityFramework6.DevData
{
    public partial class MarketingProduct: IQPArticle
    {
        #region Static members
        protected static readonly Dictionary<string, Func<MarketingProduct, IQPFormService, string>> _valueExtractors = new Dictionary<string, Func<MarketingProduct,  IQPFormService, string>>
        {
			{ "Title", new Func<MarketingProduct, IQPFormService, string>((self, ctx) => self.Title != null ? ctx.ReplacePlaceholders(self.Title) : null) },
			{ "Alias", new Func<MarketingProduct, IQPFormService, string>((self, ctx) => self.Alias != null ? ctx.ReplacePlaceholders(self.Alias) : null) },
			{ "ProductType", new Func<MarketingProduct, IQPFormService, string>((self, ctx) => self.ProductType != null ? self.ProductType.ToString() : null) },
			{ "Benefit", new Func<MarketingProduct, IQPFormService, string>((self, ctx) => self.Benefit != null ? ctx.ReplacePlaceholders(self.Benefit) : null) },
			{ "ShortBenefit", new Func<MarketingProduct, IQPFormService, string>((self, ctx) => self.ShortBenefit != null ? ctx.ReplacePlaceholders(self.ShortBenefit) : null) },
			{ "Legal", new Func<MarketingProduct, IQPFormService, string>((self, ctx) => self.Legal != null ? ctx.ReplacePlaceholders(self.Legal) : null) },
			{ "Description", new Func<MarketingProduct, IQPFormService, string>((self, ctx) => self.Description != null ? ctx.ReplacePlaceholders(self.Description) : null) },
			{ "ShortDescription", new Func<MarketingProduct, IQPFormService, string>((self, ctx) => self.ShortDescription != null ? ctx.ReplacePlaceholders(self.ShortDescription) : null) },
			{ "Purpose", new Func<MarketingProduct, IQPFormService, string>((self, ctx) => self.Purpose != null ? ctx.ReplacePlaceholders(self.Purpose) : null) },
			{ "Family_ID", new Func<MarketingProduct, IQPFormService, string>((self, ctx) => self.Family_ID != null ? self.Family_ID.ToString() : null) },
			{ "TitleForFamily", new Func<MarketingProduct, IQPFormService, string>((self, ctx) => self.TitleForFamily != null ? ctx.ReplacePlaceholders(self.TitleForFamily) : null) },
			{ "CommentForFamily", new Func<MarketingProduct, IQPFormService, string>((self, ctx) => self.CommentForFamily != null ? self.CommentForFamily : null) },
			{ "MarketingSign_ID", new Func<MarketingProduct, IQPFormService, string>((self, ctx) => self.MarketingSign_ID != null ? self.MarketingSign_ID.ToString() : null) },
			{ "OldSiteId", new Func<MarketingProduct, IQPFormService, string>((self, ctx) => self.OldSiteId != null ? self.OldSiteId.ToString() : null) },
        };

        #endregion
        #region Genarated properties
        public Int32 ProductTypeExact { get { return this.ProductType == null ? default(Int32) : this.ProductType.Value; } }
        public Int32 Family_IDExact { get { return this.Family_ID == null ? default(Int32) : this.Family_ID.Value; } }
        public Int32 MarketingSign_IDExact { get { return this.MarketingSign_ID == null ? default(Int32) : this.MarketingSign_ID.Value; } }
        public Int32 OldSiteIdExact { get { return this.OldSiteId == null ? default(Int32) : this.OldSiteId.Value; } }
        #endregion
        #region Methods
        void IQPArticle.OnMaterialized(IQPLibraryService context)
        {
			this.Title = context.ReplacePlaceholders(this.Title);
			this.Alias = context.ReplacePlaceholders(this.Alias);
			this.Benefit = context.ReplacePlaceholders(this.Benefit);
			this.ShortBenefit = context.ReplacePlaceholders(this.ShortBenefit);
			this.Legal = context.ReplacePlaceholders(this.Legal);
			this.Description = context.ReplacePlaceholders(this.Description);
			this.ShortDescription = context.ReplacePlaceholders(this.ShortDescription);
			this.Purpose = context.ReplacePlaceholders(this.Purpose);
			this.TitleForFamily = context.ReplacePlaceholders(this.TitleForFamily);
        }

        // для Poco перенести из класса куда-нибудь, так как нарушается концепция доступа к БД
        Hashtable IQPArticle.Pack(IQPFormService context, params string[] propertyNames)
        {
            Hashtable table;

            if (propertyNames == null || propertyNames.Length == 0)
            {
                // todo: filter null values
                table = new Hashtable(_valueExtractors.ToDictionary(x => context.GetFormNameByNetNames("MarketingProduct", x.Key), y => y.Value(this, context)));
            }
            else
            {
                table = new Hashtable();
                foreach (var prop in propertyNames.Join(_valueExtractors.Keys, x => x, x => x, (x, y) => x))
                {
                    string value = _valueExtractors[prop](this, context);
                    table.Add(prop, value);
                }
            }

            return table;
        }
        #endregion
    }
    public partial class Product: IQPArticle
    {
        #region Static members
        protected static readonly Dictionary<string, Func<Product, IQPFormService, string>> _valueExtractors = new Dictionary<string, Func<Product,  IQPFormService, string>>
        {
			{ "MarketingProduct_ID", new Func<Product, IQPFormService, string>((self, ctx) => self.MarketingProduct_ID != null ? self.MarketingProduct_ID.ToString() : null) },
			{ "Type", new Func<Product, IQPFormService, string>((self, ctx) => self.Type != null ? self.Type.ToString() : null) },
			{ "PDF", new Func<Product, IQPFormService, string>((self, ctx) => self.PDF != null ? self.PDF : null) },
			{ "Legal", new Func<Product, IQPFormService, string>((self, ctx) => self.Legal != null ? self.Legal : null) },
			{ "Benefit", new Func<Product, IQPFormService, string>((self, ctx) => self.Benefit != null ? self.Benefit : null) },
			{ "SortOrder", new Func<Product, IQPFormService, string>((self, ctx) => self.SortOrder != null ? self.SortOrder.ToString() : null) },
			{ "MarketingSign_ID", new Func<Product, IQPFormService, string>((self, ctx) => self.MarketingSign_ID != null ? self.MarketingSign_ID.ToString() : null) },
			{ "StartDate", new Func<Product, IQPFormService, string>((self, ctx) => self.StartDate != null ? self.StartDate.ToString() : null) },
			{ "EndDate", new Func<Product, IQPFormService, string>((self, ctx) => self.EndDate != null ? self.EndDate.ToString() : null) },
			{ "ArchiveTitle", new Func<Product, IQPFormService, string>((self, ctx) => self.ArchiveTitle != null ? ctx.ReplacePlaceholders(self.ArchiveTitle) : null) },
			{ "ArchiveNotes", new Func<Product, IQPFormService, string>((self, ctx) => self.ArchiveNotes != null ? ctx.ReplacePlaceholders(self.ArchiveNotes) : null) },
			{ "OldSiteId", new Func<Product, IQPFormService, string>((self, ctx) => self.OldSiteId != null ? self.OldSiteId.ToString() : null) },
        };

        #endregion
        #region Genarated properties
        public string PDFUrl { get; set; }
        public string PDFUploadPath { get; set; }
        public Int32 TypeExact { get { return this.Type == null ? default(Int32) : this.Type.Value; } }
        public Int32 SortOrderExact { get { return this.SortOrder == null ? default(Int32) : this.SortOrder.Value; } }
        public Int32 MarketingSign_IDExact { get { return this.MarketingSign_ID == null ? default(Int32) : this.MarketingSign_ID.Value; } }
        public Int32 OldSiteIdExact { get { return this.OldSiteId == null ? default(Int32) : this.OldSiteId.Value; } }
        #endregion
        #region Methods
        void IQPArticle.OnMaterialized(IQPLibraryService context)
        {
			this.ArchiveTitle = context.ReplacePlaceholders(this.ArchiveTitle);
			this.ArchiveNotes = context.ReplacePlaceholders(this.ArchiveNotes);
			this.PDFUrl = context.GetUrl(this.PDF, "Product", "PDF");
			this.PDFUploadPath = context.GetUploadPath(this.PDF, "Product", "PDF");
        }

        // для Poco перенести из класса куда-нибудь, так как нарушается концепция доступа к БД
        Hashtable IQPArticle.Pack(IQPFormService context, params string[] propertyNames)
        {
            Hashtable table;

            if (propertyNames == null || propertyNames.Length == 0)
            {
                // todo: filter null values
                table = new Hashtable(_valueExtractors.ToDictionary(x => context.GetFormNameByNetNames("Product", x.Key), y => y.Value(this, context)));
            }
            else
            {
                table = new Hashtable();
                foreach (var prop in propertyNames.Join(_valueExtractors.Keys, x => x, x => x, (x, y) => x))
                {
                    string value = _valueExtractors[prop](this, context);
                    table.Add(prop, value);
                }
            }

            return table;
        }
        #endregion
    }
    public partial class ProductParameter: IQPArticle
    {
        #region Static members
        protected static readonly Dictionary<string, Func<ProductParameter, IQPFormService, string>> _valueExtractors = new Dictionary<string, Func<ProductParameter,  IQPFormService, string>>
        {
			{ "Title", new Func<ProductParameter, IQPFormService, string>((self, ctx) => self.Title != null ? self.Title : null) },
			{ "Product_ID", new Func<ProductParameter, IQPFormService, string>((self, ctx) => self.Product_ID != null ? self.Product_ID.ToString() : null) },
			{ "GroupMapped_ID", new Func<ProductParameter, IQPFormService, string>((self, ctx) => self.GroupMapped_ID != null ? self.GroupMapped_ID.ToString() : null) },
			{ "BaseParameter_ID", new Func<ProductParameter, IQPFormService, string>((self, ctx) => self.BaseParameter_ID != null ? self.BaseParameter_ID.ToString() : null) },
			{ "Zone_ID", new Func<ProductParameter, IQPFormService, string>((self, ctx) => self.Zone_ID != null ? self.Zone_ID.ToString() : null) },
			{ "Direction_ID", new Func<ProductParameter, IQPFormService, string>((self, ctx) => self.Direction_ID != null ? self.Direction_ID.ToString() : null) },
			{ "SortOrder", new Func<ProductParameter, IQPFormService, string>((self, ctx) => self.SortOrder != null ? self.SortOrder.ToString() : null) },
			{ "NumValue", new Func<ProductParameter, IQPFormService, string>((self, ctx) => self.NumValue != null ? self.NumValue.ToString() : null) },
			{ "Value", new Func<ProductParameter, IQPFormService, string>((self, ctx) => self.Value != null ? ctx.ReplacePlaceholders(self.Value) : null) },
			{ "Unit_ID", new Func<ProductParameter, IQPFormService, string>((self, ctx) => self.Unit_ID != null ? self.Unit_ID.ToString() : null) },
			{ "Legal", new Func<ProductParameter, IQPFormService, string>((self, ctx) => self.Legal != null ? ctx.ReplacePlaceholders(self.Legal) : null) },
			{ "ShortTitle", new Func<ProductParameter, IQPFormService, string>((self, ctx) => self.ShortTitle != null ? self.ShortTitle : null) },
			{ "ShortValue", new Func<ProductParameter, IQPFormService, string>((self, ctx) => self.ShortValue != null ? ctx.ReplacePlaceholders(self.ShortValue) : null) },
			{ "MatrixParameter_ID", new Func<ProductParameter, IQPFormService, string>((self, ctx) => self.MatrixParameter_ID != null ? self.MatrixParameter_ID.ToString() : null) },
			{ "OldSiteId", new Func<ProductParameter, IQPFormService, string>((self, ctx) => self.OldSiteId != null ? self.OldSiteId.ToString() : null) },
        };

        #endregion
        #region Genarated properties
        public Int32 GroupMapped_IDExact { get { return this.GroupMapped_ID == null ? default(Int32) : this.GroupMapped_ID.Value; } }
        public Int32 BaseParameter_IDExact { get { return this.BaseParameter_ID == null ? default(Int32) : this.BaseParameter_ID.Value; } }
        public Int32 Zone_IDExact { get { return this.Zone_ID == null ? default(Int32) : this.Zone_ID.Value; } }
        public Int32 Direction_IDExact { get { return this.Direction_ID == null ? default(Int32) : this.Direction_ID.Value; } }
        public Int32 SortOrderExact { get { return this.SortOrder == null ? default(Int32) : this.SortOrder.Value; } }
        public Double NumValueExact { get { return this.NumValue == null ? default(Double) : this.NumValue.Value; } }
        public Int32 Unit_IDExact { get { return this.Unit_ID == null ? default(Int32) : this.Unit_ID.Value; } }
        public Int32 MatrixParameter_IDExact { get { return this.MatrixParameter_ID == null ? default(Int32) : this.MatrixParameter_ID.Value; } }
        public Int32 OldSiteIdExact { get { return this.OldSiteId == null ? default(Int32) : this.OldSiteId.Value; } }
        #endregion
        #region Methods
        void IQPArticle.OnMaterialized(IQPLibraryService context)
        {
			this.Value = context.ReplacePlaceholders(this.Value);
			this.Legal = context.ReplacePlaceholders(this.Legal);
			this.ShortValue = context.ReplacePlaceholders(this.ShortValue);
        }

        // для Poco перенести из класса куда-нибудь, так как нарушается концепция доступа к БД
        Hashtable IQPArticle.Pack(IQPFormService context, params string[] propertyNames)
        {
            Hashtable table;

            if (propertyNames == null || propertyNames.Length == 0)
            {
                // todo: filter null values
                table = new Hashtable(_valueExtractors.ToDictionary(x => context.GetFormNameByNetNames("ProductParameter", x.Key), y => y.Value(this, context)));
            }
            else
            {
                table = new Hashtable();
                foreach (var prop in propertyNames.Join(_valueExtractors.Keys, x => x, x => x, (x, y) => x))
                {
                    string value = _valueExtractors[prop](this, context);
                    table.Add(prop, value);
                }
            }

            return table;
        }
        #endregion
    }
    public partial class Region: IQPArticle
    {
        #region Static members
        protected static readonly Dictionary<string, Func<Region, IQPFormService, string>> _valueExtractors = new Dictionary<string, Func<Region,  IQPFormService, string>>
        {
			{ "Title", new Func<Region, IQPFormService, string>((self, ctx) => self.Title != null ? ctx.ReplacePlaceholders(self.Title) : null) },
			{ "Parent_ID", new Func<Region, IQPFormService, string>((self, ctx) => self.Parent_ID != null ? self.Parent_ID.ToString() : null) },
			{ "Alias", new Func<Region, IQPFormService, string>((self, ctx) => self.Alias != null ? ctx.ReplacePlaceholders(self.Alias) : null) },
			{ "OldSiteId", new Func<Region, IQPFormService, string>((self, ctx) => self.OldSiteId != null ? self.OldSiteId.ToString() : null) },
        };

        #endregion
        #region Genarated properties
        public Int32 OldSiteIdExact { get { return this.OldSiteId == null ? default(Int32) : this.OldSiteId.Value; } }
        #endregion
        #region Methods
        void IQPArticle.OnMaterialized(IQPLibraryService context)
        {
			this.Title = context.ReplacePlaceholders(this.Title);
			this.Alias = context.ReplacePlaceholders(this.Alias);
        }

        // для Poco перенести из класса куда-нибудь, так как нарушается концепция доступа к БД
        Hashtable IQPArticle.Pack(IQPFormService context, params string[] propertyNames)
        {
            Hashtable table;

            if (propertyNames == null || propertyNames.Length == 0)
            {
                // todo: filter null values
                table = new Hashtable(_valueExtractors.ToDictionary(x => context.GetFormNameByNetNames("Region", x.Key), y => y.Value(this, context)));
            }
            else
            {
                table = new Hashtable();
                foreach (var prop in propertyNames.Join(_valueExtractors.Keys, x => x, x => x, (x, y) => x))
                {
                    string value = _valueExtractors[prop](this, context);
                    table.Add(prop, value);
                }
            }

            return table;
        }
        #endregion
    }
    public partial class MobileTariff: IQPArticle
    {
        #region Static members
        protected static readonly Dictionary<string, Func<MobileTariff, IQPFormService, string>> _valueExtractors = new Dictionary<string, Func<MobileTariff,  IQPFormService, string>>
        {
			{ "Product_ID", new Func<MobileTariff, IQPFormService, string>((self, ctx) => self.Product_ID != null ? self.Product_ID.ToString() : null) },
			{ "SplitInternetDeviceCount", new Func<MobileTariff, IQPFormService, string>((self, ctx) => self.SplitInternetDeviceCount != null ? self.SplitInternetDeviceCount.ToString() : null) },
        };

        #endregion
        #region Genarated properties
        public Int32 SplitInternetDeviceCountExact { get { return this.SplitInternetDeviceCount == null ? default(Int32) : this.SplitInternetDeviceCount.Value; } }
        #endregion
        #region Methods
        void IQPArticle.OnMaterialized(IQPLibraryService context)
        {
        }

        // для Poco перенести из класса куда-нибудь, так как нарушается концепция доступа к БД
        Hashtable IQPArticle.Pack(IQPFormService context, params string[] propertyNames)
        {
            Hashtable table;

            if (propertyNames == null || propertyNames.Length == 0)
            {
                // todo: filter null values
                table = new Hashtable(_valueExtractors.ToDictionary(x => context.GetFormNameByNetNames("MobileTariff", x.Key), y => y.Value(this, context)));
            }
            else
            {
                table = new Hashtable();
                foreach (var prop in propertyNames.Join(_valueExtractors.Keys, x => x, x => x, (x, y) => x))
                {
                    string value = _valueExtractors[prop](this, context);
                    table.Add(prop, value);
                }
            }

            return table;
        }
        #endregion
    }
    public partial class Setting: IQPArticle
    {
        #region Static members
        protected static readonly Dictionary<string, Func<Setting, IQPFormService, string>> _valueExtractors = new Dictionary<string, Func<Setting,  IQPFormService, string>>
        {
			{ "Title", new Func<Setting, IQPFormService, string>((self, ctx) => self.Title != null ? ctx.ReplacePlaceholders(self.Title) : null) },
			{ "ValueMapped", new Func<Setting, IQPFormService, string>((self, ctx) => self.ValueMapped != null ? ctx.ReplacePlaceholders(self.ValueMapped) : null) },
			{ "DecimalValue", new Func<Setting, IQPFormService, string>((self, ctx) => self.DecimalValue != null ? self.DecimalValue.ToString() : null) },
        };

        #endregion
        #region Genarated properties
        public Decimal DecimalValueExact { get { return this.DecimalValue == null ? default(Decimal) : this.DecimalValue.Value; } }
        #endregion
        #region Methods
        void IQPArticle.OnMaterialized(IQPLibraryService context)
        {
			this.Title = context.ReplacePlaceholders(this.Title);
			this.ValueMapped = context.ReplacePlaceholders(this.ValueMapped);
        }

        // для Poco перенести из класса куда-нибудь, так как нарушается концепция доступа к БД
        Hashtable IQPArticle.Pack(IQPFormService context, params string[] propertyNames)
        {
            Hashtable table;

            if (propertyNames == null || propertyNames.Length == 0)
            {
                // todo: filter null values
                table = new Hashtable(_valueExtractors.ToDictionary(x => context.GetFormNameByNetNames("Setting", x.Key), y => y.Value(this, context)));
            }
            else
            {
                table = new Hashtable();
                foreach (var prop in propertyNames.Join(_valueExtractors.Keys, x => x, x => x, (x, y) => x))
                {
                    string value = _valueExtractors[prop](this, context);
                    table.Add(prop, value);
                }
            }

            return table;
        }
        #endregion
    }
}