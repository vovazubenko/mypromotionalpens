using System.Linq;
using Nop.Data;
using System.Data.Entity.Infrastructure;

namespace FoxNetSoft.Plugin.Misc.SpeedFilters.Data
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class NopObjectContextExtensions
    {
        /// <summary>
        /// Returns the set name for a given entity type (http://social.msdn.microsoft.com/Forums/en-US/adodotnetentityframework/thread/7a29d4e3-8550-43dd-aa09-2bb859466c0d)
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        public static string GetEntitySetName<T>(this NopObjectContext nopObjectContext)
        {
            var context = ((IObjectContextAdapter)(nopObjectContext)).ObjectContext;
            /* return context.MetadataWorkspace.GetEntityContainer(context.DefaultContainerName, System.Data.Metadata.Edm.DataSpace.CSpace)
                        .BaseEntitySets.Where(bes => bes.ElementType.Name == typeof(T).Name).FirstOrDefault().Name;*/
             return context.MetadataWorkspace.GetEntityContainer(context.DefaultContainerName, System.Data.Entity.Core.Metadata.Edm.DataSpace.CSpace)
                .BaseEntitySets.Where(bes => bes.ElementType.Name == typeof(T).Name).FirstOrDefault().Name;
        }
    }
}
