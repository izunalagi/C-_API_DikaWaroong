using Microsoft.AspNetCore.Mvc;
using API_DikaWaroong.Helpers;
using API_DikaWaroong.Models;
using Npgsql;

namespace API_DikaWaroong.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DetailTransaksiController : ControllerBase
    {
        private readonly SqlDBHelper _dbHelper;

        public DetailTransaksiController(IConfiguration config)
        {
            _dbHelper = new SqlDBHelper(config);
        }

        // GET: api/detailtransaksi
        [HttpGet]
        public IActionResult GetAll()
        {
            var conn = _dbHelper.GetConnection();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM detail_transaksi";

            var list = new List<DetailTransaksi>();
            try
            {
                conn.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new DetailTransaksi
                    {
                        IdDetailTransaksi = (int)reader["id_detail_transaksi"],
                        Quantity = (int)reader["quantity"],
                        ProdukIdProduk = (int)reader["Produk_id_produk"],
                        TransaksiIdTransaksi = (int)reader["Transaksi_id_transaksi"]
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

        // POST: api/detailtransaksi
        [HttpPost]
        public IActionResult Create([FromBody] DetailTransaksi request)
        {
            var conn = _dbHelper.GetConnection();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO detail_transaksi 
                (quantity, Produk_id_produk, Transaksi_id_transaksi) 
                VALUES (@quantity, @produk, @transaksi)";

            cmd.Parameters.Add(new NpgsqlParameter("@quantity", request.Quantity));
            cmd.Parameters.Add(new NpgsqlParameter("@produk", request.ProdukIdProduk));
            cmd.Parameters.Add(new NpgsqlParameter("@transaksi", request.TransaksiIdTransaksi));

            try
            {
                conn.Open();
                cmd.ExecuteNonQuery();
                return Ok(new { message = "Detail transaksi berhasil dibuat" });
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

        // PUT: api/detailtransaksi/{id}
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] DetailTransaksi request)
        {
            var conn = _dbHelper.GetConnection();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE detail_transaksi 
                SET quantity = @quantity, 
                    Produk_id_produk = @produk, 
                    Transaksi_id_transaksi = @transaksi 
                WHERE id_detail_transaksi = @id";

            cmd.Parameters.Add(new NpgsqlParameter("@quantity", request.Quantity));
            cmd.Parameters.Add(new NpgsqlParameter("@produk", request.ProdukIdProduk));
            cmd.Parameters.Add(new NpgsqlParameter("@transaksi", request.TransaksiIdTransaksi));
            cmd.Parameters.Add(new NpgsqlParameter("@id", id));

            try
            {
                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                    return Ok(new { message = "Detail transaksi berhasil diupdate" });
                else
                    return NotFound(new { message = "Data tidak ditemukan" });
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

        // DELETE: api/detailtransaksi/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var conn = _dbHelper.GetConnection();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM detail_transaksi WHERE id_detail_transaksi = @id";
            cmd.Parameters.Add(new NpgsqlParameter("@id", id));

            try
            {
                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                    return Ok(new { message = "Detail transaksi berhasil dihapus" });
                else
                    return NotFound(new { message = "Data tidak ditemukan" });
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
