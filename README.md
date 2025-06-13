# NHDAL
## Overview

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