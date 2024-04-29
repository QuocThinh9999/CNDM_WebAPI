using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CNDM.Models;

[Table("TanSuatHaiSanPham")]
public partial class TanSuatHaiSanPham
{
    [Key]
    public int ThuTu { get; set; }

    public string? TanSuat { get; set; }
}
