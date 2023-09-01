using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Main.Models;

[Keyless]
[Table("prova")]
public partial class Prova
{
    [Column("messaggio")]
    [StringLength(50)]
    public string? Messaggio { get; set; }

    [Column("username")]
    [StringLength(50)]
    public string? Username { get; set; }
}
