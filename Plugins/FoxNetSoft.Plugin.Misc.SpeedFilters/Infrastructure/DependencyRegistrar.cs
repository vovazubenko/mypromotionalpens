using Autofac;
using Autofac.Core;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Data;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Domain;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxNetSoft.Plugin.Misc.SpeedFilters.Infrastructure
{
    public class DependencyRegistrar: IDependencyRegistrar
    {
        private const string CONTEXT_NAME = "nop_object_context_foxnetsoft_speedfilters";

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            this.RegisterPluginDataContext<SpeedFiltersObjectContext>(builder, CONTEXT_NAME);

            //Register services

            builder.RegisterType<EfRepository<SS_Specific_Category_Setting>>()
          .As<IRepository<SS_Specific_Category_Setting>>()
          .WithParameter(ResolvedParameter.ForNamed<IDbContext>(CONTEXT_NAME))
          .InstancePerLifetimeScope();


        }

        public int Order
        {
            get { return 11; }
        }
    }
}
