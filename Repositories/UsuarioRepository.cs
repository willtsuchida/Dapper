using eCommerce.API.Models;
using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace eCommerce.API.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private IDbConnection _connection;
        public UsuarioRepository()
        {
            _connection = new SqlConnection("Data Source=(localdb)\\ProjectModels;Initial Catalog=eCommerce;Integrated Security=True;Connect Timeout=30;");
        }

        //ADO.NET -> Dapper
        public List<Usuario> Get()
        {
            return _connection.Query<Usuario>("SELECT * FROM Usuarios").ToList(); //Retona IEnumerable, converter pra List

            // O Model DEVE ter as props com o mesmo nome das colunas no banco. Caso queira trocar tem que passar ALIAS no nome da coluna na query.

            /*   USANDO ADO.NET
                
                  List<Usuario> usuarios = new List<Usuario>();
            try
            {
                SqlCommand command = new SqlCommand();                                        Classe para executar o comando
                command.CommandText = "SELECT * FROM Usuarios";                               Comando a ser executado
                command.Connection = (SqlConnection)_connection;                              Conexao a ser usada

                _connection.Open();                                                           Abrindo conexao

                SqlDataReader dataReader = command.ExecuteReader();                           Executando comando e armazena o resultado ness var dataReader
                    
                while(dataReader.Read())                                                      Passa linha por linha da tabela pegando as colunas, atribuindo a uma prop do usuario
                {
                    Usuario usuario = new Usuario();
                    usuario.Id = dataReader.GetInt32("Id");
                    usuario.Nome = dataReader.GetString("Nome");
                    usuario.Email = dataReader.GetString("Email");
                    usuario.Sexo = dataReader.GetString("Sexo");
                    usuario.Rg = dataReader.GetString("Rg");
                    usuario.NomeMae = dataReader.GetString("NomeMae");
                    usuario.SituacaoCadastro = dataReader.GetString("SituacaoCadastro");
                    usuario.DataCadastro = dataReader.GetDateTimeOffset(8);                   8 = numero da coluna, feito bem manualmente, nota que dependendo de como o select for feito as colunas estariam em orderm diferente

                    usuarios.Add(usuario);                                                     

                }
            }
            finally
            {
                _connection.Close();                                                          Fecha conexao
            }   
            */

        }

        public Usuario Get(int id)
        {
            return _connection.QuerySingleOrDefault<Usuario>("SELECT * FROM Usuarios WHERE Id = @Id", new { Id = id }); 
        }

        public void Insert(Usuario usuario)
        {
            string sql = @" INSERT INTO 
                            Usuarios(Nome, Email, Sexo, Rg, NomeMae, SituacaoCadastro, DataCadastro) 
                            VALUES (@Nome, @Email, @Sexo, @Rg, @NomeMae, @SituacaoCadastro, @DataCadastro);
                            SELECT CAST(SCOPE_IDENTITY() AS INT);
                            ";
            //Retorna o ID do ultimo usuario inserido

            usuario.Id = _connection.Query<int>(sql, usuario).Single();

        }

        public void Update(Usuario usuario)
        {
            string sql = @" UPDATE USUARIOS
                            SET Nome = @Nome, Email = @Email, Sexo = @Sexo, Rg = @Rg, NomeMae = @NomeMae, SituacaoCadastro = @SituacaoCadastro, DataCadastro = @DataCadastro
                            WHERE Id = @Id";

            _connection.Execute(sql, usuario);
        }

        public void Delete(int id)
        {
            _connection.Execute("DELETE FROM Usuarios WHERE Id = @Id", new { Id = id });
        }

    }
}
