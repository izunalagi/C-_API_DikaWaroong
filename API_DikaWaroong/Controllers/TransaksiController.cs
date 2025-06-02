using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using API_DikaWaroong.Helpers;
using API_DikaWaroong.Models;

namespace API_DikaWaroong.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransaksiController : ControllerBase
    {
        private readonly SqlDBHelper _dbHelper;

        public TransaksiController(IConfiguration config)
        {
            _dbHelper = new SqlDBHelper(config);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var conn = _dbHelper.GetConnection();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM transaksi";

            var list = new List<Transaksi>();
            try
            {
                conn.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Transaksi
                    {
                        IdTransaksi = Convert.ToInt32(reader["id_transaksi"]),
                        Tanggal = Convert.ToDateTime(reader["tanggal"]),
                        AkunIdAkun = Convert.ToInt32(reader["Akun_id_akun"]),
                        StatusPesanan = Convert.ToBoolean(reader["status_pesanan"]),
                        BuktiTF = reader["bukti_tf"]?.ToString() ?? ""
                    });
                }
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        [HttpPost]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> Create([FromForm] CreateTransaksiRequest request)
        {
            var conn = _dbHelper.GetConnection();
            var cmd = conn.CreateCommand();

            // Simpan gambar bukti_tf
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.BuktiTF.FileName);
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "bukti");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.BuktiTF.CopyToAsync(stream);
            }

            cmd.CommandText = @"
                INSERT INTO transaksi (tanggal, Akun_id_akun, status_pesanan, bukti_tf)
                VALUES (@tanggal, @akun, @status, @bukti)";

            cmd.Parameters.Add(new NpgsqlParameter("@tanggal", request.Tanggal));
            cmd.Parameters.Add(new NpgsqlParameter("@akun", request.AkunIdAkun));
            cmd.Parameters.Add(new NpgsqlParameter("@status", request.StatusPesanan));
            cmd.Parameters.Add(new NpgsqlParameter("@bukti", fileName));

            try
            {
                conn.Open();
                cmd.ExecuteNonQuery();
                return Ok(new { message = "Transaksi berhasil dibuat" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
