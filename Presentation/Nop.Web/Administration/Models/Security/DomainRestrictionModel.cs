using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using FluentValidation.Attributes;
using Nop.Admin.Validators.Settings;


namespace Nop.Admin.Models.Security
{
    [Validator(typeof(RestrictDomainValidator))]
    public partial class DomainRestrictionModel 
    {

        [NopResourceDisplayName("Admin.Configuration.Settings.Domain.DomainURL")]
       
        public int dId { get; set; }
        
        public string DomainURL { get; set; }

        [NopResourceDisplayName("Default Domain")]
        public string DefaultDomainURL { get; set; }
    }

   
}