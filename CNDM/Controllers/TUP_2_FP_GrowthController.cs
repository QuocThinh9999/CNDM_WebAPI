using CNDM.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using Combinatorics.Collections;
using System.Collections.Generic;
namespace CNDM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TUP_2_FP_GrowthController : ControllerBase
    {
        private readonly DbDataMiningContext _dbContext;
        public TUP_2_FP_GrowthController(DbDataMiningContext context)
        {
            _dbContext = context;
        }
        [HttpGet("TUP-2_FP-Growth/{minsup}")]
        public async Task<IActionResult> TUP_2_FP_Growth(double minsup)
        {
            var ketqua = await DuyetHoaDon(minsup);
            return Ok(ketqua);
        }
        private async Task<IActionResult> DuyetHoaDon(double s_min)
        {
            List<HoaDon> hoadons = _dbContext.HoaDons.ToList();
            List<SanPham> ldssp = _dbContext.SanPhams.ToList();
            List<List<SanPham>> dssps = new List<List<SanPham>>();
            Dictionary<int, int> sosptoida = new Dictionary<int, int>();
            double k = s_min * hoadons.Count();
            foreach (var hoadon in hoadons)
            {
                var items = System.Text.Json.JsonSerializer.Deserialize<List<SanPham>>(hoadon.SanPham);
                if (items.Count > 1)
                {
                    dssps.Add(items);
                }
                var count = items.Count;
                if (sosptoida.ContainsKey(count))
                {
                    sosptoida[count]++;
                }
                else
                {
                    sosptoida.Add(count, 1);
                }
            }
            sosptoida = sosptoida.Where(pair => pair.Value < k)
                     .ToDictionary(pair => pair.Key, pair => pair.Value);
            var maxKey = sosptoida.Keys.Max();
            int ldsspCount=ldssp.Count();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<TanSuatMotSanPham> ts1sp = new List<TanSuatMotSanPham>(ldsspCount);

            var ts1spNew = ldssp.Select((ld, i) => new TanSuatMotSanPham
            {
                ThuTu = i + 1,
                TanSuat = 0,
                IdSanPham = ld.IdSanPham
            }).ToList();

            ts1sp.AddRange(ts1spNew);
            foreach (var dssp in dssps)
            {
                foreach (var sp in dssp)
                {
                    for (int i = 0; i < ts1sp.Count; i++)
                    {
                        if (ts1sp[i].IdSanPham == sp.IdSanPham)
                        {
                            ts1sp[i].TanSuat++;
                            break;
                        }
                    }
                }
            }
            List<TanSuatMotSanPham> key = new List<TanSuatMotSanPham>(ts1sp.Count);
            for (int i = ts1sp.Count - 1; i >= 0; i--)
            {
                if (ts1sp[i].TanSuat >= k)
                {
                    key.Add(ts1sp[i]);
                }
            }
            ts1sp = key;
            ts1sp = ts1sp.OrderByDescending(item => item.TanSuat).ToList();
            var ts2sp = new List<List<TanSuatMotSanPham>>(ts1sp.Count);

            for (int i = 0; i < ts1sp.Count; i++)
            {
                var listtong = new List<TanSuatMotSanPham>(i);

                for (int j = 0; j < i; j++)
                {
                    var listphu = new TanSuatMotSanPham
                    {
                        ThuTu = j,
                        IdSanPham = ts1sp[j].IdSanPham,
                        TanSuat = 0
                    };
                    listtong.Add(listphu);
                }

                ts2sp.Add(listtong);
            }
            foreach (var dssp in dssps)
            {
                foreach (var sp in dssp)
                {
                    for (int i = 0; i < ts1sp.Count; i++)
                    {
                        if (ts1sp[i].IdSanPham == sp.IdSanPham)
                        {
                            List<SanPham> dsspx = new List<SanPham>(dssp);
                            dsspx.Remove(sp);
                            foreach (var spp in dsspx)
                            {
                                for (int j = 0; j < ts2sp[i].Count; j++)
                                {
                                    if (spp.IdSanPham == ts2sp[i][j].IdSanPham)
                                    {
                                        ts2sp[i][j].TanSuat++;
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
            List<List<string>> ketqua = new List<List<string>>();
            for (int i = 0; i < ts2sp.Count; i++)
            {
                List<string> listsp = new List<string>();
                foreach (var sp in ts2sp[i])
                {
                    if (sp.TanSuat > k)
                    {
                        listsp.Add(sp.IdSanPham);
                    }
                }
                if (listsp.Count == 0)
                {
                    continue;
                }
                int tohoptoida = listsp.Count;
                if (tohoptoida > maxKey)
                {
                    tohoptoida = maxKey;
                }   
                for (int j = 1; j <= tohoptoida; j++)
                {
                    var items = new Combinations<string>(listsp, j);
                    foreach (var item in items)
                    {
                        var newItem = item.ToList();
                        newItem.Add(ts1sp[i].IdSanPham);
                        
                        ketqua.Add(newItem);
                    }
                }
            }
            stopwatch.Stop();
            TimeSpan elapsed = stopwatch.Elapsed;
            KetQua ketQua = new KetQua();
            //ketQua.ketqua = ketqua;
            ketQua.time = elapsed.TotalMilliseconds;
            return Ok(ketQua);
        }
        public class KetQua
        {
            public List<List<string>> ketqua { get; set; }
            public double time { get; set; }
        }
        private int Test(List<string> IDSanPham)
        {
            var hoadons = _dbContext.HoaDons.ToList();
            int k = 0;
            foreach (var hoadon in hoadons)
            {
                var dssp = System.Text.Json.JsonSerializer.Deserialize<List<SanPham>>(hoadon.SanPham);
                int i = 0;
                foreach (var sp in dssp)
                {
                    foreach (var id in IDSanPham)
                    {
                        if (sp.IdSanPham == id)
                        {
                            i++;
                        }
                    }
                }
                if (i == IDSanPham.Count)
                {
                    k++;
                }
            }
            return k;
        }
    }
}
