using CNDM.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using Combinatorics.Collections;
namespace CNDM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TUP_FP_GrowthController : ControllerBase
    {
        private readonly DbDataMiningContext _dbContext;
        public TUP_FP_GrowthController(DbDataMiningContext context)
        {
            _dbContext = context;
        }
        [HttpGet("TUP-FP-Growth/{minsup}")]
        public async Task<IActionResult> TUP_FP_Growth(double minsup) 
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
            var ts1sp = new List<TanSuatMotSanPham>(ldsspCount);
            var ts2sp = new List<List<TanSuatMotSanPham>>(ldsspCount);

            for (int i = 0; i < ldsspCount; i++)
            {
                var ts1 = new TanSuatMotSanPham()
                {
                    ThuTu=i+1,
                    TanSuat = 0,
                    IdSanPham = ldssp[i].IdSanPham
            };
                ts1sp.Add(ts1);
            }
            for (int i = 0; i < ldsspCount; i++)
            {
                var listtong = new List<TanSuatMotSanPham>();
                for (int j = 0; j < i; j++)
                {
                    var listphu = new TanSuatMotSanPham()
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
                            ts1sp[i].TanSuat++;
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

            for (int i = ts1sp.Count - 1; i >= 0; i--)
            {

                if (ts1sp[i].TanSuat < k)
                {
                    ts2sp.Remove(ts2sp[i]);
                    ts1sp.Remove(ts1sp[i]);
                }
            }
            for (int i = 0; i < ts1sp.Count; i++)
            {
                for (int l = ts2sp[i].Count - 1; l >= 0; l--)
                {
                    int xoa = 0;
                    for (int j = 0; j < ts1sp.Count; j++)
                    {
                        if (ts2sp[i][l].IdSanPham == ts1sp[j].IdSanPham)
                        {
                            xoa = 1;
                            break;
                        }
                    }
                    if (xoa == 0)
                    {
                        ts2sp[i].Remove(ts2sp[i][l]);
                    }
                }
            }

            for (int i = 0; i < ts1sp.Count - 1; i++)
            {
                for (int j = 0; j < ts1sp.Count - i - 1; j++)
                {
                    if (ts1sp[j].TanSuat < ts1sp[j + 1].TanSuat)
                    {

                        var doicho = new TanSuatMotSanPham
                        {
                            IdSanPham = ts1sp[j + 1].IdSanPham,
                            TanSuat = ts2sp[j + 1].FirstOrDefault(c => c.IdSanPham == ts1sp[j].IdSanPham).TanSuat,
                            ThuTu = 0,
                        };
                        var listphu = ts2sp[j].ToList();
                        listphu.Add(doicho);
                        ts2sp[j + 1].RemoveAll(c => c.IdSanPham == ts1sp[j].IdSanPham);
                        ts2sp[j] = ts2sp[j + 1];
                        ts2sp[j + 1] = listphu.ToList();
                        var ts1 = new TanSuatMotSanPham
                        {
                            IdSanPham = ts1sp[j + 1].IdSanPham,
                            TanSuat = ts1sp[j + 1].TanSuat,
                        };
                        ts1sp[j + 1].IdSanPham = ts1sp[j].IdSanPham;
                        ts1sp[j + 1].TanSuat = ts1sp[j].TanSuat;

                        ts1sp[j].IdSanPham = ts1.IdSanPham;
                        ts1sp[j].TanSuat = ts1.TanSuat;
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
      
    }
}
