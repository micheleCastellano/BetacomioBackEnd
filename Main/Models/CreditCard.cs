using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Main.Models;

[Keyless]
[Table("CreditCard")]
public partial class CreditCard
{
    [Column("CreditCardID")]
    public int CreditCardId { get; set; }

    [StringLength(20)]
    public string Number { get; set; } = null!;

    [Column("CVV")]
    [StringLength(5)]
    public string Cvv { get; set; } = null!;

    [Column(TypeName = "date")]
    public DateTime ExpireDate { get; set; }

    [Column("CustomerID")]
    public int CustomerId { get; set; }
}
