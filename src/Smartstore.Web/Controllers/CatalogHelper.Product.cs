﻿using Microsoft.AspNetCore.Mvc.Rendering;
using Smartstore.Collections;
using Smartstore.ComponentModel;
using Smartstore.Core.Catalog.Attributes;
using Smartstore.Core.Catalog.Pricing;
using Smartstore.Core.Catalog.Products;
using Smartstore.Core.Catalog.Search;
using Smartstore.Core.Checkout.Cart;
using Smartstore.Core.Checkout.Tax;
using Smartstore.Core.Content.Media;
using Smartstore.Core.Content.Menus;
using Smartstore.Core.Localization;
using Smartstore.Core.Security;
using Smartstore.Core.Seo;
using Smartstore.Core.Stores;
using Smartstore.Diagnostics;
using Smartstore.Web.Infrastructure.Hooks;
using Smartstore.Web.Models.Catalog;
using Smartstore.Web.Models.Catalog.Mappers;
using Smartstore.Web.Models.Media;

namespace Smartstore.Web.Controllers
{
    public partial class CatalogHelper
    {
        public async Task<ProductDetailsModel> MapProductDetailsPageModelAsync(Product product, ProductVariantQuery query)
        {
            Guard.NotNull(product);

            var customer = _services.WorkContext.CurrentCustomer;
            var store = _services.StoreContext.CurrentStore;
            var modelContext = new ProductDetailsModelContext
            {
                Product = product,
                VariantQuery = query,
                Customer = customer,
                Store = store,
                Currency = _services.WorkContext.WorkingCurrency,
                BatchContext = _productService.CreateProductBatchContext(new[] { product }, store, customer, false),
                DisplayPrices = await _services.Permissions.AuthorizeAsync(Permissions.Catalog.DisplayPrice)
            };

            var model = await MapProductDetailsPageModelAsync(modelContext);

            // Specifications
            model.SpecificationAttributes = await PrepareProductSpecificationModelAsync(modelContext);

            // Reviews
            await PrepareProductReviewsModelAsync(model.ProductReviews, product, 10);

            // Tags
            await PrepareProductTagsModelAsync(model, product);

            // Related products
            await PrepareRelatedProductsModelAsync(model, product);

            // Also purchased products
            await PrepareAlsoPurchasedProductsModelAsync(model, product);

            // Custom mapping
            await MapperFactory.MapWithRegisteredMapperAsync(product, model, new { Context = modelContext, Quantity = 1 });

            return model;
        }

        protected internal virtual async Task<ProductDetailsModel> MapProductDetailsPageModelAsync(ProductDetailsModelContext modelContext)
        {
            Guard.NotNull(modelContext);

            var product = modelContext.Product;
            var query = modelContext.VariantQuery;
            var batchContext = modelContext.BatchContext;
            var isAssociatedProduct = modelContext.IsAssociatedProduct;
            var isBundleItem = modelContext.ProductBundleItem != null;

            using (_services.Chronometer.Step("PrepareProductDetailsPageModel"))
            {
                var model = new ProductDetailsModel
                {
                    Id = product.Id,
                    Name = product.GetLocalized(x => x.Name),
                    ShortDescription = product.GetLocalized(x => x.ShortDescription),
                    FullDescription = product.GetLocalized(x => x.FullDescription, detectEmptyHtml: true),
                    MetaKeywords = product.GetLocalized(x => x.MetaKeywords),
                    MetaDescription = product.GetLocalized(x => x.MetaDescription),
                    MetaTitle = product.GetLocalized(x => x.MetaTitle),
                    SeName = await product.GetActiveSlugAsync(),
                    ProductType = product.ProductType,
                    VisibleIndividually = product.Visibility != ProductVisibility.Hidden,
                    ReviewCount = product.ApprovedTotalReviews,
                    DisplayAdminLink = await _services.Permissions.AuthorizeAsync(Permissions.System.AccessBackend, batchContext.Customer),
                    Condition = product.Condition,
                    ShowCondition = _catalogSettings.ShowProductCondition,
                    LocalizedCondition = product.Condition.GetLocalizedEnum(_services.WorkContext.WorkingLanguage.Id, false),
                    ShowSku = _catalogSettings.ShowProductSku,
                    Sku = product.Sku,
                    ShowManufacturerPartNumber = _catalogSettings.ShowManufacturerPartNumber,
                    DisplayProductReviews = _catalogSettings.ShowProductReviewsInProductDetail && product.AllowCustomerReviews,
                    ManufacturerPartNumber = product.ManufacturerPartNumber,
                    ShowGtin = _catalogSettings.ShowGtin,
                    Gtin = product.Gtin,
                    StockAvailability = product.FormatStockMessage(_localizationService),
                    HasSampleDownload = product.IsDownload && product.HasSampleDownload,
                    IsCurrentCustomerRegistered = batchContext.Customer.IsRegistered(),
                    IsAssociatedProduct = isAssociatedProduct,
                    CompareEnabled = !isAssociatedProduct && _catalogSettings.CompareProductsEnabled,
                    TellAFriendEnabled = !isAssociatedProduct && _catalogSettings.EmailAFriendEnabled,
                    AskQuestionEnabled = !isAssociatedProduct && _catalogSettings.AskQuestionEnabled
                };

                #region Bundles / Grouped products

                if (product.ProductType == ProductType.GroupedProduct && !isAssociatedProduct)
                {
                    // Associated products.
                    var searchQuery = new CatalogSearchQuery()
                        .VisibleOnly(batchContext.Customer)
                        .HasStoreId(batchContext.Store.Id)
                        .HasParentGroupedProduct(product.Id);

                    modelContext.AssociatedProducts = await (await _catalogSearchService.SearchAsync(searchQuery)).GetHitsAsync();

                    // Push Ids of associated products to batch context to save roundtrips
                    batchContext.Collect(modelContext.AssociatedProducts.Select(x => x.Id).ToArray());

                    foreach (var associatedProduct in modelContext.AssociatedProducts)
                    {
                        var childModelContext = new ProductDetailsModelContext(modelContext)
                        {
                            Product = associatedProduct,
                            IsAssociatedProduct = true,
                            ProductBundleItem = null
                        };

                        var assciatedProductModel = await MapProductDetailsPageModelAsync(childModelContext);
                        model.AssociatedProducts.Add(assciatedProductModel);
                    }
                }
                else if (product.ProductType == ProductType.BundledProduct && !isBundleItem)
                {
                    var bundleItems = await batchContext.ProductBundleItems.GetOrLoadAsync(product.Id);
                    bundleItems = bundleItems.Where(x => x.Product.CanBeBundleItem()).ToList();

                    // Push Ids of bundle items to batch context to save roundtrips
                    batchContext.Collect(bundleItems.Select(x => x.ProductId).ToArray());

                    foreach (var bundleItem in bundleItems)
                    {
                        var childModelContext = new ProductDetailsModelContext(modelContext)
                        {
                            Product = bundleItem.Product,
                            IsAssociatedProduct = false,
                            ProductBundleItem = bundleItem
                        };

                        var bundledProductModel = await MapProductDetailsPageModelAsync(childModelContext);

                        bundledProductModel.ShowLegalInfo = false;
                        bundledProductModel.DeliveryTimesPresentation = DeliveryTimesPresentation.None;

                        bundledProductModel.BundleItem.Id = bundleItem.Id;
                        bundledProductModel.BundleItem.Quantity = bundleItem.Quantity;
                        bundledProductModel.BundleItem.HideThumbnail = bundleItem.HideThumbnail;
                        bundledProductModel.BundleItem.Visible = bundleItem.Visible;
                        bundledProductModel.BundleItem.IsBundleItemPricing = bundleItem.BundleProduct.BundlePerItemPricing;

                        var bundleItemName = bundleItem.GetLocalized(x => x.Name);
                        if (bundleItemName.Value.HasValue())
                        {
                            bundledProductModel.Name = bundleItemName;
                        }

                        var bundleItemShortDescription = bundleItem.GetLocalized(x => x.ShortDescription);
                        if (bundleItemShortDescription.Value.HasValue())
                        {
                            bundledProductModel.ShortDescription = bundleItemShortDescription;
                        }

                        model.BundledItems.Add(bundledProductModel);
                    }
                }

                #endregion

                #region Template

                var templateCacheKey = string.Format(ModelCacheInvalidator.PRODUCT_TEMPLATE_MODEL_KEY, product.ProductTemplateId);
                model.ProductTemplateViewPath = await _services.Cache.GetAsync(templateCacheKey, async () =>
                {
                    var template = await _db.ProductTemplates.FindByIdAsync(product.ProductTemplateId, false)
                        ?? await _db.ProductTemplates.AsNoTracking().OrderBy(x => x.DisplayOrder).FirstOrDefaultAsync();

                    return template.ViewPath;
                });

                #endregion

                #region Brands

                // Brands
                if (_catalogSettings.ShowManufacturerInProductDetail)
                {
                    var brands = _db.IsCollectionLoaded(product, x => x.ProductManufacturers)
                        ? product.ProductManufacturers
                        : await batchContext.ProductManufacturers.GetOrLoadAsync(product.Id);

                    model.Brands = await PrepareBrandOverviewModelAsync(brands, null, _catalogSettings.ShowManufacturerPicturesInProductDetail);
                }

                #endregion

                #region Review overview

                model.ReviewOverview.ProductId = product.Id;
                model.ReviewOverview.RatingSum = product.ApprovedRatingSum;
                model.ReviewOverview.TotalReviews = product.ApprovedTotalReviews;
                model.ReviewOverview.AllowCustomerReviews = product.AllowCustomerReviews;

                #endregion

                #region Giftcard

                // Get gift card values from query string.
                if (product.IsGiftCard)
                {
                    model.GiftCard.RecipientName = query.GetGiftCardValue(product.Id, 0, "RecipientName");
                    model.GiftCard.RecipientEmail = query.GetGiftCardValue(product.Id, 0, "RecipientEmail");
                    model.GiftCard.SenderName = query.GetGiftCardValue(product.Id, 0, "SenderName");
                    model.GiftCard.SenderEmail = query.GetGiftCardValue(product.Id, 0, "SenderEmail");
                    model.GiftCard.Message = query.GetGiftCardValue(product.Id, 0, "Message");
                }

                #endregion

                #region Stock subscription

                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                     product.BackorderMode == BackorderMode.NoBackorders &&
                     product.AllowBackInStockSubscriptions &&
                     product.StockQuantity <= 0)
                {
                    // Out of stock.
                    model.DisplayBackInStockSubscription = true;
                    model.BackInStockAlreadySubscribed = await _stockSubscriptionService.IsSubscribedAsync(product, batchContext.Customer, batchContext.Store.Id);
                }

                #endregion

                // ----> Core mapper <------
                await PrepareProductDetailModelAsync(model, modelContext, 1);

                #region Action items

                if (model.HasSampleDownload)
                {
                    model.ActionItems["sample"] = new ProductDetailsModel.ActionItemModel
                    {
                        Key = "sample",
                        Title = T("Products.DownloadSample"),
                        CssClass = "action-download-sample",
                        IconCssClass = "fa fa-download",
                        Href = _urlHelper.Action("Sample", "Download", new { productId = model.Id }),
                        IsPrimary = true,
                        PrimaryActionColor = "danger"
                    };
                }

                if (!model.AddToCart.DisableWishlistButton && model.ProductType != ProductType.GroupedProduct)
                {
                    model.ActionItems["wishlist"] = new ProductDetailsModel.ActionItemModel
                    {
                        Key = "wishlist",
                        Title = T("ShoppingCart.AddToWishlist.Short"),
                        Tooltip = T("ShoppingCart.AddToWishlist"),
                        CssClass = "ajax-cart-link action-add-to-wishlist",
                        IconCssClass = "icm icm-heart",
                        Href = _urlHelper.Action("AddProduct", "ShoppingCart", new { productId = model.Id, shoppingCartTypeId = (int)ShoppingCartType.Wishlist })
                    };
                }

                if (model.CompareEnabled)
                {
                    model.ActionItems["compare"] = new ProductDetailsModel.ActionItemModel
                    {
                        Key = "compare",
                        Title = T("Common.Shopbar.Compare"),
                        Tooltip = T("Products.Compare.AddToCompareList"),
                        CssClass = "action-compare ajax-cart-link",
                        IconCssClass = "icm icm-repeat",
                        Href = _urlHelper.Action("AddProductToCompare", "Catalog", new { id = model.Id })
                    };
                }

                if (model.AskQuestionEnabled && !model.Price.CallForPrice)
                {
                    model.ActionItems["ask"] = new ProductDetailsModel.ActionItemModel
                    {
                        Key = "ask",
                        Title = T("Products.AskQuestion.Short"),
                        Tooltip = T("Products.AskQuestion"),
                        CssClass = "action-ask-question",
                        IconCssClass = "icm icm-envelope",
                        Href = _urlHelper.Action("AskQuestion", new { id = model.Id })
                    };
                }

                if (model.TellAFriendEnabled)
                {
                    model.ActionItems["tell"] = new ProductDetailsModel.ActionItemModel
                    {
                        Key = "tell",
                        Title = T("Products.EmailAFriend"),
                        CssClass = "action-bullhorn",
                        IconCssClass = "icm icm-bullhorn",
                        Href = _urlHelper.Action("EmailAFriend", new { id = model.Id })
                    };
                }

                #endregion

                #region Media

                ICollection<int> combinationFileIds = null;
                ProductVariantAttributeCombination combination = null;

                if (modelContext.ProductBundleItem == null)
                {
                    combinationFileIds = await _productAttributeService.GetAttributeCombinationFileIdsAsync(product);
                    combination ??= model.SelectedCombination;
                }

                var productMediaFiles = _db.IsCollectionLoaded(product, x => x.ProductMediaFiles)
                    ? product.ProductMediaFiles
                    : await batchContext.ProductMediaFiles.GetOrLoadAsync(product.Id);

                var files = productMediaFiles
                    .Where(x => x.MediaFile != null)
                    .Select(x => _mediaService.ConvertMediaFile(x.MediaFile))
                    .ToList();

                if (product.HasPreviewPicture && files.Count > 1)
                {
                    files.RemoveAt(0);
                }

                model.MediaGalleryModel = PrepareProductDetailsMediaGalleryModel(
                    files, model.Name, combinationFileIds, isAssociatedProduct, modelContext.ProductBundleItem, combination);

                #endregion

                return model;
            }
        }

        public async Task PrepareProductDetailModelAsync(
            ProductDetailsModel model, 
            ProductDetailsModelContext modelContext, 
            int selectedQuantity = 1,
            bool callCustomMapper = false)
        {
            Guard.NotNull(model);
            Guard.NotNull(modelContext);

            var product = modelContext.Product;

            model.WeightValue = product.Weight;
            model.IsBundlePart = product.ProductType != ProductType.BundledProduct && modelContext.ProductBundleItem != null;

            // Attributes and attribute combination
            await PrepareProductAttributesModelAsync(model, modelContext, selectedQuantity);

            // Price
            await PrepareProductPriceModelAsync(model, modelContext, selectedQuantity);

            // General properties (must come after price mapping)
            await PrepareProductPropertiesModelAsync(model, modelContext);

            // AddToCart
            await PrepareProductCartModelAsync(model, modelContext, selectedQuantity);

            // GiftCards
            PrepareProductGiftCardsModel(model, modelContext);

            // Custom mapping
            if (callCustomMapper)
            {
                await MapperFactory.MapWithRegisteredMapperAsync(product, model, new { Context = modelContext, Quantity = selectedQuantity });
            }

            _services.DisplayControl.Announce(product);
        }

        #region PrepareProductDetailModelAsync helper methods

        protected internal async Task PrepareProductAttributesModelAsync(ProductDetailsModel model, ProductDetailsModelContext modelContext, int selectedQuantity)
        {
            var product = modelContext.Product;

            if (product.ProductType == ProductType.BundledProduct)
            {
                // Bundles don't have attributes.
                return;
            }

            using var chronometer = _services.Chronometer.Step("PrepareProductAttributesModel");

            var query = modelContext.VariantQuery;
            var productBundleItem = modelContext.ProductBundleItem;
            var bundleItemId = productBundleItem?.Id ?? 0;
            var isBundlePricing = productBundleItem != null && !productBundleItem.BundleProduct.BundlePerItemPricing;
            var attributes = await modelContext.BatchContext.Attributes.GetOrLoadAsync(product.Id);
            var pricingOptions = _priceCalculationService.CreateDefaultOptions(false, modelContext.Customer, null, modelContext.BatchContext);
            var linkedProducts = new Dictionary<int, Product>();
            var linkedMediaFiles = new Multimap<int, ProductMediaFile>();
            var preselectedWeightAdjustment = 0m;

            // Key: ProductVariantAttributeValue.Id, value: attribute price adjustment.
            var priceAdjustments = modelContext.DisplayPrices && !isBundlePricing
                ? await _priceCalculationService.CalculateAttributePriceAdjustmentsAsync(product, null, selectedQuantity, pricingOptions)
                : new Dictionary<int, CalculatedPriceAdjustment>();

            var linkedProductIds = attributes
                .SelectMany(x => x.ProductVariantAttributeValues)
                .Where(x => x.ValueType == ProductVariantAttributeValueType.ProductLinkage && x.LinkedProductId != 0)
                .Select(x => x.LinkedProductId)
                .Distinct()
                .ToArray();

            if (linkedProductIds.Any())
            {
                linkedProducts = await _db.Products
                    .AsNoTracking()
                    .Where(x => linkedProductIds.Contains(x.Id) && x.Visibility != ProductVisibility.Hidden)
                    .ToDictionaryAsync(x => x.Id);

                if (_catalogSettings.ShowLinkedAttributeValueImage)
                {
                    linkedMediaFiles = (await _db.ProductMediaFiles
                        .AsNoTracking()
                        .Include(x => x.MediaFile)
                        .Where(x => linkedProductIds.Contains(x.ProductId))
                        .OrderBy(x => x.ProductId)
                        .ThenBy(x => x.DisplayOrder)
                        .ToListAsync())
                        .ToMultimap(x => x.ProductId, x => x);
                }
            }


            foreach (var attribute in attributes)
            {
                var preSelectedValueId = 0;
                var attributeValues = attribute.IsListTypeAttribute()
                    ? attribute.ProductVariantAttributeValues.OrderBy(x => x.DisplayOrder).ToList()
                    : new List<ProductVariantAttributeValue>();

                var attributeModel = new ProductDetailsModel.ProductVariantAttributeModel
                {
                    Id = attribute.Id,
                    ProductId = attribute.ProductId,
                    BundleItemId = bundleItemId,
                    ProductAttributeId = attribute.ProductAttributeId,
                    ProductAttribute = attribute,
                    Alias = attribute.ProductAttribute.Alias,
                    Name = attribute.ProductAttribute.GetLocalized(x => x.Name),
                    Description = attribute.ProductAttribute.GetLocalized(x => x.Description),
                    TextPrompt = attribute.TextPrompt,
                    CustomData = attribute.CustomData,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                    AllowedFileExtensions = _catalogSettings.FileUploadAllowedExtensions
                };

                // Copy queried variant data (entered by customer) to model.
                if (query.Variants.Any())
                {
                    var selectedAttribute = query.Variants.FirstOrDefault(x =>
                        x.ProductId == product.Id &&
                        x.BundleItemId == bundleItemId &&
                        x.AttributeId == attribute.ProductAttributeId &&
                        x.VariantAttributeId == attribute.Id);

                    if (selectedAttribute != null)
                    {
                        switch (attribute.AttributeControlType)
                        {
                            case AttributeControlType.Datepicker:
                                if (selectedAttribute.Date.HasValue)
                                {
                                    attributeModel.SelectedDate = selectedAttribute.Date;
                                }
                                break;
                            case AttributeControlType.FileUpload:
                                attributeModel.UploadedFileGuid = selectedAttribute.Value;

                                if (selectedAttribute.Value.HasValue() && Guid.TryParse(selectedAttribute.Value, out var guid))
                                {
                                    attributeModel.UploadedFileName = await _db.Downloads
                                        .AsNoTracking()
                                        .Where(x => x.DownloadGuid == guid)
                                        .Select(x => x.MediaFile.Name)
                                        .FirstOrDefaultAsync();
                                }
                                break;
                            case AttributeControlType.TextBox:
                            case AttributeControlType.MultilineTextbox:
                                attributeModel.TextValue = selectedAttribute.Value;
                                break;
                        }
                    }
                }

                foreach (var value in attributeValues)
                {
                    ProductBundleItemAttributeFilter attributeFilter = null;
                    if (productBundleItem?.IsFilteredOut(value, out attributeFilter) ?? false)
                    {
                        continue;
                    }
                    if (preSelectedValueId == 0 && attributeFilter != null && attributeFilter.IsPreSelected)
                    {
                        preSelectedValueId = attributeFilter.AttributeValueId;
                    }

                    var valueModel = new ProductDetailsModel.ProductVariantAttributeValueModel
                    {
                        Id = value.Id,
                        ProductAttributeValue = value,
                        PriceAdjustment = string.Empty,
                        Name = value.GetLocalized(x => x.Name),
                        Alias = value.Alias,
                        Color = value.Color, // Used with "Boxes" attribute type.
                        IsPreSelected = value.IsPreSelected
                    };

                    if (value.ValueType == ProductVariantAttributeValueType.ProductLinkage &&
                        linkedProducts.TryGetValue(value.LinkedProductId, out var linkedProduct))
                    {
                        valueModel.SeName = await linkedProduct.GetActiveSlugAsync();
                    }

                    if (modelContext.DisplayPrices && !isBundlePricing)
                    {
                        if (priceAdjustments.TryGetValue(value.Id, out var priceAdjustment))
                        {
                            valueModel.PriceAdjustmentValue = priceAdjustment.Price.Amount;

                            if (_priceSettings.ShowVariantCombinationPriceAdjustment && !product.CallForPrice)
                            {
                                if (priceAdjustment.Price > 0)
                                {
                                    valueModel.PriceAdjustment = $" (+{priceAdjustment.Price})";
                                }
                                else if (priceAdjustment.Price < 0)
                                {
                                    valueModel.PriceAdjustment = $" (-{priceAdjustment.Price * -1})";
                                }
                            }
                        }

                        if (valueModel.IsPreSelected)
                        {
                            preselectedWeightAdjustment += value.WeightAdjustment;
                        }

                        if (_catalogSettings.ShowLinkedAttributeValueQuantity && value.ValueType == ProductVariantAttributeValueType.ProductLinkage)
                        {
                            valueModel.QuantityInfo = value.Quantity;
                        }
                    }

                    if (_catalogSettings.ShowLinkedAttributeValueImage && value.ValueType == ProductVariantAttributeValueType.ProductLinkage)
                    {
                        var file = linkedMediaFiles.ContainsKey(value.LinkedProductId)
                            ? linkedMediaFiles[value.LinkedProductId].FirstOrDefault()?.MediaFile
                            : null;
                        if (file != null)
                        {
                            valueModel.ImageUrl = _mediaService.GetUrl(file, _mediaSettings.VariantValueThumbPictureSize, null, false);
                        }
                    }
                    else if (value.MediaFileId != 0)
                    {
                        valueModel.ImageUrl = await _mediaService.GetUrlAsync(value.MediaFileId, _mediaSettings.VariantValueThumbPictureSize, null, false);
                    }

                    attributeModel.Values.Add(valueModel);
                }

                // Add selected attributes for initially displayed combination images and multiple selected checkbox values.
                if (query.VariantCombinationId == 0)
                {
                    ProductDetailsModel.ProductVariantAttributeValueModel defaultValue = null;

                    if (preSelectedValueId != 0)
                    {
                        // Value preselected by a bundle item filter discards the default preselection.
                        attributeModel.Values.Each(x => x.IsPreSelected = false);

                        defaultValue = attributeModel.Values.OfType<ProductDetailsModel.ProductVariantAttributeValueModel>().FirstOrDefault(v => v.Id == preSelectedValueId);
                        if (defaultValue != null)
                        {
                            defaultValue.IsPreSelected = true;
                            query.AddVariant(new()
                            {
                                Value = defaultValue.Id.ToString(),
                                ProductId = product.Id,
                                BundleItemId = bundleItemId,
                                AttributeId = attribute.ProductAttributeId,
                                VariantAttributeId = attribute.Id,
                                Alias = attribute.ProductAttribute.Alias,
                                ValueAlias = defaultValue.Alias
                            });
                        }
                    }

                    if (defaultValue == null)
                    {
                        // Apply attributes preselected by merchant.
                        foreach (var value in attributeModel.Values.Where(x => x.IsPreSelected))
                        {
                            query.AddVariant(new()
                            {
                                Value = value.Id.ToString(),
                                ProductId = product.Id,
                                BundleItemId = bundleItemId,
                                AttributeId = attribute.ProductAttributeId,
                                VariantAttributeId = attribute.Id,
                                Alias = attribute.ProductAttribute.Alias,
                                ValueAlias = value.Alias
                            });
                        }
                    }
                }

                model.ProductVariantAttributes.Add(attributeModel);
            }

            if (query.Variants.Any() || query.VariantCombinationId != 0)
            {
                // Apply attribute combination if any.
                await PrepareProductAttributeCombinationsModelAsync(model, modelContext);
            }
            else
            {
                // Apply weight adjustment of preselected attributes.
                model.WeightValue += preselectedWeightAdjustment;
            }
        }

        protected async Task PrepareProductAttributeCombinationsModelAsync(ProductDetailsModel model, ProductDetailsModelContext modelContext)
        {
            using var chronometer = _services.Chronometer.Step("PrepareProductAttributeCombinationsModel");

            var product = modelContext.Product;
            var query = modelContext.VariantQuery;
            var productBundleItem = modelContext.ProductBundleItem;
            var bundleItemId = productBundleItem?.Id ?? 0;
            var isBundlePricing = productBundleItem != null && !productBundleItem.BundleProduct.BundlePerItemPricing;
            var checkAvailability = product.AttributeChoiceBehaviour == AttributeChoiceBehaviour.GrayOutUnavailable;
            var attributes = await modelContext.BatchContext.Attributes.GetOrLoadAsync(product.Id);

            var res = new Dictionary<string, LocalizedString>(StringComparer.OrdinalIgnoreCase)
            {
                { "Products.Availability.IsNotActive", T("Products.Availability.IsNotActive") },
                { "Products.Availability.OutOfStock", T("Products.Availability.OutOfStock") },
                { "Products.Availability.Backordering", T("Products.Availability.Backordering") },
            };

            if (query.VariantCombinationId != 0)
            {
                var combination = await _db.ProductVariantAttributeCombinations.FindByIdAsync(query.VariantCombinationId, false);
                modelContext.SelectedAttributes = new ProductVariantAttributeSelection(combination?.RawAttributes);
            }
            else
            {
                var (selection, _) = await _productAttributeMaterializer.CreateAttributeSelectionAsync(query, attributes, product.Id, bundleItemId);
                modelContext.SelectedAttributes = selection;
            }

            var selectedValues = await _productAttributeMaterializer.MaterializeProductVariantAttributeValuesAsync(modelContext.SelectedAttributes);
            //var hasSelectedValues = modelContext.AttributeSelection.AttributesMap.Any();

            if (isBundlePricing)
            {
                model.AttributeInfo = await _productAttributeFormatter.FormatAttributesAsync(
                    modelContext.SelectedAttributes,
                    product,
                    ProductAttributeFormatOptions.PlainText,
                    modelContext.Customer);
            }

            model.SelectedCombination = await _productAttributeMaterializer.FindAttributeCombinationAsync(product.Id, modelContext.SelectedAttributes);

            if (model.SelectedCombination != null && !model.SelectedCombination.IsActive)
            {
                model.IsAvailable = false;
                model.StockAvailability = res["Products.Availability.IsNotActive"];
            }

            // INFO: MergeWithCombination is required to apply attribute combination prices
            // and for product.IsAvailableByStock() call in PrepareProductPropertiesModelAsync.
            product.MergeWithCombination(model.SelectedCombination);

            // Explicitly selected values always discards values preselected by merchant.
            var selectedValueIds = selectedValues.Select(x => x.Id).ToArray();

            foreach (var attribute in model.ProductVariantAttributes)
            {
                var updatePreselection = selectedValueIds.Any() && selectedValueIds.Intersect(attribute.Values.Select(x => x.Id)).Any();

                foreach (ProductDetailsModel.ProductVariantAttributeValueModel value in attribute.Values)
                {
                    if (updatePreselection)
                    {
                        value.IsPreSelected = selectedValueIds.Contains(value.Id);
                    }

                    if (!_priceSettings.ShowVariantCombinationPriceAdjustment)
                    {
                        value.PriceAdjustment = string.Empty;
                    }

                    if (checkAvailability)
                    {
                        var availabilityInfo = await _productAttributeMaterializer.IsCombinationAvailableAsync(
                            product,
                            attributes,
                            selectedValues,
                            value.ProductAttributeValue);

                        if (availabilityInfo != null)
                        {
                            value.IsUnavailable = true;

                            // Set title attribute for unavailable option.
                            if (product.DisplayStockAvailability && availabilityInfo.IsOutOfStock && availabilityInfo.IsActive)
                            {
                                value.Title = product.BackorderMode == BackorderMode.NoBackorders || product.BackorderMode == BackorderMode.AllowQtyBelow0
                                    ? res["Products.Availability.OutOfStock"]
                                    : res["Products.Availability.Backordering"];
                            }
                            else
                            {
                                value.Title = res["Products.Availability.IsNotActive"];
                            }
                        }
                    }
                }
            }

            // Apply weight adjustments.
            selectedValues.Each(x => model.WeightValue += x.WeightAdjustment);
        }

        protected async Task PrepareProductPropertiesModelAsync(ProductDetailsModel model, ProductDetailsModelContext modelContext)
        {
            using var chronometer = _services.Chronometer.Step("PrepareProductPropertiesModel");

            var store = modelContext.Store;
            var customer = modelContext.Customer;
            var currency = modelContext.Currency;
            var product = modelContext.Product;
            var productBundleItem = modelContext.ProductBundleItem;
            var isBundle = product.ProductType == ProductType.BundledProduct;
            var hasSelectedAttributes = modelContext.SelectedAttributes?.AttributesMap?.Any() ?? false;

            if ((productBundleItem != null && !productBundleItem.BundleProduct.BundlePerItemShoppingCart) ||
                (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes && !hasSelectedAttributes))
            {
                // Cases where stock inventory is not functional (what ShoppingCartService.GetStandardWarnings and ProductService.AdjustInventory does not handle).
                model.IsAvailable = true;

                product = _db.FindTracked<Product>(product.Id) ?? product;
                var collectionLoaded = _db.IsCollectionLoaded(product, x => x.ProductVariantAttributeCombinations, out var collectionEntry);

                var hasAttributeCombinations = collectionLoaded
                    ? product.ProductVariantAttributeCombinations.Count > 0
                    : await collectionEntry.Query().AnyAsync();

                model.StockAvailability = !hasAttributeCombinations ? product.FormatStockMessage(_localizationService) : string.Empty;
            }
            else if (model.IsAvailable)
            {
                model.IsAvailable = product.IsAvailableByStock();
                model.StockAvailability = product.FormatStockMessage(_localizationService);
            }

            model.Id = product.Id;
            model.Name = product.GetLocalized(x => x.Name);
            model.Condition = product.Condition;
            model.ShowCondition = _catalogSettings.ShowProductCondition;
            model.LocalizedCondition = product.Condition.GetLocalizedEnum(_services.WorkContext.WorkingLanguage.Id);
            model.ShowSku = _catalogSettings.ShowProductSku;
            model.Sku = product.Sku;
            model.ShortDescription = product.GetLocalized(x => x.ShortDescription);
            model.FullDescription = product.GetLocalized(x => x.FullDescription, detectEmptyHtml: true);
            model.MetaKeywords = product.GetLocalized(x => x.MetaKeywords);
            model.MetaDescription = product.GetLocalized(x => x.MetaDescription);
            model.MetaTitle = product.GetLocalized(x => x.MetaTitle);
            model.SeName = await product.GetActiveSlugAsync();
            model.ShowManufacturerPartNumber = _catalogSettings.ShowManufacturerPartNumber;
            model.ManufacturerPartNumber = product.ManufacturerPartNumber;
            model.ShowDimensions = _catalogSettings.ShowDimensions;
            model.ShowWeight = _catalogSettings.ShowWeight;
            model.ShowGtin = _catalogSettings.ShowGtin;
            model.Gtin = product.Gtin;
            model.HasSampleDownload = product.IsDownload && product.HasSampleDownload;
            model.IsCurrentCustomerRegistered = customer.IsRegistered();
            model.IsBasePriceEnabled = product.BasePriceEnabled && !(isBundle && product.BundlePerItemPricing);
            model.ShowLegalInfo = !model.IsBundlePart && _taxSettings.ShowLegalHintsInProductDetails;
            model.BundleTitleText = product.GetLocalized(x => x.BundleTitleText);
            model.BundlePerItemPricing = product.BundlePerItemPricing;
            model.BundlePerItemShipping = product.BundlePerItemShipping;
            model.BundlePerItemShoppingCart = product.BundlePerItemShoppingCart;

            var basePricePricingOptions = _priceCalculationService.CreateDefaultOptions(false, customer, currency, modelContext.BatchContext);
            basePricePricingOptions.TaxFormat = null;
            model.BasePriceInfo = await _priceCalculationService.GetBasePriceInfoAsync(product, basePricePricingOptions);

            var taxDisplayType = await _services.WorkContext.GetTaxDisplayTypeAsync(customer, store.Id);
            string taxInfo = T(taxDisplayType == TaxDisplayType.IncludingTax ? "Tax.InclVAT" : "Tax.ExclVAT");

            var defaultTaxRate = string.Empty;
            if (_taxSettings.DisplayTaxRates)
            {
                var taxRate = await _taxService.GetTaxRateAsync(product, customer: customer);
                if (taxRate.Rate != 0)
                {
                    var formattedTaxRate = _taxService.FormatTaxRate(taxRate.Rate);
                    defaultTaxRate = $"({formattedTaxRate}%), ";
                }
            }

            var additionalShippingCosts = string.Empty;
            var shippingSurcharge = model.Price.ShippingSurcharge;

            if (shippingSurcharge.GetValueOrDefault() > 0)
            {
                additionalShippingCosts = shippingSurcharge.Value.ToString(true) + ", ";
            }

            if (!product.IsShippingEnabled || product.IsFreeShipping)
            {
                model.LegalInfo += product.IsTaxExempt
                    ? T("Common.FreeShipping")
                    : "{0} {1}{2}".FormatInvariant(taxInfo, defaultTaxRate, T("Common.FreeShipping"));
            }
            else
            {
                var shippingInfoUrl = await _urlHelper.TopicAsync("ShippingInfo");

                if (shippingInfoUrl.IsEmpty())
                {
                    model.LegalInfo = T("Tax.LegalInfoProductDetail2",
                        product.IsTaxExempt ? string.Empty : taxInfo,
                        product.IsTaxExempt ? string.Empty : defaultTaxRate,
                        additionalShippingCosts);
                }
                else
                {
                    model.LegalInfo = T("Tax.LegalInfoProductDetail",
                        product.IsTaxExempt ? string.Empty : taxInfo,
                        product.IsTaxExempt ? string.Empty : defaultTaxRate,
                        additionalShippingCosts,
                        shippingInfoUrl);
                }
            }

            model.LegalInfo = model.LegalInfo.TrimSafe();

            var dimension = await _db.MeasureDimensions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == _measureSettings.BaseDimensionId);
            var weight = await _db.MeasureWeights.AsNoTracking().FirstOrDefaultAsync(x => x.Id == _measureSettings.BaseWeightId);
            var dimensionSystemKeyword = dimension?.SystemKeyword ?? string.Empty;
            var weightSystemKeyword = weight?.SystemKeyword ?? string.Empty;

            // INFO: model.WeightValue already calculated in previous helper methods.
            model.Weight = (model.WeightValue > 0) ? $"{model.WeightValue:N2} {weightSystemKeyword}" : string.Empty;
            model.Height = (product.Height > 0) ? $"{product.Height:N2} {dimensionSystemKeyword}" : string.Empty;
            model.Length = (product.Length > 0) ? $"{product.Length:N2} {dimensionSystemKeyword}" : string.Empty;
            model.Width = (product.Width > 0) ? $"{product.Width:N2} {dimensionSystemKeyword}" : string.Empty;

            if (productBundleItem != null)
            {
                model.ThumbDimensions = _mediaSettings.BundledProductPictureSize;
            }
            else if (modelContext.IsAssociatedProduct)
            {
                model.ThumbDimensions = _mediaSettings.AssociatedProductPictureSize;
            }

            // Delivery Time.
            var deliveryPresentation = _catalogSettings.DeliveryTimesInProductDetail;

            if (model.IsAvailable)
            {
                var deliveryTime = await _deliveryTimeService.GetDeliveryTimeAsync(product, _catalogSettings);
                if (deliveryTime != null)
                {
                    model.DeliveryTimeName = deliveryTime.GetLocalized(x => x.Name);
                    model.DeliveryTimeHexValue = deliveryTime.ColorHexValue;

                    if (deliveryPresentation == DeliveryTimesPresentation.DateOnly || deliveryPresentation == DeliveryTimesPresentation.LabelAndDate)
                    {
                        model.DeliveryTimeDate = _deliveryTimeService.GetFormattedDeliveryDate(deliveryTime);
                    }
                }
            }

            model.IsShippingEnabled = product.IsShippingEnabled;
            model.DeliveryTimesPresentation = deliveryPresentation;
            model.DisplayDeliveryTimeAccordingToStock = product.DisplayDeliveryTimeAccordingToStock(_catalogSettings);

            if (!model.IsAvailable && model.DeliveryTimeName.IsEmpty() && deliveryPresentation != DeliveryTimesPresentation.None)
            {
                model.DeliveryTimeName = T("ShoppingCart.NotAvailable");
            }

            var quantityUnit = await _db.QuantityUnits.GetQuantityUnitByIdAsync(product.QuantityUnitId ?? 0, _catalogSettings.ShowDefaultQuantityUnit);
            if (quantityUnit != null)
            {
                model.QuantityUnitName = quantityUnit.GetLocalized(x => x.Name);
                model.QuantityUnitNamePlural = quantityUnit.GetLocalized(x => x.NamePlural);
            }

            // Back in stock subscriptions.
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions &&
                product.StockQuantity <= 0)
            {
                // Out of stock.
                model.DisplayBackInStockSubscription = true;
                model.BackInStockAlreadySubscribed = await _stockSubscriptionService.IsSubscribedAsync(product, customer, store.Id);
            }
        }

        protected async Task PrepareProductCartModelAsync(ProductDetailsModel model, ProductDetailsModelContext modelContext, int selectedQuantity)
        {
            using var chronometer = _services.Chronometer.Step("PrepareProductCartModel");

            var product = modelContext.Product;
            var currency = modelContext.Currency;
            var displayPrices = modelContext.DisplayPrices;

            model.AddToCart.ProductId = product.Id;
            model.AddToCart.HideQuantityControl = product.HideQuantityControl;
            model.AddToCart.AvailableForPreOrder = product.AvailableForPreOrder;

            await product.MapQuantityInputAsync(model.AddToCart, selectedQuantity);
            model.AddToCart.QuantityUnitName = model.QuantityUnitName; // TODO: (mc) remove 'QuantityUnitName' from parent model later
            model.AddToCart.QuantityUnitNamePlural = model.QuantityUnitNamePlural; // TODO: (mc) remove 'QuantityUnitName' from parent model later

            // 'add to cart', 'add to wishlist' buttons.
            model.AddToCart.DisableBuyButton = !displayPrices || product.DisableBuyButton ||
                !_services.Permissions.Authorize(Permissions.Cart.AccessShoppingCart);

            model.AddToCart.DisableWishlistButton = !displayPrices || product.DisableWishlistButton
                || product.ProductType == ProductType.GroupedProduct
                || !_services.Permissions.Authorize(Permissions.Cart.AccessWishlist);

            model.AddToCart.CustomerEntersPrice = product.CustomerEntersPrice;
            if (model.AddToCart.CustomerEntersPrice)
            {
                var minimumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryCurrency(product.MinimumCustomerEnteredPrice, currency);
                var maximumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryCurrency(product.MaximumCustomerEnteredPrice, currency);

                model.AddToCart.CustomerEnteredPrice = minimumCustomerEnteredPrice.Amount;
                model.AddToCart.CustomerEnteredPriceRange = T("Products.EnterProductPrice.Range",
                    _currencyService.ConvertToWorkingCurrency(minimumCustomerEnteredPrice),
                    _currencyService.ConvertToWorkingCurrency(maximumCustomerEnteredPrice));
            }
        }

        protected void PrepareProductGiftCardsModel(ProductDetailsModel model, ProductDetailsModelContext modelContext)
        {
            model.GiftCard.IsGiftCard = modelContext.Product.IsGiftCard;
            if (model.GiftCard.IsGiftCard)
            {
                model.GiftCard.GiftCardType = modelContext.Product.GiftCardType;
                model.GiftCard.SenderName = modelContext.Customer.GetFullName();
                model.GiftCard.SenderEmail = modelContext.Customer.Email;
            }
        }

        protected async Task PrepareProductTagsModelAsync(ProductDetailsModel model, Product product)
        {
            var storeId = _services.StoreContext.CurrentStore.Id;
            var cacheKey = string.Format(ModelCacheInvalidator.PRODUCTTAG_BY_PRODUCT_MODEL_KEY, product.Id, _services.WorkContext.WorkingLanguage.Id, storeId);

            model.ProductTags = await _services.CacheFactory.GetMemoryCache().GetAsync(cacheKey, async (o) =>
            {
                o.ExpiresIn(TimeSpan.FromHours(3));

                await _db.LoadCollectionAsync(product, x => x.ProductTags);

                var models = new List<ProductTagModel>();
                var productCountsMap = await _productTagService.GetProductCountsMapAsync(null, storeId);

                foreach (var tag in product.ProductTags.Where(x => x.Published))
                {
                    if (productCountsMap.TryGetValue(tag.Id, out var productCount) && productCount > 0)
                    {
                        models.Add(new ProductTagModel
                        {
                            Id = tag.Id,
                            Name = tag.GetLocalized(x => x.Name),
                            Slug = tag.BuildSlug(),
                            ProductCount = productCount
                        });
                    }
                }

                return models.OrderBy(x => x.Name).ToList();
            });
        }

        protected async Task PrepareRelatedProductsModelAsync(ProductDetailsModel model, Product product)
        {
            var relatedProductIds = await _db.RelatedProducts
                .AsNoTracking()
                .ApplyProductId1Filter(product.Id)
                .Select(x => x.ProductId2)
                .ToArrayAsync();

            var products = await _db.Products
                .ApplyAclFilter(_services.WorkContext.CurrentCustomer)
                .ApplyStoreFilter(_services.StoreContext.CurrentStore.Id)
                .Where(x => relatedProductIds.Contains(x.Id))
                .ToListAsync();

            products = products.OrderBySequence(relatedProductIds).ToList();

            var settings = GetBestFitProductSummaryMappingSettings(ProductSummaryViewMode.Grid, x =>
            {
                x.DeliveryTimesPresentation = DeliveryTimesPresentation.None;
            });

            model.RelatedProducts = await MapProductSummaryModelAsync(products, settings);
            model.RelatedProducts.ShowBasePrice = false;
        }

        protected async Task PrepareAlsoPurchasedProductsModelAsync(ProductDetailsModel model, Product product)
        {
            if (!_catalogSettings.ProductsAlsoPurchasedEnabled)
            {
                return;
            }

            var storeId = _services.StoreContext.CurrentStore.Id;

            var alsoPurchasedProductIds = await _services.Cache.GetAsync(string.Format(ModelCacheInvalidator.PRODUCTS_ALSO_PURCHASED_IDS_KEY, product.Id, storeId), async () =>
            {
                return await _db.OrderItems
                    .SelectAlsoPurchasedProductIds(product.Id, _catalogSettings.ProductsAlsoPurchasedNumber, storeId)
                    .ToArrayAsync();
            });

            var products = await _db.Products
                .ApplyAclFilter(_services.WorkContext.CurrentCustomer)
                .ApplyStoreFilter(storeId)
                .Where(x => alsoPurchasedProductIds.Contains(x.Id))
                .ToListAsync();

            products = products.OrderBySequence(alsoPurchasedProductIds).ToList();

            var settings = GetBestFitProductSummaryMappingSettings(ProductSummaryViewMode.Mini);
            model.AlsoPurchased = await MapProductSummaryModelAsync(products, settings);
        }

        #endregion

        public MediaGalleryModel PrepareProductDetailsMediaGalleryModel(
            IList<MediaFileInfo> files,
            string productName,
            ICollection<int> allCombinationImageIds,
            bool isAssociatedProduct,
            ProductBundleItem bundleItem = null,
            ProductVariantAttributeCombination combination = null)
        {
            var model = new MediaGalleryModel
            {
                ModelName = productName,
                DefaultAlt = T("Media.Product.ImageAlternateTextFormat", productName),
                BoxEnabled = true, // TODO: make a setting for this in the future
                ImageZoomEnabled = _mediaSettings.DefaultPictureZoomEnabled,
                ImageZoomType = _mediaSettings.PictureZoomType,
                ThumbSize = _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage,
                ImageSize = _mediaSettings.ProductDetailsPictureSize
            };

            if (isAssociatedProduct)
            {
                model.ThumbSize = _mediaSettings.AssociatedProductPictureSize;
            }
            else if (bundleItem != null)
            {
                model.ThumbSize = _mediaSettings.BundledProductPictureSize;
            }

            MediaFileInfo defaultFile = null;
            var combiAssignedImages = combination?.GetAssignedMediaIds();

            if (files.Count > 0)
            {
                if (files.Count <= _catalogSettings.DisplayAllImagesNumber)
                {
                    // Show all images.
                    foreach (var file in files)
                    {
                        model.Files.Add(PrepareMediaFileInfo(file, model));

                        if (defaultFile == null && combiAssignedImages != null && combiAssignedImages.Contains(file.Id))
                        {
                            model.GalleryStartIndex = model.Files.Count - 1;
                            defaultFile = file;
                        }
                    }
                }
                else
                {
                    // Images not belonging to any combination...
                    if (allCombinationImageIds != null)
                    {
                        foreach (var file in files.Where(p => !allCombinationImageIds.Contains(p.Id)))
                        {
                            model.Files.Add(PrepareMediaFileInfo(file, model));
                        }
                    }

                    // Plus images belonging to selected combination.
                    if (combiAssignedImages != null)
                    {
                        foreach (var file in files.Where(p => combiAssignedImages.Contains(p.Id)))
                        {
                            model.Files.Add(PrepareMediaFileInfo(file, model));

                            if (defaultFile == null)
                            {
                                model.GalleryStartIndex = model.Files.Count - 1;
                                defaultFile = file;
                            }
                        }
                    }

                    if (model.Files.Count == 0)
                    {
                        // No combination exists for the selection and all images are assigned to combinations.
                        model.Files.Add(PrepareMediaFileInfo(files[0], model));
                    }
                }

                if (defaultFile == null)
                {
                    model.GalleryStartIndex = 0;
                    defaultFile = files.First();
                }
            }

            if (defaultFile == null && !_catalogSettings.HideProductDefaultPictures)
            {
                var fallbackImageSize = _mediaSettings.ProductDetailsPictureSize;
                if (isAssociatedProduct)
                {
                    fallbackImageSize = _mediaSettings.AssociatedProductPictureSize;
                }
                else if (bundleItem != null)
                {
                    fallbackImageSize = _mediaSettings.BundledProductPictureSize;
                }

                model.FallbackUrl = _mediaService.GetFallbackUrl(fallbackImageSize);
            }

            return model;
        }

        protected async Task<List<ProductSpecificationModel>> PrepareProductSpecificationModelAsync(ProductDetailsModelContext modelContext)
        {
            Guard.NotNull(modelContext, nameof(modelContext));

            var product = modelContext.Product;
            var batchContext = modelContext.BatchContext;

            string cacheKey = string.Format(ModelCacheInvalidator.PRODUCT_SPECS_MODEL_KEY, product.Id, _services.WorkContext.WorkingLanguage.Id);
            return await _services.CacheFactory.GetMemoryCache().GetAsync(cacheKey, async () =>
            {
                List<ProductSpecificationAttribute> attrs;

                if (_db.IsCollectionLoaded(product, x => x.ProductSpecificationAttributes))
                {
                    attrs = product.ProductSpecificationAttributes
                        .Where(x =>
                            (x.ShowOnProductPage == null && x.SpecificationAttributeOption?.SpecificationAttribute?.ShowOnProductPage == true) ||
                            (x.ShowOnProductPage == true))
                        .ToList();
                }
                else
                {
                    attrs = (await batchContext.SpecificationAttributes.GetOrLoadAsync(product.Id))
                        .Where(x =>
                            (x.ShowOnProductPage == null && x.SpecificationAttributeOption?.SpecificationAttribute?.ShowOnProductPage == true) ||
                            (x.ShowOnProductPage == true))
                        .ToList();
                }

                return attrs
                    .OrderBy(x => x.DisplayOrder)
                    .ThenBy(x => x.SpecificationAttributeOption.SpecificationAttribute.DisplayOrder)
                    .ThenBy(x => x.SpecificationAttributeOption.SpecificationAttribute.Name)
                    .Select(x => new ProductSpecificationModel
                    {
                        SpecificationAttributeId = x.SpecificationAttributeOption.SpecificationAttributeId,
                        SpecificationAttributeName = x.SpecificationAttributeOption.SpecificationAttribute.GetLocalized(x => x.Name),
                        SpecificationAttributeOption = x.SpecificationAttributeOption.GetLocalized(x => x.Name)
                    })
                    .ToList();
            });
        }

        public async Task PrepareProductReviewsModelAsync(ProductReviewsModel model, Product product, int? take = null)
        {
            Guard.NotNull(product);
            Guard.NotNull(model);

            model.ProductId = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name);
            model.ProductSeName = await product.GetActiveSlugAsync();
            model.CanCurrentCustomerLeaveReview = _catalogSettings.AllowAnonymousUsersToReviewProduct || !_services.WorkContext.CurrentCustomer.IsGuest();
            model.DisplayCaptcha = _captchaSettings.CanDisplayCaptcha && _captchaSettings.ShowOnProductReviewPage;
            model.ShowVerfiedPurchaseBadge = _catalogSettings.ShowVerfiedPurchaseBadge;

            await _db.LoadCollectionAsync(product, x => x.ProductReviews, false, q => q
                .Include(x => x.Customer)
                .ThenInclude(x => x.CustomerRoleMappings)
                .ThenInclude(x => x.CustomerRole));

            model.TotalReviewsCount = product.ProductReviews.Count(x => x.IsApproved);

            var reviews = product.ProductReviews
                .Where(x => x.IsApproved)
                .OrderByDescending(x => x.CreatedOnUtc)
                .Take(take ?? int.MaxValue)
                .ToList();

            if (reviews.Count > 0)
            {
                var unverifiedCustomerIds = reviews
                    .Where(x => x.IsVerifiedPurchase == null)
                    .ToDistinctArray(x => x.CustomerId);

                var orderCustomerIds = await _db.Orders
                    .Where(x => unverifiedCustomerIds.Contains(x.CustomerId) && x.OrderItems.Any(y => y.ProductId == product.Id))
                    .Select(x => x.CustomerId)
                    .ToArrayAsync();

                foreach (var review in reviews)
                {
                    var writtenOn = _services.DateTimeHelper.ConvertToUserTime(review.CreatedOnUtc, DateTimeKind.Utc);

                    model.Items.Add(new()
                    {
                        Id = review.Id,
                        CustomerId = review.CustomerId,
                        CustomerName = review.Customer.FormatUserName(),
                        AllowViewingProfiles = _customerSettings.AllowViewingProfiles && review.Customer != null && !review.Customer.IsGuest(),
                        Title = review.Title,
                        ReviewText = review.ReviewText,
                        Rating = review.Rating,
                        Helpfulness = new()
                        {
                            ProductReviewId = review.Id,
                            HelpfulYesTotal = review.HelpfulYesTotal,
                            HelpfulNoTotal = review.HelpfulNoTotal,
                        },
                        // Look in order history of customer whether he/she has purchased the product.
                        IsVerifiedPurchase = review.IsVerifiedPurchase ?? orderCustomerIds.Contains(review.CustomerId),
                        WrittenOnStr = writtenOn.ToString("M") + ' ' + writtenOn.ToString("yyyy"),
                        WrittenOn = review.CreatedOnUtc
                    });
                }
            }
        }

        private MediaFileInfo PrepareMediaFileInfo(MediaFileInfo file, MediaGalleryModel model)
        {
            file.Alt = file.File.GetLocalized(x => x.Alt)?.Value.NullEmpty() ?? model.DefaultAlt;
            file.TitleAttribute = file.File.GetLocalized(x => x.Title)?.Value.NullEmpty() ?? model.ModelName;

            _services.DisplayControl.Announce(file.File);

            // Return for chaining
            return file;
        }
    }
}
