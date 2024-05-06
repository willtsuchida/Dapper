namespace eCommerce.API.Models
{
    public class Contato
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Telefone{ get; set; }
        public string Ceulular { get; set; }
        public Usuario Usuario { get; set; } //1..1, 1 contato tem 1 usuario
    }
}
