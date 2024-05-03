using eCommerce.API.Models;

namespace eCommerce.API.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        //Simulando banco
        private static List<Usuario> _db = new List<Usuario>()
        {
            new Usuario { Id = 1, Nome = "Filipe Rogrigues", Email = "filiperodrigues@gmail.com"},
            new Usuario { Id = 2, Nome = "Marcelo Rogrigues", Email = "marcelo.rodrigues@gmail.com"},
            new Usuario { Id = 3, Nome = "Jessica Rogrigues", Email = "jessica.rodrigues@gmail.com"}
        };

        public List<Usuario> Get()
        {
            return _db;
        }

        public Usuario Get(int id)
        {
            return _db.FirstOrDefault(x => x.Id == id);
        }

        public void Insert(Usuario usuario)
        {
            var ultimoUsuario = _db.LastOrDefault();
            if (ultimoUsuario == null)
            {
                usuario.Id = 1;
            }
            else
            {
                usuario.Id = ultimoUsuario.Id;
                usuario.Id++;
            }
            _db.Add(usuario);
        }

        public void Update(Usuario usuario)
        {
            _db.Remove(_db.FirstOrDefault(x => x.Id == usuario.Id));
            _db.Add(usuario);
        }

        public void Delete(int id)
        {
            _db.Remove(_db.FirstOrDefault(x => x.Id == id));
        }
    }
}
