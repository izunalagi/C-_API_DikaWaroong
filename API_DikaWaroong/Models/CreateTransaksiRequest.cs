namespace API_DikaWaroong.Models
{
    public class CreateTransaksiRequest
    {
        public DateTime Tanggal { get; set; }
        public int AkunIdAkun { get; set; }
        public bool StatusPesanan { get; set; }
        public IFormFile BuktiTF { get; set; } = default!;
    }
}
