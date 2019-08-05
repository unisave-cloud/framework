using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unisave.Exceptions;
using Unisave.Database;

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
            if (!typeof(Migration).IsAssignableFrom(migrationType))
                throw new MigrationInstantiationException(
                    $"Provided type {migrationType} does not inherit from the Migration class.",
                    MigrationInstantiationException.ProblemType.Other
                );

            // get parameterless constructor
            ConstructorInfo ci = migrationType.GetConstructor(new Type[] { });

            if (ci == null)
                throw new MigrationInstantiationException(
                    $"Provided facet type {migrationType} lacks parameterless constructor.",
                    MigrationInstantiationException.ProblemType.Other
                );

            // create instance
            Migration migration = (Migration)ci.Invoke(new object[] { });

            // assign properties
            migration.Db = Unisave.Runtime.Endpoints.Database;

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
                throw new MigrationInstantiationException(
                    $"Migration {from} --> {to} is ambiguous. "
                    + "Make sure you don't have two migrations with the same version jump.",
                    MigrationInstantiationException.ProblemType.MigrationAmbiguous
                );

            if (migrationCandidates.Count == 0)
                throw new MigrationInstantiationException(
                    $"Migration {from} --> {to} was not found. "
                    + $"Make sure your class inherits from the {nameof(Unisave.Migration)} class.",
                    MigrationInstantiationException.ProblemType.MigrationNotFound
                );

            return migrationCandidates[0];
        }
    }
}
