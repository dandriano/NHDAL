using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NHDAL.Interfaces;

namespace NHDAL;

/// <summary>
/// Scoped executor of "ambient" unit of work
/// </summary>
public class UnitOfWorkRunner : IUnitOfWorkRunner
{
    private readonly IServiceScopeFactory _scopeFactory;

    public UnitOfWorkRunner(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void Run(Action<IServiceProvider> action)
    {
        Run(sp => { action(sp); return true; });
    }

    public TResult Run<TResult>(Func<IServiceProvider, TResult> action)
    {
        using var scope = _scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;

        var ctx = sp.GetRequiredService<IUnitOfWork>();

        try
        {
            var result = action(sp);
            ctx.Commit();

            return result;
        }
        catch
        {
            ctx.Rollback();
            throw;
        }
    }

    public async Task RunAsync(Func<IServiceProvider, Task> action, CancellationToken cancellationToken = default)
    {
        await RunAsync(async sp => { await action(sp); return true; }, cancellationToken);
    }

    public async Task<TResult> RunAsync<TResult>(Func<IServiceProvider, Task<TResult>> action, CancellationToken cancellationToken = default)
    {
        using var s = _scopeFactory.CreateScope();
        var sp = s.ServiceProvider;

        var ctx = sp.GetRequiredService<IUnitOfWork>();

        try
        {
            var result = await action(sp).ConfigureAwait(false);
            await ctx.CommitAsync().ConfigureAwait(false);

            return result;
        }
        catch
        {
            await ctx.RollbackAsync().ConfigureAwait(false);
            throw;
        }
        finally
        {
            ctx.Dispose();
        }
    }
}