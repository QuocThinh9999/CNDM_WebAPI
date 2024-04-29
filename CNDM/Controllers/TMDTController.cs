using CNDM.Models;
using Combinatorics.Collections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json;

namespace CNDM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TMDTController : ControllerBase
    {
        private readonly DbDataMiningContext _dbContext;
        public TMDTController(DbDataMiningContext context)
        {
            _dbContext = context;
        }
        [HttpGet("tao-danh-sach-pho-bien")]
        public IActionResult TaoDanhSachPhoBien()
        {
            var sanphams = _dbContext.SanPhams.ToList();
            var hoadons=_dbContext.HoaDons.ToList();
            int sosp = sanphams.Count;
            Dictionary<string, Dictionary<string, int>> dsphobien = new Dictionary<string, Dictionary<string, int>>(sosp);
            Dictionary<string, int> phu = new Dictionary<string, int>(sosp);
            List<List<SanPham>> dssps = new List<List<SanPham>>();
            foreach (var hoadon in hoadons)
            {
                var items = System.Text.Json.JsonSerializer.Deserialize<List<SanPham>>(hoadon.SanPham);
                if (items.Count > 1)
                {
                    dssps.Add(items);
                }
                
            }
                foreach (var sp in sanphams)
            {
                phu.Add(sp.IdSanPham, 0);
            }
            foreach(var sp in sanphams) 
            {
                dsphobien.Add(sp.IdSanPham, phu.ToDictionary());
            }
            foreach(var dssp in dssps)
            {
                var items = new Combinations<SanPham>(dssp, 2);
                foreach (var item in items)
                {
                    string idSanPham1 = item[0].IdSanPham;
                    string idSanPham2 = item[1].IdSanPham;
                    dsphobien[idSanPham1][idSanPham2 ] ++;
                    dsphobien[idSanPham2][idSanPham1 ] ++;
                }
            }
            foreach(var pb in dsphobien)
            {
                var dspb = new DanhSachPhoBien()
                {
                    IdSanPham=pb.Key,
                    TanSuat= JsonSerializer.Serialize(pb.Value)
                };

                _dbContext.DanhSachPhoBiens.Add(dspb);
            }
            _dbContext.SaveChanges();
            return Ok();
        }
        [HttpDelete("xoa-bang-pho-bien")]
        public IActionResult XoaBangPhoBien()
        {
            var dspbs = _dbContext.DanhSachPhoBiens.ToList();
            _dbContext.RemoveRange(dspbs);
            _dbContext.SaveChanges();
            return Ok();
        }
        [HttpGet("danh-sach-phoi-hop/{idSanPham}")]
        public IActionResult DanhSachPhoiHop(string idSanPham)
        {
            var dspbs = _dbContext.DanhSachPhoBiens
                .ToDictionary(d => d.IdSanPham, d => JsonSerializer.Deserialize<Dictionary<string, int>>(d.TanSuat));
            var sortedDspbs = dspbs[idSanPham]
    .OrderByDescending(kv => kv.Value)
    .ToList();
            return Ok(sortedDspbs);
        }
    }
}
