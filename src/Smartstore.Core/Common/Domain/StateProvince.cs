﻿using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Smartstore.Core.Localization;
using Smartstore.Data.Caching;

namespace Smartstore.Core.Common
{
    internal class StateProvinceMap : IEntityTypeConfiguration<StateProvince>
    {
        public void Configure(EntityTypeBuilder<StateProvince> builder)
        {
            builder
                .HasOne(x => x.Country)
                .WithMany(x => x.StateProvinces)
                .HasForeignKey(x => x.CountryId);

            builder
                .HasIndex(x => x.CountryId)
                .HasDatabaseName("IX_StateProvince_CountryId")
                .IncludeProperties(x => new { x.DisplayOrder });
        }
    }

    /// <summary>
    /// Represents a state/province
    /// </summary>
    [CacheableEntity]
    [LocalizedEntity("Published and Country.Published")]
    public partial class StateProvince : BaseEntity, ILocalizedEntity, IDisplayOrder
    {
        /// <summary>
        /// Gets or sets the country identifier
        /// </summary>
        public int CountryId { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        [Required, StringLength(100)]
        [LocalizedProperty]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the abbreviation
        /// </summary>
        [StringLength(100)]
        public string Abbreviation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is published
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        private Country _country;
        /// <summary>
        /// Gets or sets the country
        /// </summary>
        public Country Country
        {
            get => _country ?? LazyLoader.Load(this, ref _country);
            set => _country = value;
        }
    }
}