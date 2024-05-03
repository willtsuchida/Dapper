using eCommerce.API.Models;
using eCommerce.API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private IUsuarioRepository _usuarioRepository;
        public UsuariosController()
        {
            _usuarioRepository = new UsuarioRepository();
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_usuarioRepository.Get());
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var usuario = _usuarioRepository.Get(id);
            if (usuario == null) return NotFound("Usuário não encontrado!");
            return Ok(usuario);
        }

        [HttpPost]
        public IActionResult Insert([FromBody] Usuario usuario)
        {
            _usuarioRepository.Insert(usuario);
            return Ok(usuario);
        }


        [HttpPut]
        public IActionResult Update([FromBody] Usuario usuario)
        {
            _usuarioRepository.Update(usuario);
            return Ok(usuario);
        }


        [HttpDelete("{id}")]
        public IActionResult Insert(int id)
        {
            _usuarioRepository.Delete(id);
            return Ok();
        }

    }
}
