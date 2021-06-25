using Autofac;
using Autofac.Core;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Core.Caching;
using Nop.Web.Framework.Mvc;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Data;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Services;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Controllers;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Logger;
using Nop.Core.Configuration;
using Nop.Data;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Domain;
using Nop.Core.Data;

namespace FoxNetSoft.Plugin.Misc.SpeedFilters
{
    public class SpeedFiltersDependencyRegistrar : IDependencyRegistrar
    {
        private const string CONTEXT_NAME = "nop_object_context_foxnetsoft_speedfilters";

        #region Implementation of IDependencyRegistrar

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            this.RegisterPluginDataContext<SpeedFiltersObjectContext>(builder, CONTEXT_NAME);

            //Register services
            builder.RegisterType<SpeedFiltersService>().As<ISpeedFiltersService>().InstancePerLifetimeScope();

          //  builder.RegisterType<EfRepository<SS_Specific_Category_Setting>>()
          //.As<IRepository<SS_Specific_Category_Setting>>()
          //.WithParameter(ResolvedParameter.ForNamed<IDbContext>(CONTEXT_NAME))
          //.InstancePerLifetimeScope();

            builder.RegisterType<SpeedFiltersController>()
                .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"));

           
        }

        public int Order
        {
            get { return 10; }
        }

        #endregion
    }
}



