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
            //return _connection.Query<Usuario>("SELECT * FROM Usuarios").ToList();

            List<Usuario> usuarios = new List<Usuario>();

            string sql = @"
                            SELECT * FROM Usuarios U
                            LEFT JOIN Contatos C ON C.UsuarioId = U.Id
                            LEFT JOIN EnderecosEntrega EE ON EE.UsuarioId = U.Id";

            _connection.Query<Usuario, Contato, EnderecoEntrega, Usuario>(sql,
                (usuario, contato, enderecoEntrega) =>
                {
                    if (usuarios.SingleOrDefault(u => u.Id == usuario.Id) == null)
                    {
                        usuario.EnderecosEntrega = new List<EnderecoEntrega>();
                        usuario.Contato = contato;
                        usuarios.Add(usuario);
                    }
                    else
                    {
                        usuario = usuarios.SingleOrDefault(u => u.Id == usuario.Id);
                    }

                    usuario.EnderecosEntrega.Add(enderecoEntrega);
                    return usuario;
                });

            return usuarios;

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
            List<Usuario> usuarios = new List<Usuario>();

            string sql = @"
                            SELECT * FROM Usuarios U
                            LEFT JOIN Contatos C ON C.UsuarioId = U.Id
                            LEFT JOIN EnderecosEntrega EE ON EE.UsuarioId = U.Id
                            WHERE U.Id = @Id";

            _connection.Query<Usuario, Contato, EnderecoEntrega, Usuario>(sql,
                (usuario, contato, enderecoEntrega) =>
                {
                    if (usuarios.SingleOrDefault(u => u.Id == usuario.Id) == null)
                    {
                        usuario.EnderecosEntrega = new List<EnderecoEntrega>();
                        usuario.Contato = contato;
                        usuarios.Add(usuario);
                    }
                    else
                    {
                        usuario = usuarios.SingleOrDefault(u => u.Id == usuario.Id);
                    }

                    usuario.EnderecosEntrega.Add(enderecoEntrega);
                    return usuario;
                }, new { Id = id });

            return usuarios.SingleOrDefault();
        }

        public void Insert(Usuario usuario)
        {
            _connection.Open();
            var transaction = _connection.BeginTransaction();

            try
            {
                string sql = @"
                                INSERT INTO Usuarios(Nome, Email, Sexo, Rg, NomeMae, SituacaoCadastro, DataCadastro) 
                                VALUES (@Nome, @Email, @Sexo, @Rg, @NomeMae, @SituacaoCadastro, @DataCadastro);
                                SELECT CAST(SCOPE_IDENTITY() AS INT);";

                usuario.Id = _connection.Query<int>(sql, usuario, transaction).Single();

                if (usuario.Contato != null)
                {
                    usuario.Contato.UsuarioId = usuario.Id;
                    string sqlContato = @"
                                            INSERT INTO Contatos (UsuarioId, Telefone, Celular) VALUES (@UsuarioId, @Telefone, @Celular);
                                            SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    usuario.Contato.Id = _connection.Query<int>(sqlContato, usuario.Contato, transaction).Single();
                }

                if (usuario.EnderecosEntrega != null && usuario.EnderecosEntrega.Count > 0)
                {
                    foreach (var enderecoEntrega in usuario.EnderecosEntrega)
                    {
                        enderecoEntrega.UsuarioId = usuario.Id;
                        string sqlEndereco = @"
                                INSERT INTO EnderecosEntrega (UsuarioId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento) 
                                VALUES (@UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento);
                                SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        enderecoEntrega.Id = _connection.Query<int>(sqlEndereco, enderecoEntrega, transaction).Single();
                    }
                }
                transaction.Commit();
            }
            catch (Exception)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception)
                {
                    //Tratar..
                }
            }
            finally
            {
                _connection.Close();
            }
        }

        public void Update(Usuario usuario)
        {
            _connection.Open();
            var transaction = _connection.BeginTransaction();
            try
            {
                string sql = @"
                                UPDATE USUARIOS
                                SET Nome = @Nome, Email = @Email, Sexo = @Sexo, Rg = @Rg, NomeMae = @NomeMae, 
                                    SituacaoCadastro = @SituacaoCadastro, DataCadastro = @DataCadastro
                                WHERE Id = @Id";

                _connection.Execute(sql, usuario, transaction);

                if (usuario.Contato != null)
                {
                    string sqlContato = "UPDATE Contatos SET Telefone = @Telefone, Celular = @Celular WHERE Id = @Id";
                    _connection.Execute(sqlContato, usuario.Contato, transaction);
                }

                string sqlDeletarEnderecosEntrega = "DELETE FROM EnderecosEntrega WHERE UsuarioId = @ID";
                _connection.Execute(sqlDeletarEnderecosEntrega, usuario, transaction);

                if (usuario.EnderecosEntrega != null && usuario.EnderecosEntrega.Count > 0)
                {
                    foreach (var enderecoEntrega in usuario.EnderecosEntrega)
                    {
                        enderecoEntrega.UsuarioId = usuario.Id;
                        string sqlEndereco = @"
                                INSERT INTO EnderecosEntrega (UsuarioId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento) 
                                VALUES (@UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento);
                                SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        enderecoEntrega.Id = _connection.Query<int>(sqlEndereco, enderecoEntrega, transaction).Single();
                    }
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception)
                {
                    //Tratar..
                }
            }
            finally
            {
                _connection.Close();
            }

        }

        public void Delete(int id)
        {
            _connection.Execute("DELETE FROM Usuarios WHERE Id = @Id", new { Id = id });
        }

    }
}
