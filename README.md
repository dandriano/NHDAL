# NHDAL
## Overview

When choosing between NHibernate (NH) and Entity Framework (EF) for your Data Access Layer (DAL), you’re weighing several factors, including change tracking, lazy loading, and compatibility with your existing system. The existing system could implement a unit of work pattern, where each unit of work typically [represented by](https://gunnarpeipman.com/ef-core-repository-unit-of-work/) (or, in other words, has its own) `ISession` (in NH) or `DbContext` (in EF).

In EF, change tracking is a built-in feature where the `DbContext` monitors changes (additions, deletions, modifications) to entities after they’re loaded from the database. When you call `SaveChanges`, EF uses this information to generate the necessary SQL updates. This works seamlessly even when attaching entities to a new `DbContext`, as you can explicitly set their state (e.g., `Modified`)

In NH, changes tracked within a single session, however, sessions are typically short-lived, and entities become detached when a session closes. Unlike EF, NH doesn’t natively preserve change tracking across different sessions (or units of work).

## `UnitOfWork` implementation

Each unit of work likely uses a new instance of NH `ISession`. Stale objects entities that might have been altered in the database since they were last loaded pose a challenge when moving between sessions, as the new session isn’t aware of prior changes or updates elsewhere.

Key method related to reattaching detached objects is `Merge` (`MergeAsync` / `MergeManyAsync`):
```
public T Merge<T>(T entity) where T : class
{
    try
    {
        _session.SaveOrUpdate(entity);
        return entity;
    }
    catch (NonUniqueObjectException)
    {
        return _session.Merge(entity);
    }
}
```
This approach handles both new and detached entities effectively:
- For entities not in the session, `SaveOrUpdate` attaches them efficiently without additional database hits.
- For entities already in the session, `Merge` ensures the detached state is reconciled with the persistent instance.

And if you want to build your data access layer based on the "last write wins" strategy, then this is enough, but in most cases it is not.

TODO: write something about optimistic concurrency, "application transaction" (Chapter 12 from official documentation) and re-parenting problems..