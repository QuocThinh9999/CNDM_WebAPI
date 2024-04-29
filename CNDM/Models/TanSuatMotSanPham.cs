using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CNDM.Models;

[Table("TanSuatMotSanPham")]
public partial class TanSuatMotSanPham
{
    [Key]
    public int ThuTu { get; set; }

    public string? IdSanPham { get; set; }

    public int? TanSuat { get; set; }
}
