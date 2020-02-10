using System;
using Unisave.Contracts;
using Unisave.Database;
using Unisave.Exceptions;
using Unisave.Facades;
using Unisave.Foundation;
using Unisave.Services;

namespace Unisave
{
    /// <summary>
    /// Facade for working with the database
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class DB
    {
        private static IDatabase GetDatabase()
        {
            return Facade.App.Resolve<IDatabase>();
        }
        
        /// <summary>
        /// Starts new database transaction
        /// </summary>
        public static void StartTransaction()
        {
            GetDatabase().StartTransaction();
        }
        
        /// <summary>
        /// Rolls back the top level transaction
        /// Does nothing if no transaction open
        /// </summary>
        public static void Rollback()
        {
            GetDatabase().RollbackTransaction();
        }
        
        /// <summary>
        /// Commits the top level transaction
        /// Does nothing if no transaction open
        /// </summary>
        public static void Commit()
        {
            GetDatabase().CommitTransaction();
        }
        
        /// <summary>
        /// Returns the number of nested transactions that are open
        /// </summary>
        public static int TransactionLevel()
        {
            return GetDatabase().TransactionLevel();
        }

        /// <summary>
        /// Executes a closure within a transaction
        /// </summary>
        /// <param name="closure">Closure to execute</param>
        /// <param name="attempts">
        /// Total number of attempts when deadlock causes the failure
        /// </param>
        public static void Transaction(Action closure, int attempts = 1)
        {
            if (closure == null)
                throw new ArgumentNullException(nameof(closure));
            
            Transaction(() => {
                closure.Invoke();
                return true;
            }, attempts);
        }

        /// <summary>
        /// Executes a closure within a transaction
        /// </summary>
        /// <param name="closure">Closure to execute</param>
        /// <param name="attempts">
        /// Total number of attempts when deadlock causes the failure
        /// </param>
        public static T Transaction<T>(Func<T> closure, int attempts = 1)
        {
            if (closure == null)
                throw new ArgumentNullException(nameof(closure));
            
            for (int attempt = 1; attempt <= attempts; attempt++)
            {
                StartTransaction();

                int currentTransactionLevel = TransactionLevel();

                try
                {
                    var returnedValue = closure.Invoke();
                    Commit();
                    return returnedValue; // also breaks the attempt cycle
                }
                catch (DatabaseDeadlockException)
                {
                    // deadlock goes through any nested transactions simply
                    // because the database rolled back
                    // all the nested transactions because that's what it does
                    if (currentTransactionLevel != 1)
                        throw;
                    
                    // ok, now we are on the top-level transaction
                    // now we want to retry this transaction
                    // unless attempts have been exhausted
                    
                    // attempts exhausted
                    if (attempt >= attempts)
                        throw;
                    
                    // retry transaction
                    // (just let the for cycle repeat, so do nothing here)
                }
                catch
                {
                    // just kill it
                    Rollback();
                    throw;
                }
            }
            
            throw new UnisaveException(
                "This exception should never be thrown."
            );
        }
    }
}