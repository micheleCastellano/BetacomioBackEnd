using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Main.Models;

public partial class Credential
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CredentialsID")]
    public int CredentialsId { get; set; }

    [StringLength(50)]
    public string EmailAddress { get; set; } = null!;

    [StringLength(50)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(50)]
    public string PasswordSalt { get; set; } = null!;
}
