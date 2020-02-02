using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unisave.Contracts;
using Unisave.Exceptions;
using Unisave.Database;
using Unisave.Foundation;
using Unisave.Runtime;
using Unisave.Services;
using Unisave.Utils;

namespace Unisave
{
    public abstract class Migration
    {
        /// <summary>
        /// What database version does this migration migrate from
        /// </summary>
        public abstract int From { get; }

        /// <summary>
        /// What database version does this migration migrate to
        /// </summary>
        public abstract int To { get; }

        /// <summary>
        /// Low-level access to the database
        /// </summary>
        protected IDatabase Db { get; private set; }

        /// <summary>
        /// Execute the migration
        /// </summary>
        public abstract void Run();

        public static Migration CreateInstance(Type migrationType)
        {
            // create instance
            Migration migration = ExecutionHelper.Instantiate<Migration>(migrationType);

            // assign properties
            migration.Db = Application.Default.Resolve<IDatabase>();

            return migration;
        }

        /// <summary>
        /// Tries to find and create a migration with a specific version jump
        /// Throws MigrationInstantiationException
        /// </summary>
        /// <param name="from">Initial database version</param>
        /// <param name="to">Target database version</param>
        /// <param name="types">Types to look through</param>
        /// <returns>Instance of the correct migration</returns>
        public static Migration CreateMigrationTypeByVersionJump(int from, int to, IEnumerable<Type> types)
        {
            List<Migration> migrationCandidates = types
                .Where(t => typeof(Migration).IsAssignableFrom(t))
                .Select(t => CreateInstance(t))
                .Where(m => m.From == from && m.To == to)
                .ToList();

            if (migrationCandidates.Count > 1)
                throw new InstantiationException(
                    $"Migration {from} --> {to} is ambiguous. "
                    + "Make sure you don't have two migrations with the same version jump."
                );

            if (migrationCandidates.Count == 0)
                throw new InstantiationException(
                    $"Migration {from} --> {to} was not found. "
                    + $"Make sure your class inherits from the {nameof(Unisave.Migration)} class."
                );

            return migrationCandidates[0];
        }
    }
}
