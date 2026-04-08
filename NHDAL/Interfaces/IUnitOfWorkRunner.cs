using System;
using System.Threading;
using System.Threading.Tasks;

namespace NHDAL.Interfaces;

public interface IUnitOfWorkRunner
{
    void Run(Action<IServiceProvider> action);
    TResult Run<TResult>(Func<IServiceProvider, TResult> action);
    Task RunAsync(Func<IServiceProvider, Task> action, CancellationToken cancellationToken = default);
    Task<TResult> RunAsync<TResult>(Func<IServiceProvider, Task<TResult>> action, CancellationToken cancellationToken = default);
}