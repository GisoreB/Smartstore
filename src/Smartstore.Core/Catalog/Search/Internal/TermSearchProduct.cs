﻿using Smartstore.Core.Catalog.Products;
using Smartstore.Core.Localization;
using Smartstore.Core.Rules;
using Smartstore.Core.Rules.Filters;
using Smartstore.Core.Search;

namespace Smartstore.Core.Catalog.Search
{
    /// <summary>
    /// Helper for building catalog search query including <see cref="LocalizedProperty"/> using <see cref="MemberExpression"/>.
    /// </summary>
    internal class TermSearchProduct
    {
        public Product Product { get; set; }
        public LocalizedProperty Translation { get; set; }

        public static FilterExpression CreateFilter(
            Expression<Func<TermSearchProduct, string>> memberExpression,
            IAttributeSearchFilter filter,
            int languageId = 0)
        {
            Guard.NotNull(memberExpression);
            Guard.NotNull(filter);

            var pFilter = new FilterExpression
            {
                Descriptor = new FilterDescriptor<TermSearchProduct, string>(memberExpression, RuleScope.Product),
                Operator = filter.GetOperator(),
                Value = filter.Term
            };

            if (languageId == 0)
            {
                // Ignore LocalizedProperty.
                return pFilter;
            }

            var propertyName = ((MemberExpression)memberExpression.Body).Member.Name;

            var lpFilters = new FilterExpression[]
            {
                new()
                {
                    Descriptor = new FilterDescriptor<TermSearchProduct, int>(x => x.Translation.LanguageId, RuleScope.Other),
                    Operator = RuleOperator.IsEqualTo,
                    Value = languageId
                },
                new()
                {
                    Descriptor = new FilterDescriptor<TermSearchProduct, string>(x => x.Translation.LocaleKeyGroup, RuleScope.Other),
                    Operator = RuleOperator.IsEqualTo,
                    Value = "Product"
                },
                new()
                {
                    Descriptor = new FilterDescriptor<TermSearchProduct, string>(x => x.Translation.LocaleKey, RuleScope.Other),
                    Operator = RuleOperator.IsEqualTo,
                    Value = propertyName
                },
                new()
                {
                    Descriptor = new FilterDescriptor<TermSearchProduct, string>(x => x.Translation.LocaleValue, RuleScope.Other),
                    Operator = filter.GetOperator(),
                    Value = filter.Term
                }
            };

            // p.Name.StartsWith(term) || (lp.LanguageId == languageId && lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "Name" && lp.LocaleValue.StartsWith(term))
            var expressions = new FilterExpression[]
            {
                pFilter,
                new FilterExpressionGroup(typeof(TermSearchProduct), lpFilters)
                {
                    LogicalOperator = LogicalRuleOperator.And
                }
            };

            return new FilterExpressionGroup(typeof(TermSearchProduct), expressions)
            {
                LogicalOperator = LogicalRuleOperator.Or
            };
        }
    }
}
