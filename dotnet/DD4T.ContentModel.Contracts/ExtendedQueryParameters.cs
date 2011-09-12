using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DD4T.ContentModel.Factories
{
    public class ExtendedQueryParameters
    {
        public enum QueryLogic
        {
            AllCriteriaMatch,
            AnyCriteriaMatch
        }

        public enum MetaQueryOrder
        {
            Ascending,
            Descending
        }

        public enum MetaSortFieldType
        {
            DATETIME,
            FLOAT,
            STRING
        }

        public string[] QuerySchemas { get; set; }
        public IList<MetaQueryItem> MetaQueryValues { get; set; }
        public QueryLogic MetaQueryLogic { get; set; }

        public IList<KeywordItem> KeywordValues { get; set; }
        public QueryLogic KeywordQueryLogic { get; set; }

        public DateTime LastPublishedDate { get; set; }
        public string QuerySortField { get; set; }
        public MetaQueryOrder QuerySortOrder { get; set; }
        public int MaximumComponents { get; set; }
        public MetaSortFieldType SortType { get; set; }

        public ExtendedQueryParameters()
        {
            // Default all parameters
            QuerySchemas = new string[]{};
            MetaQueryValues = new List<MetaQueryItem>();
            MetaQueryLogic = QueryLogic.AllCriteriaMatch;

            KeywordValues = new List<KeywordItem>();
            KeywordQueryLogic = QueryLogic.AllCriteriaMatch;

            LastPublishedDate = DateTime.MinValue;

            QuerySortField = "ItemTitle";
            SortType = MetaSortFieldType.STRING;
            QuerySortOrder = MetaQueryOrder.Ascending;
            MaximumComponents = int.MaxValue;
        }
    }

    public class KeywordItem
    {
        public string KeywordUri { get; set; }
        public string CategoryUri { get; set; }

        public KeywordItem(string category, string keyword)
        {
            this.KeywordUri = keyword;
            this.CategoryUri = category;
        }
    }

    public class MetaQueryItem
    {
        private readonly string[] supportedTypes = { "Float", "DateTime", "String" };
        private object metaValueData;

        public enum QueryOperator
        {
            Equal,
            NotEqual,
            LessThanOrEqual,
            LessThan,
            GreaterThanOrEqual,
            GreaterThan,
            Like
        }

        public string MetaField { get; set; }
        public object MetaValue
        {
            get { return metaValueData; }
            set
            {
                if (!supportedTypes.Contains(value.GetType().Name))
                {
                    throw new Exception("MetaData querying only supports the types; " + string.Join(", ", supportedTypes));
                }
                metaValueData = value;
            }
        }
        public QueryOperator MetaOperator { get; set; }

        public MetaQueryItem(string fieldName, object fieldValue): this(fieldName, fieldValue, QueryOperator.Equal)
        {
        }

        public MetaQueryItem(string fieldName, object fieldValue, QueryOperator fieldOperator)
        {
            MetaField = fieldName;
            MetaValue = fieldValue;
            MetaOperator = fieldOperator;
        }
    }
}
