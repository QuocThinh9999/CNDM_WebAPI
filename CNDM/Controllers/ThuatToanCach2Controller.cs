using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CNDM.Models;
using System.Text.Json;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Combinatorics.Collections;

namespace CNDM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThuatToanCach2Controller : ControllerBase
    {
        private readonly DbDataMiningContext _dbContext;
        public ThuatToanCach2Controller(DbDataMiningContext context)
        {
            _dbContext = context;
        }
        [HttpGet("duyet-hoa-don-1-lan")]
        public IActionResult DuyetHoaDon1Lan()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            TangTanSuatHoaDon();
            //XuLyHoaDonCach2();
            stopwatch.Stop();
            TimeSpan elapsed = stopwatch.Elapsed;
            return Ok(elapsed.TotalMilliseconds);

        }
        [HttpGet("duyet-hoa-don-2-lan")]
        public IActionResult DuyetHoaDon2Lan()
        {
            
            //TangTanSuatHoaDon();
           var kq= XuLyHoaDonCach2();
           return Ok(kq);
        }
      
        private void TangTanSuatHoaDon()
        {
            var ts1sp = _dbContext.TanSuatMotSanPhams.ToList();
            var ts2sp = _dbContext.TanSuatHaiSanPhams.ToList();
            var hoadons = _dbContext.HoaDons.ToList();
            var ldssp = _dbContext.SanPhams.ToList();
            List<List<TanSuatMotSanPham>> listts2sp = new List<List<TanSuatMotSanPham>>();
            for (int i = 0; i < ldssp.Count; i++)
            {
                var ts1 = new TanSuatMotSanPham();
                ts1.ThuTu = i+1;
                ts1.TanSuat = 0;
                ts1.IdSanPham = ldssp[i].IdSanPham;
                ts1sp.Add(ts1);
                ////////////////////////////////////////
                var ts2 = new TanSuatHaiSanPham();
                ts2.ThuTu = i + 1;
                //ts2.IdHaiSanPham = ldssp[i].IdSanPham;
                var listtong = new List<TanSuatMotSanPham>();
                for (int j = 0; j < i; j++)
                {
                    var listphu = new TanSuatMotSanPham();
                    listphu.ThuTu = 0;
                    listphu.IdSanPham = ts1sp[j].IdSanPham;
                    listphu.TanSuat = 0;
                    listtong.Add(listphu);
                }
                listts2sp.Add(listtong);
                ts2sp.Add(ts2);
            }
           
            foreach (var hoadon in hoadons)
            {
                var dssp = System.Text.Json.JsonSerializer.Deserialize<List<SanPham>>(hoadon.SanPham);
                int ssp = 0;
                int ssphd = dssp.Count;
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

                for (int i = ts2sp.Count - 1; i >= 0; i--)
                {
                    foreach (var sp in dssp)
                    {
                        if (sp.IdSanPham == ts1sp[i].IdSanPham)
                        {
                            dssp.Remove(sp);
                            foreach (var spp in dssp)
                            {
                                for (int j = 0; j < listts2sp[i].Count; j++)
                                {
                                    if (spp.IdSanPham == listts2sp[i][j].IdSanPham)
                                    {
                                        listts2sp[i][j].TanSuat++;
                                        break;
                                    }
                                }
                            }
                            ssp++;
                            break;
                        }
                    }
                    if (ssp == ssphd)
                    {
                        break;
                    }
                }
            }
            for (int i = 0; i < ts1sp.Count - 1; i++)
            {
                for (int j = 0; j < ts1sp.Count - i - 1; j++)
                {
                    if (ts1sp[j].TanSuat < ts1sp[j + 1].TanSuat)
                    {
                        //var doicho = new TanSuatHaiSanPham
                        //{
                        //    IdHaiSanPham = ts2sp[j].IdHaiSanPham,

                        //};
                        //ts2sp[j].IdHaiSanPham = ts2sp[j + 1].IdHaiSanPham;
                        //ts2sp[j + 1].IdHaiSanPham = doicho.IdHaiSanPham;
                        var doicho2 = new TanSuatMotSanPham
                        {
                            IdSanPham = ts1sp[j + 1].IdSanPham,
                            TanSuat = listts2sp[j + 1].FirstOrDefault(c => c.IdSanPham == ts1sp[j].IdSanPham).TanSuat,
                            ThuTu = 0,
                        };
                        var listphu = listts2sp[j].ToList();
                        listphu.Add(doicho2);
                        listts2sp[j + 1].RemoveAll(c => c.IdSanPham == ts1sp[j].IdSanPham);
                        listts2sp[j] = listts2sp[j + 1];
                        listts2sp[j + 1] = listphu.ToList();
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
            for (int i = 0; i < ts1sp.Count; i++)
            {
                ts2sp[i].TanSuat = System.Text.Json.JsonSerializer.Serialize(listts2sp[i]);
                _dbContext.TanSuatHaiSanPhams.Add(ts2sp[i]);
                _dbContext.TanSuatMotSanPhams.Add(ts1sp[i]);

            }
            _dbContext.SaveChanges();
        }
        private double XuLyHoaDonCach2()
        {
            var hoadons = _dbContext.HoaDons.ToList();
            var ldssp = _dbContext.SanPhams.ToList();
            var ts1sp = _dbContext.TanSuatMotSanPhams.ToList();
            var ts2sp = _dbContext.TanSuatHaiSanPhams.ToList();
            List<List<SanPham>> dssps = new List<List<SanPham>>();
            for (int i = 0; i < ldssp.Count; i++)
            {
                var ts1 = new TanSuatMotSanPham();
                ts1.ThuTu = i + 1;
                ts1.TanSuat = 0;
                ts1.IdSanPham = ldssp[i].IdSanPham;
                ts1sp.Add(ts1);
            }
            foreach (var hoadon in hoadons)
            {
                var items = System.Text.Json.JsonSerializer.Deserialize<List<SanPham>>(hoadon.SanPham);
                if (items.Count > 1)
                {
                    dssps.Add(items);
                }
            }
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
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
            for (int i = 0; i < ts1sp.Count - 1; i++)
            {
                for (int j = 0; j < ts1sp.Count - i - 1; j++)
                {
                    if (ts1sp[j].TanSuat < ts1sp[j + 1].TanSuat)
                    {
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
            List<List<TanSuatMotSanPham>> listts2sp = new List<List<TanSuatMotSanPham>>();
            for (int i = 0; i < ts1sp.Count; i++)
            {
                var ts2 = new TanSuatHaiSanPham();
                ts2.ThuTu = i + 1;
                var listtong = new List<TanSuatMotSanPham>();
                for (int j = 0; j < i; j++)
                {
                    var listphu = new TanSuatMotSanPham();
                    listphu.ThuTu = 0;
                    listphu.IdSanPham = ts1sp[j].IdSanPham;
                    listphu.TanSuat = 0;
                    listtong.Add(listphu);
                    
                }
                listts2sp.Add(listtong);
                //ts2.TanSuat = System.Text.Json.JsonSerializer.Serialize(listtong);
                ts2sp.Add(ts2);
            }
           
            //foreach (var ts in ts2sp)
            //{
            //    List<TanSuatMotSanPham> list = new List<TanSuatMotSanPham>();
            //    list = System.Text.Json.JsonSerializer.Deserialize<List<TanSuatMotSanPham>>(ts.TanSuat);
            //    listts2sp.Add(list);
            //}
            foreach (var hoadon in hoadons)
            {
                var dssp = System.Text.Json.JsonSerializer.Deserialize<List<SanPham>>(hoadon.SanPham);
                int ssp = 0;
                int ssphd = dssp.Count;
                for (int i = ts2sp.Count - 1; i >= 0; i--)
                {
                    foreach (var sp in dssp)
                    {
                        if (sp.IdSanPham == ts1sp[i].IdSanPham)
                        {
                            dssp.Remove(sp);
                            foreach (var spp in dssp)
                            {
                                foreach (var dshsp in listts2sp[i])
                                {
                                    if (spp.IdSanPham == dshsp.IdSanPham)
                                    {
                                        dshsp.TanSuat++;
                                        break;
                                    }
                                }
                            }
                            ssp++;
                            break;
                        }
                    }
                    if (ssp == ssphd)
                    {
                        break;
                    }
                }
            }
            stopwatch.Stop();
            TimeSpan elapsed = stopwatch.Elapsed;
            for (int i = 0; i < ts2sp.Count; i++)
            {
                ts2sp[i].TanSuat = System.Text.Json.JsonSerializer.Serialize(listts2sp[i]);
                _dbContext.TanSuatHaiSanPhams.Add(ts2sp[i]);
                _dbContext.TanSuatMotSanPhams.Add(ts1sp[i]);

            }
            
            _dbContext.SaveChanges();
            return elapsed.TotalMilliseconds;
        }
        static IEnumerable<IEnumerable<T>> GetCombinations<T>(IEnumerable<T> elements, int k)
        {
            if (k == 0)
            {
                yield return Enumerable.Empty<T>();
            }
            else
            {
                var i = 0;
                foreach (var element in elements)
                {
                    var remaining = elements.Skip(i + 1);
                    foreach (var combination in GetCombinations(remaining, k - 1))
                    {
                        yield return new[] { element }.Concat(combination);
                    }
                    i++;
                }
            }
        }

        [HttpGet("bo-san-pham-tiem-nang")]
        public async Task<IActionResult> BoSanPhamTiemNang(double s_min)
        {

            if (s_min > 1 || s_min < 0)
            {
                return Ok("Vui long nhap lai support_min");
            }
            
            var sohoadon = _dbContext.HoaDons.Count();
            double k = s_min * sohoadon;
            var ts1sp = _dbContext.TanSuatMotSanPhams.ToList();
            var ts2sp = _dbContext.TanSuatHaiSanPhams.ToList();
            List<List<TanSuatMotSanPham>> dstss=new List<List<TanSuatMotSanPham>>(ts2sp.Count);
           
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();;
            List<TanSuatHaiSanPham> ts2sp2 = new List<TanSuatHaiSanPham>(ts2sp.Count);
            for (int i = 0; i < ts1sp.Count; i++)
            {
                if (ts1sp[i].TanSuat >= k)
                {
                    ts2sp2.Add(ts2sp[i]);
                }
                else break;
            }
            foreach (var ts2 in ts2sp2)
            {
                dstss.Add(System.Text.Json.JsonSerializer.Deserialize<List<TanSuatMotSanPham>>(ts2.TanSuat));
            }
         
            List<List<string>> ketqua = new List<List<string>>();
            for (int i = 0; i < ts2sp2.Count; i++)
            {
                List<string> listsp = new List<string>();
                foreach (var sp in dstss[i])
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
            return Ok(elapsed.TotalMilliseconds);
            //return Ok(ketqua);
        }

        [HttpDelete("xoa-bang-tan-suat")]
        public IActionResult XoaBangTanSuat()
        {
            var ts1sp = _dbContext.TanSuatMotSanPhams.ToList();
            var ts2sp = _dbContext.TanSuatHaiSanPhams.ToList();
            _dbContext.RemoveRange(ts1sp);
            _dbContext.RemoveRange(ts2sp);
            _dbContext.SaveChanges();
            return Ok();
        }
       
        [HttpPost("tan-suat-phoi-hop")]
        public IActionResult Test([FromForm] List<string> IDSanPham)
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
            return Ok(k);
        }
      
    }

}
