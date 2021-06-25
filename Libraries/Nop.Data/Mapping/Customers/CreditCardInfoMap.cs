using Nop.Core.Domain.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Data.Mapping.Customers
{
    public class CreditCardInfoMap : NopEntityTypeConfiguration<CreditCardInfo>
    {
        public CreditCardInfoMap()
        {
            this.ToTable("CreditCardInfo");
            this.HasKey(t => t.Id);

            this.HasRequired(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId);

            this.HasRequired(a => a.Address)
                .WithMany()
                .HasForeignKey(a => a.AddressId);

        }
    }
}
