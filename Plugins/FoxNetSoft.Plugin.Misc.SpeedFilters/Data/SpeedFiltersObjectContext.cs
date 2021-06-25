using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Nop.Core;
using Nop.Data;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Logger;
using System.Web.Hosting;
using System.IO;
using System.Text;
using Nop.Core.Infrastructure;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Domain;

namespace FoxNetSoft.Plugin.Misc.SpeedFilters.Data
{
    public class SpeedFiltersObjectContext : DbContext, IDbContext
    {
        public SpeedFiltersObjectContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {

        }

        #region Implementation of IDbContext

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether proxy creation setting is enabled (used in EF)
        /// </summary>
        public virtual bool ProxyCreationEnabled
        {
            get
            {
                return this.Configuration.ProxyCreationEnabled;
            }
            set
            {
                this.Configuration.ProxyCreationEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether auto detect changes setting is enabled (used in EF)
        /// </summary>
        public virtual bool AutoDetectChangesEnabled
        {
            get
            {
                return this.Configuration.AutoDetectChangesEnabled;
            }
            set
            {
                this.Configuration.AutoDetectChangesEnabled = value;
            }
        }

        #endregion

        #endregion

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new SS_Specific_Category_Setting_Map());
            base.OnModelCreating(modelBuilder);
        }
        public string CreateDatabaseInstallationScript()
        {
            return ((IObjectContextAdapter)this).ObjectContext.CreateDatabaseScript();
        }
        public void Install()
        {
            //It's required to set initializer to null (for SQL Server Compact).
            //otherwise, you'll get something like "The model backing the 'your context name' context has changed since the database was created. Consider using Code First Migrations to update the database"
            Database.SetInitializer<SpeedFiltersObjectContext>(null);

            string installationScript = CreateDatabaseInstallationScript();
            if (!String.IsNullOrWhiteSpace(installationScript))
                Database.ExecuteSqlCommand(installationScript);

            //Create Index
            /*var dbScript = "CREATE NONCLUSTERED INDEX IX_FoxNetSoft_AddonProduct_AddonId ON FoxNetSoft_AddonProduct (AddonId DESC) ";
            Database.ExecuteSqlCommand(dbScript);*/

            CreateInstallationScript();

            SaveChanges();
        }

        public void Uninstall()
        {
            this.DropPluginTable(nameof(SS_Specific_Category_Setting));
            var dbScript = "delete from LocaleStringResource where ResourceName like '%FoxNetSoft.Plugin.Misc.SpeedFilters%'";
            Database.ExecuteSqlCommand(dbScript);
            
            DeleteInstallationScript();
            SaveChanges();
        }
        public new IDbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
        {
            return base.Set<TEntity>();
        }

        public System.Collections.Generic.IList<TEntity> ExecuteStoredProcedureList<TEntity>(string commandText, params object[] parameters) where TEntity : BaseEntity, new()
        {
            throw new System.NotImplementedException();
        }

        public System.Collections.Generic.IEnumerable<TElement> SqlQuery<TElement>(string sql, params object[] parameters)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Executes the given DDL/DML command against the database.
        /// </summary>
        /// <param name="sql">The command string</param>
        /// <param name="doNotEnsureTransaction">false - the transaction creation is not ensured; true - the transaction creation is ensured.</param>
        /// <param name="timeout">Timeout value, in seconds. A null value indicates that the default value of the underlying provider will be used</param>
        /// <param name="parameters">The parameters to apply to the command string.</param>
        /// <returns>The result returned by the database after executing the command.</returns>
        public int ExecuteSqlCommand(string sql, bool doNotEnsureTransaction = false, int? timeout = null, params object[] parameters)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Detach an entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public void Detach(object entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            ((IObjectContextAdapter)this).ObjectContext.Detach(entity);
        }

        #region Script
        private void CreateInstallationScript()
        {
            var customCommands = new List<string>();
            customCommands.AddRange(ParseCommands(HostingEnvironment.MapPath("~/Plugins/FoxNetSoft.SpeedFilters/Install/SqlServer.SpeedFilters.sql"), false));

            if (customCommands.Count>0)
                ExecuteListSqlCommand(customCommands.ToArray());
        }

        private void DeleteInstallationScript()
        {
            var customCommands = new List<string>();
            customCommands.AddRange(ParseCommands(HostingEnvironment.MapPath("~/Plugins/FoxNetSoft.SpeedFilters/Install/UnSqlServer.SpeedFilters.sql"), false));

            if (customCommands.Count > 0)
                ExecuteListSqlCommand(customCommands.ToArray());
        }
        #endregion

        #region Updates
        public void UpdateInstallationScript(int version)
        {
            var customCommands = new List<string>();
            //var path = "~/Plugins/FoxNetSoft.SpeedFilters/Install/";
            var path2 = "~/Plugins/FoxNetSoft.SpeedFilters/Resources/";
            var il = new InstallLocaleResources(path2);
            il.Update();

            switch (version)
            {
                case 114:
                    CreateInstallationScript();
                    break;

            }
            if (customCommands.Count > 0)
                ExecuteListSqlCommand(customCommands.ToArray());
        }
        #endregion

        #region Utils
        private void ExecuteListSqlCommand(string[] _customCommands)
        {
            if (_customCommands != null && _customCommands.Length > 0)
            {
                foreach (var command in _customCommands)
                {
                    if (!string.IsNullOrWhiteSpace(command))
                    {
                        Database.ExecuteSqlCommand(command);
                    }
                }
            }
        }

        protected virtual string[] ParseCommands(string filePath, bool throwExceptionIfNonExists)
        {
            if (!File.Exists(filePath))
            {
                if (throwExceptionIfNonExists)
                    throw new ArgumentException(string.Format("Specified file doesn't exist - {0}", filePath));
                else
                    return new string[0];
            }


            var statements = new List<string>();
            using (var stream = File.OpenRead(filePath))
            using (var reader = new StreamReader(stream))
            {
                var statement = "";
                while ((statement = readNextStatementFromStream(reader)) != null)
                {
                    statements.Add(statement);
                }
            }

            return statements.ToArray();
        }

        protected virtual string readNextStatementFromStream(StreamReader reader)
        {
            var sb = new StringBuilder();

            string lineOfText;

            while (true)
            {
                lineOfText = reader.ReadLine();
                if (lineOfText == null)
                {
                    if (sb.Length > 0)
                        return sb.ToString();
                    else
                        return null;
                }

                if (lineOfText.TrimEnd().ToUpper() == "GO")
                    break;

                sb.Append(lineOfText + Environment.NewLine);
            }

            return sb.ToString();
        }
        #endregion

    }
}


