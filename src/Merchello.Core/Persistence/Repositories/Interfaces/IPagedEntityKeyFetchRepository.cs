﻿namespace Merchello.Core.Persistence.Repositories
{
    using System;
    using Models.EntityBase;
    using Models.Rdbms;
    using Querying;
    using Umbraco.Core.Persistence;
    using Umbraco.Core.Persistence.Querying;
    using Umbraco.Core.Persistence.Repositories;

    /// <summary>
    /// The PagedEntityKeyFetchRepository interface.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The type of entity
    /// </typeparam>
    /// <typeparam name="TDto">
    /// The type of the Dto
    /// </typeparam>
// ReSharper disable once UnusedTypeParameter
    public interface IPagedEntityKeyFetchRepository<TEntity, TDto> : IRepositoryQueryable<Guid, TEntity> 
        where TEntity : class, IEntity
        where TDto : IPageableDto
    {
        /// <summary>
        /// The get paged keys.
        /// </summary>
        /// <param name="page">
        /// The page.
        /// </param>
        /// <param name="itemsPerPage">
        /// The items per page.
        /// </param>
        /// <param name="query">
        /// The query.
        /// </param>
        /// <param name="orderExpression">
        /// The order expression.
        /// </param>
        /// <param name="sortDirection">
        /// The sort direction.
        /// </param>
        /// <returns>
        /// The page of data.
        /// </returns>
        Page<Guid> GetPagedKeys(long page, long itemsPerPage, IQuery<TEntity> query, string orderExpression, SortDirection sortDirection = SortDirection.Descending);

        Page<Guid> Search(string searchTerm, long page, long itemsPerPage, string orderExpression, SortDirection sortDirection = SortDirection.Descending);
    }
}