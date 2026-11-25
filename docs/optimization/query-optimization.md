# Query Optimization – Initial Pass

## Scope

This pass applies conservative EF Core read-query optimizations without changing API response contracts or requiring production data.

## Changes

- Product list and featured-product repository reads use `AsNoTracking()` because these paths materialize data for response DTO mapping and do not update entities.
- Product list and featured-product reads use `AsSplitQuery()` because they include the Category reference plus the Variants and Images collections. This avoids a single joined result multiplying variant and image rows.
- Product slug detail reads use `AsNoTracking()` and `AsSplitQuery()`; the slug path is read-only. The ID lookup remains tracked because update flows load the product by ID and mutate it.
- Category navigation reads use `AsNoTracking()` and `AsSplitQuery()` because the service only maps the returned graph to navigation DTOs and includes both Children and Products collections.

## Deliberately not changed yet

- No new indexes: index selection must be based on actual query plans, row counts, selectivity, and write overhead.
- No arbitrary limit added to featured products: that would change endpoint behavior.
- No change to the legacy unpaged product endpoint contract. The paged endpoint remains available and should be preferred by catalogue clients.
- No claims about measured latency or logical-read improvements: the local database currently has no representative product data.

## Local validation plan

1. Build the full solution.
2. Run the full test suite.
3. Seed a representative local dataset (for example, products with multiple variants and images across categories).
4. Compare generated SQL and actual execution plans for:
   - `GET /api/Products`
   - `GET /api/Products/featured`
   - `GET /api/Products/{slug}`
   - category navigation endpoint
5. Record elapsed time, logical reads, row counts, and query count before/after using the same dataset and warm/cold-cache conditions.
6. Review the paged catalogue query separately for search predicates, facet query count, and index candidates.

## Risks and compatibility

Split queries execute multiple SQL statements instead of one joined statement. This reduces join amplification for collection includes but can increase round trips. Benchmark with representative data before merging. No schema migration is included in this pass.
