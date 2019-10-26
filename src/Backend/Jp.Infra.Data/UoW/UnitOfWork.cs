using Jp.Domain.Interfaces;
using Jp.Infra.Data.Context;

namespace Jp.Infra.Data.UoW
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EventStoreContext _context;

        public UnitOfWork(EventStoreContext context)
        {
            _context = context;
        }

        public bool Commit()
        {
            return _context.SaveChanges() > 0;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
