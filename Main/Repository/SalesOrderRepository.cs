using Main.Data;
using Main.EmailSender;
using Main.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics.Eventing.Reader;

namespace Main.Repository
{
    public class SalesOrderRepository
    {
        private readonly BetacomioContext _contextBetacomio;
        private readonly AdventureWorksLt2019Context _contextAdventure;

        public SalesOrderRepository(BetacomioContext contextBetacomio, AdventureWorksLt2019Context contextAdventure)
        {
            _contextBetacomio = contextBetacomio;
            _contextAdventure = contextAdventure;
        }

        public async Task<IEnumerable<SalesOrderHeader>?> GetSalesOrderHeaders()
        {
            if (_contextAdventure.SalesOrderHeaders == null)
            {
                return null;
            }
            try
            {
                return await _contextAdventure.SalesOrderHeaders
                    .Include(o => o.SalesOrderDetails)
                    .ToListAsync();
            }
            catch
            {
                return null;
            }
        }
        public async Task<SalesOrderHeader?> GetSalesOrderHeader(int id)
        {
            if (_contextAdventure.SalesOrderHeaders == null)
            {
                return null;
            }
            try
            {
                var salesOrderHeader = await _contextAdventure.SalesOrderHeaders
                    .Include(o => o.SalesOrderDetails)
                    .Where(o => o.SalesOrderId == id)
                    .SingleAsync();

                if (salesOrderHeader == null)
                {
                    return null;
                }
                return salesOrderHeader;
            }
            catch (Exception)
            {
                return null;
            }


        }
        public async Task<bool> DeleteSalesOrderHeader(int id)
        {
            if (_contextAdventure.SalesOrderHeaders == null)
            {
                return false;
            }

            using (IDbContextTransaction transaction = _contextAdventure.Database.BeginTransaction())
            {
                try
                {
                    SalesOrderHeader salesOrderHeader = await _contextAdventure.SalesOrderHeaders
                       .Include(o => o.SalesOrderDetails)
                       .Where(o => o.SalesOrderId == id)
                       .SingleAsync();
                    if (salesOrderHeader == null)
                    {
                        return false;
                    }
                    _contextAdventure.SalesOrderDetails.RemoveRange(salesOrderHeader.SalesOrderDetails);
                    _contextAdventure.SalesOrderHeaders.Remove(salesOrderHeader);
                    await _contextAdventure.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }
        }
        public async Task<bool> PatchSalesOrderHeader(int id, JsonPatchDocument salesOrderHeader)
        {
            try
            {
                var item = await _contextAdventure.SalesOrderHeaders.FindAsync(id);

                if (item == null)
                {
                    return false;
                }

                salesOrderHeader.ApplyTo(item);
                await _contextAdventure.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        private bool SalesOrderHeaderExists(int id)
        {
            return (_contextAdventure.SalesOrderHeaders?.Any(e => e.SalesOrderId == id)).GetValueOrDefault();
        }
    }
}
