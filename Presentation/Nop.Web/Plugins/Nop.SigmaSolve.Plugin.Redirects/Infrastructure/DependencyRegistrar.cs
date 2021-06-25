using Autofac;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Web.Framework.Mvc;
using Nop.Data;
using Nop.Core.Data;
using Autofac.Core;
using Nop.SigmaSolve.Plugin.Redirects.Data;
using Nop.SigmaSolve.Plugin.Redirects.Domain;
using Nop.Core.Configuration;

namespace Nop.SigmaSolve.Plugin.Redirects.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string CONTEXT_NAME = "nop_object_context_product_CustomRedirection_Context";
         
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            //data context
            this.RegisterPluginDataContext<PluginContext>(builder, CONTEXT_NAME);
            //override required repository with our custom context
            builder.RegisterType<EfRepository<CustomRedirection>>()
            .As<IRepository<CustomRedirection>>()
            .WithParameter(ResolvedParameter.ForNamed<IDbContext>(CONTEXT_NAME))
            .InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
