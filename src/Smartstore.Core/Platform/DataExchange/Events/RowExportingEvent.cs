﻿using Smartstore.Core.Catalog.Products;

namespace Smartstore.Core.DataExchange.Export.Events
{
    // TODO: Another event message must be implemented, say 'ColumnsBuildingEvent'
    // The consumer of this event (most likely a plugin) could push a list of specific column headers
    // into the global export definition.

    /// <summary>
    /// An event that is fired before an entity is exported.
    /// </summary>
    public class RowExportingEvent
    {
        /// <summary>
        /// A dynamic object which wraps and extents the actual entity, e.g. <see cref="Product"/>.
        /// </summary>
        public dynamic Row { get; init; }

        /// <summary>
        /// The entity type to be exported.
        /// </summary>
        public ExportEntityType EntityType { get; init; }

        /// <summary>
        /// Request of the export.
        /// </summary>
        public DataExportRequest ExportRequest { get; init; }

        /// <summary>
        /// Context of the export.
        /// </summary>
        public ExportExecuteContext ExecuteContext { get; init; }
    }
}
