using System.Diagnostics.CodeAnalysis;

namespace NHDAL.Interfaces
{
    public interface IUnitOfWorkFactory
    {
        IUnitOfWork OpenUnitOfWork();
        bool TryOpenUnitOfWork([MaybeNullWhen(false)] out IUnitOfWork pc);
    }
}
